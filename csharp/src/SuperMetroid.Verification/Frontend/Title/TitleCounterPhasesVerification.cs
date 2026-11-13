using SuperMetroid.Core.Frontend.Title;
using SuperMetroid.Core.Frontend.Title.Artwork;
using static SuperMetroid.Verification.Compatibility.ReplacedExpressionAssertions;

namespace SuperMetroid.Verification.Frontend.Title;

/// <summary>
/// Confirms that the title's named zoom bucket and final-zoom cadence (#1619) read every zoom and phase count exactly as
/// the shift, remainder and mask expressions they replace did.
/// </summary>
internal static class TitleCounterPhasesVerification
{
    /// <summary>Compares every zoom and phase count with the expressions the named rules replace.</summary>
    internal static void VerifyTitleCounterPhases()
    {
        for (int input = 0; input <= ushort.MaxValue; input++)
        {
            AssertSameAt((input >> 4) % TitleGradientFormat.VariantCount, TitleGradientFormat.ZoomBucket.Of(input), "gradient zoom bucket", input);
            AssertSameAt((input & 1) == 0, TitleSequenceDefinitions.Scenes.FinalZoomStepEverySecondUpdate.IsDue(input), "final zoom step", input);
        }

        AssertEqual(TitleGradientFormat.VariantCount, TitleGradientFormat.ZoomBucket.Outcomes, "one gradient for every zoom bucket");
        Console.WriteLine("Title counter phases: every zoom and phase count from 0 through 65535 reads as the expressions they replace.");
    }
}
