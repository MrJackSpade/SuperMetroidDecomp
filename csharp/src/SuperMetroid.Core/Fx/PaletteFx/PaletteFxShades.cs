using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>
/// The shades the import pre-colours pictures in for the palette-FX records (#1512): one per colour group and record the
/// group shows, which colours the pictures of that group show while the record is the group's last.
/// </summary>
public static class PaletteFxShades
{
    /// <summary>The shade of <paramref name="colours"/> showing <paramref name="record"/>.</summary>
    public static PictureShade Of(PaletteFxColours colours, PaletteFxFrame record) => new($"palette-fx-{colours}", record.Record);
}
