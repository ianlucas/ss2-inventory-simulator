/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Text.Json.Serialization;
using CS2Lib.SwiftlyCS2.Core;
using SwiftlyS2.Shared.Players;
using SwiftlyS2.Shared.SchemaDefinitions;

namespace InventorySimulator;

public class StickerItem
{
    [JsonPropertyName("def")]
    public uint Def { get; set; }

    [JsonPropertyName("slot")]
    public ushort Slot { get; set; }

    [JsonPropertyName("wear")]
    public float Wear { get; set; }

    [JsonPropertyName("rotation")]
    public int? Rotation { get; set; }

    [JsonPropertyName("x")]
    public float? X { get; set; }

    [JsonPropertyName("y")]
    public float? Y { get; set; }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    public override bool Equals(object? obj)
    {
        if (obj is not StickerItem other)
            return false;

        return Def == other.Def
            && Slot == other.Slot
            && Wear == other.Wear
            && Rotation == other.Rotation
            && X == other.X
            && Y == other.Y;
    }

    public static bool operator ==(StickerItem? left, StickerItem? right)
    {
        if (ReferenceEquals(left, right))
            return true;
        if (left is null || right is null)
            return false;
        return left.Equals(right);
    }

    public static bool operator !=(StickerItem? left, StickerItem? right)
    {
        return !(left == right);
    }
}

public class BaseEconItem
{
    [JsonPropertyName("def")]
    public ushort Def { get; set; }

    [JsonPropertyName("paint")]
    public int Paint { get; set; }

    [JsonPropertyName("seed")]
    public int Seed { get; set; }

    [JsonPropertyName("wear")]
    public float Wear { get; set; }

    public float? WearOverride;

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    public override bool Equals(object? obj)
    {
        if (obj is not BaseEconItem other)
            return false;

        return Def == other.Def
            && Paint == other.Paint
            && Seed == other.Seed
            && Wear == other.Wear
            && WearOverride == other.WearOverride;
    }

    public static bool operator ==(BaseEconItem? left, BaseEconItem? right)
    {
        if (ReferenceEquals(left, right))
            return true;
        if (left is null || right is null)
            return false;
        return left.Equals(right);
    }

    public static bool operator !=(BaseEconItem? left, BaseEconItem? right)
    {
        return !(left == right);
    }
}

public class WeaponEconItem : BaseEconItem
{
    [JsonPropertyName("legacy")]
    public bool Legacy { get; set; }

    [JsonPropertyName("nametag")]
    public required string Nametag { get; set; }

    [JsonPropertyName("stattrak")]
    public required int Stattrak { get; set; }

    [JsonPropertyName("stickers")]
    public required List<StickerItem> Stickers { get; set; }

    [JsonPropertyName("uid")]
    public required int Uid { get; set; }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    public override bool Equals(object? obj)
    {
        if (obj is not WeaponEconItem other)
            return false;

        return Def == other.Def
            && Paint == other.Paint
            && Seed == other.Seed
            && Wear == other.Wear
            && Legacy == other.Legacy
            && Nametag == other.Nametag
            && Stattrak == other.Stattrak
            && Stickers.SequenceEqual(other.Stickers);
    }

    public static bool operator ==(WeaponEconItem? left, WeaponEconItem? right)
    {
        if (ReferenceEquals(left, right))
            return true;
        if (left is null || right is null)
            return false;
        return left.Equals(right);
    }

    public static bool operator !=(WeaponEconItem? left, WeaponEconItem? right)
    {
        return !(left == right);
    }
}

public class AgentItem
{
    [JsonPropertyName("def")]
    public ushort? Def { get; set; }

    [JsonPropertyName("model")]
    public string Model { get; set; } = "";

    [JsonPropertyName("patches")]
    public List<uint> Patches { get; set; } = [];

    [JsonPropertyName("vofallback")]
    public bool VoFallback { get; set; } = false;

    [JsonPropertyName("vofemale")]
    public bool VoFemale { get; set; } = false;

    [JsonPropertyName("voprefix")]
    public string VoPrefix { get; set; } = "";

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    public override bool Equals(object? obj)
    {
        if (obj is not AgentItem other)
            return false;

        return Def == other.Def
            && Model == other.Model
            && Patches.SequenceEqual(other.Patches)
            && VoFallback == other.VoFallback
            && VoFemale == other.VoFemale
            && VoPrefix == other.VoPrefix;
    }

    public static bool operator ==(AgentItem? left, AgentItem? right)
    {
        if (ReferenceEquals(left, right))
            return true;
        if (left is null || right is null)
            return false;
        return left.Equals(right);
    }

    public static bool operator !=(AgentItem? left, AgentItem? right)
    {
        return !(left == right);
    }
}

public class MusicKitItem
{
    [JsonPropertyName("def")]
    public int Def { get; set; }

    [JsonPropertyName("stattrak")]
    public required int Stattrak { get; set; }

    [JsonPropertyName("uid")]
    public required int Uid { get; set; }
}

public class GraffitiItem
{
    [JsonPropertyName("def")]
    public required int Def { get; set; }

    [JsonPropertyName("tint")]
    public required int Tint { get; set; }
}

public class InventoryItemWrapper
{
    public WeaponEconItem? WeaponItem { get; set; }
    public AgentItem? AgentItem { get; set; }
    public BaseEconItem? GloveItem { get; set; }
    public uint? PinItem { get; set; }
    public MusicKitItem? MusicKitItem { get; set; }

    public bool HasItem =>
        WeaponItem != null
        || AgentItem != null
        || GloveItem != null
        || PinItem != null
        || MusicKitItem != null;

    public static InventoryItemWrapper FromWeapon(WeaponEconItem item) =>
        new() { WeaponItem = item };

    public static InventoryItemWrapper FromAgent(AgentItem item) => new() { AgentItem = item };

    public static InventoryItemWrapper FromGlove(BaseEconItem item) => new() { GloveItem = item };

    public static InventoryItemWrapper FromPin(uint item) => new() { PinItem = item };

    public static InventoryItemWrapper FromMusicKit(MusicKitItem item) =>
        new() { MusicKitItem = item };

    public static InventoryItemWrapper Empty() => new();
}

[method: JsonConstructor]
public class PlayerInventory(
    Dictionary<byte, WeaponEconItem>? knives = null,
    Dictionary<byte, BaseEconItem>? gloves = null,
    Dictionary<ushort, WeaponEconItem>? tWeapons = null,
    Dictionary<ushort, WeaponEconItem>? ctWeapons = null,
    Dictionary<byte, AgentItem>? agents = null,
    uint? pin = null,
    MusicKitItem? musicKit = null,
    GraffitiItem? graffiti = null
)
{
    [JsonPropertyName("knives")]
    public Dictionary<byte, WeaponEconItem> Knives { get; set; } = knives ?? [];

    [JsonPropertyName("gloves")]
    public Dictionary<byte, BaseEconItem> Gloves { get; set; } = gloves ?? [];

    [JsonPropertyName("tWeapons")]
    public Dictionary<ushort, WeaponEconItem> TWeapons { get; set; } = tWeapons ?? [];

    [JsonPropertyName("ctWeapons")]
    public Dictionary<ushort, WeaponEconItem> CTWeapons { get; set; } = ctWeapons ?? [];

    [JsonPropertyName("agents")]
    public Dictionary<byte, AgentItem> Agents { get; set; } = agents ?? [];

    [JsonPropertyName("pin")]
    public uint? Pin { get; set; } = pin;

    [JsonPropertyName("musicKit")]
    public MusicKitItem? MusicKit { get; set; } = musicKit;

    [JsonPropertyName("graffiti")]
    public GraffitiItem? Graffiti { get; set; } = graffiti;

    public WeaponEconItem? GetKnife(byte team, bool fallback)
    {
        if (Knives.TryGetValue(team, out var knife))
        {
            return knife;
        }
        // TODO Refactor this castfest.
        if (fallback && Knives.TryGetValue((byte)PlayerHelpers.ToggleTeam((Team)team), out knife))
        {
            return knife;
        }
        return null;
    }

    public Dictionary<ushort, WeaponEconItem> GetWeapons(byte team)
    {
        return (Team)team == Team.T ? TWeapons : CTWeapons;
    }

    public WeaponEconItem? GetWeapon(byte team, ushort def, bool fallback)
    {
        if (GetWeapons(team).TryGetValue(def, out var weapon))
        {
            return weapon;
        }
        if (
            fallback
            && GetWeapons((byte)PlayerHelpers.ToggleTeam((Team)team)).TryGetValue(def, out weapon)
        )
        {
            return weapon;
        }
        return null;
    }

    public BaseEconItem? GetGloves(byte team, bool fallback)
    {
        if (Gloves.TryGetValue(team, out var glove))
        {
            return glove;
        }
        if (fallback && Gloves.TryGetValue((byte)PlayerHelpers.ToggleTeam((Team)team), out glove))
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

    public float GetWeaponEconItemWear(WeaponEconItem item)
    {
        var wear = item.Wear;
        var stickers = string.Join("_", item.Stickers.Select(s => s.Def));
        var cachedByWear = CachedWeaponEconItems.TryGetValue(item.Paint, out var c) ? c : [];
        while (true)
        {
            (ushort, string)? pair = cachedByWear.TryGetValue(wear, out var p) ? p : null;
            var cached = pair?.Item1 == item.Def && pair?.Item2 == stickers;
            if (pair == null || cached)
            {
                cachedByWear[wear] = (item.Def, stickers);
                CachedWeaponEconItems[item.Paint] = cachedByWear;
                return wear;
            }
            wear += 0.001f;
        }
    }

    public InventoryItemWrapper GetItemForSlot(
        loadout_slot_t slot,
        byte team,
        ushort? def,
        bool fallback,
        int minModels
    )
    {
        if (
            slot >= loadout_slot_t.LOADOUT_SLOT_MELEE
            && slot <= loadout_slot_t.LOADOUT_SLOT_EQUIPMENT5
        )
        {
            var isKnife = slot == loadout_slot_t.LOADOUT_SLOT_MELEE;
            var weaponItem =
                isKnife ? GetKnife(team, fallback)
                : def.HasValue ? GetWeapon(team, def.Value, fallback)
                : null;
            return weaponItem != null
                ? InventoryItemWrapper.FromWeapon(weaponItem)
                : InventoryItemWrapper.Empty();
        }
        if (slot == loadout_slot_t.LOADOUT_SLOT_CLOTHING_CUSTOMPLAYER)
        {
            if (minModels > 0)
                return team == (byte)Team.T
                    ? InventoryItemWrapper.FromAgent(new AgentItem { Def = 5036 })
                    : InventoryItemWrapper.FromAgent(new AgentItem { Def = 5037 });
            if (Agents.TryGetValue(team, out var agentItem) && agentItem.Def != null)
                return InventoryItemWrapper.FromAgent(agentItem);
            return InventoryItemWrapper.Empty();
        }
        if (slot == loadout_slot_t.LOADOUT_SLOT_CLOTHING_HANDS)
        {
            var gloveItem = GetGloves(team, fallback);
            return gloveItem != null
                ? InventoryItemWrapper.FromGlove(gloveItem)
                : InventoryItemWrapper.Empty();
        }
        if (slot == loadout_slot_t.LOADOUT_SLOT_FLAIR0)
        {
            return Pin.HasValue
                ? InventoryItemWrapper.FromPin(Pin.Value)
                : InventoryItemWrapper.Empty();
        }
        if (slot == loadout_slot_t.LOADOUT_SLOT_MUSICKIT)
        {
            return MusicKit != null
                ? InventoryItemWrapper.FromMusicKit(MusicKit)
                : InventoryItemWrapper.Empty();
        }
        return InventoryItemWrapper.Empty();
    }
}
