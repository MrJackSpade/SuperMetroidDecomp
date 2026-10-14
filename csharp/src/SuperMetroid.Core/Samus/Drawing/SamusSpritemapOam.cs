using SuperMetroid.Core.Samus.Artwork;

namespace SuperMetroid.Core.Samus.Drawing;

/// <summary>Stages Samus's installed spritemaps into the OAM buffer.</summary>
public static class SamusSpritemapOam
{
    /// <summary>
    /// Ports <c>AddSamusSpritemapToOAM</c> at <c>$81:89AE</c>.
    /// </summary>
    /// <remarks>
    /// This looks tantalizingly similar to <see cref="OamBuffer.AddOnScreenSpritePart"/>, but the
    /// differences are part of the game data contract. The input is an index into the
    /// bank-$92 pointer table at <c>$92:808D</c>, not a direct address. More importantly,
    /// Samus entries keep their ROM-authored palette bits and do not use the generic
    /// routine's vertical-wrap hiding rule. Keeping a distinct method prevents a future
    /// cleanup from quietly changing the native draw behavior.
    /// </remarks>
    /// <param name="oam">The OAM staging buffer receiving Samus's parts.</param>
    /// <param name="memory">Mutable memory for the native zero-pointer WRAM case.</param>
    /// <param name="spritemapIndex">Word index into <c>$92:808D</c>.</param>
    /// <param name="origin">Screen-space origin produced by bank $90.</param>
    /// <param name="artwork">Required installed Samus compositions indexed by the native pointer-table ordinal; a zero-pointer catalog entry selects the mutable WRAM spritemap case rather than an empty image.</param>
    public static void AddSamusSpritemap(
        this OamBuffer oam,
        ISnesMutableMemory memory,
        ushort spritemapIndex,
        Position16<OamPixels> origin,
        SamusSpritemapArtworkCatalog artwork)
    {
        Ensure.NotNull(oam);
        Ensure.NotNull(memory);
        Ensure.NotNull(artwork);

        if (artwork.TryGet(spritemapIndex, out SamusSpritemapDefinition? definition))
        {
            foreach (SamusSpritePart part in definition.Parts)
                oam.AddSamusSpritePart(new SnesSpritemapXWord(part.X), part.Y,
                    part.Attributes, origin);
            return;
        }

        // A zero entry in the installed pointer table is not an empty picture: the
        // native bank-local dereference lands in the WRAM mirror at $92:0000.
        // Read that mutable state explicitly. No cartridge address is reachable here.
        foreach (SamusZeroSpritemapEntry entry in SamusOverreadMemory.ZeroPointerSpritemap(memory))
            oam.AddSamusSpritePart(entry.EncodedX, entry.EncodedY, entry.Attributes, origin);
    }
}
