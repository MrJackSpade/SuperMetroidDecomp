using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.World;
using SuperMetroid.TestSupport.Assertions;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Rooms;

/// <summary>
/// The Dachora room's main moves the camera toward its shaft (#2293): loaded from its state, the room composes the main,
/// which moves the camera three whole pixels right on each update while the shaft's top screen stands open and the
/// camera lies above Y 1280 and left of X 896, and leaves it alone otherwise; the fractions, the vertical axis and the
/// ideal position stay as they were.
/// </summary>
internal static class DachoraShaftCameraVerification
{
    /// <summary>The shaft's top screen, column 4 of the room's second screen row.</summary>
    private static readonly Position16<RoomScreens> ShaftTop = new(4, 1);

    /// <summary>
    /// Loads the Dachora room, then runs its main update by update from cameras with the shaft closed, open with the camera
    /// aligned to its top, and open, and from cameras at each bound.
    /// </summary>
    internal static void VerifyDachoraShaftCameraNudge()
    {
        SuperMetroidRuntime runtime = CreateRetailRuntimeFixture();
        runtime.HudSetup.InitializeHud(HudSnapshot.NewGame);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus(runtime.Room.Loaded);
        runtime.LoadCartridgeRoomForDebug(RoomDefinitions.Dachora);
        RoomScope room = runtime.Room.Scope ?? throw new InvalidOperationException("The debug load adopted no scope.");
        VerificationAssertions.AssertTrue(room.Main is DachoraShaftCameraMain, "the Dachora room's state composes the shaft camera main");
        ScrollBoundaryCamera camera = room.Camera;

        camera.Scrolls.SetRoomScreen(ShaftTop, RoomScrollState.Closed);
        AssertCameraXs(runtime, camera, startX: 0x0370, y: 0x0400, [0x0370, 0x0370], "a closed shaft leaves the camera");
        camera.Scrolls.SetRoomScreen(ShaftTop, RoomScrollState.OpenAlignedToTop);
        AssertCameraXs(runtime, camera, startX: 0x0370, y: 0x0400, [0x0370, 0x0370],
            "a shaft open with the camera aligned to its top leaves the camera");

        camera.Scrolls.SetRoomScreen(ShaftTop, RoomScrollState.Open);
        AssertCameraXs(runtime, camera, startX: 0x0370, y: 0x0400,
            [0x0373, 0x0376, 0x0379, 0x037C, 0x037F, 0x0382, 0x0382, 0x0382],
            "an open shaft moves the camera three pixels right each update until it reaches X 896");
        AssertCameraXs(runtime, camera, startX: 0x037F, y: 0x0400, [0x0382], "a camera one pixel left of X 896 moves");
        AssertCameraXs(runtime, camera, startX: 0x0380, y: 0x0400, [0x0380], "a camera at X 896 stays");
        AssertCameraXs(runtime, camera, startX: 0x0000, y: 0x04FF, [0x0003, 0x0006], "a camera one pixel above Y 1280 moves");
        AssertCameraXs(runtime, camera, startX: 0x0000, y: 0x0500, [0x0000], "a camera at Y 1280 stays");
        Console.WriteLine("  Dachora shaft camera: the open shaft moves a camera above Y 1280 three pixels right each update until X 896.");
    }

    /// <summary>
    /// Places <paramref name="camera"/> at <paramref name="startX"/>, <paramref name="y"/> with fractions on both axes, runs
    /// the room's main once for each expected X and checks the camera after each update.
    /// </summary>
    private static void AssertCameraXs(SuperMetroidRuntime runtime, ScrollBoundaryCamera camera, ushort startX, ushort y,
        ushort[] expectedXs, string because)
    {
        global::SuperMetroid.Core.PrivateState.Set(camera, "Position", FixedPosition<RoomPixels>.FromStoredParts(startX, 0x1234, y, 0x5678));
        Position16<RoomPixels> ideal = camera.IdealPosition;
        for (int update = 0; update < expectedXs.Length; update++)
        {
            RunRoomMain(runtime);
            VerificationAssertions.AssertEqual(FixedPosition<RoomPixels>.FromStoredParts(expectedXs[update], 0x1234, y, 0x5678),
                camera.Position, $"{because}: update {update + 1} from X {startX}, Y {y}");
            VerificationAssertions.AssertEqual(ideal, camera.IdealPosition, $"{because}: the ideal position is unchanged");
        }
    }

    /// <summary>Runs the loaded room's main once, as the gameplay update does.</summary>
    private static void RunRoomMain(SuperMetroidRuntime runtime)
    {
        RoomScope room = runtime.Room.Scope ?? throw new InvalidOperationException("The runtime holds no room.");
        IRoomMain main = room.Main ?? throw new InvalidOperationException("The room composed no main.");
        main.Run(new RoomMainUpdate
        {
            Room = room,
            BackgroundScroll = runtime.BackgroundScroll, RandomNumber = runtime.System.RandomNumber,
            Escape = runtime.EscapeEffects,
            Player = runtime.Player,
            FrameCounter = runtime.Clock.Counter,
            TimeIsFrozen = runtime.Freeze.TimeIsFrozen,
            Display = runtime.Display,
            AllowCeresElevatorDeparture = true,
        });
    }

    /// <summary>Runs the Dachora shaft camera check.</summary>
    [VerificationCommand("--dachora-shaft-camera")]
    internal static int DachoraShaftCameraCommand()
    {
        Suite(nameof(VerifyDachoraShaftCameraNudge), () => VerifyDachoraShaftCameraNudge());
        return 0;
    }
}
