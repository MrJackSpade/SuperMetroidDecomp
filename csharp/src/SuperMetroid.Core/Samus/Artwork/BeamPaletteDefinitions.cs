using SuperMetroid.Core.Samus.Projectiles;


namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Presentation geometry for the sixteen colors written by $90:ACCD.</summary>
/// <remarks>
/// The stock color source at <c>$90:C3E1..C480</c> is five contiguous
/// sixteen-word little-endian BGR555 rows: Power, Ice, Wave, Plasma, and
/// Spazer. Row <c>r = 0..4</c>, color <c>c = 0..15</c> is at
/// <c>$C3E1 + $20*r + 2*c</c>; the twelve beam combinations select rows
/// through the beam palette pointer table at <c>$90:C3C9</c>.
/// All 80 words match the pinned NTSC J/U v1.0 ROM and native listing.
/// Every row begins <c>$3800,$7FFF</c>; Power, Wave, Plasma, and Spazer
/// have six zero words at color indices 9..14, while Ice has none. The
/// selected material paint is owned by BeamPaintDefinitions; its exact channel
/// composition calculates all stock words and preserves independent replacements.
/// Native palette loading copies exactly sixteen colors from the selected
/// pointer; out-of-range beam selections are physical pointer-table reads,
/// not extra color rows. Investigation: #625 / #901.
/// </remarks>
public static class BeamPaletteDefinitions
{
    /// <summary>Asset filename for the ordinary beam-combination RGB5 palettes.</summary>
    public const string FileName = "beam-palettes.json";
    /// <summary>Supported revision of the twelve-selection beam-palette JSON schema.</summary>
    public const int Version = 1;
    /// <summary>Sixteen colors copied by the native beam loader, covering all entries of OBJ palette 6, including color zero.</summary>
    public const int ColorCount = 16;
    /// <summary>$90:C3B1 contains twelve legal beam-combination pointers before palette data: one palette per retail combination.</summary>
    public const int SelectionCount = 12;
}
