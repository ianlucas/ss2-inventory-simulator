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

    public static void AssignNewItemID(this CEconItemView self)
    {
        var itemId = NextItemId++;
        self.ItemID = itemId;
        self.ItemIDLow = (uint)(itemId & 0xFFFFFFFF);
        self.ItemIDHigh = (uint)(itemId >> 32);
    }

    public static void ApplyGloveAttributes(this CEconItemView self, BaseEconItem econItem)
    {
        var netAttrs = self.NetworkedDynamicAttributes;
        var attrs = self.AttributeList;
        self.Initialized = true;
        self.ItemDefinitionIndex = econItem.Def;
        self.AssignNewItemID();
        netAttrs.Attributes.RemoveAll();
        netAttrs.SetOrAddAttribute("set item texture prefab", econItem.Paint);
        netAttrs.SetOrAddAttribute("set item texture seed", econItem.Seed);
        netAttrs.SetOrAddAttribute("set item texture wear", econItem.Wear);
        attrs.Attributes.RemoveAll();
        attrs.SetOrAddAttribute("set item texture prefab", econItem.Paint);
        attrs.SetOrAddAttribute("set item texture seed", econItem.Seed);
        attrs.SetOrAddAttribute("set item texture wear", econItem.Wear);
    }

    public static void ApplyWeaponAttributes(
        this CEconItemView self,
        WeaponEconItem econItem,
        CBasePlayerWeapon weapon,
        CCSPlayerController controller
    )
    {
        var isMelee = ItemHelper.IsMeleeDesignerName(weapon.DesignerName);
        var entityDef = weapon.AttributeManager.Item.ItemDefinitionIndex;
        var dynAttrs = self.NetworkedDynamicAttributes;
        var attrs = self.AttributeList;
        if (isMelee)
        {
            if (entityDef != econItem.Def)
                // Thanks to xstage and stefanx111
                weapon.AcceptInput("ChangeSubclass", value: econItem.Def.ToString());
            self.ItemDefinitionIndex = econItem.Def;
            self.EntityQuality = 3;
        }
        else
            self.EntityQuality = econItem.Stattrak >= 0 ? 9 : 4;
        self.AssignNewItemID();
        weapon.FallbackPaintKit = econItem.Paint;
        weapon.FallbackSeed = econItem.Seed;
        weapon.FallbackWear = econItem.WearOverride ?? econItem.Wear;
        self.AccountID = new CSteamID(controller.SteamID).GetAccountID().m_AccountID;
        self.CustomName = econItem.Nametag;
        dynAttrs.Attributes.RemoveAll();
        dynAttrs.SetOrAddAttribute("set item texture prefab", econItem.Paint);
        dynAttrs.SetOrAddAttribute("set item texture seed", econItem.Seed);
        dynAttrs.SetOrAddAttribute("set item texture wear", econItem.Wear);
        attrs.Attributes.RemoveAll();
        attrs.SetOrAddAttribute("set item texture prefab", econItem.Paint);
        attrs.SetOrAddAttribute("set item texture seed", econItem.Seed);
        attrs.SetOrAddAttribute("set item texture wear", econItem.Wear);
        if (econItem.Stattrak >= 0)
        {
            weapon.FallbackStatTrak = econItem.Stattrak;
            var statTrak = TypeHelper.ViewAs<int, float>(econItem.Stattrak);
            dynAttrs.SetOrAddAttribute("kill eater", statTrak);
            dynAttrs.SetOrAddAttribute("kill eater score type", 0);
            attrs.SetOrAddAttribute("kill eater", statTrak);
            attrs.SetOrAddAttribute("kill eater score type", 0);
        }
        if (!isMelee)
        {
            foreach (var sticker in econItem.Stickers)
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
            weapon.AcceptInput("SetBodygroup", value: $"body,{(econItem.Legacy ? 1 : 0)}");
        }
    }
}
