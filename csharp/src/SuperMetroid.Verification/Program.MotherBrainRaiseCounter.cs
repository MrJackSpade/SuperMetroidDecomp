using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Game;
using static RetailRuntimeFixtureVerification;

/// <summary>Checks which native frame counter gates Mother Brain's fake-death ascent.</summary>
internal static class MotherBrainRaiseCounterVerification
{
    /// <summary>
    /// Confirms $A9:8E4D raises Mother Brain only when $05B6 is divisible by four.
    /// Issue #1269 exposed an early ascent from gating on unrelated eight-bit $05B5.
    /// </summary>
    internal static void VerifyMotherBrainRaiseCounter()
    {
        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(bus, playerInvincibilityEnabled: true);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(FixtureRoomHeaders.MotherBrain);
        MotherBrainEnemyState state = runtime.Enemies.MotherBrainEncounter.MotherBrain
            ?? throw new InvalidOperationException("Mother Brain's room has no encounter state.");
        state.Function = MotherBrainBodyFunction.FakeDeathAscentRaiseMotherBrain;
        state.Body.Position = state.Body.Position.WithWhole(new(0x003b, 0x0117));
        var camera = runtime.Room.Camera!;

        void Step(byte counter8, ushort counter) => runtime.Enemies.StepFrame(new RoomEnemyFrame { Camera = camera.PixelPosition, TimeIsFrozen = false, Samus = runtime.Player.Samus, Level = runtime.Room.LevelData, NmiFrameCounter8 = counter8, NmiFrameCounter = counter });

        Step(0x68, 0x01ff);
        AssertEqual((ushort)0x0117, state.Body.Position.Y.Whole, "no raise while $05B6 & 3 is nonzero, whatever $05B5 holds");
        Step(0x69, 0x0200);
        AssertEqual((ushort)0x0115, state.Body.Position.Y.Whole, "the raise follows $05B6 & 3 == 0");
        Console.WriteLine("Mother Brain raise counter: the ascent gates on NMI_FrameCounter ($05B6).");
    }

    /// <summary>Runs the focused fake-death ascent counter check from the verification command line.</summary>
    [VerificationCommand("--mother-brain-raise-counter")]
    internal static int MotherBrainRaiseCounterCommand()
    {
        VerifyMotherBrainRaiseCounter();
        return 0;
    }
}
