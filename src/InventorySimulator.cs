/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using SwiftlyS2.Shared;
using SwiftlyS2.Shared.GameEventDefinitions;
using SwiftlyS2.Shared.Plugins;
using SwiftlyS2.Shared.Sounds;

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
    public readonly SoundEvent SprayCanShakeSound = new("SprayCan.Shake");
    public readonly SoundEvent SprayCanPaintSound = new("SprayCan.Paint");

    public override void Load(bool hotReload)
    {
        Natives.Initialize(Core);

        Core.Event.OnTick += OnTick;
        Core.Event.OnEntityCreated += OnEntityCreated;
        Core.Event.OnConVarValueChanged += OnConVarValueChanged;
        Core.Event.OnClientProcessUsercmds += OnClientProcessUsercmds;
        Core.GameEvent.HookPost<EventPlayerConnect>(OnPlayerConnect);
        Core.GameEvent.HookPost<EventPlayerConnectFull>(OnPlayerConnectFull);
        Core.GameEvent.HookPost<EventRoundPrestart>(OnRoundPrestart);
        Core.GameEvent.HookPost<EventPlayerSpawn>(OnPlayerSpawn);
        Core.GameEvent.HookPre<EventPlayerDeath>(OnPlayerDeathPre);
        Core.GameEvent.HookPre<EventRoundMvp>(OnRoundMvpPre);
        Core.GameEvent.HookPost<EventPlayerDisconnect>(OnPlayerDisconnect);
        Natives.CCSPlayer_ItemServices_GiveNamedItem.AddHook(OnGiveNamedItem);
        Natives.CCSPlayerController_UpdateTeamSelectionPreview.AddHook(
            OnUpdateTeamSelectionPreview
        );
        Natives.CCSPlayerInventory_GetItemInLoadout.AddHook(OnGetItemInLoadout);
        OnFileChanged();
        OnIsRequireInventoryChanged();
    }

    public override void Unload() { }
}
