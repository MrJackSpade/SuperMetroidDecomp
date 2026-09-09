using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Rendering;

internal static partial class Program
{
    private static void VerifyRidleyAcid()
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = new SuperMetroidRuntime(bus, playerInvincibilityEnabled: true);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(0xb32e);
        var fx = runtime.RoomLayer3Fx;
        AssertEqual(RoomFxType.Acid, fx.Type, "final Ridley room authors acid, not lava");
        AssertEqual((ushort)528, fx.BaseYPosition, "acid starts below the arena");
        var state = runtime.Enemies.Ridley!;
        // Enter the native palette-reveal phase, then let its real ROM table reach
        // the terminator. This does not inject a liquid command into the fixture.
        state.Function = RidleyAiFunction.WaitBeforeLiftoff;
        state.FunctionTimer = 0;
        state.FadePaletteOffset = 0;
        for (int frame = 0; frame < 200 && state.Function == RidleyAiFunction.WaitBeforeLiftoff; frame++)
            runtime.Enemies.StepFrame(0, 256, false, runtime.Samus, level: runtime.LevelData);
        AssertEqual((ushort)440, fx.TargetYPosition, "native reveal writes acid rise target to shared FX");
        AssertEqual(unchecked((ushort)-96), fx.PackedYVelocity, "native reveal writes rising velocity");
        AssertEqual((ushort)32, fx.Timer, "native reveal writes rise delay once");
        for (int frame = 0; frame < 400; frame++) fx.Step(bus, runtime.Vram, 0, 256, false);
        AssertEqual((ushort)440, fx.BaseYPosition, "acid reaches native visible battle height");
        AssertAcidVisible(true);
        state.Function = RidleyAiFunction.NorfairDeathExplosions;
        state.FunctionTimer = 0;
        runtime.Enemies.StepFrame(0, 256, false, runtime.Samus, level: runtime.LevelData);
        AssertEqual((ushort)528, fx.TargetYPosition, "native death roar writes acid drain target");
        AssertEqual((ushort)64, fx.PackedYVelocity, "native death writes draining velocity");
        AssertEqual((ushort)1, fx.Timer, "native death writes drain delay once");
        for (int frame = 0; frame < 400; frame++) fx.Step(bus, runtime.Vram, 0, 256, false);
        AssertEqual((ushort)528, fx.BaseYPosition, "acid drains below arena after death");
        AssertAcidVisible(false);
        Console.WriteLine("Ridley acid: retail reveal raises 528 -> 440; death roar drains 440 -> 528 through shared FX.");

        void AssertAcidVisible(bool expectedVisible)
        {
            var memory = PpuMemorySnapshot.Capture(runtime.Vram, runtime.Cgram, runtime.DisplayedOam);
            var effect = SnesGameplayFrameRenderer.CaptureRoomLayer3Fx(fx.CaptureForDisplay()!.Value);
            var baseline = SoftwareLayeredSnapshotRenderer.Render(new(memory, [], 3, 15));
            var actual = SoftwareLayeredSnapshotRenderer.Render(new(memory,
                effect is null ? [] : new RenderLayer[] { effect }, 3, 15));
            AssertEqual(expectedVisible, !actual.SequenceEqual(baseline), "acid visibility at lower-arena camera");
            AssertTrue(actual.Take(32 * 256).SequenceEqual(baseline.Take(32 * 256)), "acid does not overwrite HUD");
        }
    }
}
