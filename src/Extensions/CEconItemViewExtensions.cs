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

    public static void InitFrom(
        this CEconItemView self,
        ulong steamId,
        WeaponEconItem item,
        bool isMelee
    )
    {
        self.Initialized = true;
        self.ItemDefinitionIndex = item.Def;
        self.AssignNewItemID();
        self.AccountID = new CSteamID(steamId).GetAccountID().m_AccountID;
        self.ApplyAttributes(item, isMelee);
    }

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
            item.EntityQuality = 3;
        else
            item.EntityQuality = weaponItem.Stattrak >= 0 ? 9 : 4;
        item.ItemDefinitionIndex = weaponItem.Def;
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
