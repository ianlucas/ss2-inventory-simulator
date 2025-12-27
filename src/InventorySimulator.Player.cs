/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Text.Json;
using Microsoft.Extensions.Logging;
using SwiftlyS2.Shared.Players;
using SwiftlyS2.Shared.SchemaDefinitions;
using IOFile = System.IO.File;

namespace InventorySimulator;

public partial class InventorySimulator
{
    public void LoadPlayerInventories()
    {
        try
        {
            var path = Path.Combine(Core.GameDirectory, InventoryFileDir, File.Value);
            if (!IOFile.Exists(path))
                return;
            string json = IOFile.ReadAllText(path);
            var inventories = JsonSerializer.Deserialize<Dictionary<ulong, PlayerInventory>>(json);
            if (inventories != null)
            {
                LoadedInventoryManager.Clear();
                foreach (var pair in inventories)
                {
                    LoadedInventoryManager.TryAdd(pair.Key, true);
                    SetPlayerInventory(pair.Key, pair.Value);
                }
            }
        }
        catch
        {
            Core.Logger.LogError("Error when processing \"{File}\".", File.Value);
        }
    }

    public void SetPlayerInventory(ulong steamId, PlayerInventory inventory)
    {
        PlayerInventoryManager[steamId] = inventory;
    }

    public void ClearPlayerInventory(ulong steamId)
    {
        if (!LoadedInventoryManager.ContainsKey(steamId))
        {
            PlayerInventoryManager.Remove(steamId, out _);
            PlayerSprayCooldownManager.Remove(steamId, out _);
        }
    }

    public void UpdatePlayerControllerSteamID(IPlayer player)
    {
        var steamID = player.Controller.SteamID;
        var index = player.Controller.Index;
        try
        {
            if (ControllerSteamIDManager.TryGetValue(index, out var oldSteamID))
                if (oldSteamID != steamID)
                {
                    foreach (var (oldIndex, otherSteamID) in ControllerSteamIDManager)
                        if (oldIndex != index && oldSteamID == otherSteamID)
                            return;
                    ClearPlayerInventory(oldSteamID);
                }
        }
        finally
        {
            ControllerSteamIDManager[index] = steamID;
        }
    }

    public void ClearPlayerControllerSteamID(CCSPlayerController controller)
    {
        if (!controller.IsValid)
            return;
        var steamID = controller.SteamID;
        ControllerSteamIDManager.TryRemove(controller.Index, out _);
        foreach (var (oldIndex, otherSteamID) in ControllerSteamIDManager)
            if (oldIndex != controller.Index && steamID == otherSteamID)
                return;
        ClearPlayerInventory(steamID);
    }

    public void ClearPlayerPostFetch(ulong steamId)
    {
        PlayerPostFetchManager.TryRemove(steamId, out _);
    }

    public PlayerInventory GetPlayerInventory(IPlayer player)
    {
        if (PlayerInventoryManager.TryGetValue(player.SteamID, out var inventory))
            return inventory;
        return EmptyInventory;
    }

    public PlayerInventory GetPlayerInventoryBySteamID(ulong steamID)
    {
        return PlayerInventoryManager.TryGetValue(steamID, out var inventory)
            ? inventory
            : EmptyInventory;
    }
}
