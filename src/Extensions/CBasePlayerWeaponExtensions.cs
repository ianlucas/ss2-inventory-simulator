/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using SwiftlyS2.Shared.SchemaDefinitions;

namespace InventorySimulator;

public static class CBasePlayerWeaponExtensions
{
    public static bool HasCustomItemID(this CBasePlayerWeapon self)
    {
        return self.AttributeManager.Item.ItemID >= CEconItemViewExtensions.MinimumCustomItemID;
    }
}
