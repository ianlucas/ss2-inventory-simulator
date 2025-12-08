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
    private static ISwiftlyCore? _core;

    public delegate nint CCSPlayer_ItemServices_GiveNamedItemDelegate(
        nint thisPtr,
        nint pchName,
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

    private static readonly Lazy<
        IUnmanagedFunction<CCSPlayer_ItemServices_GiveNamedItemDelegate>
    > _lazyGiveNamedItem = new(() =>
        LoadFunction<CCSPlayer_ItemServices_GiveNamedItemDelegate>(
            "CCSPlayer_ItemServices::GiveNamedItem"
        )
    );

    private static readonly Lazy<
        IUnmanagedFunction<CCSPlayerPawn_IsAbleToApplySprayDelegate>
    > _lazyIsAbleToApplySpray = new(() =>
        LoadFunction<CCSPlayerPawn_IsAbleToApplySprayDelegate>("CCSPlayerPawn::IsAbleToApplySpray")
    );

    private static readonly Lazy<
        IUnmanagedFunction<CCSPlayerController_UpdateTeamSelectionPreviewDelegate>
    > _lazyUpdateTeamSelectionPreview = new(() =>
        LoadFunction<CCSPlayerController_UpdateTeamSelectionPreviewDelegate>(
            "CCSPlayerController::UpdateTeamSelectionPreview"
        )
    );

    private static readonly Lazy<
        IUnmanagedFunction<CServerSideClientBase_SetSignonStateDelegate>
    > _lazySetSignonState = new(() =>
        LoadFunction<CServerSideClientBase_SetSignonStateDelegate>(
            "CServerSideClientBase::SetSignonState"
        )
    );

    private static readonly Lazy<
        IUnmanagedFunction<CServerSideClientBase_ConnectDelegate>
    > _lazyConnect = new(() =>
        LoadFunction<CServerSideClientBase_ConnectDelegate>("CServerSideClientBase::Connect")
    );

    public static IUnmanagedFunction<CCSPlayer_ItemServices_GiveNamedItemDelegate> CCSPlayer_ItemServices_GiveNamedItem =>
        _lazyGiveNamedItem.Value;

    public static IUnmanagedFunction<CCSPlayerPawn_IsAbleToApplySprayDelegate> CCSPlayerPawn_IsAbleToApplySpray =>
        _lazyIsAbleToApplySpray.Value;

    public static IUnmanagedFunction<CCSPlayerController_UpdateTeamSelectionPreviewDelegate> CCSPlayerController_UpdateTeamSelectionPreview =>
        _lazyUpdateTeamSelectionPreview.Value;

    public static IUnmanagedFunction<CServerSideClientBase_SetSignonStateDelegate> CServerSideClientBase_SetSignonState =>
        _lazySetSignonState.Value;

    public static IUnmanagedFunction<CServerSideClientBase_ConnectDelegate> CServerSideClientBase_Connect =>
        _lazyConnect.Value;

    private static IUnmanagedFunction<TDelegate> LoadFunction<TDelegate>(string signature)
        where TDelegate : Delegate
    {
        if (_core is null)
            throw new InvalidOperationException(
                "GameFunctions not initialized. Call Initialize() first."
            );
        nint? address = _core.GameData.GetSignature(signature);
        if (address is null)
            throw new InvalidOperationException(
                $"Failed to locate game function signature '{signature}'. The function may not exist in the current game version or the signature pattern may be outdated."
            );
        return _core.Memory.GetUnmanagedFunctionByAddress<TDelegate>(address.Value);
    }

    public static void Initialize(ISwiftlyCore core)
    {
        _core = core;
    }
}

public static class CCSPlayerPawnExtensions
{
    public static bool IsAbleToApplySpray(this CCSPlayerPawn pawn, IntPtr ptr = 0)
    {
        return GameFunctions.CCSPlayerPawn_IsAbleToApplySpray.Call(pawn.Address, ptr, 0, 0)
            == nint.Zero;
    }
}
