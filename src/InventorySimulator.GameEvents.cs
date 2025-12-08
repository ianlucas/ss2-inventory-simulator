/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using SwiftlyS2.Shared.GameEventDefinitions;
using SwiftlyS2.Shared.Misc;
using SwiftlyS2.Shared.Players;

namespace InventorySimulator;

public partial class InventorySimulator
{
    public HookResult OnPlayerConnect(EventPlayerConnect @event)
    {
        var player = @event.UserIdPlayer;
        if (player != null && !player.IsFakeClient)
            OnPlayerConnect(player);
        return HookResult.Continue;
    }

    public HookResult OnPlayerConnectFull(EventPlayerConnectFull @event)
    {
        var player = @event.UserIdPlayer;
        if (player != null && !player.IsFakeClient)
            OnPlayerConnect(player);
        return HookResult.Continue;
    }

    public void OnPlayerConnect(IPlayer player)
    {
        if (PlayerOnTickInventoryManager.TryGetValue(player.SteamID, out var tuple))
            PlayerOnTickInventoryManager[player.SteamID] = (player, tuple.Item2);
        RefreshPlayerInventory(player);
    }

    public HookResult OnRoundPrestart(EventRoundPrestart @event)
    {
        Core.Scheduler.NextTick(() =>
        {
            if (Core.EntitySystem.GetGameRules()?.TeamIntroPeriod == true)
                GiveTeamPreviewItems("team_intro");
        });
        return HookResult.Continue;
    }

    public HookResult OnPlayerSpawn(EventPlayerSpawn @event)
    {
        var player = @event.UserIdPlayer;
        if (player != null && !player.IsFakeClient && player.IsValid)
            GiveOnPlayerSpawn(player);
        return HookResult.Continue;
    }

    public HookResult OnPlayerDeathPre(EventPlayerDeath @event)
    {
        var attacker = Core.PlayerManager.GetPlayer(@event.Attacker);
        var victim = @event.UserIdPlayer;
        if (attacker != null && victim != null)
        {
            var isAttackerValid = !attacker.IsFakeClient && attacker.IsValid;
            var isVictimValid =
                (!IsStatTrakIgnoreBots.Value || !victim.IsFakeClient) && victim.IsValid;
            if (isAttackerValid && isVictimValid)
                GivePlayerWeaponStatTrakIncrement(attacker, @event.Weapon, @event.WeaponItemid);
        }
        return HookResult.Continue;
    }

    public HookResult OnRoundMvpPre(EventRoundMvp @event)
    {
        var player = @event.UserIdPlayer;
        if (player != null && !player.IsFakeClient && player.IsValid)
            GivePlayerMusicKitStatTrakIncrement(player);
        return HookResult.Continue;
    }

    public HookResult OnPlayerDisconnect(EventPlayerDisconnect @event)
    {
        var player = @event.UserIdPlayer;
        if (player != null && !player.IsFakeClient)
        {
            ClearPlayerUseCmd(player.SteamID);
            ClearPlayerServerSideClient(player.PlayerID);
            RemovePlayerInventory(player.SteamID);
            ClearInventoryManager();
        }
        return HookResult.Continue;
    }
}
