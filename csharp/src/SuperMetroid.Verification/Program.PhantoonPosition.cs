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
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Runtime;
using static RetailRuntimeFixtureVerification;

/// <summary>Represents phantoon position verification.</summary>
internal static class PhantoonPositionVerification
{
    /// <summary>Verifies phantoon position.</summary>
    internal static void VerifyPhantoonPosition()
    {
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        // The native NTSC negative caps must hold at -8 and -6 pixels/frame.
        // PAL thresholds used here previously allowed continued acceleration.
        var capEnemies = new RoomEnemySystem();
        var capBody = capEnemies.SlotTable.Slots[0];
        var capTentacles = capEnemies.SlotTable.Slots[2];
        var capState = new PhantoonEnemyState(capBody) { Tentacles = capTentacles };
        capBody.Position = capBody.Position.WithWhole(new(128, 128));
        capTentacles.VariableC = 0xf800;
        capTentacles.VariableD = 0xfa00;
        PhantoonBehavior.MovePhantoonInSwoop(capBody, capState, new SamusState { Position = FixedPosition<RoomPixels>.FromWhole(new(0, 128)) }, false);
        AssertEqual((ushort)0xf800, capTentacles.VariableC, "Native NTSC negative swoop X cap");
        AssertEqual((ushort)0xfa00, capTentacles.VariableD, "Native NTSC negative swoop Y cap");
        AssertEqual((ushort)120, capBody.Position.X.Whole, "Native capped swoop X displacement");
        AssertEqual((ushort)122, capBody.Position.Y.Whole, "Native capped swoop Y displacement");
        foreach (ushort timer in new ushort[] { 2, 1, 0 })
        {
            var boundary = CreateRetailRuntimeFixture(bus);
            boundary.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
            boundary.RoomLoader.InitializeStartingCeresRoom();
            boundary.RoomLoader.InitializeCeresStartSamus();
            boundary.LoadCartridgeRoomForDebug(SuperMetroid.Core.Rooms.RoomHeaderPointers.Phantoon);
            var state = boundary.Enemies.PhantoonBehavior.Phantoon!;
            state.Body.VariableF = (ushort)PhantoonAiFunction.MoveInFigureEightThenOpenEye;
            state.Eye!.VariableA = timer;
            boundary.StepFrame(0);
            AssertEqual((ushort)(timer == 2 ? PhantoonAiFunction.MoveInFigureEightThenOpenEye : PhantoonAiFunction.NoOperation),
                state.Body.VariableF, $"Native figure-eight DEC/BEQ/BPL boundary from {timer}");
        }
        var runtime = CreateRetailRuntimeFixture(bus, playerInvincibilityEnabled: true);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(0xcd13);
        AssertEqual((ushort)120, runtime.Enemies.PhantoonBehavior.Phantoon!.Body.VariableE,
            "Original NTSC Phantoon initializer waits 120 frames, not PAL's 96");
        var samus = runtime.Player.Samus!;
        samus.Position = samus.Position.WithWhole(new(40, 184));
        samus.InputLocked = true;
        var positions = new HashSet<(ushort X, ushort Y)>();
        for (int frame = 0; frame < 1400; frame++)
        {
            var phantoon = runtime.Enemies.PhantoonBehavior.Phantoon!;
            ushort displayedX = phantoon.Bg2Scroll.X;
            ushort displayedY = phantoon.Bg2Scroll.Y;
            ushort displayedBodyX = (ushort)(phantoon.Body.Position.X.Whole - runtime.Room.Camera!.PixelPosition.X);
            ushort displayedBodyY = (ushort)(phantoon.Body.Position.Y.Whole - runtime.Room.Camera.PixelPosition.Y);
            runtime.StepFrame(0);
            AssertEqual(phantoon.Bg2Scroll, runtime.BackgroundScroll.Bg2Scroll,
                $"Phantoon live BG2 handoff frame {frame}");
            if (frame == 0) continue;
            var layer = (OrdinaryGameplayRenderLayer)GameplayDisplayCapture.CaptureOrdinaryBase(runtime).Layers[0];
            AssertEqual(displayedX, layer.Registers.Bg2Scroll.X, "Phantoon captured horizontal register follows accepted NMI");
            AssertEqual(displayedY, layer.Registers.Bg2Scroll.Y, "Phantoon captured vertical register follows accepted NMI");
            // $A7:CEA6 anchors the BG2 image's (40,40) origin to the body. Assert the
            // rendered anchor against the physical body, not merely two copied fields.
            if (phantoon.Eye!.Parameter1 == 0)
            {
                AssertEqual(displayedBodyX, unchecked((ushort)(40 - layer.Registers.Bg2Scroll.X)),
                    "Phantoon displayed BG2 anchor shares the collision body's horizontal position");
                AssertEqual(displayedBodyY, unchecked((ushort)(40 - layer.Registers.Bg2Scroll.Y)),
                    "Phantoon displayed BG2 anchor shares the collision body's vertical position");
            }
            positions.Add((phantoon.Body.Position.X.Whole, phantoon.Body.Position.Y.Whole));
        }
        AssertTrue(positions.Count > 20, "Phantoon test includes moving body positions, not only stationary introduction");
        Console.WriteLine($"  Phantoon: 1400 live/NMI/capture frames follow {positions.Count} distinct body positions.");
    }

    /// <summary>Phantoon position command.</summary>
    [VerificationCommand("--phantoon-position")]
    internal static int PhantoonPositionCommand()
    {
        Suite(nameof(VerifyPhantoonPosition), () => VerifyPhantoonPosition());
        return 0;
    }
}
