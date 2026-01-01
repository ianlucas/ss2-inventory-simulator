/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using SwiftlyS2.Shared.Memory;

namespace InventorySimulator;

public static partial class Natives
{
    private static IUnmanagedFunction<TDelegate> FromSignature<TDelegate>(string signatureName)
        where TDelegate : Delegate
    {
        nint? address = Swiftly.Core.GameData.GetSignature(signatureName);
        if (address is null)
            throw new InvalidOperationException(
                $"Failed to locate game function signature '{signatureName}'. The function may not exist in the current game version or the signature pattern may be outdated."
            );
        return Swiftly.Core.Memory.GetUnmanagedFunctionByAddress<TDelegate>(address.Value);
    }

    private static int FromOffset(string offset)
    {
        return Swiftly.Core.GameData.GetOffset(offset);
    }
}
