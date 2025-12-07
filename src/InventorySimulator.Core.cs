/*---------------------------------------------------------------------------------------------
*  Copyright (c) Ian Lucas. All rights reserved.
*  Licensed under the MIT License. See License.txt in the project root for license information.
*--------------------------------------------------------------------------------------------*/

using CS2Lib.SwiftlyCS2.Core;
using CS2Lib.SwiftlyCS2.Extensions;
using SwiftlyS2.Shared.Events;
using SwiftlyS2.Shared.Natives;
using SwiftlyS2.Shared.Players;
using SwiftlyS2.Shared.SchemaDefinitions;
using static CS2Lib.CS2Lib;

namespace InventorySimulator;

public partial class InventorySimulator
{
    public void GivePlayerMusicKit(IPlayer player, PlayerInventory inventory)
    {
        if (!player.IsValid || player.IsFakeClient)
            return;
        if (player.Controller.InventoryServices == null)
            return;
        var item = inventory.MusicKit;
        if (item != null)
        {
            player.Controller.InventoryServices.MusicID = (ushort)item.Def;
            player.Controller.InventoryServices.MusicIDUpdated();
            player.Controller.MusicKitID = item.Def;
            player.Controller.MusicKitIDUpdated();
            player.Controller.MusicKitMVPs = item.Stattrak;
            player.Controller.MusicKitMVPsUpdated();
        }
    }

    public void GivePlayerPin(IPlayer player, PlayerInventory inventory)
    {
        if (player.Controller.InventoryServices == null)
            return;
        var pin = inventory.Pin;
        if (pin == null)
            return;
        for (var index = 0; index < player.Controller.InventoryServices.Rank.ElementSize; index++)
        {
            player.Controller.InventoryServices.Rank[index] =
                index == 5 ? (MedalRank_t)pin.Value : MedalRank_t.MEDAL_RANK_NONE;
            player.Controller.InventoryServicesUpdated();
        }
    }

    public void GivePlayerGloves(IPlayer player, PlayerInventory inventory)
    {
        var pawn = player.PlayerPawn;
        if (pawn?.IsValid != true)
            return;
        var fallback = IsFallbackTeam.Value;
        var item = inventory.GetGloves(player.Controller.TeamNum, fallback);
        if (item != null)
        {
            // Workaround by @daffyyyy.
            if (IsWsGlovesFix.Value)
            {
                var model = pawn
                    .CBodyComponent?.SceneNode?.GetSkeletonInstance()
                    ?.ModelState.ModelName;
                if (!string.IsNullOrEmpty(model))
                {
                    pawn.SetModel("characters/models/tm_jumpsuit/tm_jumpsuit_varianta.vmdl");
                    pawn.SetModel(model);
                }
            }
            var glove = pawn.EconGloves;
            Core.Scheduler.NextTick(() =>
            {
                if (pawn.IsValid)
                {
                    ApplyGloveAttributesFromItem(glove, item);
                    // Thanks to xstage and stefanx111
                    pawn.AcceptInput("SetBodygroup", value: "default_gloves,1");
                }
            });
        }
    }

    public void GivePlayerAgent(IPlayer player, PlayerInventory inventory)
    {
        if (MinModels.Value > 0)
        {
            // For now any value non-zero will force SAS & Phoenix.
            // In the future: 1 - Map agents only, 2 - SAS & Phoenix.
            if (player.Controller.Team == Team.T)
                SetPlayerModel(player, "characters/models/tm_phoenix/tm_phoenix.vmdl");
            if (player.Controller.Team == Team.CT)
                SetPlayerModel(player, "characters/models/ctm_sas/ctm_sas.vmdl");
            return;
        }
        if (inventory.Agents.TryGetValue(player.Controller.TeamNum, out var item))
        {
            var patches =
                item.Patches.Count != 5 ? [.. Enumerable.Repeat((uint)0, 5)] : item.Patches;
            SetPlayerModel(
                player,
                EntityUtils.GetAgentModelPath(item.Model),
                item.VoFallback,
                item.VoPrefix,
                item.VoFemale,
                patches
            );
        }
    }

    public void GivePlayerWeaponSkin(IPlayer player, CBasePlayerWeapon weapon)
    {
        if (IsCustomWeaponItemID(weapon))
            return;
        var isKnife = IsMeleeDesignerName(weapon.DesignerName);
        var entityDef = weapon.AttributeManager.Item.ItemDefinitionIndex;
        var inventory = GetPlayerInventory(player);
        var fallback = IsFallbackTeam.Value;
        var item = isKnife
            ? inventory.GetKnife(player.Controller.TeamNum, fallback)
            : inventory.GetWeapon(player.Controller.TeamNum, entityDef, fallback);
        if (item != null)
        {
            item.WearOverride ??= inventory.GetWeaponEconItemWear(item);
            ApplyWeaponAttributesFromItem(weapon.AttributeManager.Item, item, weapon, player);
        }
    }

    public void GivePlayerWeaponStatTrakIncrement(
        IPlayer player,
        string designerName,
        string weaponItemId
    )
    {
        try
        {
            var weapon = player.PlayerPawn.Value?.WeaponServices?.ActiveWeapon.Value;
            if (
                weapon == null
                || !IsCustomWeaponItemID(weapon)
                || weapon.FallbackStatTrak < 0
                || weapon.AttributeManager.Item.AccountID != (uint)player.SteamID
                || weapon.AttributeManager.Item.ItemID != ulong.Parse(weaponItemId)
                || weapon.FallbackStatTrak >= 999_999
            )
                return;
            var isKnife = IsMeleeDesignerName(designerName);
            var newValue = weapon.FallbackStatTrak + 1;
            var def = weapon.AttributeManager.Item.ItemDefinitionIndex;
            weapon.FallbackStatTrak = newValue;
            weapon.AttributeManager.Item.NetworkedDynamicAttributes.SetOrAddAttribute(
                "kill eater",
                EngineUtils.ViewAs<int, float>(newValue)
            );
            weapon.AttributeManager.Item.AttributeList.SetOrAddAttribute(
                "kill eater",
                EngineUtils.ViewAs<int, float>(newValue)
            );
            var inventory = GetPlayerInventory(player);
            var fallback = IsFallbackTeam.Value;
            var item = isKnife
                ? inventory.GetKnife(player.Controller.TeamNum, fallback)
                : inventory.GetWeapon(player.Controller.TeamNum, def, fallback);
            if (item != null)
            {
                item.Stattrak = newValue;
                SendStatTrakIncrement(player.SteamID, item.Uid);
            }
        }
        catch
        {
            // Ignore any errors.
        }
    }

    public void GivePlayerMusicKitStatTrakIncrement(CCSPlayerController player)
    {
        if (PlayerInventoryManager.TryGetValue(player.SteamID, out var inventory))
        {
            var item = inventory.MusicKit;
            if (item != null)
            {
                item.Stattrak += 1;
                SendStatTrakIncrement(player.SteamID, item.Uid);
            }
        }
    }

    public void GivePlayerCurrentWeapons(
        IPlayer player,
        PlayerInventory inventory,
        PlayerInventory oldInventory
    )
    {
        var pawn = player.PlayerPawn;
        var weaponServices = pawn?.WeaponServices?.As<CCSPlayer_WeaponServices>();
        if (pawn == null || weaponServices == null)
            return;
        var activeDesignerName = weaponServices.ActiveWeapon.Value?.DesignerName;
        var targets = new List<(string, string, int, int, bool, gear_slot_t)>();
        foreach (var handle in weaponServices.MyWeapons)
        {
            var weapon = handle.Value?.As<CCSWeaponBase>();
            if (weapon == null || weapon.DesignerName.Contains("weapon_") != true)
                continue;
            if (weapon.OriginalOwnerXuidLow != (uint)player.SteamID)
                continue;
            var data = weapon.VData.As<CCSWeaponBaseVData>();
            if (
                data.GearSlot
                is gear_slot_t.GEAR_SLOT_RIFLE
                    or gear_slot_t.GEAR_SLOT_PISTOL
                    or gear_slot_t.GEAR_SLOT_KNIFE
            )
            {
                var entityDef = weapon.AttributeManager.Item.ItemDefinitionIndex;
                var fallback = IsFallbackTeam.Value;
                var oldItem =
                    data.GearSlot is gear_slot_t.GEAR_SLOT_KNIFE
                        ? oldInventory.GetKnife(player.Controller.TeamNum, fallback)
                        : oldInventory.GetWeapon(player.Controller.TeamNum, entityDef, fallback);
                var item =
                    data.GearSlot is gear_slot_t.GEAR_SLOT_KNIFE
                        ? inventory.GetKnife(player.Controller.TeamNum, fallback)
                        : inventory.GetWeapon(player.Controller.TeamNum, entityDef, fallback);
                if (oldItem == item)
                    continue;
                var clip = weapon.Clip1;
                var reserve = weapon.ReserveAmmo[0];
                targets.Add(
                    (
                        weapon.DesignerName,
                        weapon.GetDesignerName(),
                        clip,
                        reserve,
                        activeDesignerName == weapon.DesignerName,
                        data.GearSlot
                    )
                );
            }
        }
        foreach (var target in targets)
        {
            var designerName = target.Item1;
            var actualDesignerName = target.Item2;
            var clip = target.Item3;
            var reserve = target.Item4;
            var active = target.Item5;
            var gearSlot = target.Item6;
            var oldWeapon = (
                (CHandle<CBasePlayerWeapon>?)
                    weaponServices.MyWeapons.FirstOrDefault(h =>
                        h.Value?.DesignerName == designerName
                    )
            )?.Value;
            if (oldWeapon != null)
            {
                weaponServices.DropWeapon(oldWeapon);
                oldWeapon.Despawn();
            }
            var weapon = player.PlayerPawn?.ItemServices?.GiveItem<CBasePlayerWeapon>(
                actualDesignerName
            );
            if (weapon != null)
                Core.Scheduler.Delay(
                    32,
                    () =>
                    {
                        if (weapon.IsValid)
                        {
                            weapon.Clip1 = clip;
                            weapon.Clip1Updated();
                            weapon.ReserveAmmo[0] = reserve;
                            weapon.ReserveAmmoUpdated();
                            Core.Scheduler.NextTick(() =>
                            {
                                if (active && player.IsValid)
                                {
                                    var command = gearSlot switch
                                    {
                                        gear_slot_t.GEAR_SLOT_RIFLE => "slot1",
                                        gear_slot_t.GEAR_SLOT_PISTOL => "slot2",
                                        gear_slot_t.GEAR_SLOT_KNIFE => "slot3",
                                        _ => null,
                                    };
                                    if (command != null)
                                        player.ExecuteCommand(command);
                                }
                            });
                        }
                    }
                );
        }
    }

    public void GiveOnPlayerSpawn(IPlayer player)
    {
        var inventory = GetPlayerInventory(player);
        GivePlayerPin(player, inventory);
        GivePlayerAgent(player, inventory);
        GivePlayerGloves(player, inventory);
    }

    public void GiveOnLoadPlayerInventory(IPlayer player)
    {
        GiveTeamPreviewItems("team_select", player);
        GiveTeamPreviewItems("team_intro", player);
    }

    public void GiveOnRefreshPlayerInventory(IPlayer player, PlayerInventory oldInventory)
    {
        var inventory = GetPlayerInventory(player);
        GivePlayerPin(player, inventory);
        if (IsWsImmediately.Value)
        {
            GivePlayerGloves(player, inventory);
            GivePlayerCurrentWeapons(player, inventory, oldInventory);
        }
    }

    public void GivePlayerGraffiti(IPlayer player, CPlayerSprayDecal sprayDecal)
    {
        var inventory = GetPlayerInventory(player);
        var item = inventory.Graffiti;
        if (item != null)
        {
            sprayDecal.Player = item.Def;
            sprayDecal.PlayerUpdated();
            sprayDecal.TintID = item.Tint;
            sprayDecal.TintIDUpdated();
        }
    }

    public unsafe void SprayPlayerGraffiti(IPlayer player)
    {
        if (!player.IsValid)
            return;
        var inventory = GetPlayerInventory(player);
        var item = inventory.Graffiti;
        if (item == null)
            return;
        var pawn = player.PlayerPawn;
        if (pawn == null || pawn.LifeState != (int)LifeState_t.LIFE_ALIVE)
            return;
        var movementServices = pawn.MovementServices?.As<CCSPlayer_MovementServices>();
        if (movementServices == null)
            return;
        var trace = stackalloc GameTrace[1];
        if (!pawn.IsAbleToApplySpray((IntPtr)trace) || (IntPtr)trace == IntPtr.Zero)
            return;
        SprayCanShakeSound.Recipients.AddRecipient(player.PlayerID);
        SprayCanShakeSound.Emit();
        SprayCanShakeSound.Recipients.RemoveRecipient(player.PlayerID);
        PlayerSprayCooldownManager[player.SteamID] = Now();
        var endPos = Vector3toVector(trace->EndPos);
        var normalPos = Vector3toVector(trace->Normal);
        var sprayDecal = Core.EntitySystem.CreateEntityByDesignerName<CPlayerSprayDecal>(
            "player_spray_decal"
        );
        if (sprayDecal != null)
        {
            sprayDecal.EndPos.Add(endPos);
            sprayDecal.Start.Add(endPos);
            sprayDecal.Left.Add(movementServices.Left);
            sprayDecal.Normal.Add(normalPos);
            sprayDecal.AccountID = (uint)player.SteamID;
            sprayDecal.Player = item.Def;
            sprayDecal.TintID = item.Tint;
            sprayDecal.DispatchSpawn();
            SprayCanPaintSound.Recipients.AddRecipient(player.PlayerID);
            SprayCanPaintSound.Emit();
            SprayCanPaintSound.Recipients.RemoveRecipient(player.PlayerID);
        }
    }

    public void SprayPlayerGraffitiThruPlayerButtons(IPlayer player)
    {
        if (
            (player.PressedButtons & GameButtonFlags.E) != 0
            && player.PlayerPawn?.IsAbleToApplySpray() == true
        )
        {
            if (IsPlayerUseCmdBusy(player))
                PlayerUseCmdBlockManager[player.SteamID] = true;
            if (PlayerUseCmdManager.TryGetValue(player.SteamID, out var timer))
                timer.Kill();
            PlayerUseCmdManager[player.SteamID] = Core.Scheduler.DelayBySeconds(
                0.1f,
                () =>
                {
                    if (PlayerUseCmdBlockManager.ContainsKey(player.SteamID))
                        PlayerUseCmdBlockManager.Remove(player.SteamID, out var _);
                    else if (player.IsValid && !IsPlayerUseCmdBusy(player))
                        player.ExecuteCommand("css_spray");
                }
            );
        }
    }

    public void GiveTeamPreviewItems(string prefix, IPlayer? player = null)
    {
        var teamPreviews = TeamSelectSuffixes.SelectMany(team =>
            Core.EntitySystem.GetAllEntitiesByDesignerName<CCSGO_TeamPreviewCharacterPosition>(
                $"{prefix}_{team}"
            )
        );
        foreach (var teamPreview in teamPreviews)
        {
            if (teamPreview.Xuid == 0 || (player != null && player.SteamID != teamPreview.Xuid))
                continue;
            player ??= Utilities.GetPlayerFromSteamID(Core, teamPreview.Xuid);
            if (player == null || player.IsValid)
                continue;
            var inventory = GetPlayerInventory(player);
            GivePlayerTeamPreview(player, teamPreview, inventory);
        }
    }

    public void GivePlayerTeamPreview(
        IPlayer player,
        CCSGO_TeamPreviewCharacterPosition teamPreview,
        PlayerInventory inventory
    )
    {
        var fallback = IsFallbackTeam.Value;
        var gloveItem = inventory.GetGloves(player.Controller.TeamNum, fallback);
        if (gloveItem != null)
        {
            ApplyGloveAttributesFromItem(teamPreview.GlovesItem, gloveItem);
            teamPreview.GlovesItemUpdated();
        }

        var weaponItem = teamPreview.WeaponItem.IsMelee()
            ? inventory.GetKnife(player.Controller.TeamNum, fallback)
            : inventory.GetWeapon(
                player.Controller.TeamNum,
                teamPreview.WeaponItem.ItemDefinitionIndex,
                fallback
            );
        if (weaponItem != null)
        {
            ApplyWeaponAttributesFromItem(teamPreview.WeaponItem, weaponItem);
            teamPreview.WeaponItemUpdated();
        }

        if (
            inventory.Agents.TryGetValue(player.Controller.TeamNum, out var agentItem)
            && agentItem.Def != null
        )
        {
            teamPreview.AgentItem.ItemDefinitionIndex = agentItem.Def.Value;
            teamPreview.AgentItemUpdated();
        }
    }
}
