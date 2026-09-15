using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    private static void VerifySpikeShinesparkSuit()
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));

        // The ordinary spike method has a one-frame retained-suit window. Jump one
        // frame early installs the real Shinespark mover; Jump one frame late misses
        // completely. The middle case keeps C7 art and palette six after command one
        // has restored the ordinary movement handler: that mismatch is the technique.
        foreach ((int LaunchFrame, ShinesparkPhase Phase, byte Pose, ushort Palette) expected in new[]
        {
            (8, ShinesparkPhase.Windup, SamusPoseIds.ShinesparkWindupRightPose, (ushort)6),
            (9, ShinesparkPhase.Inactive, SamusPoseIds.ShinesparkWindupRightPose, (ushort)6),
            (10, ShinesparkPhase.Stored, SamusPoseIds.FallingRightPose, (ushort)1),
        })
        {
            SuperMetroidRuntime runtime = CreateSpikeSuitRuntime(bus, underwater: false);
            SamusState samus = runtime.Samus!;
            for (int frame = 0; frame < 15; frame++)
            {
                runtime.StepFrame(frame is 1 || frame == expected.LaunchFrame
                    ? (ushort)SnesButton.A
                    : (ushort)0);
            }
            AssertEqual(expected.Phase, samus.Shinespark.Phase,
                $"dry spike-suit launch frame {expected.LaunchFrame} movement ownership");
            AssertEqual(expected.Pose, samus.Pose,
                $"dry spike-suit launch frame {expected.LaunchFrame} final pose");
            AssertEqual(expected.Palette, samus.Shinespark.PaletteType,
                $"dry spike-suit launch frame {expected.LaunchFrame} palette state");
        }

        // Command zero does not merely ignore controller input. It replaces Samus's
        // complete alpha/beta pair, so an in-flight spike knockback must retain its
        // coordinates, animation cursor, movement owner, and stored-shine palette
        // while automatic Reserve recovery advances only global hurt timers.
        SuperMetroidRuntime frozenRuntime = CreateSpikeSuitRuntime(bus, underwater: true);
        SamusState frozenSamus = frozenRuntime.Samus!;
        for (int frame = 0; frame < 8; frame++)
            frozenRuntime.StepFrame(frame == 1 ? (ushort)SnesButton.A : (ushort)0);
        frozenSamus.Health = 0;
        frozenSamus.MaxHealth = 99;
        frozenSamus.ReserveEnergy = frozenSamus.MaxReserveEnergy = 2;
        frozenSamus.ReserveTankMode = 1;
        var shortRecovery = new SamusReserveAutoRecoveryState();
        shortRecovery.Begin(frozenSamus);
        frozenRuntime.GameplayTimeFrozen = true;
        uint frozenX = frozenSamus.Kinematics.XFixed;
        uint frozenY = frozenSamus.Kinematics.YFixed;
        ushort frozenAnimationFrame = frozenSamus.AnimationFrame;
        ushort frozenAnimationTimer = frozenSamus.AnimationFrameTimer;
        ushort frozenShineTimer = frozenSamus.SharedShineTimer;
        ushort frozenPalette = frozenSamus.Shinespark.PaletteType;
        ushort frozenKnockbackTimer = frozenSamus.KnockbackTimer;
        frozenRuntime.StepFrame(0, afterAcceptedNmi: () =>
            shortRecovery.StepAfterNmi(frozenSamus, frozenRuntime.NmiFrameCounter));
        AssertEqual((frozenX, frozenY),
            (frozenSamus.Kinematics.XFixed, frozenSamus.Kinematics.YFixed),
            "Reserve command zero freezes spike-knockback position");
        AssertEqual((frozenAnimationFrame, frozenAnimationTimer),
            (frozenSamus.AnimationFrame, frozenSamus.AnimationFrameTimer),
            "Reserve command zero freezes the unmorph animation cursor");
        AssertEqual((frozenShineTimer, frozenPalette),
            (frozenSamus.SharedShineTimer, frozenSamus.Shinespark.PaletteType),
            "Reserve command zero freezes stored-shine palette progression");
        AssertEqual(frozenKnockbackTimer - 1, frozenSamus.KnockbackTimer,
            "global hurt timer continues while Reserve owns Samus");
        AssertTrue(frozenSamus.KnockbackActive && frozenSamus.KnockbackDirection != 0,
            "Reserve command zero retains the interrupted knockback movement owner");

        // With no Gravity Suit, a 60-point automatic refill creates the wider native
        // underwater timing window. Freeze frames eight through ten plus post-recovery
        // Jump frame six retain the suit; adjacent setup/input frames do not.
        foreach ((int FreezeAfter, int LaunchAfter, bool RetainsSuit) expected in new[]
        {
            (7, 6, false),
            (8, 5, false),
            (8, 6, true),
            (8, 7, false),
            (9, 6, true),
            (10, 6, true),
            (11, 6, false),
        })
        {
            SamusState samus = RunReserveSpikeSuitCase(bus, expected.FreezeAfter, expected.LaunchAfter);
            bool retainedSuit = samus.Pose == SamusPoseIds.ShinesparkWindupRightPose &&
                samus.Shinespark.Phase == ShinesparkPhase.Inactive &&
                samus.Shinespark.PaletteType == 6 &&
                samus.SharedShineTimer != 0;
            AssertEqual(expected.RetainsSuit, retainedSuit,
                $"underwater Reserve suit freeze {expected.FreezeAfter}, launch {expected.LaunchAfter}");
        }

        Console.WriteLine(
            "Spike Shinespark Suit: dry one-frame window, command-zero freeze, and suitless underwater Reserve window.");
    }

    private static SamusState RunReserveSpikeSuitCase(
        SuperMetroidAddressSpace bus,
        int freezeAfter,
        int launchAfter)
    {
        SuperMetroidRuntime runtime = CreateSpikeSuitRuntime(bus, underwater: true);
        SamusState samus = runtime.Samus!;
        for (int frame = 0; frame < freezeAfter; frame++)
            runtime.StepFrame(frame == 1 ? (ushort)SnesButton.A : (ushort)0);

        samus.Health = 0;
        samus.MaxHealth = 99;
        samus.ReserveEnergy = 60;
        samus.MaxReserveEnergy = 100;
        samus.ReserveTankMode = 1;
        var recovery = new SamusReserveAutoRecoveryState();
        recovery.Begin(samus);
        runtime.GameplayTimeFrozen = true;
        while (recovery.IsActive)
        {
            ushort input = samus.ReserveEnergy == 1 && launchAfter == 0
                ? (ushort)SnesButton.A
                : (ushort)0;
            runtime.StepFrame(input, afterAcceptedNmi: () =>
            {
                if (recovery.StepAfterNmi(samus, runtime.NmiFrameCounter).Completed)
                    runtime.GameplayTimeFrozen = false;
            });
        }
        for (int frame = 1; frame < 25; frame++)
            runtime.StepFrame(frame == launchAfter ? (ushort)SnesButton.A : (ushort)0);
        return samus;
    }

    private static SuperMetroidRuntime CreateSpikeSuitRuntime(
        SuperMetroidAddressSpace bus,
        bool underwater)
    {
        var runtime = new SuperMetroidRuntime(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RunNmi(0, true);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(runtime.ActiveRoom!.Pointer, 0, 0);
        RoomLevelData level = runtime.LevelData!;
        runtime.InitializeDebugGroundedSamus(200, 166, 16);
        foreach (RoomEnemySlot enemy in runtime.Enemies.Slots)
            enemy.Clear();
        foreach (RoomEnemyProjectileSlot projectile in runtime.Enemies.EnemyProjectiles)
            projectile.Clear();
        for (int block = 0; block < level.WidthInBlocks * level.HeightInBlocks; block++)
        {
            level.SetForegroundEntry(block, 0);
            level.SetBehavior(block, 0);
        }

        SamusState samus = runtime.Samus!;
        samus.InputLocked = false;
        samus.Pose = SamusPoseIds.MorphBallGroundRightPose;
        samus.EquippedItems = samus.CollectedItems = (ushort)(
            SamusEquipmentFlags.MorphBall |
            SamusEquipmentFlags.SpeedBooster);
        samus.Health = samus.MaxHealth = 199;
        samus.ReserveEnergy = 0;
        if (underwater)
            samus.LiquidPhysics.ConfigureWater(8, 0x80);
        samus.Kinematics.XPosition = 200;
        samus.Kinematics.XSubposition = 0;
        samus.Kinematics.YPosition = 166;
        samus.Kinematics.YSubposition = 0;
        samus.Kinematics.YSpeed = samus.Kinematics.YSubspeed = 0;
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);
        samus.PoseHistory.PreviousPose = samus.Pose;
        samus.PoseHistory.PreviousDirectionAndMovement =
            (ushort)(((byte)SamusMovementType.MorphBallGround << 8) | 8);
        samus.PoseHistory.LastDifferentPose = 0;
        samus.PoseHistory.LastDifferentDirectionAndMovement = 0;
        AssertTrue(samus.Shinespark.TryStoreFromSpeedBooster(
            SamusSpecialSequenceRomData.Shinespark.ActiveSpeedBoostCounter),
            "spike-suit fixture accepts stored Shinespark");

        int centerBlock = (samus.YPosition >> 4) * level.WidthInBlocks + (samus.XPosition >> 4);
        level.SetForegroundEntry(centerBlock, (ushort)((ushort)RoomCollisionType.SpikeAir << 12));
        level.SetBehavior(centerBlock, SamusTerrainHazardRomData.DamagingSpikeAirBehavior);
        if (underwater)
        {
            // The reserve technique requires persistent spike/electricity contact while
            // the hurt launch crosses block boundaries. A uniform synthetic field avoids
            // coupling this timing test to any one retail room's surrounding geometry.
            for (int block = 0; block < level.WidthInBlocks * level.HeightInBlocks; block++)
            {
                level.SetForegroundEntry(block, (ushort)((ushort)RoomCollisionType.SpikeAir << 12));
                level.SetBehavior(block, SamusTerrainHazardRomData.DamagingSpikeAirBehavior);
            }
        }
        return runtime;
    }
}
