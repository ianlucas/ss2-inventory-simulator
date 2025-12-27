/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using SwiftlyS2.Shared.Events;
using SwiftlyS2.Shared.Natives;
using SwiftlyS2.Shared.Players;
using SwiftlyS2.Shared.SchemaDefinitions;
using SwiftlyS2.Shared.SteamAPI;

namespace InventorySimulator;

public partial class InventorySimulator
{
    public void RegivePlayerAgent(
        IPlayer player,
        PlayerInventory inventory,
        PlayerInventory oldInventory
    )
    {
        if (MinModels.Value > 0)
            return;
        var pawn = player.PlayerPawn;
        if (pawn == null)
            return;
        var item = inventory.Agents.TryGetValue(player.Controller.TeamNum, out var a) ? a : null;
        var oldItem = oldInventory.Agents.TryGetValue(player.Controller.TeamNum, out a) ? a : null;
        if (oldItem == item)
            return;
        pawn.SetModelFromLoadout();
        pawn.SetModelFromClass();
        pawn.AcceptInput("SetBodygroup", "default_gloves,1");
    }

    public void RegivePlayerGloves(
        IPlayer player,
        PlayerInventory inventory,
        PlayerInventory oldInventory
    )
    {
        var pawn = player.PlayerPawn;
        var itemServices = pawn?.ItemServices;
        if (pawn == null || itemServices == null)
            return;
        var isFallbackTeam = IsFallbackTeam.Value;
        var item = inventory.GetGloves(player.Controller.TeamNum, isFallbackTeam);
        var oldItem = oldInventory.GetGloves(player.Controller.TeamNum, isFallbackTeam);
        if (oldItem == item)
            return;
        // Workaround by @daffyyyy.
        var model = pawn.CBodyComponent?.SceneNode?.GetSkeletonInstance()?.ModelState.ModelName;
        if (!string.IsNullOrEmpty(model))
        {
            pawn.SetModel("characters/models/tm_jumpsuit/tm_jumpsuit_varianta.vmdl");
            pawn.SetModel(model);
        }
        Core.Scheduler.NextWorldUpdate(() =>
        {
            itemServices.UpdateWearables();
            if (item != null)
                pawn.AcceptInput("SetBodygroup", "default_gloves,1");
        });
    }

    public void RegivePlayerWeapons(
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
                var isFallbackTeam = IsFallbackTeam.Value;
                var oldItem =
                    data.GearSlot is gear_slot_t.GEAR_SLOT_KNIFE
                        ? oldInventory.GetKnife(player.Controller.TeamNum, isFallbackTeam)
                        : oldInventory.GetWeapon(
                            player.Controller.TeamNum,
                            entityDef,
                            isFallbackTeam
                        );
                var item =
                    data.GearSlot is gear_slot_t.GEAR_SLOT_KNIFE
                        ? inventory.GetKnife(player.Controller.TeamNum, isFallbackTeam)
                        : inventory.GetWeapon(player.Controller.TeamNum, entityDef, isFallbackTeam);
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
                            Core.Scheduler.NextWorldUpdate(() =>
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

    public void GivePlayerWeaponStatTrakIncrement(
        IPlayer player,
        string designerName,
        string weaponItemId
    )
    {
        var weapon = player.PlayerPawn?.WeaponServices?.ActiveWeapon.Value;
        if (
            weapon == null
            || !weapon.HasCustomItemID()
            || weapon.AttributeManager.Item.AccountID
                != new CSteamID(player.SteamID).GetAccountID().m_AccountID
            || weapon.AttributeManager.Item.ItemID != ulong.Parse(weaponItemId)
        )
            return;
        var inventory = GetPlayerInventory(player);
        var isFallbackTeam = IsFallbackTeam.Value;
        var item = ItemHelper.IsMeleeDesignerName(designerName)
            ? inventory.GetKnife(player.Controller.TeamNum, isFallbackTeam)
            : inventory.GetWeapon(
                player.Controller.TeamNum,
                weapon.AttributeManager.Item.ItemDefinitionIndex,
                isFallbackTeam
            );
        if (item == null || item.Stattrak == null || item.Uid == null)
            return;
        item.Stattrak += 1;
        var statTrak = TypeHelper.ViewAs<int, float>(item.Stattrak.Value);
        weapon.AttributeManager.Item.NetworkedDynamicAttributes.SetOrAddAttribute(
            "kill eater",
            statTrak
        );
        SendStatTrakIncrement(player.SteamID, item.Uid.Value);
    }

    public void GivePlayerMusicKitStatTrakIncrement(IPlayer player)
    {
        if (PlayerInventoryManager.TryGetValue(player.SteamID, out var inventory))
        {
            var item = inventory.MusicKit;
            if (item != null && item.Uid != null)
            {
                item.Stattrak += 1;
                SendStatTrakIncrement(player.SteamID, item.Uid.Value);
            }
        }
    }

    public void GiveOnLoadPlayerInventory(IPlayer player)
    {
        var inventory = player.Controller.InventoryServices?.GetInventory();
        if (inventory?.IsValid == true)
            inventory.SendInventoryUpdateEvent();
    }

    public void GiveOnRefreshPlayerInventory(IPlayer player, PlayerInventory oldInventory)
    {
        var inventory = GetPlayerInventory(player);
        if (IsWsImmediately.Value)
        {
            RegivePlayerAgent(player, inventory, oldInventory);
            RegivePlayerGloves(player, inventory, oldInventory);
            RegivePlayerWeapons(player, inventory, oldInventory);
        }
    }

    public nint GivePlayerEconItem(
        ulong steamId,
        int team,
        int slot,
        EconItem econItem,
        nint copyFrom = 0
    )
    {
        var key = $"{steamId}_{team}_{slot}";
        if (CreatedCEconItemViewManager.TryGetValue(key, out var existingPtr))
        {
            var existingItem = Core.Memory.ToSchemaClass<CEconItemView>(existingPtr);
            existingItem.ApplyAttributes(econItem, (loadout_slot_t)slot, steamId);
            return existingPtr;
        }
        var item = SchemaHelper.CreateCEconItemView(copyFrom);
        item.ApplyAttributes(econItem, (loadout_slot_t)slot, steamId);
        CreatedCEconItemViewManager[key] = item.Address;
        return item.Address;
    }

    public void GivePlayerGraffiti(IPlayer player, CPlayerSprayDecal sprayDecal)
    {
        var inventory = GetPlayerInventory(player);
        var item = inventory.Graffiti;
        if (item != null && item.Def != null && item.Tint != null)
        {
            sprayDecal.Player = item.Def.Value;
            sprayDecal.PlayerUpdated();
            sprayDecal.TintID = item.Tint.Value;
            sprayDecal.TintIDUpdated();
        }
    }

    public unsafe void SprayPlayerGraffiti(IPlayer player)
    {
        if (!player.IsValid)
            return;
        var inventory = GetPlayerInventory(player);
        var item = inventory.Graffiti;
        if (item == null || item.Def == null || item.Tint == null)
            return;
        var pawn = player.PlayerPawn;
        if (pawn == null || pawn.LifeState != (int)LifeState_t.LIFE_ALIVE)
            return;
        var movementServices = pawn.MovementServices?.As<CCSPlayer_MovementServices>();
        if (movementServices == null)
            return;
        var trace = stackalloc CGameTrace[1];
        if (!pawn.IsAbleToApplySpray((nint)trace) || (nint)trace == nint.Zero)
            return;
        SprayCanShakeSound.Recipients.AddRecipient(player.PlayerID);
        SprayCanShakeSound.Emit();
        SprayCanShakeSound.Recipients.RemoveRecipient(player.PlayerID);
        PlayerSprayCooldownManager[player.SteamID] = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var sprayDecal = Core.EntitySystem.CreateEntityByDesignerName<CPlayerSprayDecal>(
            "player_spray_decal"
        );
        if (sprayDecal != null)
        {
            sprayDecal.EndPos += trace->EndPos;
            sprayDecal.Start += trace->EndPos;
            sprayDecal.Left += movementServices.Left;
            sprayDecal.Normal += trace->HitNormal;
            sprayDecal.AccountID = (uint)player.SteamID;
            sprayDecal.Player = item.Def.Value;
            sprayDecal.TintID = item.Tint.Value;
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
            if (player.IsUseCmdBusy())
                PlayerUseCmdBlockManager[player.SteamID] = true;
            if (PlayerUseCmdManager.TryGetValue(player.SteamID, out var timer))
            {
                timer.Cancel();
                timer.Dispose();
            }
            PlayerUseCmdManager[player.SteamID] = Core.Scheduler.DelayBySeconds(
                0.1f,
                () =>
                {
                    if (PlayerUseCmdBlockManager.ContainsKey(player.SteamID))
                        PlayerUseCmdBlockManager.Remove(player.SteamID, out var _);
                    else if (player.IsValid && !player.IsUseCmdBusy())
                        player.ExecuteCommand("css_spray");
                }
            );
        }
    }

    public void OnFileChanged()
    {
        LoadPlayerInventories();
    }

    public void OnIsRequireInventoryChanged()
    {
        if (IsRequireInventory.Value)
            OnActivatePlayerHookGuid = Natives.CServerSideClientBase_ActivatePlayer.AddHook(
                OnActivatePlayer
            );
        else if (OnActivatePlayerHookGuid != null)
            Natives.CServerSideClientBase_ActivatePlayer.RemoveHook(OnActivatePlayerHookGuid.Value);
    }
}
