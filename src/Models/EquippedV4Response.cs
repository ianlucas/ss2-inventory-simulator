/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Text.Json.Serialization;

namespace InventorySimulator;

public class EquippedV4Response
{
    [JsonPropertyName("agents")]
    public Dictionary<byte, EconItem> Agents { get; set; } = [];

    [JsonPropertyName("collectible")]
    public EconItem? Collectible { get; set; }

    [JsonPropertyName("ctWeapons")]
    public Dictionary<ushort, EconItem> CTWeapons { get; set; } = [];

    [JsonPropertyName("gloves")]
    public Dictionary<byte, EconItem> Gloves { get; set; } = [];

    [JsonPropertyName("graffiti")]
    public EconItem? Graffiti { get; set; }

    [JsonPropertyName("knives")]
    public Dictionary<byte, EconItem> Knives { get; set; } = [];

    [JsonPropertyName("musicKit")]
    public EconItem? MusicKit { get; set; }

    [JsonPropertyName("tWeapons")]
    public Dictionary<ushort, EconItem> TWeapons { get; set; } = [];
}
