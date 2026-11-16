using System.Collections.Immutable;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Title;
using SuperMetroid.Core.Frontend.Title.Artwork;
using SuperMetroid.Core.Rendering;
using static SuperMetroid.Verification.Compatibility.ReplacedExpressionAssertions;

namespace SuperMetroid.Verification.Frontend.Title;

/// <summary>
/// Confirms that the title's gradient selection by zoom range (#1781) and final-zoom cadence (#1619) read every zoom and
/// phase count exactly as the shift, remainder and mask expressions they replace did.
/// </summary>
internal static class TitleCounterPhasesVerification
{
    /// <summary>Compares every zoom the title shows and every phase count with the expressions the named rules replace.</summary>
    internal static void VerifyTitleCounterPhases()
    {
        // Sixteen gradients told apart by their colour, so the gradient a zoom resolves to names its range.
        ScreenGradient[] variants = [.. Enumerable.Range(0, TitleGradientFormat.VariantCount).Select(variant => new ScreenGradient(
            [new GradientStop(0, new RgbColor(variant, 0, 0), GradientBlend.DarkensFloorAndBackdrop)], DisplayScreen.Height))];
        var presentation = new TitleGradientPresentation(variants);
        AssertEqual(FloorZoom.OwnSize.TwoHundredFiftySixths, TitleGradientFormat.VariantCount * TitleGradientFormat.ZoomRangeWidth,
            "the zoom ranges cover every zoom below the floor's own size");
        // The title shows its gradient from the final zoom's start up to the floor's own size, where the zoom stops; the
        // replaced expression read the zoom's bits 4 through 7, which at the floor's own size select the first gradient.
        for (int zoom = 0; zoom <= FloorZoom.OwnSize.TwoHundredFiftySixths; zoom++)
        {
            AssertSameAt(variants[(zoom >> 4) % TitleGradientFormat.VariantCount],
                presentation.Resolve(FloorZoom.FromTwoHundredFiftySixths(zoom)), "gradient zoom range", zoom);
        }
        AssertThrows<ArgumentOutOfRangeException>(
            () => presentation.Resolve(FloorZoom.OwnSize.Widened(1)),
            "the title gradient refuses a zoom past the floor's own size, which the title never reaches");

        for (int input = 0; input <= ushort.MaxValue; input++)
            AssertSameAt((input & 1) == 0, TitleSequenceDefinitions.Scenes.FinalZoomStepEverySecondUpdate.IsDue(input), "final zoom step", input);

        Console.WriteLine("Title counter phases: every zoom up to the floor's own size and every phase count from 0 through 65535 reads as the expressions they replace.");
    }
}
