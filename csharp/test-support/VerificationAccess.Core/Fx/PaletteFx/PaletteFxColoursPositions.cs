namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>
/// Verification access: the palette-FX target a native SetColorIndex operand names (#1405), for checks that compare a
/// program with its bank-$8D list. The background sets are the colour memory's first half and the sprite sets its
/// second, except where Samus's suit and beams are.
/// </summary>
internal static class PaletteFxColoursPositions
{
    /// <summary>The target whose first colour is the cartridge's colour <paramref name="position"/>.</summary>
    internal static PaletteFxColours FromCartridge(int position) => position switch
    {
        < 128 => new(PaletteFxColourOwner.Background, position),
        >= 192 and < 208 => new(PaletteFxColourOwner.SamusSuit, position - 192),
        >= 224 and < 240 => new(PaletteFxColourOwner.SamusBeams, position - 224),
        >= 128 and < 256 when position != 223 => new(PaletteFxColourOwner.Sprite, position - 128),
        _ => throw new ArgumentOutOfRangeException(nameof(position), position, "No palette-FX target starts at this colour."),
    };
}
