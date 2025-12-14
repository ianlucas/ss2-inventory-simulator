/*---------------------------------------------------------------------------------------------
*  Copyright (c) Ian Lucas. All rights reserved.
*  Licensed under the MIT License. See License.txt in the project root for license information.
*--------------------------------------------------------------------------------------------*/

using System.Runtime.InteropServices;
using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Memory;
using SwiftlyS2.Shared.SchemaDefinitions;
using SwiftlyS2.Shared.Schemas;

namespace InventorySimulator;

public static class Natives
{
    private static ISwiftlyCore? _core;

    public const int CEconItemView_Size = 1024;

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

    public delegate void CServerSideClientBase_ActivatePlayerDelegate(nint thisPtr);

    public delegate nint CCSPlayerInventory_GetItemInLoadoutDelegate(
        nint thisPtr,
        int iTeam,
        int iSlot
    );

    public delegate nint CEconItemView_ConstructorDelegate(nint thisPtr);

    public delegate nint CEconItemView_OperatorEqualsDelegate(nint thisPtr, nint other);

    public delegate nint CCSPlayerInventory_ResetDelegate(nint thisPtr);

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

    private static readonly Lazy<
        IUnmanagedFunction<CEconItemView_ConstructorDelegate>
    > _lazyEconItemViewConstructor = new(() =>
        FromSignature<CEconItemView_ConstructorDelegate>("CEconItemView::CEconItemView")
    );

    private static readonly Lazy<
        IUnmanagedFunction<CEconItemView_OperatorEqualsDelegate>
    > _lazyEconItemViewOperatorEquals = new(() =>
        FromSignature<CEconItemView_OperatorEqualsDelegate>("CEconItemView::operator=")
    );

    private static readonly Lazy<
        IUnmanagedFunction<CCSPlayerInventory_ResetDelegate>
    > _lazyPlayerInventoryReset = new(() =>
        FromSignature<CCSPlayerInventory_ResetDelegate>("CCSPlayerInventory::Reset")
    );

    public static IUnmanagedFunction<CCSPlayer_ItemServices_GiveNamedItemDelegate> CCSPlayer_ItemServices_GiveNamedItem =>
        _lazyGiveNamedItem.Value;

    public static IUnmanagedFunction<CCSPlayerPawn_IsAbleToApplySprayDelegate> CCSPlayerPawn_IsAbleToApplySpray =>
        _lazyIsAbleToApplySpray.Value;

    public static IUnmanagedFunction<CServerSideClientBase_ActivatePlayerDelegate> CServerSideClientBase_ActivatePlayer =>
        _lazyActivatePlayer.Value;

    public static IUnmanagedFunction<CCSPlayerInventory_GetItemInLoadoutDelegate> CCSPlayerInventory_GetItemInLoadout =>
        _lazyGetItemInLoadout.Value;

    public static IUnmanagedFunction<CEconItemView_ConstructorDelegate> CEconItemView_Constructor =>
        _lazyEconItemViewConstructor.Value;

    public static IUnmanagedFunction<CEconItemView_OperatorEqualsDelegate> CEconItemView_OperatorEquals =>
        _lazyEconItemViewOperatorEquals.Value;

    public static IUnmanagedFunction<CCSPlayerInventory_ResetDelegate> CCSPlayerInventory_Reset =>
        _lazyPlayerInventoryReset.Value;

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

    public static T ToSchemaClass<T>(nint address)
        where T : class, ISchemaClass<T>
    {
        if (_core is null)
            throw new InvalidOperationException(
                "Natives not initialized. Call Initialize() first."
            );
        return _core.Memory.ToSchemaClass<T>(address);
    }

    public static void Initialize(ISwiftlyCore core)
    {
        _core = core;
    }

    public static nint CreateEconItemView(nint copyFrom = 0)
    {
        var ptr = Marshal.AllocHGlobal(CEconItemView_Size);
        CEconItemView_Constructor.Call(ptr);
        if (copyFrom != 0)
            CEconItemView_OperatorEquals.Call(ptr, copyFrom);
        return ptr;
    }

    public static void FreeMemory(nint ptr)
    {
        if (ptr != 0)
            Marshal.FreeHGlobal(ptr);
    }
}

public static class CCSPlayerPawnExtensions
{
    public static bool IsAbleToApplySpray(this CCSPlayerPawn pawn, IntPtr ptr = 0)
    {
        return Natives.CCSPlayerPawn_IsAbleToApplySpray.Call(pawn.Address, ptr, 0, 0) == nint.Zero;
    }
}

public static class CCSPlayerController_InventoryServicesExtensions
{
    public static CCSPlayerController? GetController(this CCSPlayer_ItemServices itemServices)
    {
        var pawn = itemServices.Pawn;
        return
            pawn != null && pawn.IsValid && pawn.Controller.IsValid && pawn.Controller.Value != null
            ? pawn.Controller.Value.As<CCSPlayerController>()
            : null;
    }

    public static CCSPlayerInventory GetInventory(
        this CCSPlayerController_InventoryServices itemServices
    )
    {
        return new CCSPlayerInventory(
            itemServices.Address + Natives.CCSPlayerController_InventoryServices_m_pInventory
        );
    }
}
