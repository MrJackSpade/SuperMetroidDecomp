using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>One editable sixteen-color RGB5 OBJ palette; selection and animation remain engine-owned.</summary>
public sealed class EnemyPaletteSheet
{
    /// <summary>Owned RGB5 palette entries in OBJ color-index order.</summary>
    internal readonly RgbColor[] colors;
    /// <summary>Retains the validated sixteen-color array after JSON decoding.</summary>
    internal EnemyPaletteSheet(RgbColor[] colors) => this.colors = colors;

    /// <summary>Copies all sixteen selected colors, including color zero, into consecutive current palette colours; the room graphics-set owner selects the destination and controls palette animation.</summary>
    /// <param name="destination">The sixteen-colour OBJ colour set the room graphics-set record selects.</param>
    public void LoadTo(ColourRun destination) => destination.CopyFrom(colors);
}
