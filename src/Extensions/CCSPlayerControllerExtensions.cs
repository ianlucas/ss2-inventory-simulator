/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Collections.Concurrent;
using SwiftlyS2.Shared.SchemaDefinitions;

namespace InventorySimulator;

public static class CCSPlayerControllerExtensions
{
    private static readonly ConcurrentDictionary<
        uint,
        CCSPlayerControllerState
    > _controllerStateManager = [];

    extension(CCSPlayerController self)
    {
        public CCSPlayerControllerState State =>
            _controllerStateManager.GetOrAdd(self.Index, _ => new(self.SteamID));

        public void Revalidate()
        {
            if (self.State.SteamID != self.SteamID)
                self.RemoveState();
        }

        public void RemoveState()
        {
            self.State.DisposeUseCmdTimer();
            self.State.ClearCEconItemView();
            _controllerStateManager.TryRemove(self.Index, out var _);
        }
    }
}
