using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    /// <summary>
    /// When Baby Metroid finishes rising after feeding ($A9:F16F), Spawn_Hardcoded_PLM runs the
    /// invisible-wall clear's setup during EnemyMain, so Samus's movement later that frame
    /// meets open space. In the 13% movie Samus dashes through the wall's column on that
    /// frame; the port cleared it only in the PLM handler and stopped her against it.
    /// </summary>
    private static void VerifyBabyMetroidWallClearTiming()
    {
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        SuperMetroidRuntime runtime = CreateRetailRuntimeFixture(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(FixtureRoomHeaders.BigBoy);
        RoomLevelData level = runtime.LevelData!;
        const int WallColumn = 0x30, WallRow = 11;
        // The wall as Baby Metroid's earlier create request leaves it: rows 9-12 solid.
        for (int row = 9; row <= 12; row++)
            level.SetForegroundEntry(row * level.WidthInBlocks + WallColumn,
                RoomLevelWord.Create(0x00ff, 0, RoomCollisionType.SolidBlock).Raw);

        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var shitroid = (ShitroidEnemyState)typeof(RoomEnemySystem).GetField("_shitroid", flags)!.GetValue(runtime.Enemies)!;
        shitroid.Function = ShitroidAiFunction.RiseAfterFeeding;
        shitroid.StateTimer = 0;

        // A dash like the movie's: the wall's left edge is three pixels ahead and she moves 2.F000.
        SamusState samus = runtime.Samus!;
        samus.InputLocked = false;
        samus.Pose = SamusPoseId.MovingRightNormalPose;
        samus.XPosition = 0x02f9;
        samus.YPosition = 0x00bb;
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);
        samus.HorizontalSpeed.BaseSubspeed = 0x3000;
        samus.HorizontalSpeed.ExtraRunSpeed = 2;
        samus.HorizontalSpeed.ExtraRunSubspeed = 0xc000;
        runtime.Camera!.SetPosition(0x0200, 0x0000);
        runtime.StepFrame(unchecked((ushort)(SnesButton.Right | SnesButton.B)));

        AssertEqual(ShitroidAiFunction.HoverNearSamus, shitroid.Function, "Baby Metroid finished rising this frame");
        AssertEqual(RoomCollisionType.Air, level.GetCollisionBlock(WallColumn, WallRow).CollisionType,
            "the wall is cleared");
        AssertEqual(SamusPoseId.MovingRightNormalPose, samus.Pose, "Samus keeps running");
        AssertTrue(samus.XPosition > 0x02fb, "Samus moves into the cleared column in the same frame");
        Console.WriteLine("  Baby Metroid wall clear: the wall is gone before Samus moves that frame.");
    }
}
