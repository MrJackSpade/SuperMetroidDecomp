using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Editable RGB5 visor colors shared by X-ray and room palette cycling.</summary>
public sealed class InstalledSamusVisorColors
{
    /// <summary>Only installed color edits differing from the reviewed $9B:A3C0 stock words.</summary>
    internal readonly Dictionary<ColourCell, RgbColor> colors;

    /// <summary>Retains copied visor color edits independently of the mutable JSON document.</summary>
    internal InstalledSamusVisorColors(Dictionary<ColourCell, RgbColor> colors) => this.colors = colors;

    /// <summary>Returns a visor shade for the owners that animate Samus's visor colour.</summary>
    /// <param name="index">Visor shade 0..5: 0..2 are X-ray widening colors and 3..5 are the steady X-ray/room-backdrop cycle.</param>
    /// <returns>Packed SNES BGR555 word from the independent edit or calculated stock definition.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The color ordinal is outside 0..5.</exception>
    public RgbColor Resolve(int index)
    {
        if ((uint)index >= SamusVisorColorFormat.ColorCount)
            throw new ArgumentOutOfRangeException(nameof(index));
        return colors.TryGetValue(new ColourCell(0, index), out RgbColor color) ? color : SamusVisorColorSteps.Color(index);
    }
}
