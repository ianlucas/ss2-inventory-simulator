/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Runtime.InteropServices;
using SwiftlyS2.Shared.SchemaDefinitions;

namespace InventorySimulator;

public partial class InventorySimulator
{
    public Natives.CServerSideClientBase_ActivatePlayerDelegate OnActivatePlayer(
        Func<Natives.CServerSideClientBase_ActivatePlayerDelegate> next
    )
    {
        return (thisPtr) =>
        {
            var userid = (ushort)
                Marshal.ReadInt16(thisPtr + Natives.CServerSideClientBase_m_UserID);
            var player = Core.PlayerManager.GetPlayer(userid);
            if (player != null && !player.IsFakeClient && player.Controller != null)
                if (!PlayerInventoryManager.ContainsKey(player.SteamID))
                {
                    PlayerPostFetchManager[player.SteamID] = () =>
                        Core.Scheduler.NextWorldUpdate(() =>
                        {
                            if (player.Controller.IsValid)
                                Natives.CServerSideClientBase_ActivatePlayer.CallOriginal(thisPtr);
                        });
                    if (!PlayerInFetchManager.ContainsKey(player.SteamID))
                        RefreshPlayerInventory(player);
                    return;
                }
            next()(thisPtr);
        };
    }

    public Natives.CCSPlayer_ItemServices_GiveNamedItemDelegate OnGiveNamedItem(
        Func<Natives.CCSPlayer_ItemServices_GiveNamedItemDelegate> next
    )
    {
        return (thisPtr, pchName, a3, pScriptItem, a5, a6) =>
        {
            var designerName = Marshal.PtrToStringUTF8(pchName);
            if (designerName != null && pScriptItem == nint.Zero)
            {
                var itemServices = Core.Memory.ToSchemaClass<CCSPlayer_ItemServices>(thisPtr);
                var controller = itemServices.GetController();
                if (controller?.SteamID != 0 && controller?.InventoryServices != null)
                {
                    var itemDef = SchemaHelper
                        .GetItemSchema()
                        ?.GetItemDefinitionByName(designerName);
                    if (itemDef != null)
                    {
                        var econItem = GetPlayerInventoryBySteamID(controller.SteamID)
                            .GetEconItemForSlot(
                                itemDef.DefaultLoadoutSlot,
                                controller.TeamNum,
                                itemDef.DefIndex,
                                IsFallbackTeam.Value
                            );
                        if (econItem != null)
                        {
                            var item = SchemaHelper.CreateCEconItemView();
                            item.Apply(econItem, itemDef.DefaultLoadoutSlot, controller.SteamID);
                            pScriptItem = item.Address;
                        }
                    }
                }
            }
            return next()(thisPtr, pchName, a3, pScriptItem, a5, a6);
        };
    }

    public Natives.CCSPlayerInventory_GetItemInLoadoutDelegate OnGetItemInLoadout(
        Func<Natives.CCSPlayerInventory_GetItemInLoadoutDelegate> next
    )
    {
        return (thisPtr, team, slot) =>
        {
            var ret = next()(thisPtr, team, slot);
            var nativeInventory = new CCSPlayerInventory(thisPtr);
            if (!nativeInventory.IsValid)
                return ret;
            var baseItem = Core.Memory.ToSchemaClass<CEconItemView>(ret);
            if (!baseItem.IsValid)
                return ret;
            var steamId = nativeInventory.SOCache.Owner.SteamID;
            var isFallbackTeam = IsFallbackTeam.Value;
            var inventory = GetPlayerInventoryBySteamID(nativeInventory.SOCache.Owner.SteamID);
            var slotType = (loadout_slot_t)slot;
            var econItem = inventory.GetEconItemForSlot(
                slotType,
                (byte)team,
                baseItem.ItemDefinitionIndex,
                isFallbackTeam,
                MinModels.Value
            );
            if (econItem == null)
                return ret;
            var key = $"{steamId}_{team}_{slot}";
            if (CreatedCEconItemViewManager.TryGetValue(key, out var existingPtr))
            {
                var existingItem = Core.Memory.ToSchemaClass<CEconItemView>(existingPtr);
                existingItem.Apply(econItem, slotType, steamId);
                return existingPtr;
            }
            var item = SchemaHelper.CreateCEconItemView(copyFrom: ret);
            item.Apply(econItem, slotType, steamId);
            CreatedCEconItemViewManager[key] = item.Address;
            return item.Address;
        };
    }
}
