/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using SwiftlyS2.Shared.Players;
using SwiftlyS2.Shared.SchemaDefinitions;

namespace InventorySimulator;

public class PlayerInventory(EquippedV4Response data)
{
    private readonly EquippedV4Response _data = data;
    public Dictionary<byte, EconItem> Agents => _data.Agents;
    public EconItem? MusicKit => _data.MusicKit;
    public EconItem? Graffiti => _data.Graffiti;

    public static PlayerInventory Empty() => new(new());

    public EconItem? GetKnife(byte team, bool fallback)
    {
        if (_data.Knives.TryGetValue(team, out var knife))
        {
            knife.WearOverride = GetWeaponEconItemWear(knife);
            return knife;
        }
        if (fallback && _data.Knives.TryGetValue(TeamHelper.ToggleTeam(team), out knife))
        {
            knife.WearOverride = GetWeaponEconItemWear(knife);
            return knife;
        }
        return null;
    }

    public Dictionary<ushort, EconItem> GetWeapons(byte team)
    {
        return (Team)team == Team.T ? _data.TWeapons : _data.CTWeapons;
    }

    public EconItem? GetWeapon(byte team, ushort def, bool fallback)
    {
        if (GetWeapons(team).TryGetValue(def, out var weapon))
        {
            weapon.WearOverride = GetWeaponEconItemWear(weapon);
            return weapon;
        }
        if (fallback && GetWeapons(TeamHelper.ToggleTeam(team)).TryGetValue(def, out weapon))
        {
            weapon.WearOverride = GetWeaponEconItemWear(weapon);
            return weapon;
        }
        return null;
    }

    public EconItem? GetGloves(byte team, bool fallback)
    {
        if (_data.Gloves.TryGetValue(team, out var glove))
        {
            return glove;
        }
        if (fallback && _data.Gloves.TryGetValue(TeamHelper.ToggleTeam(team), out glove))
        {
            return glove;
        }
        return null;
    }

    // It looks like CS2's client caches the weapon materials based on wear and seed. Until we figure out
    // the proper way to force a material update, we need to ensure that every paint has a unique wear so
    // that: 1) it gets regenerated, and 2) there are no rendering issues. As a drawback, every time
    // players use !ws, their weapon's wear will decay, but I think it's a good trade-off since it also
    // forces the sticker to regenerate. This approach is based on workarounds by @stefanx111 and @bklol.

    public Dictionary<int, Dictionary<float, (ushort, string)>> CachedWeaponEconItems = [];

    public float GetWeaponEconItemWear(EconItem econItem)
    {
        if (
            econItem.Def == null
            || econItem.Paint == null
            || econItem.Wear == null
            || econItem.Stickers == null
        )
            return 0;
        var def = econItem.Def.Value;
        var paint = econItem.Paint.Value;
        var wear = econItem.Wear.Value;
        var stickers = string.Join("_", econItem.Stickers.Select(s => s.Def));
        var cachedByWear = CachedWeaponEconItems.TryGetValue(paint, out var c) ? c : [];
        while (true)
        {
            (ushort, string)? pair = cachedByWear.TryGetValue(wear, out var p) ? p : null;
            var cached = pair?.Item1 == econItem.Def && pair?.Item2 == stickers;
            if (pair == null || cached)
            {
                cachedByWear[wear] = (def, stickers);
                CachedWeaponEconItems[paint] = cachedByWear;
                return wear;
            }
            wear += 0.001f;
        }
    }

    public EconItem? GetEconItemForSlot(
        loadout_slot_t slot,
        byte team,
        ushort? def,
        bool fallback,
        int minModels = 0
    )
    {
        if (
            slot >= loadout_slot_t.LOADOUT_SLOT_MELEE
            && slot <= loadout_slot_t.LOADOUT_SLOT_EQUIPMENT5
        )
        {
            var isMelee = slot == loadout_slot_t.LOADOUT_SLOT_MELEE;
            return isMelee ? GetKnife(team, fallback)
                : def.HasValue ? GetWeapon(team, def.Value, fallback)
                : null;
        }
        if (slot == loadout_slot_t.LOADOUT_SLOT_CLOTHING_CUSTOMPLAYER)
        {
            if (minModels > 0)
                return team == (byte)Team.T
                    ? new EconItem { Def = 5036 }
                    : new EconItem { Def = 5037 };
            if (_data.Agents.TryGetValue(team, out var agentEconItem))
                return agentEconItem;
            return null;
        }
        if (slot == loadout_slot_t.LOADOUT_SLOT_CLOTHING_HANDS)
        {
            return GetGloves(team, fallback);
        }
        if (slot == loadout_slot_t.LOADOUT_SLOT_FLAIR0)
            return _data.Collectible;
        if (slot == loadout_slot_t.LOADOUT_SLOT_MUSICKIT)
            return _data.MusicKit;
        return null;
    }
}
