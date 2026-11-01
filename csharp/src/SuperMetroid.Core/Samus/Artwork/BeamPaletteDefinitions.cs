namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Presentation geometry for the sixteen colors the beam palette load writes.</summary>
/// <remarks>
/// The stock color source is five contiguous
/// sixteen-word little-endian BGR555 rows: Power, Ice, Wave, Plasma, and
/// Spazer. Row <c>r = 0..4</c>, color <c>c = 0..15</c> is at
/// offset <c>32*r + 2*c</c> from the first row; the twelve beam combinations select rows
/// through the beam palette pointer table.
/// All 80 words match the original data.
/// Every row begins 14336, 32767; Power, Wave, Plasma, and Spazer
/// have six zero words at color indices 9..14, while Ice has none. The
/// selected material paint is owned by BeamPaintDefinitions; its exact channel
/// composition calculates all stock words and preserves independent replacements.
/// The palette load copies exactly sixteen colors from the selected
/// pointer; out-of-range beam selections are physical pointer-table reads,
/// not extra color rows. Investigation: #625 / #901.
/// </remarks>
public static class BeamPaletteDefinitions
{
    /// <summary>Asset filename for the ordinary beam-combination RGB5 palettes.</summary>
    public const string FileName = "beam-palettes.json";
    /// <summary>Supported revision of the twelve-selection beam-palette JSON schema.</summary>
    public const int Version = 1;
    /// <summary>Sixteen colors copied by the beam loader, covering all entries of OBJ palette 6, including color zero.</summary>
    public const int ColorCount = 16;
    /// <summary>Twelve legal beam-combination pointers precede the palette data: one palette per retail combination.</summary>
    public const int SelectionCount = 12;
}
