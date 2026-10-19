using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using System.Reflection;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Poses;
namespace SuperMetroid.Verification.Bosses.MotherBrain;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Rooms;

/// <summary>Checks the frame ordering between Baby Metroid's wall-clear spawn and Samus's movement in the escape room.</summary>
internal static class BabyMetroidWallClearTimingVerification
{
    /// <summary>
    /// When Baby Metroid finishes rising after feeding ($A9:F16F), Spawn_Hardcoded_PLM runs the
    /// invisible-wall clear's setup during EnemyMain, so Samus's movement later that frame
    /// meets open space. In the 13% movie Samus dashes through the wall's column on that
    /// frame; the port cleared it only in the PLM handler and stopped her against it.
    /// </summary>
    internal static void VerifyBabyMetroidWallClearTiming()
    {
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        SuperMetroidRuntime runtime = CreateRetailRuntimeFixture();
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.BigBoy);
        RoomLevelData level = runtime.Room.LoadedLevelData;
        const int WallColumn = 0x30, WallRow = 11;
        // The wall as Baby Metroid's earlier create request leaves it: rows 9-12 solid.
        for (int row = 9; row <= 12; row++)
            level.SetForegroundEntry(row * level.Size.X + WallColumn,
                RoomLevelWord.Create(0x00ff, 0, RoomCollisionType.SolidBlock).Raw);

        var shitroid = (ShitroidEnemyState)(runtime.Enemies.ShitroidBehavior._shitroid ?? throw new InvalidOperationException("Verification requires runtime.Enemies.ShitroidBehavior._shitroid."));
        (shitroid ?? throw new InvalidOperationException("Verification requires shitroid.")).Function = ShitroidAiFunction.RiseAfterFeeding;
        shitroid.StateTimer = NativeCountdown.Start(0);

        // A dash like the movie's: the wall's left edge is three pixels ahead and she moves 2.F000.
        SamusState samus = runtime.Player.IntroducedSamus;
        samus.InputLocked = false;
        samus.Pose = SamusPoseId.MovingRightNormalPose;
        samus.Position = samus.Position.WithWhole(new(0x02f9, 0x00bb));
        samus.PoseMetadata.RefreshCollisionRadii();
        samus.Presentation.InitializeAnimation();
        samus.HorizontalSpeed.BaseSubspeed = 0x3000;
        samus.HorizontalSpeed.ExtraRunSpeed = 2;
        samus.HorizontalSpeed.ExtraRunSubspeed = 0xc000;
        runtime.Room.LoadedCamera.SetPosition(new Position16<RoomPixels>(0x0200, 0x0000));
        runtime.StepFrame(unchecked((SnesButton.Right | SnesButton.B)));

        AssertEqual(ShitroidAiFunction.HoverNearSamus, shitroid.Function, "Baby Metroid finished rising this frame");
        AssertEqual(RoomCollisionType.Air, level.GetCollisionBlock(new(WallColumn, WallRow)).CollisionType,
            "the wall is cleared");
        AssertEqual(SamusPoseId.MovingRightNormalPose, samus.Pose, "Samus keeps running");
        AssertTrue(samus.Position.X.Whole > 0x02fb, "Samus moves into the cleared column in the same frame");
        Console.WriteLine("  Baby Metroid wall clear: the wall is gone before Samus moves that frame.");
    }

    /// <summary>Runs the same-frame wall-clear regression through the verification command host.</summary>
    [VerificationCommand("--baby-metroid-wall-clear-timing")]
    internal static int BabyMetroidWallClearTimingCommand()
    {
        Suite(nameof(VerifyBabyMetroidWallClearTiming), () => VerifyBabyMetroidWallClearTiming());
        return 0;
    }
}
