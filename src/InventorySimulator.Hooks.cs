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
            var userid = (ushort)Marshal.ReadInt16(thisPtr + (IsWindows ? 160 : 168));
            var player = Core.PlayerManager.GetPlayer(userid);
            if (player != null && !player.IsFakeClient && player.Controller != null)
                if (!PlayerInventoryManager.ContainsKey(player.SteamID))
                {
                    PlayerInventoryPostFetchHandlers[player.SteamID] = () =>
                        Core.Scheduler.NextTick(() =>
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
            if (
                designerName != null
                && pScriptItem == nint.Zero
                && CS2Items.IsMeleeDesignerName(designerName)
            )
            {
                var itemServices = Core.Memory.ToSchemaClass<CCSPlayer_ItemServices>(thisPtr);
                var controller = itemServices.GetController();
                if (controller?.SteamID != 0 && controller?.InventoryServices?.IsValid == true)
                {
                    var inventory = controller.InventoryServices.GetInventory();
                    if (inventory.IsValid)
                        pScriptItem = Natives.CCSPlayerInventory_GetItemInLoadout.Call(
                            inventory.Address,
                            controller.TeamNum,
                            (int)loadout_slot_t.LOADOUT_SLOT_FIRST_AUTO_BUY_WEAPON
                        );
                }
            }
            var ret = next()(thisPtr, pchName, a3, pScriptItem, a5, a6);
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
                isFallbackTeam
            );
            if (!itemWrapper.HasItem)
                return ret;
            var key = $"{steamId}_{team}_{slot}";
            if (CreatedEconItemViewPointers.TryRemove(key, out var oldPtr))
                Core.Scheduler.Delay(64, () => Natives.FreeMemory(oldPtr));
            var newItemPtr = Natives.CreateEconItemView(copyFrom: ret);
            var item = Core.Memory.ToSchemaClass<CEconItemView>(newItemPtr);
            ApplyAttributesFromWrapper(item, itemWrapper, inventory, steamId);
            CreatedEconItemViewPointers[key] = newItemPtr;
            return newItemPtr;
        };
    }
}
