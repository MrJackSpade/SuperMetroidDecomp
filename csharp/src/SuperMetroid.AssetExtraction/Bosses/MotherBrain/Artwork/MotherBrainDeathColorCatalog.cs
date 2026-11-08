using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Bosses.MotherBrain.Artwork;

/// <summary>Editable Mother Brain death-fade RGB5 images, which the import pre-colours her body layer and corpse from.</summary>
public sealed class MotherBrainDeathColorCatalog
{
    /// <summary>The selected body, leg and corpse fades.</summary>
    internal readonly MotherBrainDeathColorSelection selection;

    /// <summary>Keeps the fades as calculated stages and independently edited frames.</summary>
    /// <param name="selection">The fades the import selected.</param>
    internal MotherBrainDeathColorCatalog(MotherBrainDeathColorSelection selection) => this.selection = selection;

    /// <summary>Returns one body-fade color from the first segment of the death-fade images, applied to both colours 65..78 and 145..158 during the death fade.</summary>
    /// <param name="frame">Zero-based authored palette stage 0..15, not an elapsed gameplay-update count; the stock sequence ends at black.</param>
    /// <param name="color">Zero-based color within the fourteen-color body segment, 0..13.</param>
    /// <returns>Packed RGB5 color, preserving any independently edited stage.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The stage or color index is outside the authored images.</exception>
    public RgbColor BodyColor(int frame, int color) =>
        selection.BodyFade.Checked(frame, color, nameof(BodyColor));

    /// <summary>Returns one back-leg fade color from the second fourteen-color segment of each death-fade image, applied to colours 177..190.</summary>
    /// <param name="frame">Zero-based authored palette stage 0..15; the stock back-leg sequence fades to black alongside the body.</param>
    /// <param name="color">Zero-based color within the back-leg segment, 0..13.</param>
    /// <returns>Packed RGB5 color from the selected back-leg image.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The stage or color index is outside the authored images.</exception>
    public RgbColor LegColor(int frame, int color) =>
        selection.LegFade.Checked(frame, color, nameof(LegColor));

    /// <summary>Returns one detached-head corpse color from the fade-to-gray images, applied to colours 241..255 before corpse rotting.</summary>
    /// <param name="frame">Zero-based authored palette stage 0..7, progressing from the death-start colors to the drained corpse appearance.</param>
    /// <param name="color">Zero-based opaque color in sprite palette seven, 0..14; transparent color zero is omitted.</param>
    /// <returns>Packed RGB5 color from the selected corpse image.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The stage or color index is outside the authored images.</exception>
    public RgbColor CorpseColor(int frame, int color) =>
        selection.CorpseFade.Checked(frame, color, nameof(CorpseColor));
}
