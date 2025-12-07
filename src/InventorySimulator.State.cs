/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Collections.Concurrent;
using SwiftlyS2.Shared.Players;

namespace InventorySimulator;

public partial class InventorySimulator
{
    public readonly ConcurrentDictionary<ulong, bool> FetchingPlayerInventory = [];
    public readonly ConcurrentDictionary<ulong, bool> AuthenticatingPlayer = [];
    public readonly ConcurrentDictionary<ulong, bool> LoadedPlayerInventory = [];
    public readonly ConcurrentDictionary<ulong, long> PlayerCooldownManager = [];
    public readonly ConcurrentDictionary<ulong, long> PlayerSprayCooldownManager = [];
    public readonly ConcurrentDictionary<
        ulong,
        (IPlayer?, PlayerInventory)
    > PlayerOnTickInventoryManager = [];
    public readonly ConcurrentDictionary<ulong, PlayerInventory> PlayerInventoryManager = [];
    public readonly ConcurrentDictionary<ulong, Timer> PlayerUseCmdManager = [];
    public readonly ConcurrentDictionary<ulong, bool> PlayerUseCmdBlockManager = [];
    public readonly ConcurrentDictionary<IntPtr, short> ServerSideClientUserid = [];

    public readonly PlayerInventory EmptyInventory = new();

    public static readonly string InventoryFileDir = "csgo/addons/swiftlycs2/configs";
    public static readonly ulong MinimumCustomItemID = 68719476736;

    public ulong NextItemId = MinimumCustomItemID;
}
