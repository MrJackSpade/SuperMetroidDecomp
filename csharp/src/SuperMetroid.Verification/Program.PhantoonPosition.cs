using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    private static void VerifyPhantoonPosition()
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        // The native NTSC negative caps must hold at -8 and -6 pixels/frame.
        // PAL thresholds used here previously allowed continued acceleration.
        var capEnemies = new RoomEnemySystem();
        var capBody = capEnemies.Slots[0];
        var capTentacles = capEnemies.Slots[2];
        var capState = new PhantoonEnemyState(capBody) { Tentacles = capTentacles };
        capBody.XPosition = capBody.YPosition = 128;
        capTentacles.VariableC = 0xf800;
        capTentacles.VariableD = 0xfa00;
        typeof(RoomEnemySystem).GetMethod("MovePhantoonInSwoop",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
            .Invoke(null, [capBody, capState, new SamusState { YPosition = 128 }, false]);
        AssertEqual((ushort)0xf800, capTentacles.VariableC, "Native NTSC negative swoop X cap");
        AssertEqual((ushort)0xfa00, capTentacles.VariableD, "Native NTSC negative swoop Y cap");
        AssertEqual((ushort)120, capBody.XPosition, "Native capped swoop X displacement");
        AssertEqual((ushort)122, capBody.YPosition, "Native capped swoop Y displacement");
        foreach (ushort timer in new ushort[] { 2, 1, 0 })
        {
            var boundary = new SuperMetroidRuntime(bus);
            boundary.InitializeHud(HudSnapshot.CeresDebug);
            boundary.InitializeStartingCeresRoom();
            boundary.InitializeCeresStartSamus();
            boundary.LoadCartridgeRoomForDebug(SuperMetroid.Core.Rooms.RoomHeaderPointers.Phantoon);
            var state = boundary.Enemies.Phantoon!;
            state.Body.VariableF = (ushort)PhantoonAiFunction.MoveInFigureEightThenOpenEye;
            state.Eye!.VariableA = timer;
            boundary.StepFrame(0);
            AssertEqual((ushort)(timer == 2 ? PhantoonAiFunction.MoveInFigureEightThenOpenEye : PhantoonAiFunction.NoOperation),
                state.Body.VariableF, $"Native figure-eight DEC/BEQ/BPL boundary from {timer}");
        }
        var runtime = new SuperMetroidRuntime(bus, playerInvincibilityEnabled: true);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(0xcd13);
        AssertEqual((ushort)120, runtime.Enemies.Phantoon!.Body.VariableE,
            "Original NTSC Phantoon initializer waits 120 frames, not PAL's 96");
        var samus = runtime.Samus!;
        samus.XPosition = 40;
        samus.YPosition = 184;
        samus.InputLocked = true;
        var positions = new HashSet<(ushort X, ushort Y)>();
        for (int frame = 0; frame < 1400; frame++)
        {
            var phantoon = runtime.Enemies.Phantoon!;
            ushort displayedX = phantoon.Bg2HorizontalScroll;
            ushort displayedY = phantoon.Bg2VerticalScroll;
            ushort displayedBodyX = (ushort)(phantoon.Body.XPosition - runtime.Camera!.XPosition);
            ushort displayedBodyY = (ushort)(phantoon.Body.YPosition - runtime.Camera.YPosition);
            runtime.StepFrame(0);
            AssertEqual(phantoon.Bg2HorizontalScroll, runtime.BackgroundScroll.Bg2HorizontalScroll,
                $"Phantoon live BG2 horizontal handoff frame {frame}");
            AssertEqual(phantoon.Bg2VerticalScroll, runtime.BackgroundScroll.Bg2VerticalScroll,
                $"Phantoon live BG2 vertical handoff frame {frame}");
            if (frame == 0) continue;
            var layer = (OrdinaryGameplayRenderLayer)GameplayDisplayCapture.CaptureOrdinaryBase(runtime).Layers[0];
            AssertEqual(displayedX, layer.Registers.Bg2X, "Phantoon captured horizontal register follows accepted NMI");
            AssertEqual(displayedY, layer.Registers.Bg2Y, "Phantoon captured vertical register follows accepted NMI");
            // $A7:CEA6 anchors the BG2 image's (40,40) origin to the body. Assert the
            // rendered anchor against the physical body, not merely two copied fields.
            if (phantoon.Eye!.Parameter1 == 0)
            {
                AssertEqual(displayedBodyX, unchecked((ushort)(40 - layer.Registers.Bg2X)),
                    "Phantoon displayed BG2 anchor shares the collision body's horizontal position");
                AssertEqual(displayedBodyY, unchecked((ushort)(40 - layer.Registers.Bg2Y)),
                    "Phantoon displayed BG2 anchor shares the collision body's vertical position");
            }
            positions.Add((phantoon.Body.XPosition, phantoon.Body.YPosition));
        }
        AssertTrue(positions.Count > 20, "Phantoon test includes moving body positions, not only stationary introduction");
        Console.WriteLine($"  Phantoon: 1400 live/NMI/capture frames follow {positions.Count} distinct body positions.");
    }
}
