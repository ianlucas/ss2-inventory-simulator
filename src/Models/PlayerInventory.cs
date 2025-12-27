/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using SwiftlyS2.Shared.Players;
using SwiftlyS2.Shared.SchemaDefinitions;

namespace InventorySimulator;

public class PlayerInventory
{
    private readonly EquippedV4Response _data;
    public Dictionary<byte, EconItem> Agents => _data.Agents;
    public EconItem? MusicKit => _data.MusicKit;
    public EconItem? Graffiti => _data.Graffiti;

    public Dictionary<
        (int paint, float wear),
        (ushort def, string stickers)
    > CachedWeaponEconItems = [];

    public PlayerInventory(EquippedV4Response data)
    {
        _data = data;
        InitializeWearOverrides();
    }

    public static PlayerInventory Empty() => new(new());

    private void InitializeWearOverrides()
    {
        foreach (var knife in _data.Knives.Values)
            knife.WearOverride = GetWeaponEconItemWear(knife);
        foreach (var weapon in _data.CTWeapons.Values)
            weapon.WearOverride = GetWeaponEconItemWear(weapon);
        foreach (var weapon in _data.TWeapons.Values)
            weapon.WearOverride = GetWeaponEconItemWear(weapon);
    }

    public EconItem? GetKnife(byte team, bool fallback)
    {
        if (_data.Knives.TryGetValue(team, out var knife))
            return knife;
        if (fallback && _data.Knives.TryGetValue(TeamHelper.ToggleTeam(team), out knife))
            return knife;
        return null;
    }

    public Dictionary<ushort, EconItem> GetWeapons(byte team)
    {
        return (Team)team == Team.T ? _data.TWeapons : _data.CTWeapons;
    }

    public EconItem? GetWeapon(byte team, ushort def, bool fallback)
    {
        if (GetWeapons(team).TryGetValue(def, out var weapon))
            return weapon;
        if (fallback && GetWeapons(TeamHelper.ToggleTeam(team)).TryGetValue(def, out weapon))
            return weapon;
        return null;
    }

    public EconItem? GetGloves(byte team, bool fallback)
    {
        if (_data.Gloves.TryGetValue(team, out var glove))
            return glove;
        if (fallback && _data.Gloves.TryGetValue(TeamHelper.ToggleTeam(team), out glove))
            return glove;
        return null;
    }

    // It looks like CS2's client caches the weapon materials based on wear and seed. Until we figure out
    // the proper way to force a material update, we need to ensure that every paint has a unique wear so
    // that: 1) it gets regenerated, and 2) there are no rendering issues. As a drawback, every time
    // players use !ws, their weapon's wear will decay, but I think it's a good trade-off since it also
    // forces the sticker to regenerate. This approach is based on workarounds by @stefanx111 and @bklol.
    private float GetWeaponEconItemWear(EconItem econItem)
    {
        if (econItem is not { Def: not null, Paint: not null, Wear: not null, Stickers: not null })
            return 0;
        var def = econItem.Def.Value;
        var paint = econItem.Paint.Value;
        var wear = econItem.Wear.Value;
        var stickers = string.Join("_", econItem.Stickers.Select(s => s.Def));
        while (
            CachedWeaponEconItems.TryGetValue((paint, wear), out var cached)
            && (cached.def != def || cached.stickers != stickers)
        )
            wear += 0.001f;
        CachedWeaponEconItems[(paint, wear)] = (def, stickers);
        return wear;
    }

    public EconItem? GetEconItemForSlot(
        byte team,
        loadout_slot_t slot,
        ushort def,
        bool fallback,
        int minModels = 0
    )
    {
        if (
            slot >= loadout_slot_t.LOADOUT_SLOT_MELEE
            && slot <= loadout_slot_t.LOADOUT_SLOT_EQUIPMENT5
        )
        {
            return slot == loadout_slot_t.LOADOUT_SLOT_MELEE
                ? GetKnife(team, fallback)
                : GetWeapon(team, def, fallback);
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
