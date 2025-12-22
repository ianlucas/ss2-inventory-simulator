/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using SwiftlyS2.Shared.Convars;

namespace InventorySimulator.Services;

public class SignInUserResponse
{
    [JsonPropertyName("token")]
    public required string Token { get; set; }
}

public class InventorySimulatorApi
{
    private static ILogger? _logger;
    private static IConVar<string>? _url;

    public static void Initialize(ILogger logger, IConVar<string> url)
    {
        _logger = logger;
        _url = url;
    }

    public static string GetAPIUrl(string pathname = "")
    {
        if (_url == null)
            throw new InvalidOperationException("API not initialized. Call Initialize() first.");
        return $"{_url.Value}{pathname}";
    }

    public static async Task<PlayerInventory?> FetchPlayerInventory(ulong steamId)
    {
        var url = GetAPIUrl($"/api/equipped/v3/{steamId}.json");
        for (var attempt = 0; attempt < 3; attempt++)
        {
            try
            {
                try
                {
                    using HttpClient client = new();
                    var response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();
                    string jsonContent = response.Content.ReadAsStringAsync().Result;
                    return JsonSerializer.Deserialize<PlayerInventory>(jsonContent);
                }
                catch (Exception error)
                {
                    _logger?.LogError("GET {Url} failed: {Message}", url, error.Message);
                    throw;
                }
            }
            catch { }
        }
        return null;
    }

    public static async Task SendStatTrakIncrement(string apiKey, int targetUid, string userId)
    {
        var url = GetAPIUrl("/api/increment-item-stattrak");
        try
        {
            var json = JsonSerializer.Serialize(
                new
                {
                    apiKey,
                    targetUid,
                    userId,
                }
            );
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            using HttpClient client = new();
            var response = await client.PostAsync(url, content);
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                _logger?.LogError("POST {Url} failed, check your invsim_apikey's value.", url);
                return;
            }
            if (!response.IsSuccessStatusCode)
            {
                _logger?.LogError(
                    "POST {Url} failed with status code: {StatusCode}",
                    url,
                    response.StatusCode
                );
            }
        }
        catch (Exception error)
        {
            _logger?.LogError("POST {Url} failed: {Message}", url, error.Message);
        }
    }

    public static async Task<SignInUserResponse?> SendSignIn(string apiKey, string userId)
    {
        var url = GetAPIUrl("/api/sign-in");
        try
        {
            var json = JsonSerializer.Serialize(new { apiKey, userId });
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            using HttpClient client = new();
            var response = await client.PostAsync(url, content);
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                _logger?.LogError("POST {Url} failed, check your invsim_apikey's value.", url);
                return null;
            }
            if (!response.IsSuccessStatusCode)
            {
                _logger?.LogError(
                    "POST {Url} failed with status code: {StatusCode}",
                    url,
                    response.StatusCode
                );
                return null;
            }
            var responseContent = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrEmpty(responseContent))
                return null;
            return JsonSerializer.Deserialize<SignInUserResponse>(responseContent);
        }
        catch (Exception error)
        {
            _logger?.LogError("POST {Url} failed: {Message}", url, error.Message);
            return null;
        }
    }
}
