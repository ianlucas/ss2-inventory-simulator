/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using SwiftlyS2.Shared.Convars;

namespace InventorySimulator;

public partial class InventorySimulator
{
    public readonly IConVar<bool> IsStatTrakIgnoreBots = core.ConVar.Create(
        "invsim_stattrak_ignore_bots",
        "Ignore StatTrak kill count increments for bot kills.",
        true
    );
    public readonly IConVar<bool> IsSprayChangerEnabled = core.ConVar.Create(
        "invsim_spraychanger_enabled",
        "Replace the player's vanilla spray with their equipped graffiti.",
        false
    );
    public readonly IConVar<bool> IsSprayEnabled = core.ConVar.Create(
        "invsim_spray_enabled",
        "Enable spraying via the !spray command and/or use key.",
        true
    );
    public readonly IConVar<bool> IsSprayOnUse = core.ConVar.Create(
        "invsim_spray_on_use",
        "Apply spray when the player presses the use key.",
        false
    );
    public readonly IConVar<bool> IsWsEnabled = core.ConVar.Create(
        "invsim_ws_enabled",
        "Allow players to refresh their inventory using the !ws command.",
        false
    );
    public readonly IConVar<string> WsUrlPrintFormat = core.ConVar.Create(
        "invsim_ws_url_print_format",
        "URL format string displayed when using the !ws command.",
        "{Host}"
    );
    public readonly IConVar<bool> IsWsImmediately = core.ConVar.Create(
        "invsim_ws_immediately",
        "Apply skin changes immediately without requiring a respawn.",
        false
    );
    public readonly IConVar<bool> IsFallbackTeam = core.ConVar.Create(
        "invsim_fallback_team",
        "Allow using skins from any team (prioritizes current team first).",
        false
    );
    public readonly IConVar<bool> IsRequireInventory = core.ConVar.Create(
        "invsim_require_inventory",
        "Require the player's inventory to be fetched before allowing them to join the game.",
        false
    );
    public readonly IConVar<int> MinModels = core.ConVar.Create(
        "invsim_minmodels",
        "Enable player agents (0 = enabled, 1 = use map models per team, 2 = SAS & Phoenix).",
        0
    );
    public readonly IConVar<int> WsCooldown = core.ConVar.Create(
        "invsim_ws_cooldown",
        "Cooldown duration in seconds between inventory refreshes per player.",
        30
    );
    public readonly IConVar<int> SprayCooldown = core.ConVar.Create(
        "invsim_spray_cooldown",
        "Cooldown duration in seconds between sprays per player.",
        30
    );
    public readonly IConVar<string> ApiKey = core.ConVar.Create(
        "invsim_apikey",
        "API key for the Inventory Simulator service.",
        ""
    );
    public readonly IConVar<string> Url = core.ConVar.Create(
        "invsim_url",
        "API URL for the Inventory Simulator service.",
        "https://inventory.cstrike.app"
    );
    public readonly IConVar<bool> IsWsLogin = core.ConVar.Create(
        "invsim_wslogin",
        "Allow players to authenticate with Inventory Simulator and display their login URL (not recommended).",
        false
    );
    public readonly IConVar<string> File = core.ConVar.Create(
        "invsim_file",
        "Inventory data file to load when the plugin starts.",
        "inventories.json"
    );
}
