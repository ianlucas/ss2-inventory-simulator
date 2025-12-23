/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

namespace InventorySimulator;

public class PlayerInventoryItem
{
    public WeaponEconItem? WeaponItem { get; set; }
    public AgentItem? AgentItem { get; set; }
    public BaseEconItem? GloveItem { get; set; }
    public uint? PinItem { get; set; }
    public MusicKitItem? MusicKitItem { get; set; }

    public static PlayerInventoryItem FromWeapon(WeaponEconItem item) =>
        new() { WeaponItem = item };

    public static PlayerInventoryItem FromAgent(AgentItem item) => new() { AgentItem = item };

    public static PlayerInventoryItem FromGlove(BaseEconItem item) => new() { GloveItem = item };

    public static PlayerInventoryItem FromPin(uint item) => new() { PinItem = item };

    public static PlayerInventoryItem FromMusicKit(MusicKitItem item) =>
        new() { MusicKitItem = item };
}
