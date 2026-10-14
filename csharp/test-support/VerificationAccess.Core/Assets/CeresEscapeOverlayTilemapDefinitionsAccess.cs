using SuperMetroid.Core.Assets;

/// <summary>Verification access to <see cref="CeresEscapeOverlayTilemapDefinitions"/> members production does not use.</summary>
internal static class CeresEscapeOverlayTilemapDefinitionsAccess
{
    /// <summary>Provides range checks against all authored Ceres escape overlay pages.</summary>
    extension(CeresEscapeOverlayTilemapDefinitions)
    {
        /// <summary>Checks whether a byte falls within any page's source-word interval.</summary>
        internal static bool ContainsByteAddress(int address)
        {
            foreach (CeresEscapeOverlayTilemapDefinition page in CeresEscapeOverlayTilemapDefinitions.All)
                if (address >= page.SourceAddress &&
                    address < page.SourceAddress + page.WordCount * sizeof(ushort))
                    return true;
            return false;
        }
    }
}
