/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Runtime.InteropServices;
using CS2Lib;
using CS2Lib.SwiftlyCS2.Core;
using SwiftlyS2.Shared.Players;
using SwiftlyS2.Shared.SchemaDefinitions;
using SwiftlyS2.Shared.SteamAPI;

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
                var controller = GetControllerFromItemServices(itemServices);
                if (controller?.SteamID != 0 && controller?.InventoryServices?.IsValid == true)
                {
                    var inventory = new CCSPlayerInventory(
                        controller.InventoryServices.Address
                            + Natives.CCSPlayerController_InventoryServices_m_pInventory
                    );
                    if (inventory.IsValid)
                        pScriptItem = Natives.CCSPlayerInventory_GetItemInLoadout.Call(
                            inventory.Address,
                            controller.TeamNum,
                            (int)loadout_slot_t.LOADOUT_SLOT_FIRST_AUTO_BUY_WEAPON
                        );
                }
            }
            var ret = next()(thisPtr, pchName, a3, pScriptItem, a5, a6);
            //
            // if (designerName != null && !designerName.Contains("weapon"))
            //     return ret;
            // var itemServices = Core.Memory.ToSchemaClass<CCSPlayer_ItemServices>(thisPtr);
            // var weapon = Core.Memory.ToSchemaClass<CBasePlayerWeapon>(ret);
            // var player = GetPlayerFromItemServices(itemServices);
            // if (player != null)
            //     GivePlayerWeaponSkin(player, weapon);
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
            if (nativeInventory.IsValid)
            {
                var item = Core.Memory.ToSchemaClass<CEconItemView>(ret);
                if (item.IsValid)
                {
                    ;
                    // Console.WriteLine(
                    //     $"[GetItemInLoadout^] inventory={nativeInventory.Address} team={(Team)team} slot={(loadout_slot_t)slot} def={item.ItemDefinitionIndex}"
                    // );
                    var steamId = nativeInventory.SOCache.Owner.SteamID;
                    var isFallbackTeam = IsFallbackTeam.Value;
                    var inventory = GetPlayerInventoryBySteamID(
                        nativeInventory.SOCache.Owner.SteamID
                    );
                    item.AccountID = new CSteamID(steamId).GetAccountID().m_AccountID;
                    if (
                        (loadout_slot_t)slot >= loadout_slot_t.LOADOUT_SLOT_MELEE
                        && (loadout_slot_t)slot <= loadout_slot_t.LOADOUT_SLOT_EQUIPMENT5
                    )
                    {
                        var isKnife =
                            (loadout_slot_t)slot
                            == loadout_slot_t.LOADOUT_SLOT_FIRST_AUTO_BUY_WEAPON;
                        var weaponItem = isKnife
                            ? inventory.GetKnife((byte)team, isFallbackTeam)
                            : inventory.GetWeapon(
                                (byte)team,
                                item.ItemDefinitionIndex,
                                isFallbackTeam
                            );
                        // Console.WriteLine(
                        //     $"[GetItemInLoadout] inventory={nativeInventory.Address} slot={(loadout_slot_t)slot} designerName={item.GetDesignerName()} def={item.ItemDefinitionIndex} isKnife={isKnife} cs2IsKnife={item.IsMelee()} weaponItem.Def={weaponItem?.Def}"
                        // );
                        if (weaponItem != null)
                        {
                            weaponItem.WearOverride ??= inventory.GetWeaponEconItemWear(weaponItem);
                            ApplyWeaponAttributesFromItem(item, weaponItem);
                            // Console.WriteLine(
                            //     $"[GetItemInLoadout!] inventory={nativeInventory.Address} slot={(loadout_slot_t)slot} designerName={item.GetDesignerName()} def={item.ItemDefinitionIndex}"
                            // );
                        }
                    }
                    else if (
                        (loadout_slot_t)slot == loadout_slot_t.LOADOUT_SLOT_CLOTHING_CUSTOMPLAYER
                        && inventory.Agents.TryGetValue((byte)team, out var agentItem)
                        && agentItem.Def != null
                    )
                    {
                        item.ItemDefinitionIndex = agentItem.Def.Value;
                        for (var i = 0; i < agentItem.Patches.Count; i++)
                        {
                            var patch = agentItem.Patches[i];
                            if (patch != 0)
                            {
                                item.AttributeList.SetOrAddAttribute(
                                    $"sticker slot {i} id",
                                    UnsafeHelpers.ViewAs<uint, float>(patch)
                                );
                            }
                        }
                    }
                    else if ((loadout_slot_t)slot == loadout_slot_t.LOADOUT_SLOT_FIRST_COSMETIC)
                    {
                        var gloveItem = inventory.GetGloves((byte)team, isFallbackTeam);
                        if (gloveItem != null)
                            ApplyGloveAttributesFromItem(item, gloveItem);
                    }
                    else if ((loadout_slot_t)slot == loadout_slot_t.LOADOUT_SLOT_FLAIR0)
                    {
                        if (inventory.Pin != null)
                            item.ItemDefinitionIndex = (ushort)inventory.Pin.Value;
                    }
                    else if ((loadout_slot_t)slot == loadout_slot_t.LOADOUT_SLOT_MUSICKIT)
                    {
                        if (inventory.MusicKit != null)
                        {
                            item.NetworkedDynamicAttributes.Attributes.RemoveAll();
                            item.NetworkedDynamicAttributes.SetOrAddAttribute(
                                "music id",
                                UnsafeHelpers.ViewAs<int, float>(inventory.MusicKit.Def)
                            );
                        }
                    }
                    else
                        Console.WriteLine(
                            $"[GetItemInLoadout$] inventory={nativeInventory.Address} team={(Team)team} slot={(loadout_slot_t)slot} def={item.ItemDefinitionIndex}"
                        );
                }
            }
            return ret;
        };
    }
}
