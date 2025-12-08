/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using SwiftlyS2.Shared.Players;

namespace InventorySimulator;

public class SignInUserResponse
{
    [JsonPropertyName("token")]
    public required string Token { get; set; }
}

public partial class InventorySimulator
{
    public string GetAPIUrl(string pathname = "")
    {
        return $"{Url.Value}{pathname}";
    }

    public async Task<T?> Fetch<T>(string pathname, bool rethrow = false)
    {
        var url = GetAPIUrl(pathname);
        try
        {
            using HttpClient client = new();
            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            string jsonContent = response.Content.ReadAsStringAsync().Result;
            T? data = JsonSerializer.Deserialize<T>(jsonContent);
            return data;
        }
        catch (Exception error)
        {
            Core.Logger.LogError("GET {Url} failed: {Message}", url, error.Message);
            if (rethrow)
                throw;
            return default;
        }
    }

    public async Task<T?> Send<T>(string pathname, object data)
    {
        var url = GetAPIUrl(pathname);
        try
        {
            var json = JsonSerializer.Serialize(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            using HttpClient client = new();
            var response = await client.PostAsync(url, content);
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                Core.Logger.LogError("POST {Url} failed, check your invsim_apikey's value.", url);
                return default;
            }
            if (!response.IsSuccessStatusCode)
            {
                Core.Logger.LogError(
                    "POST {Url} failed with status code: {StatusCode}",
                    url,
                    response.StatusCode
                );
                return default;
            }
            var responseContent = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrEmpty(responseContent))
                return default;
            return JsonSerializer.Deserialize<T>(responseContent);
        }
        catch (Exception error)
        {
            Core.Logger.LogError("POST {Url} failed: {Message}", url, error.Message);
            return default;
        }
    }

    public async Task FetchPlayerInventory(ulong steamId, bool force = false)
    {
        var existing = PlayerInventoryManager.TryGetValue(steamId, out var i) ? i : null;
        if (!force && existing != null)
            return;
        if (FetchingPlayerInventory.ContainsKey(steamId))
            return;
        FetchingPlayerInventory.TryAdd(steamId, true);
        for (var attempt = 0; attempt < 3; attempt++)
            try
            {
                var inventory = await Fetch<PlayerInventory>(
                    $"/api/equipped/v3/{steamId}.json",
                    true
                );
                if (inventory != null)
                {
                    if (existing != null)
                        inventory.CachedWeaponEconItems = existing.CachedWeaponEconItems;
                    PlayerCooldownManager[steamId] = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                    AddPlayerInventory(steamId, inventory);
                }
                break;
            }
            catch
            {
                // Try again to fetch data (up to 3 times).
            }
        FetchingPlayerInventory.Remove(steamId, out var _);
    }

    public async void RefreshPlayerInventory(IPlayer player, bool force = false)
    {
        if (!force)
        {
            await FetchPlayerInventory(player.SteamID);
            Core.Scheduler.NextTick(() =>
            {
                if (player.IsValid)
                    GiveOnLoadPlayerInventory(player);
            });
            return;
        }
        var oldInventory = GetPlayerInventory(player);
        await FetchPlayerInventory(player.SteamID, true);
        Core.Scheduler.NextTick(() =>
        {
            if (player.IsValid)
            {
                player.SendChat(Core.Localizer["invsim.ws_completed"]);
                GiveOnLoadPlayerInventory(player);
                GiveOnRefreshPlayerInventory(player, oldInventory);
            }
        });
    }

    public async Task Send(string pathname, object data)
    {
        await Task.Run(() => Send<object>(pathname, data));
    }

    public async void SendStatTrakIncrement(ulong userId, int targetUid)
    {
        if (ApiKey.Value == "")
            return;
        await Send(
            "/api/increment-item-stattrak",
            new
            {
                apiKey = ApiKey.Value,
                targetUid,
                userId = userId.ToString(),
            }
        );
    }

    public async void SendSignIn(ulong userId)
    {
        if (AuthenticatingPlayer.ContainsKey(userId))
            return;
        AuthenticatingPlayer.TryAdd(userId, true);
        var response = await Send<SignInUserResponse>(
            "/api/sign-in",
            new { apiKey = ApiKey.Value, userId = userId.ToString() }
        );
        AuthenticatingPlayer.TryRemove(userId, out var _);
        Core.Scheduler.NextTick(() =>
        {
            var player = Utilities.GetPlayerFromSteamID(Core, userId);
            if (response == null)
            {
                player?.SendChat(Core.Localizer["invsim.login_failed"]);
                return;
            }
            player?.SendChat(
                Core.Localizer[
                    "invsim.login",
                    $"{GetAPIUrl("/api/sign-in/callback")}?token={response.Token}"
                ]
            );
        });
    }
}
