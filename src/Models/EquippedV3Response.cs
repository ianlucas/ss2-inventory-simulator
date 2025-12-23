/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Text.Json.Serialization;

namespace InventorySimulator;

public class EquippedV3Response
{
    [JsonPropertyName("knives")]
    public Dictionary<byte, WeaponEconItem> Knives { get; set; } = [];

    [JsonPropertyName("gloves")]
    public Dictionary<byte, BaseEconItem> Gloves { get; set; } = [];

    [JsonPropertyName("tWeapons")]
    public Dictionary<ushort, WeaponEconItem> TWeapons { get; set; } = [];

    [JsonPropertyName("ctWeapons")]
    public Dictionary<ushort, WeaponEconItem> CTWeapons { get; set; } = [];

    [JsonPropertyName("agents")]
    public Dictionary<byte, AgentItem> Agents { get; set; } = [];

    [JsonPropertyName("pin")]
    public uint? Pin { get; set; }

    [JsonPropertyName("musicKit")]
    public MusicKitItem? MusicKit { get; set; }

    [JsonPropertyName("graffiti")]
    public GraffitiItem? Graffiti { get; set; }
}
