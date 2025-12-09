/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using SwiftlyS2.Shared.Commands;

namespace InventorySimulator;

public partial class InventorySimulator
{
    [Command("ws")]
    public void OnWSCommand(ICommandContext context)
    {
        var player = context.Sender;
        var url = Utilities.FormatUrl(WsUrlPrintFormat.Value, Url.Value);
        player?.SendChat(Core.Localizer["invsim.announce", url]);
        if (!IsWsEnabled.Value || player == null)
            return;
        if (PlayerCooldownManager.TryGetValue(player.SteamID, out var timestamp))
        {
            var cooldown = WsCooldown.Value;
            var diff = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - timestamp;
            if (diff < cooldown)
            {
                player.SendChat(Core.Localizer["invsim.ws_cooldown", cooldown - diff]);
                return;
            }
        }
        if (FetchingPlayerInventory.ContainsKey(player.SteamID))
        {
            player.SendChat(Core.Localizer["invsim.ws_in_progress"]);
            return;
        }
        RefreshPlayerInventory(player, true);
        player.SendChat(Core.Localizer["invsim.ws_new"]);
    }

    [Command("spray")]
    public void OnSprayCommand(ICommandContext context)
    {
        var player = context.Sender;
        if (player != null && IsSprayEnabled.Value)
        {
            if (PlayerSprayCooldownManager.TryGetValue(player.SteamID, out var timestamp))
            {
                var cooldown = SprayCooldown.Value;
                var diff = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - timestamp;
                if (diff < cooldown)
                {
                    player.SendChat(Core.Localizer["invsim.spray_cooldown", cooldown - diff]);
                    return;
                }
            }
            SprayPlayerGraffiti(player);
        }
    }

    [Command("wslogin")]
    public void OnWsloginCommand(ICommandContext context)
    {
        var player = context.Sender;
        if (ApiKey.Value != "" && IsWsLogin.Value && player != null)
        {
            player.SendChat(Core.Localizer["invsim.login_in_progress"]);
            if (AuthenticatingPlayer.ContainsKey(player.SteamID))
                return;
            SendSignIn(player.SteamID);
        }
    }
}
