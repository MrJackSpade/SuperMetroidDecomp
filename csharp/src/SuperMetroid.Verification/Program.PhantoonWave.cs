using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Desktop;

internal static partial class Program
{
    private static void VerifyPhantoonWaveLifecycle()
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var boss = new PhantoonEnemyState(new RoomEnemySlot(0))
        {
            Eye = new RoomEnemySlot(1), Mouth = new RoomEnemySlot(3),
        };
        boss.Mouth.VariableD = 3072;
        boss.Mouth.VariableF = 8;
        var wave = boss.Wave;
        wave.Begin(PhantoonWaveRomData.IntroMode);
        AssertTrue(!wave.Active, "wave spawn waits for HDMA setup");
        wave.Step(bus, boss);
        AssertEqual(2, boss.Eye.Parameter1, "HDMA setup copies pending mode to eye");
        AssertEqual(65534, wave.Phase, "native setup-only phase");
        AssertTrue(wave.ScrollCycle.ToArray().All(word => word == 0), "setup does not run the scroll pre-instruction");
        // Original CPU lifecycle capture: zero base, amplitude $C00, phase delta 8.
        foreach (var (phase, first) in new[] { (14, 2), (30, 4), (46, 6), (62, 8) })
        {
            wave.Step(bus, boss);
            AssertEqual(phase, wave.Phase, "original-CPU phase progression");
            AssertEqual(first, wave.ScrollCycle[0], "original-CPU first scroll word");
        }
        wave.LatchDisplay();
        ushort[] savedDisplay = wave.DisplayedScrolls!.ToArray();
        wave.Step(bus, boss);
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
        wave.Step(bus, boss);
        AssertTrue(!wave.Active && retained.AsSpan().SequenceEqual(wave.ScrollCycle), "native deletion retains data");
        wave.LatchDisplay();
        AssertTrue(wave.DisplayedScrolls is null, "deleted wave stops on next display latch");
        var fields = typeof(PhantoonEnemyState).GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
            .OrderBy(field => field.MetadataToken).ToArray();
        var legacy = DebuggerStateFieldMigrations.SelectSerializedFields(typeof(PhantoonEnemyState), fields, fields.Length - 2);
        AssertTrue(legacy.SequenceEqual(fields.Where(field => field.Name is not ("_wave" or "_blending"))), "legacy migration preserves every prior field identity/order");
        var waveEra = DebuggerStateFieldMigrations.SelectSerializedFields(typeof(PhantoonEnemyState), fields, fields.Length - 1);
        AssertTrue(waveEra.SequenceEqual(fields.Where(field => field.Name != "_blending")), "wave-era migration preserves recorded wave history");
        VerifyPhantoonBlendingLifecycle(boss);
        Console.WriteLine("  Phantoon wave: original-CPU lifecycle, display latch, debugger round-trip and explicit legacy migration agree.");
    }

    private static void VerifyPhantoonBlendingLifecycle(PhantoonEnemyState boss)
    {
        var blend = boss.Blending;
        boss.SemiTransparencyLayerFlags = PhantoonBlendingRomData.SemiTransparentBit;
        boss.Mouth!.Parameter1 = PhantoonBlendingRomData.DeleteControl;
        for (int i = 0; i < PhantoonBlendingRomData.SetupCalls; i++)
        {
            blend.Step(boss, LayerBlendingConfiguration.NormalGameplay);
            AssertEqual(LayerBlendingConfiguration.NormalGameplay, blend.Configuration, "blend setup does not run pre-instruction");
        }
        blend.Step(boss, LayerBlendingConfiguration.NormalGameplay);
        AssertEqual(LayerBlendingConfiguration.PhantoonSemiTransparent, blend.Configuration, "flag takes priority over delete control");
        blend.LatchDisplay();
        boss.SemiTransparencyLayerFlags = 0;
        boss.Mouth.Parameter1 = 0x8001;
        blend.Step(boss, LayerBlendingConfiguration.NormalGameplay);
        AssertEqual(LayerBlendingConfiguration.NormalGameplay, blend.Configuration, "nonzero low byte retains current room default, not previous blend");
        AssertEqual(LayerBlendingConfiguration.PhantoonSemiTransparent, blend.DisplayedConfiguration, "live change cannot mutate latched blend");
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
    }
}
