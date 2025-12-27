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

    public static void Apply(
        this CEconItemView self,
        EconItem econItem,
        loadout_slot_t? slot,
        ulong? steamId
    )
    {
        var isMelee = slot == loadout_slot_t.LOADOUT_SLOT_MELEE;
        self.Initialized = true;
        if (econItem.Def != null)
            self.ItemDefinitionIndex = econItem.Def.Value;
        var itemId = NextItemId++;
        self.ItemID = itemId;
        self.ItemIDLow = (uint)(itemId & 0xFFFFFFFF);
        self.ItemIDHigh = (uint)(itemId >> 32);
        if (steamId != null)
            self.AccountID = new CSteamID(steamId.Value).GetAccountID().m_AccountID;
        if (isMelee)
            self.EntityQuality = 3;
        else
            self.EntityQuality = econItem.Stattrak >= 0 ? 9 : 4;
        if (econItem.Nametag != null)
            self.CustomName = econItem.Nametag;
        var customAttrs = econItem.GetAttributes();
        var attrs = self.NetworkedDynamicAttributes;
        attrs.Attributes.RemoveAll();
        foreach (var (attributeName, value) in customAttrs)
            attrs.SetOrAddAttribute(attributeName, value);
    }
}
