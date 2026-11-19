using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Bosses.Phantoon;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rendering;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;

namespace SuperMetroid.Verification.Rendering;

using SuperMetroid.Verification.Harness;

/// <summary>
/// Checks that the display's line-scroll rows are signed (#2308), as its background scroll is (#2305): the row the screen's
/// top shows, the heat haze's base row and the bosses' line scrolls read negative above or left of the background's origin
/// instead of wrapping, and each line still shows the sixteen bits the wrapped row gave it.
/// </summary>
internal static class SignedLineScrollRowsVerification
{
    /// <summary>
    /// Checks the row the screen's top shows, the vertical heat haze, Crocomire's melt and Phantoon's wave with the
    /// background's camera above or left of the background's origin.
    /// </summary>
    internal static void VerifySignedLineScrollRows()
    {

        // The vertical heat haze moves about the row the screen's top shows: a camera above the origin gives it a negative
        // base, and every line shows the sixteen bits the camera's wrapped row gave it.
        var heat = new RoomFxOverlayRenderSnapshot(RoomFxType.Lava, RoomEffectOverlayLook.Liquid(RoomEffectBlendDefinitions.OverlayDarkens),
            new Position16<OverlayPixels>(0, 0), Surface: 100, Liquid: new RoomLavaOrAcid(LiquidTide.None, HeatHazeDirection.Vertical))
            { LavaAcidBackgroundWavePhase = 5 };
        SampledScrollWave signed = GameplayEffectCapture.BuildLavaAcidBackgroundVerticalScrolls(heat, -3)
            ?? throw new InvalidOperationException("A vertical heat haze describes the background's vertical scroll.");
        SampledScrollWave wrapped = GameplayEffectCapture.BuildLavaAcidBackgroundVerticalScrolls(heat, ushort.MaxValue - 2)
            ?? throw new InvalidOperationException("A vertical heat haze describes the background's vertical scroll.");
        AssertEqual(-2, RoomView.ShownRow(-3), "the row the screen's top shows below a camera above the origin is negative");
        AssertEqual(-2, signed.BaseScroll, "the heat haze's base row above the origin is negative");
        AssertSequenceEqual(ScrollShapeLines.GameplayScrolls(wrapped), ScrollShapeLines.GameplayScrolls(signed),
            "the signed heat haze shows the lines the wrapped row showed");

        // Crocomire's melt with the camera above his body's rows: the rows the screen's top shows of his body are negative.
        var camera = new Position16<RoomPixels>(0, 290);
        ScrollStretch melt = BossLineScrolls.Of(new CrocomireMeltDistortion(300, 310, 100, 150, UnsignedSixteenSixteen.FromTwoHundredFiftySixthsOfPixel(64)), camera);
        AssertEqual(RoomView.ShownRow(290 - 300), melt.ScrollAbove, "the melt's levelled row above the camera's is negative");
        AssertEqual(RoomView.ShownRow(290 - 310), melt.BaseScroll, "the melt's row above the camera's is negative");

        // Phantoon's wave with his body right of the camera's corner: the wave's base scroll is negative.
        ScrollWave wave = BossLineScrolls.Of(new PhantoonWave(new UnsignedSixteenSixteen(1, 0), default, 64), new Position16<RoomPixels>(100, 0), new Position16<RoomPixels>(130, 0));
        AssertEqual(-30, wave.BaseScroll, "Phantoon's wave about a body right of the camera's corner is negative");
        Console.WriteLine("Signed line-scroll rows: the haze, the melt and Phantoon's wave read negative above or left of the origin.");
    }

    /// <summary>Runs the signed line-scroll rows check.</summary>
    [VerificationCommand("--signed-line-scroll-rows")]
    internal static int SignedLineScrollRowsCommand()
    {
        Suite(nameof(VerifySignedLineScrollRows), () => VerifySignedLineScrollRows());
        return 0;
    }
}
