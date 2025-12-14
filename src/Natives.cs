/*---------------------------------------------------------------------------------------------
*  Copyright (c) Ian Lucas. All rights reserved.
*  Licensed under the MIT License. See License.txt in the project root for license information.
*--------------------------------------------------------------------------------------------*/

using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Memory;
using SwiftlyS2.Shared.SchemaDefinitions;

namespace InventorySimulator;

public static class Natives
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

    public delegate void CServerSideClientBase_ActivatePlayerDelegate(nint thisPtr);

    public delegate nint CCSPlayerInventory_GetItemInLoadoutDelegate(
        nint pInventory,
        int team,
        int slot
    );

    private static readonly Lazy<
        IUnmanagedFunction<CCSPlayer_ItemServices_GiveNamedItemDelegate>
    > _lazyGiveNamedItem = new(() =>
        FromSignature<CCSPlayer_ItemServices_GiveNamedItemDelegate>(
            "CCSPlayer_ItemServices::GiveNamedItem"
        )
    );

    private static readonly Lazy<
        IUnmanagedFunction<CCSPlayerPawn_IsAbleToApplySprayDelegate>
    > _lazyIsAbleToApplySpray = new(() =>
        FromSignature<CCSPlayerPawn_IsAbleToApplySprayDelegate>("CCSPlayerPawn::IsAbleToApplySpray")
    );

    private static readonly Lazy<
        IUnmanagedFunction<CCSPlayerController_UpdateTeamSelectionPreviewDelegate>
    > _lazyUpdateTeamSelectionPreview = new(() =>
        FromSignature<CCSPlayerController_UpdateTeamSelectionPreviewDelegate>(
            "CCSPlayerController::UpdateTeamSelectionPreview"
        )
    );

    private static readonly Lazy<
        IUnmanagedFunction<CServerSideClientBase_ActivatePlayerDelegate>
    > _lazyActivatePlayer = new(() =>
        FromSignature<CServerSideClientBase_ActivatePlayerDelegate>(
            "CServerSideClientBase::ActivatePlayer"
        )
    );

    private static readonly Lazy<
        IUnmanagedFunction<CCSPlayerInventory_GetItemInLoadoutDelegate>
    > _lazyGetItemInLoadout = new(() =>
        FromSignature<CCSPlayerInventory_GetItemInLoadoutDelegate>(
            "CCSPlayerInventory::GetItemInLoadout"
        )
    );

    public static IUnmanagedFunction<CCSPlayer_ItemServices_GiveNamedItemDelegate> CCSPlayer_ItemServices_GiveNamedItem =>
        _lazyGiveNamedItem.Value;

    public static IUnmanagedFunction<CCSPlayerPawn_IsAbleToApplySprayDelegate> CCSPlayerPawn_IsAbleToApplySpray =>
        _lazyIsAbleToApplySpray.Value;

    public static IUnmanagedFunction<CCSPlayerController_UpdateTeamSelectionPreviewDelegate> CCSPlayerController_UpdateTeamSelectionPreview =>
        _lazyUpdateTeamSelectionPreview.Value;

    public static IUnmanagedFunction<CServerSideClientBase_ActivatePlayerDelegate> CServerSideClientBase_ActivatePlayer =>
        _lazyActivatePlayer.Value;

    public static IUnmanagedFunction<CCSPlayerInventory_GetItemInLoadoutDelegate> CCSPlayerInventory_GetItemInLoadout =>
        _lazyGetItemInLoadout.Value;

    public static int CCSPlayerInventory_m_pSOCache =>
        new Lazy<int>(() => FromOffset("CCSPlayerInventory::m_pSOCache")).Value;

    public static int CGCClientSharedObjectCache_m_Owner =>
        new Lazy<int>(() => FromOffset("CGCClientSharedObjectCache::m_Owner")).Value;

    public static int CCSPlayerController_InventoryServices_m_pInventory =>
        new Lazy<int>(() =>
            FromOffset("CCSPlayerController_InventoryServices::m_pInventory")
        ).Value;

    private static IUnmanagedFunction<TDelegate> FromSignature<TDelegate>(string signature)
        where TDelegate : Delegate
    {
        if (_core is null)
            throw new InvalidOperationException(
                "Natives not initialized. Call Initialize() first."
            );
        nint? address = _core.GameData.GetSignature(signature);
        if (address is null)
            throw new InvalidOperationException(
                $"Failed to locate game function signature '{signature}'. The function may not exist in the current game version or the signature pattern may be outdated."
            );
        return _core.Memory.GetUnmanagedFunctionByAddress<TDelegate>(address.Value);
    }

    private static int FromOffset(string offset)
    {
        if (_core is null)
            throw new InvalidOperationException(
                "Natives not initialized. Call Initialize() first."
            );
        return _core.GameData.GetOffset(offset);
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
        return Natives.CCSPlayerPawn_IsAbleToApplySpray.Call(pawn.Address, ptr, 0, 0) == nint.Zero;
    }
}
