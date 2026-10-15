using SuperMetroid.Core.Input;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using static SuperMetroid.Verification.Rooms.RoomDefinitionIntegrationVerification;
using SuperMetroid.Core.Enemies.Gunship;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Runtime;
namespace SuperMetroid.Verification.Areas.Ceres;

/// <summary>Checks station-load state and the gunship unlock boundary after the Ceres escape.</summary>
internal static class PostCeresLandingHandOffVerification
{
    /// <summary>
    /// Confirms the post-Ceres station load and gunship hand-off boundaries against the
    /// native word ownership: fractions survive the load, the hatch placement writes
    /// both Samus X words, and the unlock affects only the following alpha.
    /// </summary>
    internal static void VerifyPostCeresLandingHandOff()
    {
        var runtime = CreateRetailRuntimeFixture(
            SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(
                Path.GetFullPath("Super Metroid.smc")));
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.System.SetEvent(EventNumber.ZebesTimebombSet);
        ScrollBoundaryCamera ceresCamera = runtime.Room.LoadedCamera;
        ceresCamera.SetDoorTransitionPosition(ceresCamera.PixelPosition, FixedPosition<RoomPixels>.FromWords(0, 0xa000, 0, 0x1234));
        // The escape ends with Samus running: the 13% capture enters the load carrying
        // base speed $1.8000 and extra run speed $0.8000, plus inventory from Ceres.
        SamusState escaped = runtime.Player.IntroducedSamus;
        escaped.HorizontalSpeed.BaseSpeed = 1;
        escaped.HorizontalSpeed.BaseSubspeed = 0x8000;
        escaped.HorizontalSpeed.ExtraRunSpeed = 0;
        escaped.HorizontalSpeed.ExtraRunSubspeed = 0x8000;
        escaped.Missiles = 7;
        escaped.MaxMissiles = 10;
        runtime.RoomLoader.InitializePostCeresZebesRoom();
        // $82:8057 InitializeSamus clears Samus RAM ($0A02 on) but not the inventory below it.
        SamusState landed = runtime.Player.IntroducedSamus;
        AssertEqual((0u, (ushort)0, (ushort)0),
            (landed.HorizontalSpeed.BaseFixed, landed.HorizontalSpeed.ExtraRunSpeed, landed.HorizontalSpeed.ExtraRunSubspeed),
            "station load clears the escape's running speeds");
        AssertEqual(((ushort)7, (ushort)10), (landed.Missiles, landed.MaxMissiles),
            "station load keeps the inventory below Samus RAM");
        AssertEqual(SamusPoseId.ForwardFacingPowerSuitPose, landed.Pose, "station load clears Samus's pose");
        // $80:C470/$C479 write only the integer layer-one words.
        AssertEqual(((ushort)0xa000, (ushort)0x1234),
            (runtime.Room.LoadedCamera.Position.X.Fraction, runtime.Room.LoadedCamera.Position.Y.Fraction),
            "station load keeps the previous camera fractions");

        // Hold Left+B throughout: locked handlers ignore it until the ship unlocks Samus.
        const ushort input = (ushort)(SnesButton.Left | SnesButton.B);
        int frames = 0;
        while (runtime.Enemies.GunshipBehavior.LastGunshipEvent != GunshipFrameEvent.LandingCompleted &&
               frames++ < PostCeresLandingMaximumFrames)
        {
            ushort cameraX = runtime.Room.LoadedCamera.PixelPosition.X;
            runtime.StepFrame(input);
            if (runtime.Enemies.GunshipBehavior.LastGunshipEvent == GunshipFrameEvent.LandingPadOpened)
                AssertEqual(cameraX, runtime.Room.LoadedCamera.PixelPosition.X,
                    "hatch placement writes SamusPreviousXPosition, so the camera does not scroll");
        }
        AssertEqual(GunshipFrameEvent.LandingCompleted, runtime.Enemies.GunshipBehavior.LastGunshipEvent,
            "post-Ceres landing completes");
        // $A2:A987 runs in EnemyMain, after this frame's locked current-state handler.
        AssertEqual((byte)0, (byte)runtime.Player.IntroducedSamus.Pose, "unlock frame keeps the locked alpha's forward pose");
        ushort unlockedX = runtime.Player.IntroducedSamus.Position.X.Whole;
        runtime.StepFrame(input);
        AssertEqual(SamusPoseId.TurningRightToLeftPose, runtime.Player.IntroducedSamus.Pose, "first unlocked alpha accepts the held turn");
        AssertEqual(unlockedX, runtime.Player.IntroducedSamus.Position.X.Whole, "the turn carries no speed from the Ceres escape");

        Console.WriteLine("  Post-Ceres hand-off: station-load fractions, hatch X words and unlock alpha agree.");
    }
}
