/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Runtime.InteropServices;
using SwiftlyS2.Shared.Natives;
using SwiftlyS2.Shared.SchemaDefinitions;

namespace InventorySimulator;

// Thanks @Kxnrl.
public class CEconItemDefinition : INativeHandle
{
    public nint Address { get; set; }
    public bool IsValid => Address != 0;

    public CEconItemDefinition(nint address)
    {
        Address = address;
    }

    public ushort DefIndex => (ushort)Marshal.ReadInt16(Address + 0x10);

    public byte ItemRarity => Marshal.ReadByte(Address + 0x42);

    public byte ItemQuality => Marshal.ReadByte(Address + 0x43);

    public string? ItemBaseName
    {
        get
        {
            var ptr = Marshal.ReadIntPtr(Address + 0x70);
            return ptr != 0 ? Marshal.PtrToStringUTF8(ptr) : null;
        }
    }

    public string? ItemTypeName
    {
        get
        {
            var ptr = Marshal.ReadIntPtr(Address + 0x80);
            return ptr != 0 ? Marshal.PtrToStringUTF8(ptr) : null;
        }
    }

    public string? BaseDisplayModel
    {
        get
        {
            var ptr = Marshal.ReadIntPtr(Address + 0x148);
            return ptr != 0 ? Marshal.PtrToStringUTF8(ptr) : null;
        }
    }

    public string? WorldDisplayModel
    {
        get
        {
            var ptr = Marshal.ReadIntPtr(Address + 0x158);
            return ptr != 0 ? Marshal.PtrToStringUTF8(ptr) : null;
        }
    }

    public uint StickerSlots => (uint)Marshal.ReadInt32(Address + 0x168);

    public uint ItemType => (uint)Marshal.ReadInt32(Address + 0x1B8);

    public string? ItemClassname
    {
        get
        {
            var ptr = Marshal.ReadIntPtr(Address + 0x248);
            return ptr != 0 ? Marshal.PtrToStringUTF8(ptr) : null;
        }
    }

    public string? DefinitionName
    {
        get
        {
            var ptr = Marshal.ReadIntPtr(Address + 0x260);
            return ptr != 0 ? Marshal.PtrToStringUTF8(ptr) : null;
        }
    }

    public loadout_slot_t DefaultLoadoutSlot => (loadout_slot_t)Marshal.ReadInt32(Address + 0x338);
}
