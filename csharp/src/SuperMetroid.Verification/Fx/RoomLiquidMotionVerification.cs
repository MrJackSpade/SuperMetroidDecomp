using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.Core.Progression;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Verification.Harness;

namespace SuperMetroid.Verification.Fx;

/// <summary>
/// Checks the room liquid's motion in room lines and its waves' start (#1987, #1974): a liquid rising past the room's top
/// reaches its target, a surface the room effect does not name starts above the room's top, a surface running past the
/// deepest room line reaches its target, and water's two waves start at their first phase.
/// </summary>
internal static class RoomLiquidMotionVerification
{
    /// <summary>The motion passes a check allows a rise to finish in.</summary>
    private const int MostPasses = 16;

    /// <summary>
    /// Raises and lowers water through the liquid's own motion passes and checks where each rise ends: a surface whose
    /// step carries it above the room's top has reached its target there, as has an unnamed surface, which starts above
    /// the room's top, and a surface whose step carries it past the deepest room line has passed its target.
    /// </summary>
    internal static void VerifyLiquidRiseRoomLines()
    {
        // Line 2 and a half, rising three pixels: the step lands above the room's top, which no target lies above.
        RoomFxOverlayState rising = Water(startLevel: 2, targetLevel: 0);
        rising.MoveLiquidTo(0, SignedEightEight.FromPixels(-3), 1);
        Finish(rising, "a rise past the room's top");
        AssertEqual<FixedCoordinate?>(new FixedCoordinate(0, 0), rising.BaseY, "a rise past the room's top stops at its target");
        AssertEqual<int?>(0, rising.Surface, "a rise past the room's top leaves the surface on its target's line");

        // No named surface, rising by the least speed: it starts above the room's top, so its first step reaches the target.
        RoomFxOverlayState unnamed = Water(startLevel: null, targetLevel: 10);
        unnamed.MoveLiquidTo(10, SignedEightEight.FromTwoHundredFiftySixthsOfPixel(-1), 1);
        Finish(unnamed, "an unnamed surface's rise");
        AssertEqual<FixedCoordinate?>(new FixedCoordinate(10, 0), unnamed.BaseY, "an unnamed surface's rise reaches its target at once");

        // Near the deepest room line, sinking 127 pixels: the step runs past the deepest line, so it has passed its target.
        const int deepest = CoordinateAxis.LastNonNegative;
        RoomFxOverlayState sinking = Water(startLevel: deepest - 67, targetLevel: deepest);
        sinking.MoveLiquidTo(deepest, SignedEightEight.FromPixels(127), 1);
        Finish(sinking, "a fall past the deepest room line");
        AssertEqual<FixedCoordinate?>(new FixedCoordinate(deepest, 0), sinking.BaseY,
            "a fall past the deepest room line stops at its target");
        AssertEqual<int?>(deepest, sinking.Surface, "a fall past the deepest room line leaves the surface within the room");

        Console.WriteLine("Room liquid rise: rises past the room's top, unnamed surfaces and falls past the deepest line end on their targets.");
    }

    /// <summary>
    /// Loads water with both waves and steps it through more than a whole cycle of each: before its first update and from
    /// it, each wave shows its first phase for a whole phase, then steps on one phase a phase, back to the start after its
    /// last.
    /// </summary>
    internal static void VerifyWaterWavesStartAtFirstPhase()
    {
        RoomFxOverlayState water = Water(startLevel: 100, targetLevel: 100);
        RoomFxOverlayRenderSnapshot loaded = water.CaptureForDisplay()
            ?? throw new InvalidOperationException("Verification requires the water's display snapshot.");
        AssertEqual(0, loaded.WaterOverlayWavePhase, "the surface wave starts at its first phase");
        AssertEqual(0, loaded.WaterBackgroundWavePhase, "the background wave starts at its first phase");

        int updates = WaterWaveDefinitions.SurfaceWaveform.Length * WaterWaveDefinitions.OverlayWavePhaseDuration + 1;
        for (int update = 1; update <= updates; update++)
        {
            water.Step(new ArtworkFrameSelections(), new RoomFxOverlayFrame
            {
                Camera = Position16<RoomPixels>.Zero,
                TimeIsFrozen = false,
            });
            RoomFxOverlayRenderSnapshot shown = water.CaptureForDisplay()
                ?? throw new InvalidOperationException("Verification requires the water's display snapshot.");
            AssertEqual(PhaseAfter(update, WaterWaveDefinitions.OverlayWavePhaseDuration), shown.WaterOverlayWavePhase,
                $"the surface wave's phase after update {update}");
            AssertEqual(PhaseAfter(update, WaterWaveDefinitions.BackgroundWavePhaseDuration), shown.WaterBackgroundWavePhase,
                $"the background wave's phase after update {update}");
        }

        Console.WriteLine($"Water waves: both start at their first phase and step on through {updates} updates.");

        // The phase a wave shows after its update-th update: the first phase for a whole phase, then one phase a phase.
        static int PhaseAfter(int update, int phaseLength) =>
            (update - 1) / phaseLength % WaterWaveDefinitions.SurfaceWaveform.Length;
    }

    /// <summary>Runs the liquid's motion passes until its rise ends, failing if it has not ended within <see cref="MostPasses"/>.</summary>
    private static void Finish(RoomFxOverlayState liquid, string context)
    {
        var system = new SharedGameState(new SharedRandom());
        for (int pass = 0; pass < MostPasses; pass++)
        {
            liquid.AdvanceLiquidMotion(system, timeIsFrozen: false);
            if (pass > 0 && liquid.LiquidLevelSpeed.IsZero)
                return;
        }
        throw new InvalidDataException($"Verification failed: {context} did not end within {MostPasses} motion passes.");
    }

    /// <summary>Loads still water that waves on the effect overlay and the background, its surface and target as given.</summary>
    private static RoomFxOverlayState Water(int? startLevel, int? targetLevel) =>
        RoomFxOverlayState.ForDefinition(new RoomEffectDefinition(LiquidStartLevel: startLevel,
            LiquidTargetLevel: targetLevel, LiquidLevelSpeed: default, LiquidMoveDelay: 0, Type: RoomFxType.Water,
            VisorAnimation: RoomVisorAnimation.Steady, EffectBlend: RoomEffectBlendDefinitions.OverlayBrightens,
            Liquid: new RoomWater(LiquidTide.None, SurfaceDrift: false, BackgroundWave: true, IgnoresSurfaceUntilEnabled: false),
            ColourEffects: [], BlockAnimations: [], Tint: RoomEffectTint.None), 0);

    /// <summary>Runs the room liquid's motion and wave-start checks.</summary>
    [VerificationCommand("--room-liquid-motion")]
    internal static int RoomLiquidMotionCommand()
    {
        Suite(nameof(VerifyLiquidRiseRoomLines), () => VerifyLiquidRiseRoomLines());
        Suite(nameof(VerifyWaterWavesStartAtFirstPhase), () => VerifyWaterWavesStartAtFirstPhase());
        return 0;
    }
}
