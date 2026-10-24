using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>One editable sixteen-color RGB5 OBJ palette; selection and animation remain engine-owned.</summary>
public sealed class EnemyPaletteSheet
{
    /// <summary>Canonical selected RGB5 colors and ordered rows, independent of JSON encoding.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create("EnemyPaletteSheet-v1", content =>
        {
            content.AppendColors("colors", colors);
        });

    /// <summary>Sixteen RGB5 colors in a complete four-bit OBJ palette, including its color-zero entry.</summary>
    public const int ColorCount = 16;
    /// <summary>Owned RGB5 palette entries in OBJ color-index order.</summary>
    private readonly Bgr555[] colors;
    /// <summary>Retains the validated sixteen-color array after JSON decoding.</summary>
    internal EnemyPaletteSheet(Bgr555[] colors) => this.colors = colors;

    /// <summary>Copies all sixteen selected colors, including color zero, into consecutive current CGRAM entries; the room graphics-set owner selects the destination and controls palette animation.</summary>
    /// <param name="destination">The sixteen-colour OBJ palette row the room graphics-set record selects.</param>
    public void LoadTo(ColourRun destination) => destination.CopyFrom(colors);
}
