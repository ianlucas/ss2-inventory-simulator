/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using SwiftlyS2.Shared.Convars;

namespace InventorySimulator;

public class Api
{
    private const int MaxRetries = 3;
    private const int RetryDelayMs = 100;

    private static readonly HttpClient _httpClient = new();
    private static ILogger? _logger;
    private static IConVar<string>? _url;

    public static void Initialize(ILogger logger, IConVar<string> url)
    {
        _logger = logger;
        _url = url;
        _httpClient.Timeout = TimeSpan.FromSeconds(30);
    }

    public static string GetUrl(string pathname = "")
    {
        if (_url == null)
            throw new InvalidOperationException("API not initialized. Call Initialize() first.");
        return $"{_url.Value}{pathname}";
    }

    private static async Task<HttpResponseMessage?> SendPostRequest(string url, object request)
    {
        try
        {
            var content = JsonContent.Create(request);
            var response = await _httpClient.PostAsync(url, content);
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
            return response;
        }
        catch (Exception error)
        {
            _logger?.LogError("POST {Url} failed: {Message}", url, error.Message);
            return null;
        }
    }

    private static async Task PostAsync(string url, object request)
    {
        await SendPostRequest(url, request);
    }

    private static async Task<T?> PostAsync<T>(string url, object request)
        where T : class
    {
        var response = await SendPostRequest(url, request);
        if (response == null)
            return null;
        var responseContent = await response.Content.ReadAsStringAsync();
        return string.IsNullOrEmpty(responseContent)
            ? null
            : JsonSerializer.Deserialize<T>(responseContent);
    }

    public static async Task<PlayerInventory?> FetchPlayerInventory(ulong steamId)
    {
        var url = GetUrl($"/api/equipped/v3/{steamId}.json");
        for (var attempt = 1; attempt <= MaxRetries; attempt++)
            try
            {
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                var jsonContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<PlayerInventory>(jsonContent);
            }
            catch (Exception error)
            {
                _logger?.LogError(
                    "GET {Url} failed (attempt {Attempt}/{MaxRetries}): {Message}",
                    url,
                    attempt,
                    MaxRetries,
                    error.Message
                );
                if (attempt == MaxRetries)
                    return null;
                await Task.Delay(TimeSpan.FromMilliseconds(RetryDelayMs * attempt));
            }
        return null;
    }

    public static async Task SendStatTrakIncrement(string apiKey, int targetUid, string userId)
    {
        var url = GetUrl("/api/increment-item-stattrak");
        var request = new StatTrakIncrementRequest
        {
            ApiKey = apiKey,
            TargetUid = targetUid,
            UserId = userId,
        };
        await PostAsync(url, request);
    }

    public static async Task<SignInUserResponse?> SendSignIn(string apiKey, string userId)
    {
        var url = GetUrl("/api/sign-in");
        var request = new SignInRequest { ApiKey = apiKey, UserId = userId };
        return await PostAsync<SignInUserResponse>(url, request);
    }
}
