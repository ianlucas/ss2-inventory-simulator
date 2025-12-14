/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using CS2Lib.SwiftlyCS2.Core;
using CS2Lib.SwiftlyCS2.Extensions;
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
            var statTrak = UnsafeHelpers.ViewAs<int, float>(weaponItem.Stattrak);
            attrs.SetOrAddAttribute("kill eater", statTrak);
            attrs.SetOrAddAttribute("kill eater score type", 0);
        }
        if (!isKnife)
            foreach (var sticker in weaponItem.Stickers)
            {
                var id = UnsafeHelpers.ViewAs<uint, float>(sticker.Def);
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
                    UnsafeHelpers.ViewAs<uint, float>(patch)
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
            UnsafeHelpers.ViewAs<int, float>(musicKitItem.Def)
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
            wrapper.WeaponItem.WearOverride ??= inventory.GetWeaponEconItemWear(wrapper.WeaponItem);
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
