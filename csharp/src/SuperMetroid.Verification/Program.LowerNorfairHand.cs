using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyLowerNorfairHand()
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = new SuperMetroidRuntime(bus, playerInvincibilityEnabled: true);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(0xb1e5);
        runtime.StepFrame(0);
        var level = runtime.LevelData!;
        var hand = level.GetCollisionBlock(4, 8);
        AssertTrue(runtime.Plms.TrySpawnProjectileShotBlock(level, hand.Index,
            hand.Bts, new SamusProjectileTypeWord(0x0300), true), "power bomb breaks the retail hand cover");
        for (int frame = 0; frame < 30; frame++) runtime.StepFrame(0);
        hand = level.GetCollisionBlock(4, 8);
        AssertEqual(RoomCollisionType.SpecialBlock, hand.CollisionType, "destroyed cover becomes solid hand");
        AssertEqual((byte)0x83, hand.Behavior, "Norfair hand activation BTS");
        var samus = runtime.Samus!;
        foreach (bool hasSpaceJump in new[] { false, true })
        {
            samus.CollectedItems = hasSpaceJump ? (ushort)SamusEquipmentFlags.SpaceJump : (ushort)0;
            samus.PoseId = hasSpaceJump ? SamusPoseId.FacingRightNormalPose : SamusPoseId.MorphBallGroundRightPose;
            samus.InitializeAnimation(bus);
            samus.XPosition = 4 * 16 + 8;
            samus.YPosition = (ushort)(8 * 16 - samus.Kinematics.YRadius - 1);
            var rejected = SamusBlockCollision.MoveVertical(bus, level, samus.Kinematics,
                2 << 16, true, false, false, runtime.Plms);
            AssertTrue(rejected.Collided, "hand remains solid for ineligible contact");
            AssertTrue(!runtime.System.HasEvent(EventNumber.LowerNorfairChozoLoweredAcid),
                "standing or missing Space Jump must not activate statue");
        }
        samus.CollectedItems |= (ushort)SamusEquipmentFlags.SpaceJump;
        samus.PoseId = SamusPoseId.MorphBallGroundRightPose;
        samus.InitializeAnimation(bus);
        samus.XPosition = 4 * 16 + 8;
        samus.YPosition = (ushort)(8 * 16 - samus.Kinematics.YRadius - 1);
        var collision = SamusBlockCollision.MoveVertical(bus, level, samus.Kinematics,
            2 << 16, true, false, false, runtime.Plms);
        AssertTrue(collision.Collided && collision.CollisionBlock?.Index == hand.Index,
            "downward morph contact lands on the actual hand tile");
        AssertTrue(runtime.System.HasEvent(EventNumber.LowerNorfairChozoLoweredAcid), "hand sets native acid event");
        AssertEqual((ushort)1, runtime.Enemies.Slots[0].Parameter1, "hand wakes statue");
        AssertTrue(!runtime.GroundedSamusMovementEnabled, "statue takes control on hand contact");
        ushort startingAcidY = runtime.RoomLayer3Fx.BaseYPosition;
        bool sawAcidDrain = false;
        var displayedHeights = new HashSet<ushort>();
        bool sawStartCommand = false;
        ushort previousBaseY = startingAcidY;
        for (int frame = 0; frame < 3000 && !runtime.GroundedSamusMovementEnabled; frame++)
        {
            runtime.StepFrame(0);
            if (!sawStartCommand && runtime.Enemies.ChozoStatueFxTimer != 0)
            {
                sawStartCommand = true;
                AssertEqual((ushort)32, runtime.RoomLayer3Fx.Timer, "start instruction writes live delay once");
                AssertEqual((ushort)64, runtime.RoomLayer3Fx.PackedYVelocity, "start instruction writes live 8.8 velocity");
            }
            sawAcidDrain |= runtime.RoomLayer3Fx.BaseYPosition > startingAcidY &&
                runtime.RoomLayer3Fx.BaseYPosition < 0x2d2;
            if (!runtime.GroundedSamusMovementEnabled)
            {
                AssertTrue(runtime.RoomLayer3Fx.BaseYPosition - previousBaseY is 0 or 1,
                    "live acid moves gradually rather than teleporting before release");
                if (runtime.DisplayedRoomLayer3Fx is { } displayed &&
                    displayed.CurrentYPosition > startingAcidY && displayed.CurrentYPosition < 0x2d2)
                    displayedHeights.Add(displayed.CurrentYPosition);
            }
            previousBaseY = runtime.RoomLayer3Fx.BaseYPosition;
        }
        AssertTrue(runtime.GroundedSamusMovementEnabled, "statue finishes and releases Samus");
        AssertTrue(sawAcidDrain, "acid surface must visibly descend during the live statue sequence");
        AssertEqual((ushort)0x2d2, runtime.RoomLayer3Fx.BaseYPosition,
            "statue completes acid lowering before leaving the room");
        AssertTrue(displayedHeights.Count > 50, "accepted display snapshots contain the progressing acid surface");
        // Enemy AI publishes the final base after this frame's FX handler. The next
        // handler updates the surface registers, then the following NMI displays them.
        runtime.StepFrame(0);
        runtime.RunNmi(0, mainLoopRequestedNmi: true);
        AssertEqual((ushort)0x2d2, runtime.DisplayedRoomLayer3Fx!.Value.CurrentYPosition,
            "final drained height reaches display without a room transition");
        runtime.LoadCartridgeRoomForDebug(0xb1e5);
        runtime.StepFrame(0);
        AssertEqual((ushort)0x2d2, runtime.RoomLayer3Fx.BaseYPosition, "re-entry restores drained acid");
        for (int frame = 0; frame < 30; frame++) runtime.StepFrame(0);
        AssertTrue(level != runtime.LevelData, "re-entry uses newly loaded room terrain");
        AssertTrue(runtime.LevelData!.GetCollisionBlock(4, 8).CollisionType != RoomCollisionType.SpecialBlock,
            "completed statue does not install another hand trigger");
        Console.WriteLine($"Lower Norfair hand: collision/admission, live FX writes, {displayedHeights.Count} displayed drain heights, control release and re-entry pass.");
    }
}
