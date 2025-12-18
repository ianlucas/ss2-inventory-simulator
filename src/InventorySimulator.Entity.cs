/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using CS2Lib;
using SwiftlyS2.Shared.Players;
using SwiftlyS2.Shared.SchemaDefinitions;
using SwiftlyS2.Shared.SteamAPI;

namespace InventorySimulator;

public partial class InventorySimulator
{
    public void ApplyWeaponAttributesFromItem(CEconItemView item, WeaponEconItem weaponItem)
    {
        var isKnife = item.IsMelee();
        var attrs = item.NetworkedDynamicAttributes;
        var wear = weaponItem.WearOverride ?? weaponItem.Wear;
        if (isKnife)
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
        if (!isKnife)
            foreach (var sticker in weaponItem.Stickers)
            {
                var id = TypeHelper.ViewAs<uint, float>(sticker.Def);
                var slot = $"sticker slot {sticker.Slot}";
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

    public void ApplyWeaponAttributesFromItem(
        CEconItemView item,
        WeaponEconItem weaponItem,
        CBasePlayerWeapon weapon,
        CCSPlayerController controller
    )
    {
        var isKnife = CS2Items.IsMeleeDesignerName(weapon.DesignerName);
        var entityDef = weapon.AttributeManager.Item.ItemDefinitionIndex;
        if (isKnife)
        {
            if (entityDef != weaponItem.Def)
                // Thanks to xstage and stefanx111
                weapon.AcceptInput("ChangeSubclass", value: weaponItem.Def.ToString());
            item.ItemDefinitionIndex = weaponItem.Def;
            item.EntityQuality = 3;
        }
        else
            item.EntityQuality = weaponItem.Stattrak >= 0 ? 9 : 4;
        UpdateEconItemID(item);
        weapon.FallbackPaintKit = weaponItem.Paint;
        weapon.FallbackSeed = weaponItem.Seed;
        weapon.FallbackWear = weaponItem.WearOverride ?? weaponItem.Wear;
        item.AccountID = new CSteamID(controller.SteamID).GetAccountID().m_AccountID;
        item.CustomName = weaponItem.Nametag;
        item.NetworkedDynamicAttributes.Attributes.RemoveAll();
        item.NetworkedDynamicAttributes.SetOrAddAttribute(
            "set item texture prefab",
            weaponItem.Paint
        );
        item.NetworkedDynamicAttributes.SetOrAddAttribute("set item texture seed", weaponItem.Seed);
        item.NetworkedDynamicAttributes.SetOrAddAttribute("set item texture wear", weaponItem.Wear);
        item.AttributeList.Attributes.RemoveAll();
        item.AttributeList.SetOrAddAttribute("set item texture prefab", weaponItem.Paint);
        item.AttributeList.SetOrAddAttribute("set item texture seed", weaponItem.Seed);
        item.AttributeList.SetOrAddAttribute("set item texture wear", weaponItem.Wear);
        if (weaponItem.Stattrak >= 0)
        {
            weapon.FallbackStatTrak = weaponItem.Stattrak;
            item.NetworkedDynamicAttributes.SetOrAddAttribute(
                "kill eater",
                TypeHelper.ViewAs<int, float>(weaponItem.Stattrak)
            );
            item.NetworkedDynamicAttributes.SetOrAddAttribute("kill eater score type", 0);
            item.AttributeList.SetOrAddAttribute(
                "kill eater",
                TypeHelper.ViewAs<int, float>(weaponItem.Stattrak)
            );
            item.AttributeList.SetOrAddAttribute("kill eater score type", 0);
        }
        if (!isKnife)
        {
            foreach (var sticker in weaponItem.Stickers)
            {
                var slot = $"sticker slot {sticker.Slot}";
                item.NetworkedDynamicAttributes.SetOrAddAttribute(
                    $"{slot} id",
                    TypeHelper.ViewAs<uint, float>(sticker.Def)
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
            weapon.AcceptInput("SetBodygroup", value: $"body,{(weaponItem.Legacy ? 1 : 0)}");
        }
    }

    public void ApplyGloveAttributesFromItem(CEconItemView glove, BaseEconItem item)
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

    public void ApplyAgentAttributesFromItem(CEconItemView item, AgentItem agentItem)
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

    public void ApplyPinAttributesFromItem(CEconItemView item, uint pinDef)
    {
        item.ItemDefinitionIndex = (ushort)pinDef;
    }

    public void ApplyMusicKitAttributesFromItem(CEconItemView item, MusicKitItem musicKitItem)
    {
        item.NetworkedDynamicAttributes.Attributes.RemoveAll();
        item.NetworkedDynamicAttributes.SetOrAddAttribute(
            "music id",
            TypeHelper.ViewAs<int, float>(musicKitItem.Def)
        );
    }

    public void ApplyAttributesFromWrapper(
        CEconItemView item,
        InventoryItemWrapper wrapper,
        PlayerInventory inventory,
        ulong steamId
    )
    {
        UpdateEconItemID(item);
        item.AccountID = new CSteamID(steamId).GetAccountID().m_AccountID;
        if (wrapper.WeaponItem != null)
        {
            ApplyWeaponAttributesFromItem(item, wrapper.WeaponItem);
        }
        else if (wrapper.AgentItem != null)
        {
            ApplyAgentAttributesFromItem(item, wrapper.AgentItem);
        }
        else if (wrapper.GloveItem != null)
        {
            ApplyGloveAttributesFromItem(item, wrapper.GloveItem);
        }
        else if (wrapper.PinItem != null)
        {
            ApplyPinAttributesFromItem(item, wrapper.PinItem.Value);
        }
        else if (wrapper.MusicKitItem != null)
        {
            ApplyMusicKitAttributesFromItem(item, wrapper.MusicKitItem);
        }
    }

    public void UpdateEconItemID(CEconItemView econItemView)
    {
        var itemId = NextItemId++;
        econItemView.ItemID = itemId;
        econItemView.ItemIDLow = (uint)(itemId & 0xFFFFFFFF);
        econItemView.ItemIDHigh = (uint)(itemId >> 32);
    }

    public bool IsCustomWeaponItemID(CBasePlayerWeapon weapon)
    {
        return weapon.AttributeManager.Item.ItemID >= MinimumCustomItemID;
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
