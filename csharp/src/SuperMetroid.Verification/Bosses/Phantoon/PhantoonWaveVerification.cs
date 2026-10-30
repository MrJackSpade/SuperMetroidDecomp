using SuperMetroid.Core.Assets;
using SuperMetroid.Core;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.Phantoon;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Diagnostics.SaveStates;
using SuperMetroid.TestSupport.FrameRendering;

namespace SuperMetroid.Verification.Bosses.Phantoon;
using SuperMetroid.Verification.Harness;

/// <summary>Checks Phantoon's wave effect, palette fade, and blending state across display latches.</summary>
internal static class PhantoonWaveVerification
{
    /// <summary>Checks setup cadence, native scroll progression, deletion, and debugger state restoration.</summary>
    internal static void VerifyPhantoonWaveLifecycle()
    {
        var boss = new PhantoonEnemyState(new RoomEnemySlot(0, EnemySlotFixtures.Registry))
        {
            Eye = new RoomEnemySlot(1, EnemySlotFixtures.Registry), Mouth = new RoomEnemySlot(3, EnemySlotFixtures.Registry),
        };
        boss.Parts[boss.Mouth.SlotIndex].WaveAmplitude = 3072;
        boss.Parts[boss.Mouth.SlotIndex].WavyPhaseDelta = 8;
        var wave = boss.Wave;
        wave.Begin(PhantoonWaveDefinitions.IntroMode);
        AssertTrue(!wave.Active, "a requested wave waits for its setup update");
        wave.Step(boss);
        AssertEqual(2, boss.Parts[boss.Eye.SlotIndex].WaveMode, "the wave's setup copies the pending mode to the eye");
        AssertEqual(PhantoonWaveDefinitions.InitialPhase, wave.Phase, "the setup update only seeds the phase");
        // The setup does not run the scroll pre-instruction: the wave is flat at the scroll it was last built from.
        AssertEqual(new PhantoonWave(0, 0, new ByteAngle(0), PhantoonWaveDefinitions.ShortPeriod),
            wave.CurrentWave, "setup does not run the scroll pre-instruction");
        // Original CPU lifecycle capture: zero base, amplitude $C00, phase delta 8. The described wave's first line gives
        // the cartridge's first scroll word.
        foreach (var (phase, first) in new[] { new VerifyPhantoonWaveLifecycleCase(14, 2), new VerifyPhantoonWaveLifecycleCase(30, 4), new VerifyPhantoonWaveLifecycleCase(46, 6), new VerifyPhantoonWaveLifecycleCase(62, 8) })
        {
            wave.Step(boss);
            AssertEqual(phase, wave.Phase, "original-CPU phase progression");
            PhantoonWave current = wave.CurrentWave ?? throw new InvalidOperationException("Verification requires a set-up Phantoon wave.");
            AssertEqual(new PhantoonWave(0, 3072, new ByteAngle((byte)(phase / 2)), PhantoonWaveDefinitions.ShortPeriod),
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
        var restored = DebuggerObjectGraphSerializer.Deserialize<PhantoonWaveEffect>(stream);
        AssertEqual(wave.Phase, restored.Phase, "wave phase survives exact debugger state");
        AssertEqual(wave.CurrentWave, restored.CurrentWave, "wave data survives exact debugger state");
        AssertEqual(savedDisplay, restored.DisplayedWave, "latched wave survives debugger state");
        boss.Parts[boss.Eye.SlotIndex].WaveMode = 0;
        PhantoonWave? retained = wave.CurrentWave;
        wave.Step(boss);
        AssertTrue(!wave.Active && retained == wave.CurrentWave, "native deletion retains data");
        wave.LatchDisplay();
        AssertTrue(wave.DisplayedWave is null, "deleted wave stops on next display latch");
        Suite(nameof(VerifyPhantoonBlendingLifecycle), () => VerifyPhantoonBlendingLifecycle(boss));
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
            RgbColor actual = PhantoonPresentation.CalculatePhantoonTransitionColor(1, 12, RgbColor.FromWord((ushort)current), RgbColor.FromWord((ushort)target));
            AssertEqual(expected, actual, "original-CPU Phantoon fixed-point palette component rounding");
        }
        AssertEqual(0, PhantoonPresentation.CalculatePhantoonTransitionColor(1, 1, RgbColor.FromWord(14), RgbColor.Black),
            "fast fade reaches target on first interpolated update");
    }

    /// <summary>Checks transparency priority, hidden state, NMI latch timing, and deletion persistence.</summary>
    private static void VerifyPhantoonBlendingLifecycle(PhantoonEnemyState boss)
    {
        var blend = boss.Blending;
        boss.SemiTransparent = true;
        boss.Parts[boss.Mouth.SlotIndex].LayerControl = PhantoonLayerControl.Removed;
        for (int i = 0; i < PhantoonBlendingState.SetupCalls; i++)
        {
            blend.Step(boss, LayerBlendingConfiguration.NormalGameplay);
            AssertEqual(LayerBlendingConfiguration.NormalGameplay, blend.Configuration, "blend setup does not run pre-instruction");
        }
        blend.Step(boss, LayerBlendingConfiguration.NormalGameplay);
        AssertEqual(LayerBlendingConfiguration.PhantoonSemiTransparent, blend.Configuration, "flag takes priority over delete control");
        blend.LatchDisplay(mosaicBlockSize: 5);
        boss.MosaicBlockSize = 6;
        boss.SemiTransparent = false;
        // The fade-in's control (the cartridge's word $8001, low byte one) is the room-default state.
        boss.Parts[boss.Mouth.SlotIndex].LayerControl = PhantoonLayerControl.RoomDefault;
        blend.Step(boss, LayerBlendingConfiguration.NormalGameplay);
        AssertEqual(LayerBlendingConfiguration.NormalGameplay, blend.Configuration, "the room-default control retains the current room default, not the previous blend");
        AssertEqual(LayerBlendingConfiguration.PhantoonSemiTransparent, blend.DisplayedConfiguration, "live change cannot mutate latched blend");
        AssertEqual(5, blend.DisplayedMosaicBlockSize, "live mosaic size change waits for next accepted NMI");
        boss.Parts[boss.Mouth.SlotIndex].LayerControl = PhantoonLayerControl.Hidden;
        blend.Step(boss, LayerBlendingConfiguration.NormalGameplay);
        AssertEqual(LayerBlendingConfiguration.PhantoonHidden, blend.Configuration, "the hidden control hides the body");
        boss.Parts[boss.Mouth.SlotIndex].LayerControl = PhantoonLayerControl.Removed;
        blend.Step(boss, LayerBlendingConfiguration.NormalGameplay);
        AssertEqual(LayerBlendingConfiguration.PhantoonHidden, blend.Configuration, "delete call still publishes hidden configuration");
        boss.SemiTransparent = true;
        blend.Step(boss, LayerBlendingConfiguration.NormalGameplay);
        AssertEqual(LayerBlendingConfiguration.NormalGameplay, blend.Configuration, "deleted owner no longer overrides room");
        using var stream = new MemoryStream();
        DebuggerObjectGraphSerializer.Serialize(stream, blend);
        stream.Position = 0;
        var restored = DebuggerObjectGraphSerializer.Deserialize<PhantoonBlendingState>(stream);
        restored.Step(boss, LayerBlendingConfiguration.NormalGameplay);
        AssertEqual(blend.Configuration, restored.Configuration, "deleted blend owner survives debugger roundtrip");
        AssertEqual(5, restored.DisplayedMosaicBlockSize, "display mosaic size survives debugger roundtrip");
    }

    /// <summary>Runs only the Phantoon wave lifecycle check.</summary>
    [VerificationCommand("--phantoon-wave-lifecycle")]
    internal static int PhantoonWaveLifecycleCommand()
    {
        VerifyPhantoonWaveLifecycle();
        return 0;
    }
}
