/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Runtime.InteropServices;
using SwiftlyS2.Shared.Natives;

namespace InventorySimulator;

// Thanks to @samyycX.
public class CCSPlayerInventory : INativeHandle
{
    public nint Address { get; set; }
    public bool IsValid => Address != 0 && SOCache.IsValid;
    public ulong SteamID => SOCache.Owner.SteamID;

    public CCSPlayerInventory(nint address)
    {
        Address = address;
    }

    public CGCClientSharedObjectCache SOCache =>
        new CGCClientSharedObjectCache(
            Marshal.ReadIntPtr(Address + Natives.CCSPlayerInventory_m_pSOCache)
        );
}

// Thanks to @samyycX.
public struct CGCClientSharedObjectCache(nint address) : INativeHandle
{
    public nint Address { get; set; } = address;
    public bool IsValid => Address != 0;

    public SOID_t Owner =>
        !IsValid
            ? throw new InvalidOperationException("Invalid cache.")
            : Marshal.PtrToStructure<SOID_t>(Address + Natives.CGCClientSharedObjectCache_m_Owner);
}

// Thanks to @samyycX.
[StructLayout(LayoutKind.Sequential)]
public readonly struct SOID_t
{
    private readonly ulong m_id;
    private readonly uint m_type;
    private readonly uint m_padding;
    public readonly ulong SteamID => m_id;
    public readonly ulong Part1 => m_id;
    public readonly ulong Part2 => m_type;
}
