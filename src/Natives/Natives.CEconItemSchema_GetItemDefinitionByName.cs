/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using SwiftlyS2.Shared.Memory;

namespace InventorySimulator;

public static partial class Natives
{
    public delegate nint CEconItemSchema_GetItemDefinitionByNameDelegate(
        nint thisPtr,
        nint pchName
    );

    private static readonly Lazy<
        IUnmanagedFunction<CEconItemSchema_GetItemDefinitionByNameDelegate>
    > _lazyGetItemDefinitionByName = new(() =>
        FromSignature<CEconItemSchema_GetItemDefinitionByNameDelegate>(
            "CEconItemSchema::GetItemDefinitionByName"
        )
    );

    public static IUnmanagedFunction<CEconItemSchema_GetItemDefinitionByNameDelegate> CEconItemSchema_GetItemDefinitionByName =>
        _lazyGetItemDefinitionByName.Value;
}
