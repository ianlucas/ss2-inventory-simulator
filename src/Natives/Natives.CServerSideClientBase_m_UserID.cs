/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

namespace InventorySimulator;

public static partial class Natives
{
    public static int CServerSideClientBase_m_UserID =>
        new Lazy<int>(() => FromOffset("CServerSideClientBase::m_UserID")).Value;
}
