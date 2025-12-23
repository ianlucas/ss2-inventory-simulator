/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Runtime.InteropServices;
using CS2Lib;
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
                    PlayerInventoryPostFetchHandlers[player.SteamID] = () =>
                        Core.Scheduler.NextWorldUpdate(() =>
                        {
                            if (player.Controller.IsValid)
                                Natives.CServerSideClientBase_ActivatePlayer.CallOriginal(thisPtr);
                        });
                    if (!FetchingPlayerInventory.ContainsKey(player.SteamID))
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
            var isKnife = designerName != null && CS2Items.IsMeleeDesignerName(designerName);
            if (isKnife && pScriptItem == nint.Zero)
            {
                var itemServices = Core.Memory.ToSchemaClass<CCSPlayer_ItemServices>(thisPtr);
                var controller = itemServices.GetController();

                if (controller?.SteamID != 0 && controller?.InventoryServices != null)
                {
                    controller.InventoryServices.ServerAuthoritativeWeaponSlots.RemoveAll();
                    controller.InventoryServices.ServerAuthoritativeWeaponSlotsUpdated();
                    if (isKnife)
                    {
                        var inventory = controller.InventoryServices.GetInventory();
                        if (inventory.IsValid)
                            pScriptItem = inventory.GetItemInLoadout(
                                controller.TeamNum,
                                loadout_slot_t.LOADOUT_SLOT_MELEE
                            );
                    }
                }
            }
            var ret = next()(thisPtr, pchName, a3, pScriptItem, a5, a6);
            var weapon = Core.Memory.ToSchemaClass<CBasePlayerWeapon>(ret);
            if (!isKnife && !IsCustomWeaponItemID(weapon))
            {
                var itemServices = Core.Memory.ToSchemaClass<CCSPlayer_ItemServices>(thisPtr);
                var controller = itemServices.GetController();
                if (controller?.SteamID != 0 && controller?.InventoryServices?.IsValid == true)
                {
                    var inventory = GetPlayerInventoryBySteamID(controller.SteamID);
                    var item = inventory.GetWeapon(
                        controller.TeamNum,
                        weapon.AttributeManager.Item.ItemDefinitionIndex,
                        IsFallbackTeam.Value
                    );
                    if (item != null)
                    {
                        ApplyWeaponAttributesFromItem(
                            weapon.AttributeManager.Item,
                            item,
                            weapon,
                            controller
                        );
                    }
                }
            }
            return ret;
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
            var itemWrapper = inventory.GetItemForSlot(
                slotType,
                (byte)team,
                baseItem.ItemDefinitionIndex,
                isFallbackTeam,
                MinModels.Value
            );
            if (itemWrapper == null)
                return ret;
            var key = $"{steamId}_{team}_{slot}";
            if (CreatedEconItemViewPointers.TryGetValue(key, out var existingPtr))
            {
                var existingItem = Core.Memory.ToSchemaClass<CEconItemView>(existingPtr);
                ApplyAttributesFromInventoryItem(existingItem, itemWrapper, steamId);
                return existingPtr;
            }
            var newItemPtr = EconItemHelper.CreateCEconItemView(copyFrom: ret);
            var item = Core.Memory.ToSchemaClass<CEconItemView>(newItemPtr);
            ApplyAttributesFromInventoryItem(item, itemWrapper, steamId);
            CreatedEconItemViewPointers[key] = newItemPtr;
            return newItemPtr;
        };
    }
}
