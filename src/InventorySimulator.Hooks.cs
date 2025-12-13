/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Runtime.InteropServices;
using SwiftlyS2.Shared.SchemaDefinitions;

namespace InventorySimulator;

public partial class InventorySimulator
{
    public GameFunctions.CServerSideClientBase_ActivatePlayerDelegate OnActivatePlayer(
        Func<GameFunctions.CServerSideClientBase_ActivatePlayerDelegate> next
    )
    {
        return (thisPtr) =>
        {
            var userid = (ushort)Marshal.ReadInt16(thisPtr + (IsWindows ? 160 : 168));
            var player = Core.PlayerManager.GetPlayer(userid);
            if (player != null && !player.IsFakeClient && player.Controller != null)
                if (!PlayerInventoryManager.ContainsKey(player.SteamID))
                {
                    PlayerInventoryPostFetchHandlers[player.SteamID] = () =>
                        Core.Scheduler.NextTick(() =>
                        {
                            if (player.Controller.IsValid)
                                GameFunctions.CServerSideClientBase_ActivatePlayer.CallOriginal(
                                    thisPtr
                                );
                        });
                    if (!FetchingPlayerInventory.ContainsKey(player.SteamID))
                        RefreshPlayerInventory(player);
                    return;
                }
            next()(thisPtr);
        };
    }

    public GameFunctions.CCSPlayerController_UpdateTeamSelectionPreviewDelegate OnUpdateTeamSelectionPreview(
        Func<GameFunctions.CCSPlayerController_UpdateTeamSelectionPreviewDelegate> next
    )
    {
        return (thisPtr, a2) =>
        {
            var ret = next()(thisPtr, a2);
            var controller = Core.Memory.ToSchemaClass<CCSPlayerController>(thisPtr);
            // TODO Pass controller directly to GiveTeamPreviewItems.
            var player = Core.PlayerManager.GetPlayerFromSteamID(controller.SteamID);
            if (player != null)
                GiveTeamPreviewItems("team_select", player);
            return ret;
        };
    }

    public GameFunctions.CCSPlayer_ItemServices_GiveNamedItemDelegate OnGiveNamedItem(
        Func<GameFunctions.CCSPlayer_ItemServices_GiveNamedItemDelegate> next
    )
    {
        return (thisPtr, pchName, a3, a4, a5, a6) =>
        {
            var ret = next()(thisPtr, pchName, a3, a4, a5, a6);
            var designerName = Marshal.PtrToStringUTF8(pchName);
            if (designerName != null && !designerName.Contains("weapon"))
                return ret;
            var itemServices = Core.Memory.ToSchemaClass<CCSPlayer_ItemServices>(thisPtr);
            var weapon = Core.Memory.ToSchemaClass<CBasePlayerWeapon>(ret);
            var player = GetPlayerFromItemServices(itemServices);
            if (player != null)
                GivePlayerWeaponSkin(player, weapon);
            return ret;
        };
    }
}
