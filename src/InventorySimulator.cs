/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using SwiftlyS2.Shared;
using SwiftlyS2.Shared.GameEventDefinitions;
using SwiftlyS2.Shared.Plugins;

namespace InventorySimulator;

[PluginMetadata(
    Id = "InventorySimulator",
    Version = "1.0.0",
    Name = "InventorySimulator",
    Author = "Ian Lucas",
    Description = "Inventory Simulator (inventory.cstrike.app) plugin."
)]
public partial class InventorySimulator(ISwiftlyCore core) : BasePlugin(core)
{
    public override void Load(bool hotReload)
    {
        Core.Event.OnTick += OnTick;
        Core.Event.OnEntityCreated += OnEntityCreated;
        Core.Event.OnConVarValueChanged += OnConVarValueChanged;
        Core.GameEvent.HookPost<EventPlayerConnect>(OnPlayerConnect);
        Core.GameEvent.HookPost<EventPlayerConnectFull>(OnPlayerConnectFull);
        Core.GameEvent.HookPost<EventRoundPrestart>(OnRoundPrestart);
        Core.GameEvent.HookPost<EventPlayerSpawn>(OnPlayerSpawn);
        Core.GameEvent.HookPre<EventPlayerDeath>(OnPlayerDeathPre);
        Core.GameEvent.HookPre<EventRoundMvp>(OnRoundMvpPre);
        Core.GameEvent.HookPost<EventPlayerDisconnect>(OnPlayerDisconnect);
        // hook give named item
        // hook update select team preview
    }

    public override void Unload() { }
}
