using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using static SuiteTimingVerification;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Rooms;
using static ChozoStatueVisualsVerification;
using static RetailRuntimeFixtureVerification;

/// <summary>Checks Lower Norfair Chozo hand admission, acid draining, and completed-room re-entry.</summary>
internal static class LowerNorfairHandVerification
{
    /// <summary>Drives retail hand collision and the full statue sequence with PLM data reads blocked.</summary>
    internal static void VerifyLowerNorfairHand()
    {
        var source = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        var bus = NewChozoProgramAndDrawReadGuard(source);
        var runtime = CreateRetailRuntimeFixture(bus, playerInvincibilityEnabled: true);
        var handEntries = ChozoStatuePlmDrawDefinitions.All.Select(draw =>
            new RoomPlmChozoStatueVisualEntry(
                ChozoStatuePlmDrawDefinitions.VisualId(draw.Pointer),
                draw.Runs.Span.ToArray().SelectMany(run =>
                    run.LevelWords.Span.ToArray().Select(word =>
                        new RoomLevelWord(word).VisualWord)).ToArray())).ToArray();
        handEntries.Single(entry => entry.Id == "lower-norfair-cleared-hand")
            .Blocks[0] = 0x0053;
        runtime.ShotBlockVisuals.RoomPlmChozoStatueVisuals =
            new RoomPlmChozoStatueVisualCatalog(handEntries);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(0xb1e5);
        runtime.StepFrame(0);
        var level = runtime.Room.LevelData!;
        var hand = level.GetCollisionBlock(new(4, 8));
        AssertTrue(runtime.Plms.BlockReactions.TrySpawnProjectileShotBlock(level, hand.Index,
            hand.Bts, new SamusProjectileTypeWord(0x0300), true), "power bomb breaks the retail hand cover");
        for (int frame = 0; frame < 30; frame++) runtime.StepFrame(0);
        hand = level.GetCollisionBlock(new(4, 8));
        AssertEqual(RoomCollisionType.SpecialBlock, hand.CollisionType, "destroyed cover becomes solid hand");
        AssertEqual((byte)0x83, hand.Behavior, "Norfair hand activation BTS");
        var samus = runtime.Player.Samus!;
        foreach (bool hasSpaceJump in new[] { false, true })
        {
            samus.CollectedItems = hasSpaceJump ? (ushort)SamusEquipmentFlags.SpaceJump : (ushort)0;
            samus.PoseId = hasSpaceJump ? SamusPoseId.FacingRightNormalPose : SamusPoseId.MorphBallGroundRightPose;
            samus.Presentation.InitializeAnimation(bus);
            samus.Position = samus.Position.WithWhole(new(4 * 16 + 8, (ushort)(8 * 16 - samus.Kinematics.Radius.Y - 1)));
            var rejected = SamusBlockCollision.MoveVertical(new SamusTerrain { Bus = bus, Level = level, Plms = runtime.Plms }, samus.Kinematics, new VerticalBlockMove { Displacement = 2 << 16, ScanLeftToRight = true, CanBreakBombBlocks = false, IncludeSolidEnemies = false });
            AssertTrue(rejected.Collided, "hand remains solid for ineligible contact");
            AssertTrue(!runtime.System.HasEvent(EventNumber.LowerNorfairChozoLoweredAcid),
                "standing or missing Space Jump must not activate statue");
        }
        samus.CollectedItems |= (ushort)SamusEquipmentFlags.SpaceJump;
        samus.PoseId = SamusPoseId.MorphBallGroundRightPose;
        samus.Presentation.InitializeAnimation(bus);
        samus.Position = samus.Position.WithWhole(new(4 * 16 + 8, (ushort)(8 * 16 - samus.Kinematics.Radius.Y - 1)));
        var collision = SamusBlockCollision.MoveVertical(new SamusTerrain { Bus = bus, Level = level, Plms = runtime.Plms }, samus.Kinematics, new VerticalBlockMove { Displacement = 2 << 16, ScanLeftToRight = true, CanBreakBombBlocks = false, IncludeSolidEnemies = false });
        // Samus's whole width lies in the hand's block column, so the downward probe's
        // only candidate is the hand block at (4, 8).
        int leftColumn = (samus.Kinematics.Position.X.Whole - samus.Kinematics.Radius.X) >> 4;
        int rightColumn = (samus.Kinematics.Position.X.Whole + samus.Kinematics.Radius.X - 1) >> 4;
        AssertTrue(collision.Collided && leftColumn == hand.Index % level.Size.X &&
            rightColumn == hand.Index % level.Size.X,
            "downward morph contact lands on the actual hand tile");
        AssertTrue(runtime.System.HasEvent(EventNumber.LowerNorfairChozoLoweredAcid), "hand sets native acid event");
        AssertEqual((ushort)1, runtime.Enemies.SlotTable.Slots[0].Parameter1, "hand wakes statue");
        AssertTrue(!runtime.Player.GroundedSamusMovementEnabled, "statue takes control on hand contact");
        ushort startingAcidY = runtime.RoomLayer3Fx.BaseYPosition;
        bool sawAcidDrain = false;
        var displayedHeights = new HashSet<ushort>();
        bool sawStartCommand = false;
        ushort previousBaseY = startingAcidY;
        for (int frame = 0; frame < 3000 && !runtime.Player.GroundedSamusMovementEnabled; frame++)
        {
            runtime.StepFrame(0);
            if (!sawStartCommand && runtime.Enemies.ChozoStatueBehavior.ChozoStatueFxTimer != 0)
            {
                sawStartCommand = true;
                AssertEqual((ushort)32, runtime.RoomLayer3Fx.Timer, "start instruction writes live delay once");
                AssertEqual((ushort)64, runtime.RoomLayer3Fx.PackedYVelocity, "start instruction writes live 8.8 velocity");
            }
            sawAcidDrain |= runtime.RoomLayer3Fx.BaseYPosition > startingAcidY &&
                runtime.RoomLayer3Fx.BaseYPosition < 0x2d2;
            if (!runtime.Player.GroundedSamusMovementEnabled)
            {
                AssertTrue(runtime.RoomLayer3Fx.BaseYPosition - previousBaseY is 0 or 1,
                    "live acid moves gradually rather than teleporting before release");
                if (runtime.Display.DisplayedRoomLayer3Fx is { } displayed &&
                    displayed.CurrentYPosition > startingAcidY && displayed.CurrentYPosition < 0x2d2)
                    displayedHeights.Add(displayed.CurrentYPosition);
            }
            previousBaseY = runtime.RoomLayer3Fx.BaseYPosition;
        }
        AssertTrue(runtime.Player.GroundedSamusMovementEnabled, "statue finishes and releases Samus");
        AssertTrue(sawAcidDrain, "acid surface must visibly descend during the live statue sequence");
        AssertEqual((ushort)0x2d2, runtime.RoomLayer3Fx.BaseYPosition,
            "statue completes acid lowering before leaving the room");
        AssertTrue(displayedHeights.Count > 50, "accepted display snapshots contain the progressing acid surface");
        // Enemy AI publishes the final base after this frame's FX handler. The next
        // handler updates the surface registers, then the following NMI displays them.
        runtime.StepFrame(0);
        runtime.NmiHandler.RunNmi(0, mainLoopRequestedNmi: true);
        AssertEqual((ushort)0x2d2, runtime.Display.DisplayedRoomLayer3Fx!.Value.CurrentYPosition,
            "final drained height reaches display without a room transition");
        runtime.LoadCartridgeRoomForDebug(0xb1e5);
        runtime.StepFrame(0);
        AssertEqual((ushort)0x2d2, runtime.RoomLayer3Fx.BaseYPosition, "re-entry restores drained acid");
        for (int frame = 0; frame < 30; frame++) runtime.StepFrame(0);
        AssertTrue(level != runtime.Room.LevelData, "re-entry uses newly loaded room terrain");
        AssertTrue(runtime.Room.LevelData!.GetCollisionBlock(new(4, 8)).CollisionType != RoomCollisionType.SpecialBlock,
            "completed statue does not install another hand trigger");
        RoomLevelData reentered = runtime.Room.LevelData!;
        reentered.SetBlockDefinitionWord(0x53 * 4, 0x0053);
        AssertEqual((ushort)0x0053,
            reentered.CreateBackgroundStreamer().BuildPlmLevelBlockUpdate(
                0x1d * reentered.Size.X + 0x0c, 0).TopRow[0],
            "event-completed hand re-entry streams the installed visual override");
        AssertEqual(0, bus.ForbiddenReadAttempts,
            "complete Lower Norfair hand sequence did not reread compiled PLM data");
        Console.WriteLine($"Lower Norfair hand: collision/admission, live FX writes, {displayedHeights.Count} displayed drain heights, control release and re-entry pass.");
    }

    /// <summary>Runs the Lower Norfair Chozo hand and acid-drain fixture.</summary>
    [VerificationCommand("--lower-norfair-hand")]
    internal static int LowerNorfairHandCommand()
    {
        Suite(nameof(VerifyLowerNorfairHand), () => VerifyLowerNorfairHand());
        return 0;
    }
}
