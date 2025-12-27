/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Collections.Concurrent;

namespace InventorySimulator;

public partial class InventorySimulator
{
    public readonly ConcurrentDictionary<ulong, bool> PlayerInFetchManager = [];
    public readonly ConcurrentDictionary<ulong, bool> PlayerInAuthManager = [];
    public readonly ConcurrentDictionary<ulong, bool> LoadedInventoryManager = [];
    public readonly ConcurrentDictionary<ulong, long> PlayerSprayCooldownManager = [];
    public readonly ConcurrentDictionary<ulong, PlayerInventory> PlayerInventoryManager = [];
    public readonly ConcurrentDictionary<ulong, Action> PlayerPostFetchManager = [];
    public readonly ConcurrentDictionary<uint, ulong> ControllerSteamIDManager = [];
    public readonly PlayerInventory EmptyInventory = PlayerInventory.Empty();
    public static readonly string InventoryFileDir = "csgo/addons/swiftlycs2/configs";
    public Guid? OnActivatePlayerHookGuid = null;
}
