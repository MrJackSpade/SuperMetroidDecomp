using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.Animals;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>
/// Editable default, speed, and shine RGB5 images for Dachora. Native speed/shine
/// timers and their frame-index arithmetic remain compiled enemy behavior.
/// </summary>
public sealed class DachoraColorCatalog
{
    /// <summary>Canonical selected RGB5 colors and ordered rows, independent of JSON encoding.</summary>
    public string ContentIdentity => DachoraColors.ContentIdentity(this);

    /// <summary>The sixteen supplied normal colors used as the basis of both animated phases.</summary>
    internal readonly RgbColor[] normal;
    /// <summary>Speed-frame colors that differ from the normal palette's calculated blue ramp.</summary>
    internal readonly Dictionary<ColourCell, RgbColor> speedEdits;
    /// <summary>Shine-frame colors that differ from the calculated fade toward white.</summary>
    internal readonly Dictionary<ColourCell, RgbColor> shineEdits;

    /// <summary>Retains the normal palette and only deviations from calculated speed and shine frames.</summary>
    internal DachoraColorCatalog(RgbColor[] normal, RgbColor[][] speed, RgbColor[][] shine) =>
        (this.normal, speedEdits, shineEdits) =
            (normal, DachoraColors.SpeedEdits(normal, speed), DachoraColors.ShineEdits(normal, shine));

    /// <summary>$A7:F245 speed-boost sequence's flash backdrop in its first frame, authored paint.</summary>
    internal static readonly RgbColor SpeedFlashBackdrop = new(0, 12, 5);

    /// <summary>Resolves one selected Dachora color, using the normal palette as a calculated animated basis while preserving every supplied speed/shine deviation.</summary>
    /// <param name="phase">Exclusive default, speed-boost, or stored-shine palette phase selected by enemy behavior.</param>
    /// <param name="frame">Zero for the default phase, or zero-based animated frame 0 through 3 for speed and shine.</param>
    /// <param name="color">Palette color index 0 through 15, including the transparent slot at zero.</param>
    /// <returns>SNES RGB555 word with red in bits 0..4, green in bits 5..9, and blue in bits 10..14; the caller chooses the actor's OBJ palette destination.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The phase/frame combination is unsupported or the color index is outside the palette.</exception>
    public RgbColor Resolve(DachoraPalettePhase phase, int frame, int color) => DachoraColors.Resolve(this, phase, frame, color);
}
