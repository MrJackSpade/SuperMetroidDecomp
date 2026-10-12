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

internal static class MotherBrainRaiseCounterVerification
{
    // #1269: Function_MotherBrainBody_FakeDeath_Ascent_RaiseMotherBrain ($A9:8E4D) raises
    // Mother Brain only when NMI_FrameCounter ($05B6) & 3 is zero. The port gated on the
    // separate 8-bit $05B5, which is not aligned with it: in the 100% movie the ascent began
    // one update early, at $05B5=$68 rather than $05B6=$200.
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

    [VerificationCommand("--mother-brain-raise-counter")]
    internal static int MotherBrainRaiseCounterCommand()
    {
        VerifyMotherBrainRaiseCounter();
        return 0;
    }
}
