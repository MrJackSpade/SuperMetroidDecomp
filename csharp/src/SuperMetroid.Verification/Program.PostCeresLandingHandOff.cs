using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    /// <summary>
    /// Confirms the post-Ceres station load and gunship hand-off boundaries against the
    /// native word ownership: fractions survive the load, the hatch placement writes
    /// both Samus X words, and the unlock affects only the following alpha.
    /// </summary>
    private static void VerifyPostCeresLandingHandOff()
    {
        var runtime = CreateRetailRuntimeFixture(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(
                Path.GetFullPath("Super Metroid.smc")));
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.System.SetEvent(EventNumber.ZebesTimebombSet);
        ScrollBoundaryCamera ceresCamera = runtime.Camera!;
        ceresCamera.SetDoorTransitionPosition(ceresCamera.XPosition, ceresCamera.YPosition, 0xa000, 0x1234);
        // The escape ends with Samus running: the 13% capture enters the load carrying
        // base speed $1.8000 and extra run speed $0.8000, plus inventory from Ceres.
        SamusState escaped = runtime.Samus!;
        escaped.HorizontalSpeed.BaseSpeed = 1;
        escaped.HorizontalSpeed.BaseSubspeed = 0x8000;
        escaped.HorizontalSpeed.ExtraRunSpeed = 0;
        escaped.HorizontalSpeed.ExtraRunSubspeed = 0x8000;
        escaped.Missiles = 7;
        escaped.MaxMissiles = 10;
        runtime.InitializePostCeresZebesRoom();
        // $82:8057 InitializeSamus clears Samus RAM ($0A02 on) but not the inventory below it.
        SamusState landed = runtime.Samus!;
        AssertEqual((0u, (ushort)0, (ushort)0),
            (landed.HorizontalSpeed.BaseFixed, landed.HorizontalSpeed.ExtraRunSpeed, landed.HorizontalSpeed.ExtraRunSubspeed),
            "station load clears the escape's running speeds");
        AssertEqual(((ushort)7, (ushort)10), (landed.Missiles, landed.MaxMissiles),
            "station load keeps the inventory below Samus RAM");
        AssertEqual((byte)0, landed.Pose, "station load clears Samus's pose");
        // $80:C470/$C479 write only the integer layer-one words.
        AssertEqual(((ushort)0xa000, (ushort)0x1234),
            (runtime.Camera!.XSubposition, runtime.Camera.YSubposition),
            "station load keeps the previous camera fractions");

        // Hold Left+B throughout: locked handlers ignore it until the ship unlocks Samus.
        const ushort input = (ushort)(SnesButton.Left | SnesButton.B);
        int frames = 0;
        while (runtime.Enemies.LastGunshipEvent != GunshipFrameEvent.LandingCompleted &&
               frames++ < PostCeresLandingMaximumFrames)
        {
            ushort cameraX = runtime.Camera.XPosition;
            runtime.StepFrame(input);
            if (runtime.Enemies.LastGunshipEvent == GunshipFrameEvent.LandingPadOpened)
                AssertEqual(cameraX, runtime.Camera.XPosition,
                    "hatch placement writes SamusPreviousXPosition, so the camera does not scroll");
        }
        AssertEqual(GunshipFrameEvent.LandingCompleted, runtime.Enemies.LastGunshipEvent,
            "post-Ceres landing completes");
        // $A2:A987 runs in EnemyMain, after this frame's locked current-state handler.
        AssertEqual((byte)0, runtime.Samus!.Pose, "unlock frame keeps the locked alpha's forward pose");
        ushort unlockedX = runtime.Samus.XPosition;
        runtime.StepFrame(input);
        AssertEqual((byte)0x25, runtime.Samus.Pose, "first unlocked alpha accepts the held turn");
        AssertEqual(unlockedX, runtime.Samus.XPosition, "the turn carries no speed from the Ceres escape");

        Console.WriteLine("  Post-Ceres hand-off: station-load fractions, hatch X words and unlock alpha agree.");
    }
}
