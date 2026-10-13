using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using static SuiteTimingVerification;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Rendering;
using static RetailRuntimeFixtureVerification;

/// <summary>Checks Norfair Ridley's shared FX acid rise and death drain through room rendering.</summary>
internal static class RidleyAcidVerification
{
    /// <summary>Runs native palette reveal and death phases, checking acid targets and visible lower-arena pixels.</summary>
    internal static void VerifyRidleyAcid()
    {
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(bus, playerInvincibilityEnabled: true);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(0xb32e);
        var fx = runtime.RoomLayer3Fx;
        AssertEqual(RoomFxType.Acid, fx.Type, "final Ridley room authors acid, not lava");
        AssertEqual((ushort)528, fx.BaseYPosition, "acid starts below the arena");
        var state = runtime.Enemies.RidleyEncounter.Ridley!;
        // The shared Ridley state has no Ceres Baby program in Norfair. The
        // global actor-draw seam must not invoke Ceres's private graphics hook.
        var expectedEmptyOam = new OamBuffer();
        expectedEmptyOam.BeginFrame();
        expectedEmptyOam.FinalizeFrame();
        var ridleyOam = new OamBuffer();
        ridleyOam.BeginFrame();
        runtime.Enemies.CeresRidleyBehavior.DrawCeresRidleyImmediateBabyAndDoor(ridleyOam, new(0, 256));
        runtime.Enemies.CeresRidleyBehavior.DrawCeresRidleyPostEnemyHook(ridleyOam, new(0, 256));
        ridleyOam.FinalizeFrame();
        AssertTrue(ridleyOam.ImageEquals(expectedEmptyOam),
            "Norfair Ridley does not draw the Ceres Baby or arena-door overlay");
        AssertEqual((ushort)0, state.BabyInstruction,
            "Norfair Ridley never advances an uninitialized Ceres Baby program");
        // Enter the native palette-reveal phase, then let its real ROM table reach
        // the terminator. This does not inject a liquid command into the fixture.
        state.Function = RidleyAiFunction.WaitBeforeLiftoff;
        state.FunctionTimer = NativeCountdown.Start(0);
        state.FadePaletteOffset = 0;
        for (int frame = 0; frame < 200 && state.Function == RidleyAiFunction.WaitBeforeLiftoff; frame++)
            runtime.Enemies.StepFrame(new RoomEnemyFrame { Camera = new(0, 256), TimeIsFrozen = false, Samus = runtime.Player.Samus, Level = runtime.Room.LevelData });
        AssertEqual((ushort)440, fx.TargetYPosition, "native reveal writes acid rise target to shared FX");
        AssertEqual(unchecked((ushort)-96), fx.PackedYVelocity, "native reveal writes rising velocity");
        AssertEqual((ushort)32, fx.Timer.RawValue, "native reveal writes rise delay once");
        for (int frame = 0; frame < 400; frame++) fx.Step(bus, runtime.Vram, new RoomLayer3FxFrame { Camera = new Position16<RoomPixels>(0, 256), TimeIsFrozen = false, MainGameLoopCarry = true });
        AssertEqual((ushort)440, fx.BaseYPosition, "acid reaches native visible battle height");
        AssertAcidVisible(true);
        state.Function = RidleyAiFunction.NorfairDeathExplosions;
        state.FunctionTimer = NativeCountdown.Start(0);
        runtime.Enemies.StepFrame(new RoomEnemyFrame { Camera = new(0, 256), TimeIsFrozen = false, Samus = runtime.Player.Samus, Level = runtime.Room.LevelData });
        AssertEqual((ushort)528, fx.TargetYPosition, "native death roar writes acid drain target");
        AssertEqual((ushort)64, fx.PackedYVelocity, "native death writes draining velocity");
        AssertEqual((ushort)1, fx.Timer.RawValue, "native death writes drain delay once");
        for (int frame = 0; frame < 400; frame++) fx.Step(bus, runtime.Vram, new RoomLayer3FxFrame { Camera = new Position16<RoomPixels>(0, 256), TimeIsFrozen = false, MainGameLoopCarry = true });
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

    /// <summary>Runs the focused Ridley acid FX reproduction.</summary>
    [VerificationCommand("--ridley-acid")]
    internal static int RidleyAcidCommand()
    {
        Suite(nameof(VerifyRidleyAcid), () => VerifyRidleyAcid());
        return 0;
    }
}
