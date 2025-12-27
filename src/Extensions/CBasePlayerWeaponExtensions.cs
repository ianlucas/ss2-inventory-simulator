/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using SwiftlyS2.Shared.SchemaDefinitions;

namespace InventorySimulator;

public static class CBasePlayerWeaponExtensions
{
    public static string GetDesignerName(this CBasePlayerWeapon weapon)
    {
        var designerName =
            SchemaHelper
                .GetItemSchema()
                ?.GetItemDefinition(weapon.AttributeManager.Item.ItemDefinitionIndex)
                ?.DefinitionName ?? weapon.DesignerName;
        return ItemHelper.IsMeleeDesignerName(designerName) ? "weapon_knife" : designerName;
    }

    public static bool HasCustomItemID(this CBasePlayerWeapon weapon)
    {
        return weapon.AttributeManager.Item.ItemID >= CEconItemViewExtensions.MinimumCustomItemID;
    }
}
