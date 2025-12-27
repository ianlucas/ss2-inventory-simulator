/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using SwiftlyS2.Shared.Natives;
using SwiftlyS2.Shared.Players;
using SwiftlyS2.Shared.SchemaDefinitions;
using SwiftlyS2.Shared.SteamAPI;

namespace InventorySimulator;

public partial class InventorySimulator
{
    public void GivePlayerAgent(IPlayer player)
    {
        if (MinModels.Value > 0)
        {
            if (player.Controller.Team == Team.T)
                player.PlayerPawn?.SetModel("characters/models/tm_phoenix/tm_phoenix.vmdl");
            if (player.Controller.Team == Team.CT)
                player.PlayerPawn?.SetModel("characters/models/ctm_sas/ctm_sas.vmdl");
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
            var model = pawn.CBodyComponent?.SceneNode?.GetSkeletonInstance()?.ModelState.ModelName;
            if (!string.IsNullOrEmpty(model))
            {
                pawn.SetModel("characters/models/tm_jumpsuit/tm_jumpsuit_varianta.vmdl");
                pawn.SetModel(model);
            }
            var glove = pawn.EconGloves;
            Core.Scheduler.NextTick(() =>
            {
                if (pawn.IsValid)
                {
                    glove.ApplyAttributes(item);
                    // Thanks to xstage and stefanx111
                    pawn.AcceptInput("SetBodygroup", value: "default_gloves,1");
                }
            });
        }
    }

    public void GivePlayerWeaponSkin(
        CCSPlayerController controller,
        CBasePlayerWeapon weapon,
        bool isMelee
    )
    {
        var isFallbackTeam = IsFallbackTeam.Value;
        if (controller?.SteamID != 0 && controller?.InventoryServices?.IsValid == true)
        {
            var inventory = GetPlayerInventoryBySteamID(controller.SteamID);
            var item = isMelee
                ? inventory.GetKnife(controller.TeamNum, isFallbackTeam)
                : inventory.GetWeapon(
                    controller.TeamNum,
                    weapon.AttributeManager.Item.ItemDefinitionIndex,
                    isFallbackTeam
                );
            if (item != null)
                weapon.AttributeManager.Item.ApplyAttributes(item, weapon, controller);
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
        if (item == null)
            return;
        item.Stattrak += 1;
        var statTrak = TypeHelper.ViewAs<int, float>(item.Stattrak);
        weapon.AttributeManager.Item.NetworkedDynamicAttributes.SetOrAddAttribute(
            "kill eater",
            statTrak
        );
        SendStatTrakIncrement(player.SteamID, item.Uid);
    }

    public void GiveOnPlayerSpawn(IPlayer player)
    {
        var inventory = GetPlayerInventory(player);
        GivePlayerAgent(player);
        GivePlayerGloves(player, inventory);
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
