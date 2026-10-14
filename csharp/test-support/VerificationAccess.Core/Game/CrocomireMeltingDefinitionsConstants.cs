using SuperMetroid.Core.Game;

/// <summary>Cartridge values of <see cref="CrocomireMeltingDefinitions"/> that only verification reads.</summary>
internal static class CrocomireMeltingDefinitionsConstants
{
    /// <summary>
    /// <c>TilePixelColumnBitmasks</c> at <c>$A4:9BBD-$A4:9BC4</c>. Native indexes
    /// these by chronological table cursor rather than selected X column.
    /// </summary>
    /// <remarks>
    /// Exact bounded rule for cursor 0..48:
    /// <c>$FF XOR ($80 &gt;&gt; (cursor &amp; 7))</c>. All eight physical bytes
    /// match the pinned NTSC J/U v1.0 ROM; the rule reproduces all 49 cursor
    /// results and the complete production erase silhouette. The cartridge
    /// selects the mask by chronological cursor, not by the separately selected
    /// X column. This proof covers only the masks, not that column permutation.
    /// Independently reviewed for #1165 against all original NTSC bytes and pinned
    /// bank_A4.asm. Implement the exact bit-clear rule; keep cursor bounds and the
    /// native chronological-index bug. No column permutation is inferred.
    /// </remarks>
    public const int MaskReferenceAddress = 0xa49bbd;

    /// <summary>Provides the native mask reference address used to check Crocomire's melting presentation.</summary>
    extension(CrocomireMeltingDefinitions)
    {
        /// <inheritdoc cref="CrocomireMeltingDefinitionsConstants.MaskReferenceAddress"/>
        internal static int MaskReferenceAddress => CrocomireMeltingDefinitionsConstants.MaskReferenceAddress;
    }
}
