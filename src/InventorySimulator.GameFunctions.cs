/*---------------------------------------------------------------------------------------------
*  Copyright (c) Ian Lucas. All rights reserved.
*  Licensed under the MIT License. See License.txt in the project root for license information.
*--------------------------------------------------------------------------------------------*/

using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Memory;
using SwiftlyS2.Shared.SchemaDefinitions;

namespace InventorySimulator;

public static class GameFunctions
{
    public delegate nint CCSPlayer_ItemServices_GiveNamedItemDelegate(
        nint thisPtr,
        nint itemName,
        int subType,
        nint itemDef,
        byte forceGive,
        nint position
    );

    public delegate nint CCSPlayerPawn_IsAbleToApplySprayDelegate(
        nint thisPtr,
        nint traceResultOut,
        nint sprayPosOut,
        nint eyePosOut
    );

    public delegate long CCSPlayerController_UpdateTeamSelectionPreviewDelegate(
        nint thisPtr,
        int teamIndex
    );

    public delegate byte CServerSideClientBase_SetSignonStateDelegate(
        nint thisPtr,
        uint newSignonState
    );

    public delegate byte CServerSideClientBase_ConnectDelegate(
        nint thisPtr,
        nint unknown,
        nint playerName,
        ushort userId,
        nint netChannel,
        byte connectionTypeFlags,
        int challengeNr
    );

    public static IUnmanagedFunction<CCSPlayer_ItemServices_GiveNamedItemDelegate>? CCSPlayer_ItemServices_GiveNamedItem;

    public static IUnmanagedFunction<CCSPlayerPawn_IsAbleToApplySprayDelegate>? CCSPlayerPawn_IsAbleToApplySpray;

    public static IUnmanagedFunction<CCSPlayerController_UpdateTeamSelectionPreviewDelegate>? CCSPlayerController_UpdateTeamSelectionPreview;

    public static IUnmanagedFunction<CServerSideClientBase_SetSignonStateDelegate>? CServerSideClientBase_SetSignonState;

    public static IUnmanagedFunction<CServerSideClientBase_ConnectDelegate>? CServerSideClientBase_Connect;

    public static IUnmanagedFunction<TDelegate> LoadFunction<TDelegate>(
        ISwiftlyCore core,
        string signature
    )
        where TDelegate : Delegate
    {
        nint? address = core.GameData.GetSignature(signature);
        if (address is null)
            throw new Exception($"Failed to read {signature} function!");
        return core.Memory.GetUnmanagedFunctionByAddress<TDelegate>(address.Value);
    }

    public static void Initialize(ISwiftlyCore core)
    {
        CCSPlayer_ItemServices_GiveNamedItem =
            LoadFunction<CCSPlayer_ItemServices_GiveNamedItemDelegate>(
                core,
                "CCSPlayer_ItemServices::GiveNamedItem"
            );
        CCSPlayerPawn_IsAbleToApplySpray = LoadFunction<CCSPlayerPawn_IsAbleToApplySprayDelegate>(
            core,
            "CCSPlayerPawn::IsAbleToApplySpray"
        );
        CCSPlayerController_UpdateTeamSelectionPreview =
            LoadFunction<CCSPlayerController_UpdateTeamSelectionPreviewDelegate>(
                core,
                "CCSPlayerController::UpdateSelectTeamPreview"
            );
        CCSPlayerController_UpdateTeamSelectionPreview =
            LoadFunction<CCSPlayerController_UpdateTeamSelectionPreviewDelegate>(
                core,
                "CCSPlayerController::UpdateSelectTeamPreview"
            );
        CServerSideClientBase_SetSignonState =
            LoadFunction<CServerSideClientBase_SetSignonStateDelegate>(
                core,
                "CServerSideClientBase::SetSignonState"
            );
        CServerSideClientBase_Connect = LoadFunction<CServerSideClientBase_ConnectDelegate>(
            core,
            "CServerSideClientBase::Connect"
        );
    }
}

public static class CCSPlayerPawnExtensions
{
    public static nint IsAbleToApplySpray(this CCSPlayerPawn pawn, IntPtr ptr = 0)
    {
        return GameFunctions.CCSPlayerPawn_IsAbleToApplySpray.Call(pawn.Address, ptr, 0, 0);
    }
}
