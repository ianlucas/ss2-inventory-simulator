/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using SwiftlyS2.Shared.Memory;

namespace InventorySimulator;

public static partial class Natives
{
    public delegate nint GetItemSchemaDelegate();

    private static readonly Lazy<IUnmanagedFunction<GetItemSchemaDelegate>> _lazyGetItemSchema =
        new(() => FromSignature<GetItemSchemaDelegate>("GetItemSchema"));

    public static IUnmanagedFunction<GetItemSchemaDelegate> GetItemSchema =>
        _lazyGetItemSchema.Value;
}
