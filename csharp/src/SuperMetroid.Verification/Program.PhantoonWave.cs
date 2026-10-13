using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Desktop;
using static SuiteTimingVerification;

/// <summary>Checks Phantoon HDMA wave, palette fade, and blending state across display latches.</summary>
internal static class PhantoonWaveVerification
{
    /// <summary>Checks setup cadence, native scroll progression, deletion, and debugger state restoration.</summary>
    internal static void VerifyPhantoonWaveLifecycle()
    {
        var boss = new PhantoonEnemyState(new RoomEnemySlot(0))
        {
            Eye = new RoomEnemySlot(1), Mouth = new RoomEnemySlot(3),
        };
        boss.Mouth.VariableD = 3072;
        boss.Mouth.VariableF = 8;
        var wave = boss.Wave;
        wave.Begin(PhantoonWaveRomData.IntroMode);
        AssertTrue(!wave.Active, "wave spawn waits for HDMA setup");
        wave.Step(boss);
        AssertEqual(2, boss.Eye.Parameter1, "HDMA setup copies pending mode to eye");
        AssertEqual(65534, wave.Phase, "native setup-only phase");
        AssertTrue(wave.ScrollCycle.ToArray().All(word => word == 0), "setup does not run the scroll pre-instruction");
        // Original CPU lifecycle capture: zero base, amplitude $C00, phase delta 8.
        foreach (var (phase, first) in new[] { (14, 2), (30, 4), (46, 6), (62, 8) })
        {
            wave.Step(boss);
            AssertEqual(phase, wave.Phase, "original-CPU phase progression");
            AssertEqual(first, wave.ScrollCycle[0], "original-CPU first scroll word");
        }
        wave.LatchDisplay();
        ushort[] savedDisplay = (wave.DisplayedScrolls ?? throw new InvalidOperationException("Verification requires wave.DisplayedScrolls.")).ToArray();
        wave.Step(boss);
        AssertTrue(savedDisplay.AsSpan().SequenceEqual(wave.DisplayedScrolls), "live HDMA step leaves latched display immutable");
        using var stream = new MemoryStream();
        DebuggerObjectGraphSerializer.Serialize(stream, wave);
        stream.Position = 0;
        var restored = DebuggerObjectGraphSerializer.Deserialize<PhantoonWaveHdmaState>(stream);
        AssertEqual(wave.Phase, restored.Phase, "wave phase survives exact debugger state");
        AssertTrue(wave.ScrollCycle.SequenceEqual(restored.ScrollCycle), "wave data survives exact debugger state");
        AssertTrue(savedDisplay.AsSpan().SequenceEqual(restored.DisplayedScrolls), "latched wave survives debugger state");
        boss.Eye.Parameter1 = 0;
        var retained = wave.ScrollCycle.ToArray();
        wave.Step(boss);
        AssertTrue(!wave.Active && retained.AsSpan().SequenceEqual(wave.ScrollCycle), "native deletion retains data");
        wave.LatchDisplay();
        AssertTrue(wave.DisplayedScrolls is null, "deleted wave stops on next display latch");
        Suite(nameof(VerifyPhantoonBlendingLifecycle), () => VerifyPhantoonBlendingLifecycle(boss));
        Suite(nameof(VerifyPhantoonFadeColors), () => VerifyPhantoonFadeColors());
        Console.WriteLine("  Phantoon wave: original-CPU lifecycle, display latch and debugger round-trip agree.");
    }

    /// <summary>Checks captured native palette colors that distinguish signed rounding and fade denominator.</summary>
    private static void VerifyPhantoonFadeColors()
    {
        // Original $A7:D464/$D486 captures: health one, denominator twelve,
        // second even-frame update. These fixed colors distinguish signed rounding
        // and the old extra-step denominator without requiring the local CPU trace.
        foreach (var (current, target, expected) in new[] { (14, 0, 12), (9692, 0, 8601), (0, 14, 1), (0, 9692, 34) })
        {
            Bgr555 actual = (Bgr555)PhantoonBehavior.CalculatePhantoonTransitionColor((ushort)1, (ushort)12, Bgr555.FromWord((ushort)current), Bgr555.FromWord((ushort)target));
            AssertEqual(expected, actual, "original-CPU Phantoon fixed-point palette component rounding");
        }
        AssertEqual(0, (Bgr555)PhantoonBehavior.CalculatePhantoonTransitionColor((ushort)1, (ushort)1, Bgr555.FromWord(14), Bgr555.Black),
            "fast fade reaches target on first interpolated update");
    }

    /// <summary>Checks transparency priority, hidden state, NMI latch timing, and deletion persistence.</summary>
    private static void VerifyPhantoonBlendingLifecycle(PhantoonEnemyState boss)
    {
        var blend = boss.Blending;
        boss.SemiTransparencyLayerFlags = PhantoonBlendingRomData.SemiTransparentBit;
        boss.Mouth.Parameter1 = PhantoonBlendingRomData.DeleteControl;
        for (int i = 0; i < PhantoonBlendingRomData.SetupCalls; i++)
        {
            blend.Step(boss, LayerBlendingConfiguration.NormalGameplay);
            AssertEqual(LayerBlendingConfiguration.NormalGameplay, blend.Configuration, "blend setup does not run pre-instruction");
        }
        blend.Step(boss, LayerBlendingConfiguration.NormalGameplay);
        AssertEqual(LayerBlendingConfiguration.PhantoonSemiTransparent, blend.Configuration, "flag takes priority over delete control");
        blend.LatchDisplay(0x42);
        boss.MosaicRegister = 0x52;
        boss.SemiTransparencyLayerFlags = 0;
        boss.Mouth.Parameter1 = 0x8001;
        blend.Step(boss, LayerBlendingConfiguration.NormalGameplay);
        AssertEqual(LayerBlendingConfiguration.NormalGameplay, blend.Configuration, "nonzero low byte retains current room default, not previous blend");
        AssertEqual(LayerBlendingConfiguration.PhantoonSemiTransparent, blend.DisplayedConfiguration, "live change cannot mutate latched blend");
        AssertEqual(0x42, blend.DisplayedMosaic, "live mosaic change waits for next accepted NMI");
        boss.Mouth.Parameter1 = 0x8000;
        blend.Step(boss, LayerBlendingConfiguration.NormalGameplay);
        AssertEqual(LayerBlendingConfiguration.PhantoonHidden, blend.Configuration, "zero low byte hides body");
        boss.Mouth.Parameter1 = 0xffff;
        blend.Step(boss, LayerBlendingConfiguration.NormalGameplay);
        AssertEqual(LayerBlendingConfiguration.PhantoonHidden, blend.Configuration, "delete call still publishes hidden configuration");
        boss.SemiTransparencyLayerFlags = PhantoonBlendingRomData.SemiTransparentBit;
        blend.Step(boss, LayerBlendingConfiguration.NormalGameplay);
        AssertEqual(LayerBlendingConfiguration.NormalGameplay, blend.Configuration, "deleted owner no longer overrides room");
        using var stream = new MemoryStream();
        DebuggerObjectGraphSerializer.Serialize(stream, blend);
        stream.Position = 0;
        var restored = DebuggerObjectGraphSerializer.Deserialize<PhantoonBlendingState>(stream);
        restored.Step(boss, LayerBlendingConfiguration.NormalGameplay);
        AssertEqual(blend.Configuration, restored.Configuration, "deleted blend owner survives debugger roundtrip");
        AssertEqual(0x42, restored.DisplayedMosaic, "display mosaic survives debugger roundtrip");
    }
}
