/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using SwiftlyS2.Shared.Memory;

namespace InventorySimulator;

public static partial class Natives
{
    public delegate nint CEconItemSchema_GetItemDefinitionDelegate(
        nint thisPtr,
        uint defIndex,
        byte flag
    );

    private static readonly Lazy<
        IUnmanagedFunction<CEconItemSchema_GetItemDefinitionDelegate>
    > _lazyGetItemDefinition = new(() =>
        FromSignature<CEconItemSchema_GetItemDefinitionDelegate>(
            "CEconItemSchema::GetItemDefinition"
        )
    );

    public static IUnmanagedFunction<CEconItemSchema_GetItemDefinitionDelegate> CEconItemSchema_GetItemDefinition =>
        _lazyGetItemDefinition.Value;
}
