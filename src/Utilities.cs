/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Players;

public static class Utilities
{
    public static IPlayer? GetPlayerFromSteamID(ISwiftlyCore core, ulong steamId)
    {
        return core.PlayerManager.GetAllPlayers().FirstOrDefault(p => p.SteamID == steamId);
    }
}
