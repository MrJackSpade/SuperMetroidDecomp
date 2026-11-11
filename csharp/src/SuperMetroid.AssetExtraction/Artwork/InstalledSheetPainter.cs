using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>
/// Paints laid-out sheets on planes it reuses from one sheet to the next (#1490) and converts each into what the installation
/// holds of it (#1633), so a thread painting many sheets allocates only their installed pixels. The picture store writes
/// their pictures (#1635). Not shared between threads.
/// </summary>
internal sealed class InstalledSheetPainter
{
    /// <summary>The planes a laid-out sheet is painted on before it is converted.</summary>
    private readonly SpriteSheetCanvas canvas = new();

    /// <summary>Paints a laid-out sheet on the reused canvas and converts it, so the sheet needs no planes of its own (#1490).</summary>
    /// <param name="layout">The sheet's layout.</param>
    internal InstalledSheet Install(SpriteSheetLayout layout)
    {
        Ensure.NotNull(layout);
        SpriteSheetPlanes planes = canvas.Cleared(layout.Size);
        SpriteSheetDocument manifest = layout.Paint(planes);
        return InstalledSheetConversion.Install(manifest, planes.Size, planes.Pixels, planes.PaletteIndices);
    }

    /// <summary>Paints a sheet of pictures already painted (#1513) on the reused canvas and converts it.</summary>
    /// <param name="layout">The sheet's pictures.</param>
    internal InstalledSheet Install(PictureSheetLayout layout)
    {
        Ensure.NotNull(layout);
        SpriteSheetPlanes planes = canvas.Cleared(layout.Size);
        SpriteSheetDocument manifest = layout.Paint(planes);
        return InstalledSheetConversion.Install(manifest, planes.Size, planes.Pixels, planes.PaletteIndices);
    }
}
