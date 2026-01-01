/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using SwiftlyS2.Shared;
using SwiftlyS2.Shared.SchemaDefinitions;

namespace InventorySimulator;

public class CCSPlayerControllerState(ulong steamId)
{
    [SwiftlyInject]
    private ISwiftlyCore Core { get; set; } = null!;
    public ulong SteamID = steamId;
    public bool IsFetching = false;
    public bool IsAuthenticating = false;
    public bool IsLoadedFromFile = false;
    public long WsCooldown = 0;
    public long SprayCooldown = 0;
    public PlayerInventory? Inventory = InventoriesFile.GetBySteamID(steamId);
    public CancellationTokenSource? UseCmdTimer;
    public bool IsUseCmdBlocked = false;
    public Action? PostFetchCallback;
    public static readonly ConcurrentDictionary<string, nint> CEconItemViewManager = [];

    public void DisposeUseCmdTimer()
    {
        UseCmdTimer?.Cancel();
        UseCmdTimer?.Dispose();
        UseCmdTimer = null;
    }

    public nint GetCEconItemView(int team, int slot, EconItem econItem, nint copyFrom = 0)
    {
        var key = $"{SteamID}_{team}_{slot}";
        if (CEconItemViewManager.TryGetValue(key, out var existingPtr))
        {
            var existingItem = Core.Memory.ToSchemaClass<CEconItemView>(existingPtr);
            existingItem.ApplyAttributes(econItem, (loadout_slot_t)slot, SteamID);
            return existingPtr;
        }
        var item = SchemaHelper.CreateCEconItemView(copyFrom);
        item.ApplyAttributes(econItem, (loadout_slot_t)slot, SteamID);
        CEconItemViewManager[key] = item.Address;
        return item.Address;
    }

    public void ClearCEconItemView()
    {
        foreach (var key in CEconItemViewManager.Keys)
            if (key.StartsWith($"{SteamID}_"))
                if (CEconItemViewManager.TryRemove(key, out var ptr))
                    Marshal.FreeHGlobal(ptr);
    }

    public static void ClearAllCEconItemView()
    {
        foreach (var ptr in CEconItemViewManager.Values)
            Marshal.FreeHGlobal(ptr);
    }
}
