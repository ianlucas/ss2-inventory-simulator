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
    public readonly ConcurrentDictionary<ulong, CancellationTokenSource> PlayerUseCmdManager = [];
    public readonly ConcurrentDictionary<ulong, bool> PlayerUseCmdBlockManager = [];
    public readonly ConcurrentDictionary<ulong, Action> PlayerInventoryPostFetchHandlers = [];

    public readonly PlayerInventory EmptyInventory = new();

    private static readonly string[] TeamSelectSuffixes = ["counterterrorist", "terrorist"];
    public static readonly string InventoryFileDir = "csgo/addons/swiftlycs2/configs";
    public static readonly ulong MinimumCustomItemID = 68719476736;

    public static readonly bool IsWindows = OperatingSystem.IsWindows();

    public ulong NextItemId = MinimumCustomItemID;
    public Guid? OnActivatePlayerHookGuid = null;
}
