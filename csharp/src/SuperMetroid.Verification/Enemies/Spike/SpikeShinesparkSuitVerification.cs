using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Rooms;
namespace SuperMetroid.Verification.Enemies.Spike;

/// <summary>Checks the spike-induced Shinespark suit timing window and Reserve freeze behavior.</summary>
internal static class SpikeShinesparkSuitVerification
{
    /// <summary>Compares adjacent dry launch frames and the underwater Reserve timing window.</summary>
    internal static void VerifySpikeShinesparkSuit()
    {
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));

        // The ordinary spike method has a one-frame retained-suit window. Jump one
        // frame early installs the real Shinespark mover; Jump one frame late misses
        // completely. The middle case keeps C7 art and palette six after command one
        // has restored the ordinary movement handler: that mismatch is the technique.
        foreach ((int LaunchFrame, ShinesparkPhase Phase, SamusPoseId Pose, SamusSpecialPaletteType Palette) expected in new[]
        {
            (8, ShinesparkPhase.Windup, SamusPoseId.ShinesparkWindupRightPose, SamusSpecialPaletteType.Shinesparking),
            (9, ShinesparkPhase.Inactive, SamusPoseId.ShinesparkWindupRightPose, SamusSpecialPaletteType.Shinesparking),
            (10, ShinesparkPhase.Stored, SamusPoseId.FallingRightPose, SamusSpecialPaletteType.SpeedBoosterShine),
        })
        {
            SuperMetroidRuntime runtime = CreateSpikeSuitRuntime(underwater: false);
            SamusState samus = runtime.Player.IntroducedSamus;
            for (int frame = 0; frame < 15; frame++)
            {
                runtime.StepFrame(frame is 1 || frame == expected.LaunchFrame
                    ? SnesButton.A
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
        SuperMetroidRuntime frozenRuntime = CreateSpikeSuitRuntime(underwater: true);
        SamusState frozenSamus = frozenRuntime.Player.IntroducedSamus;
        for (int frame = 0; frame < 8; frame++)
            frozenRuntime.StepFrame(frame == 1 ? SnesButton.A : (ushort)0);
        frozenSamus.Health = 0;
        frozenSamus.MaxHealth = 99;
        frozenSamus.ReserveEnergy = frozenSamus.MaxReserveEnergy = 2;
        frozenSamus.ReserveTankMode = ReserveTankMode.Auto;
        var shortRecovery = new SamusReserveAutoRecoveryState();
        shortRecovery.Begin(frozenSamus);
        frozenRuntime.Freeze.GameplayTimeFrozen = true;
        uint frozenX = frozenSamus.Kinematics.Position.X.RawValue;
        uint frozenY = frozenSamus.Kinematics.Position.Y.RawValue;
        int frozenAnimationFrame = frozenSamus.AnimationFrame;
        ushort frozenAnimationTimer = frozenSamus.AnimationFrameTimer.RawValue;
        int frozenShineTimer = frozenSamus.InputSnapshot.SharedShineTimer;
        SamusSpecialPaletteType frozenPalette = frozenSamus.Shinespark.PaletteType;
        ushort frozenKnockbackTimer = frozenSamus.KnockbackTimer.RawValue;
        frozenRuntime.StepFrame(0, new RuntimeFrameRequest { AfterAcceptedNmi = () =>
            shortRecovery.StepAfterNmi(frozenSamus, frozenRuntime.Clock.Counter) });
        AssertEqual((frozenX, frozenY),
            (frozenSamus.Kinematics.Position.X.RawValue, frozenSamus.Kinematics.Position.Y.RawValue),
            "Reserve command zero freezes spike-knockback position");
        AssertEqual((frozenAnimationFrame, frozenAnimationTimer),
            (frozenSamus.AnimationFrame, frozenSamus.AnimationFrameTimer.RawValue),
            "Reserve command zero freezes the unmorph animation cursor");
        AssertEqual((frozenShineTimer, frozenPalette),
            (frozenSamus.InputSnapshot.SharedShineTimer, frozenSamus.Shinespark.PaletteType),
            "Reserve command zero freezes stored-shine palette progression");
        AssertEqual(frozenKnockbackTimer - 1, frozenSamus.KnockbackTimer.RawValue,
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
            SamusState samus = RunReserveSpikeSuitCase(expected.FreezeAfter, expected.LaunchAfter);
            bool retainedSuit = samus.Pose == SamusPoseId.ShinesparkWindupRightPose &&
                samus.Shinespark.Phase == ShinesparkPhase.Inactive &&
                samus.Shinespark.PaletteType == SamusSpecialPaletteType.Shinesparking &&
                samus.InputSnapshot.SharedShineTimer != 0;
            AssertEqual(expected.RetainsSuit, retainedSuit,
                $"underwater Reserve suit freeze {expected.FreezeAfter}, launch {expected.LaunchAfter}");
        }

        Console.WriteLine(
            "Spike Shinespark Suit: dry one-frame window, command-zero freeze, and suitless underwater Reserve window.");
    }

    /// <summary>Runs one underwater automatic-Reserve freeze and post-recovery Jump timing case.</summary>
    private static SamusState RunReserveSpikeSuitCase(
        int freezeAfter,
        int launchAfter)
    {
        SuperMetroidRuntime runtime = CreateSpikeSuitRuntime(underwater: true);
        SamusState samus = runtime.Player.IntroducedSamus;
        for (int frame = 0; frame < freezeAfter; frame++)
            runtime.StepFrame(frame == 1 ? SnesButton.A : (ushort)0);

        samus.Health = 0;
        samus.MaxHealth = 99;
        samus.ReserveEnergy = 60;
        samus.MaxReserveEnergy = 100;
        samus.ReserveTankMode = ReserveTankMode.Auto;
        var recovery = new SamusReserveAutoRecoveryState();
        recovery.Begin(samus);
        runtime.Freeze.GameplayTimeFrozen = true;
        while (recovery.IsActive)
        {
            SnesButton input = samus.ReserveEnergy == 1 && launchAfter == 0
                ? SnesButton.A
                : (ushort)0;
            runtime.StepFrame(input, new RuntimeFrameRequest { AfterAcceptedNmi = () =>
            {
                if (recovery.StepAfterNmi(samus, runtime.Clock.Counter).Completed)
                    runtime.Freeze.GameplayTimeFrozen = false;
            } });
        }
        for (int frame = 1; frame < 25; frame++)
            runtime.StepFrame(frame == launchAfter ? SnesButton.A : (ushort)0);
        return samus;
    }

    /// <summary>Creates a grounded morph-ball fixture with stored shine and spike contact.</summary>
    private static SuperMetroidRuntime CreateSpikeSuitRuntime(bool underwater)
    {
        var runtime = CreateRetailRuntimeFixture();
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(runtime.Room.LoadedRoom.Definition, new Position16<RoomPixels>(0, 0));
        RoomLevelData level = runtime.Room.LoadedLevelData;
        runtime.InitializeDebugGroundedSamus(200, 166, 16);
        foreach (RoomEnemySlot enemy in runtime.Enemies.SlotTable.Slots)
            enemy.Clear();
        foreach (RoomEnemyProjectileSlot projectile in runtime.Enemies.Projectiles.EnemyProjectiles)
            projectile.Clear();
        for (int block = 0; block < level.Size.X * level.Size.Y; block++)
        {
            level.SetForegroundEntry(block, 0);
            level.SetBehavior(block, 0);
        }

        SamusState samus = runtime.Player.IntroducedSamus;
        samus.Control.Unlock();
        samus.Pose = SamusPoseId.MorphBallGroundRightPose;
        samus.EquippedItems = samus.CollectedItems = (SamusEquipmentFlags)((ushort)(
            SamusEquipmentFlags.MorphBall |
            SamusEquipmentFlags.SpeedBooster));
        samus.Health = samus.MaxHealth = 199;
        samus.ReserveEnergy = 0;
        if (underwater)
            samus.LiquidPhysics.ConfigureWater(8, RoomLiquidOptions.SmallTide);
        samus.Kinematics.Position = samus.Kinematics.Position.WithWholeX(200).WithFractionX(0).WithWholeY(166).WithFractionY(0);
        samus.Kinematics.YSpeed = default;
        samus.PoseMetadata.RefreshCollisionRadii();
        samus.Presentation.InitializeAnimation();
        samus.PoseHistory.PreviousPose = samus.Pose;
        samus.PoseHistory.PreviousDirectionAndMovement =
            SnesLittleEndian.Word(8, (byte)SamusMovementType.MorphBallGround);
        samus.PoseHistory.LastDifferentPose = 0;
        samus.PoseHistory.LastDifferentDirectionAndMovement = 0;
        AssertTrue(samus.Shinespark.TryStoreFromSpeedBooster(new SamusSpeedBoostCounter(
            SamusSpecialSequenceRomData.Shinespark.ActiveSpeedBoostCounter)),
            "spike-suit fixture accepts stored Shinespark");

        int centerBlock = (samus.Position.Y.Whole >> 4) * level.Size.X + (samus.Position.X.Whole >> 4);
        level.SetForegroundEntry(centerBlock, (ushort)((ushort)RoomCollisionType.SpikeAir << 12));
        level.SetBehavior(centerBlock, SamusTerrainHazardRomData.DamagingSpikeAirBehavior);
        if (underwater)
        {
            // The reserve technique requires persistent spike/electricity contact while
            // the hurt launch crosses block boundaries. A uniform synthetic field avoids
            // coupling this timing test to any one retail room's surrounding geometry.
            for (int block = 0; block < level.Size.X * level.Size.Y; block++)
            {
                level.SetForegroundEntry(block, (ushort)((ushort)RoomCollisionType.SpikeAir << 12));
                level.SetBehavior(block, SamusTerrainHazardRomData.DamagingSpikeAirBehavior);
            }
        }
        return runtime;
    }
}
