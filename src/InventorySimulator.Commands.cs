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
        var url = UrlHelper.FormatUrl(ConVars.WsUrlPrintFormat.Value, ConVars.Url.Value);
        player?.SendChat(Core.Localizer["invsim.announce", url]);
        if (!ConVars.IsWsEnabled.Value || player == null)
            return;
        var controllerState = player.Controller.State;
        var cooldown = ConVars.WsCooldown.Value;
        var diff = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - controllerState.WsCooldown;
        if (diff < cooldown)
        {
            player.SendChat(Core.Localizer["invsim.ws_cooldown", cooldown - diff]);
            return;
        }
        if (controllerState.IsFetching)
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
        if (player != null && ConVars.IsSprayEnabled.Value)
        {
            var controllerState = player.Controller.State;
            var cooldown = ConVars.SprayCooldown.Value;
            var diff = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - controllerState.SprayCooldown;
            if (diff < cooldown)
            {
                player.SendChat(Core.Localizer["invsim.spray_cooldown", cooldown - diff]);
                return;
            }
            SprayPlayerGraffiti(player);
        }
    }

    [Command("wslogin")]
    public void OnWsloginCommand(ICommandContext context)
    {
        var player = context.Sender;
        if (ConVars.IsWsLogin.Value && Api.HasApiKey() && player != null)
        {
            var controllerState = player.Controller.State;
            player.SendChat(Core.Localizer["invsim.login_in_progress"]);
            if (controllerState.IsAuthenticating)
                return;
            SendSignIn(player.SteamID);
        }
    }
}
