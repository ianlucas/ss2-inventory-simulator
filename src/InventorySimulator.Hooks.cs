/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Runtime.InteropServices;
using SwiftlyS2.Shared.SchemaDefinitions;

namespace InventorySimulator;

public partial class InventorySimulator
{
    public GameFunctions.CServerSideClientBase_ConnectDelegate OnConnect(
        Func<GameFunctions.CServerSideClientBase_ConnectDelegate> next
    )
    {
        return (thisPtr, a2, a3, userId, a5, a6, a7) =>
        {
            ServerSideClientUserid[thisPtr] = userId;
            return next()(thisPtr, a2, a3, userId, a5, a6, a7);
        };
    }

    public GameFunctions.CServerSideClientBase_SetSignonStateDelegate OnSetSignonState(
        Func<GameFunctions.CServerSideClientBase_SetSignonStateDelegate> next
    )
    {
        return (thisPtr, newSignonState) =>
        {
            var userid = ServerSideClientUserid.TryGetValue(thisPtr, out var id)
                ? (ushort?)id
                : null;
            if (userid != null)
            {
                var player = Core.PlayerManager.GetPlayer((int)userid);
                if (player != null && !player.IsFakeClient && player.Controller != null)
                {
                    if (!FetchingPlayerInventory.ContainsKey(player.SteamID))
                        RefreshPlayerInventory(player);
                    var allowed = PlayerInventoryManager.ContainsKey(player.SteamID);
                    if (newSignonState >= 0 && !allowed)
                        return 0;
                }
            }
            return next()(thisPtr, newSignonState);
        };
    }

    public GameFunctions.CCSPlayerController_UpdateTeamSelectionPreviewDelegate OnUpdateTeamSelectionPreview(
        Func<GameFunctions.CCSPlayerController_UpdateTeamSelectionPreviewDelegate> next
    )
    {
        return (thisPtr, a2) =>
        {
            var controller = Core.Memory.ToSchemaClass<CCSPlayerController>(thisPtr);
            // TODO Pass controller directly to GiveTeamPreviewItems.
            var player = Utilities.GetPlayerFromSteamID(Core, controller.SteamID);
            if (player != null)
                GiveTeamPreviewItems("team_select", player);
            return next()(thisPtr, a2);
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
