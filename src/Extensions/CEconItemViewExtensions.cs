/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using SwiftlyS2.Shared.SchemaDefinitions;
using SwiftlyS2.Shared.SteamAPI;

namespace InventorySimulator;

public static class CEconItemViewExtensions
{
    public static readonly ulong MinimumCustomItemID = 65155030971;
    private static ulong NextItemId = MinimumCustomItemID;

    public static void AssignNewItemID(this CEconItemView econItemView)
    {
        var itemId = NextItemId++;
        econItemView.ItemID = itemId;
        econItemView.ItemIDLow = (uint)(itemId & 0xFFFFFFFF);
        econItemView.ItemIDHigh = (uint)(itemId >> 32);
    }

    public static void ApplyAttributes(
        this CEconItemView item,
        WeaponEconItem weaponItem,
        bool isMelee
    )
    {
        var attrs = item.NetworkedDynamicAttributes;
        var wear = weaponItem.WearOverride ?? weaponItem.Wear;
        if (isMelee)
        {
            item.ItemDefinitionIndex = weaponItem.Def;
            item.EntityQuality = 3;
        }
        else
            item.EntityQuality = weaponItem.Stattrak >= 0 ? 9 : 4;
        item.CustomName = weaponItem.Nametag;
        attrs.Attributes.RemoveAll();
        attrs.SetOrAddAttribute("set item texture prefab", weaponItem.Paint);
        attrs.SetOrAddAttribute("set item texture seed", weaponItem.Seed);
        attrs.SetOrAddAttribute("set item texture wear", wear);
        if (weaponItem.Stattrak >= 0)
        {
            var statTrak = TypeHelper.ViewAs<int, float>(weaponItem.Stattrak);
            attrs.SetOrAddAttribute("kill eater", statTrak);
            attrs.SetOrAddAttribute("kill eater score type", 0);
        }
        if (!isMelee)
            foreach (var sticker in weaponItem.Stickers)
            {
                var slot = $"sticker slot {sticker.Slot}";
                var id = TypeHelper.ViewAs<uint, float>(sticker.Def);
                attrs.SetOrAddAttribute($"{slot} id", id);
                attrs.SetOrAddAttribute($"{slot} wear", sticker.Wear);
                if (sticker.Rotation != null)
                    attrs.SetOrAddAttribute($"{slot} rotation", sticker.Rotation.Value);
                if (sticker.X != null)
                    attrs.SetOrAddAttribute($"{slot} offset x", sticker.X.Value);
                if (sticker.Y != null)
                    attrs.SetOrAddAttribute($"{slot} offset y", sticker.Y.Value);
            }
    }

    public static void ApplyAttributes(
        this CEconItemView item,
        WeaponEconItem weaponItem,
        CBasePlayerWeapon weapon,
        CCSPlayerController controller
    )
    {
        var isMelee = ItemHelper.IsMeleeDesignerName(weapon.DesignerName);
        var entityDef = weapon.AttributeManager.Item.ItemDefinitionIndex;
        var dynAttrs = item.NetworkedDynamicAttributes;
        var attrs = item.AttributeList;
        if (isMelee)
        {
            if (entityDef != weaponItem.Def)
                // Thanks to xstage and stefanx111
                weapon.AcceptInput("ChangeSubclass", value: weaponItem.Def.ToString());
            item.ItemDefinitionIndex = weaponItem.Def;
            item.EntityQuality = 3;
        }
        else
            item.EntityQuality = weaponItem.Stattrak >= 0 ? 9 : 4;
        item.AssignNewItemID();
        weapon.FallbackPaintKit = weaponItem.Paint;
        weapon.FallbackSeed = weaponItem.Seed;
        weapon.FallbackWear = weaponItem.WearOverride ?? weaponItem.Wear;
        item.AccountID = new CSteamID(controller.SteamID).GetAccountID().m_AccountID;
        item.CustomName = weaponItem.Nametag;
        dynAttrs.Attributes.RemoveAll();
        dynAttrs.SetOrAddAttribute("set item texture prefab", weaponItem.Paint);
        dynAttrs.SetOrAddAttribute("set item texture seed", weaponItem.Seed);
        dynAttrs.SetOrAddAttribute("set item texture wear", weaponItem.Wear);
        attrs.Attributes.RemoveAll();
        attrs.SetOrAddAttribute("set item texture prefab", weaponItem.Paint);
        attrs.SetOrAddAttribute("set item texture seed", weaponItem.Seed);
        attrs.SetOrAddAttribute("set item texture wear", weaponItem.Wear);
        if (weaponItem.Stattrak >= 0)
        {
            weapon.FallbackStatTrak = weaponItem.Stattrak;
            var statTrak = TypeHelper.ViewAs<int, float>(weaponItem.Stattrak);
            dynAttrs.SetOrAddAttribute("kill eater", statTrak);
            dynAttrs.SetOrAddAttribute("kill eater score type", 0);
            attrs.SetOrAddAttribute("kill eater", statTrak);
            attrs.SetOrAddAttribute("kill eater score type", 0);
        }
        if (!isMelee)
        {
            foreach (var sticker in weaponItem.Stickers)
            {
                var slot = $"sticker slot {sticker.Slot}";
                var id = TypeHelper.ViewAs<uint, float>(sticker.Def);
                dynAttrs.SetOrAddAttribute($"{slot} id", id);
                dynAttrs.SetOrAddAttribute($"{slot} wear", sticker.Wear);
                if (sticker.Rotation != null)
                    dynAttrs.SetOrAddAttribute($"{slot} rotation", sticker.Rotation.Value);
                if (sticker.X != null)
                    dynAttrs.SetOrAddAttribute($"{slot} offset x", sticker.X.Value);
                if (sticker.Y != null)
                    dynAttrs.SetOrAddAttribute($"{slot} offset y", sticker.Y.Value);
            }
            weapon.AcceptInput("SetBodygroup", value: $"body,{(weaponItem.Legacy ? 1 : 0)}");
        }
    }

    public static void ApplyAttributes(this CEconItemView glove, BaseEconItem item)
    {
        var netAttrs = glove.NetworkedDynamicAttributes;
        var attrs = glove.AttributeList;
        glove.ItemDefinitionIndex = item.Def;
        netAttrs.Attributes.RemoveAll();
        netAttrs.SetOrAddAttribute("set item texture prefab", item.Paint);
        netAttrs.SetOrAddAttribute("set item texture seed", item.Seed);
        netAttrs.SetOrAddAttribute("set item texture wear", item.Wear);
        attrs.Attributes.RemoveAll();
        attrs.SetOrAddAttribute("set item texture prefab", item.Paint);
        attrs.SetOrAddAttribute("set item texture seed", item.Seed);
        attrs.SetOrAddAttribute("set item texture wear", item.Wear);
    }

    public static void ApplyAttributes(this CEconItemView item, AgentItem agentItem)
    {
        if (agentItem.Def == null)
            return;
        item.ItemDefinitionIndex = agentItem.Def.Value;
        for (var i = 0; i < agentItem.Patches.Count; i++)
        {
            var patch = agentItem.Patches[i];
            if (patch != 0)
                item.AttributeList.SetOrAddAttribute(
                    $"sticker slot {i} id",
                    TypeHelper.ViewAs<uint, float>(patch)
                );
        }
    }

    public static void ApplyAttributes(this CEconItemView item, uint pinDef)
    {
        item.ItemDefinitionIndex = (ushort)pinDef;
    }

    public static void ApplyAttributes(this CEconItemView item, MusicKitItem musicKitItem)
    {
        item.NetworkedDynamicAttributes.Attributes.RemoveAll();
        item.NetworkedDynamicAttributes.SetOrAddAttribute(
            "music id",
            TypeHelper.ViewAs<int, float>(musicKitItem.Def)
        );
    }

    public static void ApplyAttributes(
        this CEconItemView item,
        PlayerInventoryItem inventoryItem,
        ulong steamId,
        bool isMelee
    )
    {
        item.AssignNewItemID();
        item.AccountID = new CSteamID(steamId).GetAccountID().m_AccountID;
        if (inventoryItem.WeaponItem != null)
            item.ApplyAttributes(inventoryItem.WeaponItem, isMelee);
        else if (inventoryItem.AgentItem != null)
            item.ApplyAttributes(inventoryItem.AgentItem);
        else if (inventoryItem.GloveItem != null)
            item.ApplyAttributes(inventoryItem.GloveItem);
        else if (inventoryItem.PinItem != null)
            item.ApplyAttributes(inventoryItem.PinItem.Value);
        else if (inventoryItem.MusicKitItem != null)
            item.ApplyAttributes(inventoryItem.MusicKitItem);
    }
}
