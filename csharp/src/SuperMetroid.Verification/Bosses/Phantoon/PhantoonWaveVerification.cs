using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Assets;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.Phantoon;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Diagnostics.SaveStates;

namespace SuperMetroid.Verification.Bosses.Phantoon;
using SuperMetroid.Verification.Harness;

/// <summary>Checks the wave of Phantoon's body, his palette fade, and his body's visibility across display latches.</summary>
internal static class PhantoonWaveVerification
{
    /// <summary>Checks setup cadence, native scroll progression, deletion, and debugger state restoration.</summary>
    internal static void VerifyPhantoonWaveLifecycle()
    {
        var boss = new PhantoonEnemyState(new RoomEnemySlot(0, EnemySlotFixtures.Registry))
        {
            Eye = new RoomEnemySlot(1, EnemySlotFixtures.Registry), Mouth = new RoomEnemySlot(3, EnemySlotFixtures.Registry),
        };
        boss.MouthState.WaveAmplitude = 3072;
        boss.MouthState.WavyPhaseDelta = 8;
        var wave = boss.BodyWave;
        wave.Begin(PhantoonWaveDefinitions.IntroLength);
        AssertTrue(!wave.Active, "a requested wave waits for its setup update");
        wave.Step(boss);
        AssertEqual<PhantoonWaveLength?>(PhantoonWaveLength.Short, boss.EyeState.WaveLength, "the wave's setup copies the pending length to the eye");
        AssertEqual(PhantoonWaveDefinitions.InitialPhase, wave.Phase, "the setup update only seeds the phase");
        // The setup does not run the scroll pre-instruction: the wave is flat at the scroll it was last built from.
        AssertEqual(new PhantoonWave(0, 0, new TrigonometryAngle(0), PhantoonWaveDefinitions.ShortPeriod),
            wave.CurrentWave, "setup does not run the scroll pre-instruction");
        // Original CPU lifecycle capture: zero base, amplitude $C00, phase delta 8. The described wave's first line gives
        // the cartridge's first scroll word.
        foreach (var (phase, first) in new[] { new VerifyPhantoonWaveLifecycleCase(14, 2), new VerifyPhantoonWaveLifecycleCase(30, 4), new VerifyPhantoonWaveLifecycleCase(46, 6), new VerifyPhantoonWaveLifecycleCase(62, 8) })
        {
            wave.Step(boss);
            AssertEqual(phase, wave.Phase, "original-CPU phase progression");
            PhantoonWave current = wave.CurrentWave ?? throw new InvalidOperationException("Verification requires a set-up Phantoon wave.");
            AssertEqual(new PhantoonWave(0, 3072, new TrigonometryAngle((byte)(phase / 2)), PhantoonWaveDefinitions.ShortPeriod),
                current, "the wave stands at its base scroll, amplitude, phase's angle and period");
            AssertEqual(first, ScrollShapeLines.WaveScroll(BossLineScrolls.Of(current), 0), "original-CPU first scroll word");
        }
        wave.LatchDisplay();
        PhantoonWave savedDisplay = wave.DisplayedWave ?? throw new InvalidOperationException("Verification requires a latched Phantoon wave.");
        wave.Step(boss);
        AssertEqual(savedDisplay, wave.DisplayedWave, "a live wave update leaves the latched display unchanged");
        using var stream = new MemoryStream();
        DebuggerObjectGraphSerializer.Serialize(stream, wave);
        stream.Position = 0;
        var restored = DebuggerObjectGraphSerializer.Deserialize<PhantoonBodyWaveEffect>(stream);
        AssertEqual(wave.Phase, restored.Phase, "wave phase survives exact debugger state");
        AssertEqual(wave.CurrentWave, restored.CurrentWave, "wave data survives exact debugger state");
        AssertEqual(savedDisplay, restored.DisplayedWave, "latched wave survives debugger state");
        boss.EyeState.WaveLength = null;
        PhantoonWave? retained = wave.CurrentWave;
        wave.Step(boss);
        AssertTrue(!wave.Active && retained == wave.CurrentWave, "native deletion retains data");
        wave.LatchDisplay();
        AssertTrue(wave.DisplayedWave is null, "deleted wave stops on next display latch");
        Suite(nameof(VerifyPhantoonBodyVisibilityLifecycle), () => VerifyPhantoonBodyVisibilityLifecycle(boss));
        Suite(nameof(VerifyPhantoonFadeColors), () => VerifyPhantoonFadeColors());
        Suite(nameof(VerifyPhantoonDeathMosaicSizes), () => VerifyPhantoonDeathMosaicSizes());
        Console.WriteLine("  Phantoon wave: original-CPU lifecycle, display latch and debugger round-trip agree.");
    }

    /// <summary>
    /// Checks that each death-mosaic block size, from the first to the last, is the size the PPU decodes from the MOSAIC
    /// value the cartridge writes for that step ($02 first, sixteen more each step: the size less one in the high
    /// nibble, BG2 enabled).
    /// </summary>
    private static void VerifyPhantoonDeathMosaicSizes()
    {
        int steps = 0;
        for (int size = PhantoonDeathDefinitions.InitialMosaicSize; size <= PhantoonDeathDefinitions.FinalMosaicSize; size++)
        {
            byte mosaic = (byte)PhantoonNativeWords.DeathMosaicWord(size);
            AssertEqual(BackgroundMosaicSamplingConstants.ForBg2(mosaic).Size, size, $"death mosaic step {steps} block size");
            steps++;
        }
        AssertEqual(16, steps, "the death mosaic grows through all sixteen block sizes");
    }

    /// <summary>Checks captured native palette colors that distinguish signed rounding and fade denominator.</summary>
    private static void VerifyPhantoonFadeColors()
    {
        // Original $A7:D464/$D486 captures: health one, denominator twelve,
        // second even-frame update. These fixed colors distinguish signed rounding
        // and the old extra-step denominator without requiring the local CPU trace.
        foreach (var (current, target, expected) in new[] { new VerifyPhantoonFadeColorsCase(14, 0, 12), new VerifyPhantoonFadeColorsCase(9692, 0, 8601), new VerifyPhantoonFadeColorsCase(0, 14, 1), new VerifyPhantoonFadeColorsCase(0, 9692, 34) })
        {
            RgbColor actual = PhantoonPresentationConstants.CalculatePhantoonTransitionColor(1, 12, RgbColor.FromWord((ushort)current), RgbColor.FromWord((ushort)target));
            AssertEqual(expected, actual, "original-CPU Phantoon fixed-point palette component rounding");
        }
        AssertEqual(0, PhantoonPresentationConstants.CalculatePhantoonTransitionColor(1, 1, RgbColor.FromWord(14), RgbColor.Black),
            "fast fade reaches target on first interpolated update");
    }

    /// <summary>Checks translucency's precedence, the hidden body, display latch timing, and the effect's end persisting.</summary>
    private static void VerifyPhantoonBodyVisibilityLifecycle(PhantoonEnemyState boss)
    {
        var visibility = boss.BodyVisibility;
        boss.Translucent = true;
        boss.MouthState.BodyLayer = PhantoonBodyLayer.Cleared;
        for (int i = 0; i < PhantoonBodyVisibilityEffect.SetupCalls; i++)
        {
            visibility.Step(boss);
            AssertEqual(PhantoonBodyVisibility.Shown, visibility.Visibility, "the effect's setup updates show the body as the room does");
        }
        visibility.Step(boss);
        AssertEqual(PhantoonBodyVisibility.Translucent, visibility.Visibility, "translucency takes priority over a cleared body");
        visibility.LatchDisplay(mosaicBlockSize: 5);
        boss.MosaicBlockSize = 6;
        boss.Translucent = false;
        boss.MouthState.BodyLayer = PhantoonBodyLayer.Shown;
        visibility.Step(boss);
        AssertEqual(PhantoonBodyVisibility.Shown, visibility.Visibility, "a shown body shows as the room does, not as the previous update showed it");
        AssertEqual(PhantoonBodyVisibility.Translucent, visibility.DisplayedVisibility, "a live change cannot change the latched visibility");
        AssertEqual(5, visibility.DisplayedMosaicBlockSize, "a live pixelation change waits for the next display latch");
        boss.MouthState.BodyLayer = PhantoonBodyLayer.Hidden;
        visibility.Step(boss);
        AssertEqual(PhantoonBodyVisibility.Hidden, visibility.Visibility, "a hidden body is hidden");
        boss.MouthState.BodyLayer = PhantoonBodyLayer.Cleared;
        visibility.Step(boss);
        AssertEqual(PhantoonBodyVisibility.Hidden, visibility.Visibility, "the update that clears the body still hides it");
        boss.Translucent = true;
        visibility.Step(boss);
        AssertEqual(PhantoonBodyVisibility.Shown, visibility.Visibility, "an ended effect shows the body as the room does");
        using var stream = new MemoryStream();
        DebuggerObjectGraphSerializer.Serialize(stream, visibility);
        stream.Position = 0;
        var restored = DebuggerObjectGraphSerializer.Deserialize<PhantoonBodyVisibilityEffect>(stream);
        restored.Step(boss);
        AssertEqual(visibility.Visibility, restored.Visibility, "the ended effect survives a debugger roundtrip");
        AssertEqual(5, restored.DisplayedMosaicBlockSize, "the displayed pixelation survives a debugger roundtrip");
    }

    /// <summary>Runs only the Phantoon wave lifecycle check.</summary>
    [VerificationCommand("--phantoon-wave-lifecycle")]
    internal static int PhantoonWaveLifecycleCommand()
    {
        VerifyPhantoonWaveLifecycle();
        return 0;
    }
}
