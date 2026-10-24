namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>How far the bounded SpaceTime palette-copy overrun runs, and from where it reads.</summary>
/// <remarks>
/// With direct-page pointer zero, inherited animation Y=$912F enters the signed
/// $90:AD12 loop at $9131. Subsequent animation records start later in this same
/// window. Y advances by two through $FFFF, then wraps into live low WRAM and
/// exits at $0021. The window's bytes are bank-$80 program observed through bank $00;
/// the import pipeline reads them from the player's cartridge into <see cref="SamusOverreadCatalog"/>.
/// </remarks>
internal static class SpacetimeBeamCopyDefinitions
{
    /// <summary>
    /// The direct-page long pointer <c>$00-$02</c> that <c>LDA [$00],Y</c> consumes: direct-page scratch the port does
    /// not model. It has always read zero in the port, as the retail probe observed (<c>$00:0000</c>), so the inherited Y
    /// is the source's place in bank $00.
    /// </summary>
    internal const int SourceLongPointerValue = 0;
    /// <summary>
    /// The bytes from sprite palette six, where the interrupted <c>STA.l $7EC1C0,X</c> at $90:AD14 starts the copy, to the
    /// event bitset that starts the persistent-progress slice of the SRAM mirror. The inherited X runs the copy past the
    /// palette; only the bytes landing in that slice are kept (owner decision on #1318).
    /// </summary>
    internal const int ProgressionSliceDistance = 0x1660;
    /// <summary>$90:AD1C compares inherited Y against $0020 after incrementing it.</summary>
    internal const ushort PaletteByteCount = 0x0020;
}
