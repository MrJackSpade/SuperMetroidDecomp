using SuperMetroid.Core.Game;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    // #1269: releasing every input during a wall jump falls back to spin-jump art, and movement
    // type $14 selects command six (kill X speed). HandleSamusPoseChange sets carry only when
    // InitializeSamusPose replaces the installed pose, and UpdateSamusPose then skips the
    // command: with Space Jump the spin is promoted and the launch speed carries; without it
    // the pose is kept and command six clears the speed.
    private static void VerifyWallJumpSpinExit()
    {
        Confirm(SamusEquipmentFlags.SpaceJump, speedKept: true);
        Confirm(0, speedKept: false);
        Console.WriteLine("Wall-jump spin exit: command six runs unless pose initialization promotes the spin.");

        static void Confirm(SamusEquipmentFlags items, bool speedKept)
        {
            var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
            var runtime = CreateRetailRuntimeFixture(bus);
            runtime.InitializeHud(HudSnapshot.CeresDebug);
            runtime.InitializeStartingCeresRoom();
            runtime.InitializeCeresStartSamus();
            runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.LandingSite);
            var level = runtime.LevelData!;
            for (int y = 16; y < 36; y++)
            for (int x = 16; x < 48; x++)
            {
                int index = y * level.WidthInBlocks + x;
                level.SetForegroundEntry(index, RoomLevelWord.Create(0, 0, RoomCollisionType.Air).Raw);
                level.SetBehavior(index, 0);
            }
            var samus = runtime.Samus!;
            samus.InputLocked = false;
            samus.EquippedItems = (ushort)items;
            samus.Pose = SamusPoseId.WallJumpRightPose;
            samus.XPosition = 512;
            samus.YPosition = 400;
            samus.RefreshCollisionRadii(bus);
            samus.InitializeAnimation(bus);
            samus.Kinematics.YDirection = 1;
            samus.Kinematics.YSpeed = 3;
            samus.HorizontalSpeed.BaseSpeed = 1;
            samus.HorizontalSpeed.BaseSubspeed = 0x6000;
            samus.HorizontalSpeed.AccelerationMode = 2;

            runtime.StepFrame(0);
            string label = speedKept ? "Space Jump promotion" : "plain spin jump";
            AssertTrue(speedKept
                    ? samus.Pose is SamusPoseId.SpaceJumpRightPose or SamusPoseId.SpaceJumpLeftPose
                    : samus.Pose is SamusPoseId.SpinJumpRightPose or SamusPoseId.SpinJumpLeftPose,
                $"{label}: input-free wall jump falls back to the expected spin art");
            AssertEqual(speedKept ? (ushort)1 : (ushort)0, samus.HorizontalSpeed.BaseSpeed, $"{label}: base speed");
            AssertEqual(speedKept ? (ushort)0x6000 : (ushort)0, samus.HorizontalSpeed.BaseSubspeed, $"{label}: base subspeed");
            AssertEqual(speedKept ? (ushort)2 : (ushort)0, samus.HorizontalSpeed.AccelerationMode, $"{label}: acceleration mode");
        }
    }
}
