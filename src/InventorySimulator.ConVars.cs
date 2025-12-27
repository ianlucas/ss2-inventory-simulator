/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using SwiftlyS2.Shared.Convars;

namespace InventorySimulator;

public partial class InventorySimulator
{
    public readonly IConVar<int> MinModels = core.ConVar.Create(
        "invsim_minmodels",
        "Enable player agents (0 = enabled, 1 = use map models per team, 2 = SAS & Phoenix).",
        0
    );
    public readonly IConVar<bool> IsStatTrakIgnoreBots = core.ConVar.Create(
        "invsim_stattrak_ignore_bots",
        "Ignore StatTrak kill count increments for bot kills.",
        true
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
