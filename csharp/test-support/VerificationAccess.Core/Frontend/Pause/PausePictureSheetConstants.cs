using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Frontend.Pause.Artwork;
using SuperMetroid.Core.Progression;
using SuperMetroid.AssetExtraction.Frontend.Pause.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Map;

namespace SuperMetroid.AssetExtraction.Frontend.Pause;

/// <summary>Cartridge values and helpers of <see cref="PausePictureSheet"/> that only verification reads.</summary>
internal static class PausePictureSheetConstants
{
    /// <summary>
    /// The pause frame <paramref name="area"/> shows under <paramref name="highlight"/>, as the cartridge's tiles: the backdrop
    /// with the button page's two shown rows over its button rows, each label in its shade, for verification.
    /// </summary>
    internal static BackgroundTile[] Frame(AreaMapPresentation maps, AreaId area, PauseButtonHighlight highlight)
    {
        Ensure.NotNull(maps);
        var frame = new BackgroundTile[PauseBackdropDefinitions.Cells];
        InstalledAreaMapPresentation.PauseOf(maps).Backdrops.LoadTo(frame, area);
        BackgroundTile[] buttons = PausePictureSheet.ButtonRows(maps);
        foreach (int cell in PausePictureSheet.ButtonRowCells())
            frame[cell] = buttons[cell];
        foreach (PauseButtonLabel label in Enum.GetValues<PauseButtonLabel>())
        {
            int palette = PausePictureSheet.PaletteOf(PauseFramePictures.ShadeOf(highlight, label));
            foreach (int cell in PausePictureSheet.LabelCells(label))
                frame[cell] = frame[cell] with { ColourVariant = palette };
        }
        return frame;
    }
}
