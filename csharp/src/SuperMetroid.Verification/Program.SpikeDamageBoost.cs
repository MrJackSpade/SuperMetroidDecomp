using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    private static void VerifyMorphedSpikeRelease()
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = new SuperMetroidRuntime(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RunNmi(0, true);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(runtime.ActiveRoom!.Pointer, 0, 0);
        var level = runtime.LevelData!;
        for (int y = 0; y <= 16; y++)
        for (int x = 0; x < level.WidthInBlocks; x++)
            level.SetForegroundEntry(y * level.WidthInBlocks + x,
                y is 0 or 16 || x is 0 or 15 ? (ushort)0x8000 : (ushort)0);
        int hazardBlock = 10 * level.WidthInBlocks + 8;
        level.SetForegroundEntry(hazardBlock, 0x2000);
        level.SetBehavior(hazardBlock, SamusTerrainHazardRomData.DamagingSpikeAirBehavior);
        runtime.InitializeDebugGroundedSamus(128, 160, 16);
        var samus = runtime.Samus!;
        samus.InputLocked = false;
        samus.Health = 99;
        samus.EquippedItems = (ushort)SamusEquipmentFlags.MorphBall;
        samus.Pose = SamusPoseIds.MorphBallGroundRightPose;
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);
        samus.SetAnimationFrameFromSpecialHandler(0, 1);
        samus.XPosition = 128;
        samus.YPosition = 160;
        samus.Kinematics.XSubposition = samus.Kinematics.YSubposition = 0;

        // Original CPU contact=2, right-facing ball, lower inside point, delay zero.
        // Frame three releases direction with subpixel base speed still present. Type
        // four's command six selects the stationary pose and clears momentum that frame.
        (uint X, uint Y, byte Pose, uint Base)[] expected =
        [
            (0x00800000, 0x00a10000, SamusPoseIds.MorphBallGroundRightPose, 0),
            (0x007f4000, 0x009c0000, SamusPoseIds.MorphBallMovingLeftPose, 0xc000),
            (0x007f0000, 0x00971c00, SamusPoseIds.MorphBallMovingLeftPose, 0x4000),
            (0x007f0000, 0x00925400, SamusPoseIds.MorphBallGroundLeftPose, 0),
            (0x007e4000, 0x008da800, SamusPoseIds.MorphBallGroundLeftPose, 0),
            (0x007d8000, 0x00891800, SamusPoseIds.MorphBallGroundLeftPose, 0),
            (0x007cc000, 0x0084a400, SamusPoseIds.MorphBallGroundLeftPose, 0),
        ];
        for (int frame = 0; frame < expected.Length; frame++)
        {
            runtime.StepFrame(frame < 3 ? (ushort)0x0280 : (ushort)0x0080);
            AssertEqual(expected[frame], (samus.Kinematics.XFixed, samus.Kinematics.YFixed,
                samus.Pose, samus.HorizontalSpeed.BaseFixed), $"morphed spike release frame {frame}");
            AssertEqual(83, samus.Health, "spike contact damage");
            AssertEqual(9 - frame, samus.KnockbackTimer, "spike hurt timer");
        }
    }
}
