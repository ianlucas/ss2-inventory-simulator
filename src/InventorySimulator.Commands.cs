/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using SwiftlyS2.Shared.Commands;

namespace InventorySimulator;

public partial class InventorySimulator
{
    [Command("wslogin")]
    public void OnWsloginCommand(ICommandContext context)
    {
        var player = context.Sender;
        if (IsWsLogin.Value && Api.HasApiKey() && player != null)
        {
            player.SendChat(Core.Localizer["invsim.login_in_progress"]);
            if (PlayerInAuthManager.ContainsKey(player.SteamID))
                return;
            SendSignIn(player.SteamID);
        }
    }
}
