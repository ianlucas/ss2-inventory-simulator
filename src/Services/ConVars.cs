/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using SwiftlyS2.Shared.Convars;

namespace InventorySimulator;

public static class ConVars
{
    public static readonly IConVar<string> Url = Swiftly.Core.ConVar.Create(
        "invsim_url",
        "API URL for the Inventory Simulator service.",
        "https://inventory.cstrike.app"
    );

    public static readonly IConVar<string> ApiKey = Swiftly.Core.ConVar.Create(
        "invsim_apikey",
        "API key for the Inventory Simulator service.",
        ""
    );

    public static readonly IConVar<string> File = Swiftly.Core.ConVar.Create(
        "invsim_file",
        "Inventory data file to load when the plugin starts.",
        "inventories.json"
    );

    public static readonly IConVar<bool> IsWsEnabled = Swiftly.Core.ConVar.Create(
        "invsim_ws_enabled",
        "Allow players to refresh their inventory using the !ws command.",
        false
    );

    public static readonly IConVar<bool> IsWsImmediately = Swiftly.Core.ConVar.Create(
        "invsim_ws_immediately",
        "Apply skin changes immediately without requiring a respawn.",
        false
    );

    public static readonly IConVar<int> WsCooldown = Swiftly.Core.ConVar.Create(
        "invsim_ws_cooldown",
        "Cooldown duration in seconds between inventory refreshes per player.",
        30
    );

    public static readonly IConVar<string> WsUrlPrintFormat = Swiftly.Core.ConVar.Create(
        "invsim_ws_url_print_format",
        "URL format string displayed when using the !ws command.",
        "{Host}"
    );

    public static readonly IConVar<bool> IsWsLogin = Swiftly.Core.ConVar.Create(
        "invsim_wslogin",
        "Allow players to authenticate with Inventory Simulator and display their login URL (not recommended).",
        false
    );

    public static readonly IConVar<bool> IsRequireInventory = Swiftly.Core.ConVar.Create(
        "invsim_require_inventory",
        "Require the player's inventory to be fetched before allowing them to join the game.",
        false
    );

    public static readonly IConVar<bool> IsSprayEnabled = Swiftly.Core.ConVar.Create(
        "invsim_spray_enabled",
        "Enable spraying via the !spray command and/or use key.",
        true
    );

    public static readonly IConVar<bool> IsSprayOnUse = Swiftly.Core.ConVar.Create(
        "invsim_spray_on_use",
        "Apply spray when the player presses the use key.",
        false
    );

    public static readonly IConVar<int> SprayCooldown = Swiftly.Core.ConVar.Create(
        "invsim_spray_cooldown",
        "Cooldown duration in seconds between sprays per player.",
        30
    );

    public static readonly IConVar<bool> IsSprayChangerEnabled = Swiftly.Core.ConVar.Create(
        "invsim_spraychanger_enabled",
        "Replace the player's vanilla spray with their equipped graffiti.",
        false
    );

    public static readonly IConVar<bool> IsStatTrakIgnoreBots = Swiftly.Core.ConVar.Create(
        "invsim_stattrak_ignore_bots",
        "Ignore StatTrak kill count increments for bot kills.",
        true
    );

    public static readonly IConVar<bool> IsFallbackTeam = Swiftly.Core.ConVar.Create(
        "invsim_fallback_team",
        "Allow using skins from any team (prioritizes current team first).",
        false
    );

    public static readonly IConVar<int> MinModels = Swiftly.Core.ConVar.Create(
        "invsim_minmodels",
        "Enable player agents (0 = enabled, 1 = use map models per team, 2 = SAS & Phoenix).",
        0
    );

    public static void Initialize()
    {
        _ = Url;
        _ = ApiKey;
        _ = File;
        _ = IsWsEnabled;
        _ = IsWsImmediately;
        _ = WsCooldown;
        _ = WsUrlPrintFormat;
        _ = IsWsLogin;
        _ = IsRequireInventory;
        _ = IsSprayEnabled;
        _ = IsSprayOnUse;
        _ = SprayCooldown;
        _ = IsSprayChangerEnabled;
        _ = IsStatTrakIgnoreBots;
        _ = IsFallbackTeam;
        _ = MinModels;
    }
}
