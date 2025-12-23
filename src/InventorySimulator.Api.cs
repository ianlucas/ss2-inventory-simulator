/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using SwiftlyS2.Shared.Players;

namespace InventorySimulator;

public partial class InventorySimulator
{
    public async Task FetchPlayerInventory(ulong steamId, bool force = false)
    {
        var existing = PlayerInventoryManager.TryGetValue(steamId, out var i) ? i : null;
        if (!force && existing != null)
            return;
        if (PlayerInFetchManager.ContainsKey(steamId))
            return;
        PlayerInFetchManager.TryAdd(steamId, true);
        var response = await Api.FetchEquipped(steamId);
        if (response != null)
        {
            var inventory = new PlayerInventory(response);
            if (existing != null)
                inventory.CachedWeaponEconItems = existing.CachedWeaponEconItems;
            PlayerCooldownManager[steamId] = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            SetPlayerInventory(steamId, inventory);
        }
        PlayerInFetchManager.Remove(steamId, out var _);
        if (PlayerPostFetchManager.TryGetValue(steamId, out var callback))
        {
            callback();
            PlayerPostFetchManager.TryRemove(steamId, out _);
        }
    }

    public async void RefreshPlayerInventory(IPlayer player, bool force = false)
    {
        if (!force)
        {
            await FetchPlayerInventory(player.SteamID);
            Core.Scheduler.NextWorldUpdate(() =>
            {
                if (player.IsValid)
                    GiveOnLoadPlayerInventory(player);
            });
            return;
        }
        var oldInventory = GetPlayerInventory(player);
        await FetchPlayerInventory(player.SteamID, true);
        Core.Scheduler.NextWorldUpdate(() =>
        {
            if (player.IsValid)
            {
                player.SendChat(Core.Localizer["invsim.ws_completed"]);
                GiveOnLoadPlayerInventory(player);
                GiveOnRefreshPlayerInventory(player, oldInventory);
            }
        });
    }

    public async void SendStatTrakIncrement(ulong userId, int targetUid)
    {
        if (Api.HasApiKey())
            await Api.SendStatTrakIncrement(targetUid, userId.ToString());
    }

    public async void SendSignIn(ulong userId)
    {
        if (PlayerInAuthManager.ContainsKey(userId))
            return;
        PlayerInAuthManager.TryAdd(userId, true);
        var response = await Api.SendSignIn(userId.ToString());
        PlayerInAuthManager.TryRemove(userId, out var _);
        Core.Scheduler.NextWorldUpdate(() =>
        {
            var player = Core.PlayerManager.GetPlayerFromSteamID(userId);
            if (response == null)
            {
                player?.SendChat(Core.Localizer["invsim.login_failed"]);
                return;
            }
            player?.SendChat(
                Core.Localizer[
                    "invsim.login",
                    $"{Api.GetUrl("/api/sign-in/callback")}?token={response.Token}"
                ]
            );
        });
    }
}
