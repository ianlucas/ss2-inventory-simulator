/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using CS2Lib;
using CS2Lib.SwiftlyCS2.Core;
using CS2Lib.SwiftlyCS2.Extensions;
using SwiftlyS2.Shared.Players;
using SwiftlyS2.Shared.SchemaDefinitions;

namespace InventorySimulator;

public partial class InventorySimulator
{
    public void ApplyGloveAttributesFromItem(CEconItemView glove, BaseEconItem item)
    {
        glove.Initialized = true;
        glove.ItemDefinitionIndex = item.Def;
        UpdateEconItemID(glove);
        glove.NetworkedDynamicAttributes.Attributes.RemoveAll();
        glove.NetworkedDynamicAttributes.SetOrAddAttribute("set item texture prefab", item.Paint);
        glove.NetworkedDynamicAttributes.SetOrAddAttribute("set item texture seed", item.Seed);
        glove.NetworkedDynamicAttributes.SetOrAddAttribute("set item texture wear", item.Wear);
        glove.AttributeList.Attributes.RemoveAll();
        glove.AttributeList.SetOrAddAttribute("set item texture prefab", item.Paint);
        glove.AttributeList.SetOrAddAttribute("set item texture seed", item.Seed);
        glove.AttributeList.SetOrAddAttribute("set item texture wear", item.Wear);
    }

    public void ApplyWeaponAttributesFromItem(
        CEconItemView item,
        WeaponEconItem weaponItem,
        CBasePlayerWeapon? weapon = null,
        IPlayer? player = null
    )
    {
        var designerName = weapon?.DesignerName;
        var isKnife =
            designerName != null ? CS2Items.IsMeleeDesignerName(designerName) : item.IsMelee();
        var entityDef =
            weapon?.AttributeManager.Item.ItemDefinitionIndex ?? item.ItemDefinitionIndex;
        if (isKnife)
        {
            if (weapon != null && entityDef != weaponItem.Def)
                // Thanks to xstage and stefanx111
                weapon.AcceptInput("ChangeSubclass", value: weaponItem.Def.ToString());
            item.ItemDefinitionIndex = weaponItem.Def;
            item.EntityQuality = 3;
        }
        else
            item.EntityQuality = weaponItem.Stattrak >= 0 ? 9 : 4;
        UpdateEconItemID(item);
        if (weapon != null)
        {
            weapon.FallbackPaintKit = weaponItem.Paint;
            weapon.FallbackSeed = weaponItem.Seed;
            weapon.FallbackWear = weaponItem.WearOverride ?? weaponItem.Wear;
        }
        if (player != null)
            item.AccountID = (uint)player.SteamID;
        item.CustomName = weaponItem.Nametag;
        item.NetworkedDynamicAttributes.Attributes.RemoveAll();
        item.NetworkedDynamicAttributes.SetOrAddAttribute(
            "set item texture prefab",
            weaponItem.Paint
        );
        item.NetworkedDynamicAttributes.SetOrAddAttribute("set item texture seed", weaponItem.Seed);
        item.NetworkedDynamicAttributes.SetOrAddAttribute(
            "set item texture wear",
            weaponItem.WearOverride ?? weaponItem.Wear
        );
        if (weaponItem.Stattrak >= 0)
        {
            weapon?.FallbackStatTrak = weaponItem.Stattrak;
            item.NetworkedDynamicAttributes.SetOrAddAttribute(
                "kill eater",
                UnsafeHelpers.ViewAs<int, float>(weaponItem.Stattrak)
            );
            item.NetworkedDynamicAttributes.SetOrAddAttribute("kill eater score type", 0);
        }
        if (!isKnife)
        {
            foreach (var sticker in weaponItem.Stickers)
            {
                var slot = $"sticker slot {sticker.Slot}";
                item.NetworkedDynamicAttributes.SetOrAddAttribute(
                    $"{slot} id",
                    UnsafeHelpers.ViewAs<uint, float>(sticker.Def)
                );
                item.NetworkedDynamicAttributes.SetOrAddAttribute($"{slot} wear", sticker.Wear);
                if (sticker.Rotation != null)
                    item.NetworkedDynamicAttributes.SetOrAddAttribute(
                        $"{slot} rotation",
                        sticker.Rotation.Value
                    );
                if (sticker.X != null)
                    item.NetworkedDynamicAttributes.SetOrAddAttribute(
                        $"{slot} offset x",
                        sticker.X.Value
                    );
                if (sticker.Y != null)
                    item.NetworkedDynamicAttributes.SetOrAddAttribute(
                        $"{slot} offset y",
                        sticker.Y.Value
                    );
            }
            if (weapon != null && player != null)
                weapon.AcceptInput("SetBodygroup", value: $"body,{(weaponItem.Legacy ? 1 : 0)}");
        }
    }

    public void UpdateEconItemID(CEconItemView econItemView)
    {
        // ItemID serves as a global identifier for items. Since we're
        // simulating it, we're using arbitrary large numbers.
        var itemId = NextItemId++;
        econItemView.ItemID = itemId;
        // @see https://gitlab.com/KittenPopo/csgo-2018-source/-/blob/main/game/shared/econ/econ_item_view.h#L313
        econItemView.ItemIDLow = (uint)itemId & 0xFFFFFFFF;
        econItemView.ItemIDHigh = (uint)itemId >> 32;
    }

    public bool IsCustomWeaponItemID(CBasePlayerWeapon weapon)
    {
        return weapon.AttributeManager.Item.ItemID >= MinimumCustomItemID;
    }

    public CCSPlayerController? GetControllerFromItemServices(CCSPlayer_ItemServices itemServices)
    {
        var pawn = itemServices.Pawn;
        return
            pawn != null && pawn.IsValid && pawn.Controller.IsValid && pawn.Controller.Value != null
            ? pawn.Controller.Value.As<CCSPlayerController>()
            : null;
    }

    public bool IsPlayerUseCmdBusy(IPlayer player)
    {
        if (player.PlayerPawn?.IsBuyMenuOpen == true)
            return true;
        if (player.PlayerPawn?.IsDefusing == true)
            return true;
        var weapon = player.PlayerPawn?.WeaponServices?.ActiveWeapon.Value;
        if (weapon?.DesignerName != "weapon_c4")
            return false;
        var c4 = weapon.As<CC4>();
        return c4.IsPlantingViaUse;
    }
}
