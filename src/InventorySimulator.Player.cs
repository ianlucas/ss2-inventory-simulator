/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Text.Json;
using Microsoft.Extensions.Logging;
using SwiftlyS2.Shared.Players;
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
                LoadedPlayerInventory.Clear();
                foreach (var pair in inventories)
                {
                    LoadedPlayerInventory.TryAdd(pair.Key, true);
                    AddPlayerInventory(pair.Key, pair.Value);
                }
            }
        }
        catch
        {
            Core.Logger.LogError("Error when processing \"{File}\".", File.Value);
        }
    }

    public void AddPlayerInventory(ulong steamId, PlayerInventory inventory)
    {
        PlayerInventoryManager[steamId] = inventory;
        Core.Scheduler.NextTick(() =>
        {
            var player = Core.PlayerManager.GetPlayerFromSteamID(steamId);
            if (inventory.MusicKit != null || inventory.Graffiti != null)
                PlayerOnTickInventoryManager[steamId] = (player, inventory);
            else
                PlayerOnTickInventoryManager.Remove(steamId, out _);
        });
    }

    public void ClearInventoryManager()
    {
        var connected = Core
            .PlayerManager.GetAllPlayers()
            .Select(player => player.SteamID)
            .ToHashSet();
        var disconnected = PlayerInventoryManager.Keys.Except(connected).ToList();
        foreach (var steamId in disconnected)
            ClearPlayerInventory(steamId);
    }

    public void ClearPlayerInventory(ulong steamId)
    {
        if (!LoadedPlayerInventory.ContainsKey(steamId))
        {
            PlayerInventoryManager.Remove(steamId, out _);
            PlayerCooldownManager.Remove(steamId, out _);
            PlayerSprayCooldownManager.Remove(steamId, out _);
            PlayerOnTickInventoryManager.Remove(steamId, out _);
        }
        if (PlayerOnTickInventoryManager.TryGetValue(steamId, out var tuple))
            PlayerOnTickInventoryManager[steamId] = (null, tuple.Item2);
    }

    public void ClearPlayerUseCmd(ulong steamId)
    {
        PlayerUseCmdManager.Remove(steamId, out var _);
        PlayerUseCmdBlockManager.Remove(steamId, out var _);
    }

    public void ClearPlayerInventoryPostFetchHandler(ulong steamId)
    {
        PlayerInventoryPostFetchHandlers.TryRemove(steamId, out _);
    }

    public PlayerInventory GetPlayerInventory(IPlayer player)
    {
        return PlayerInventoryManager.TryGetValue(player.SteamID, out var inventory)
            ? inventory
            : EmptyInventory;
    }

    public PlayerInventory GetPlayerInventoryBySteamID(ulong steamID)
    {
        return PlayerInventoryManager.TryGetValue(steamID, out var inventory)
            ? inventory
            : EmptyInventory;
    }
}
