using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Enemies.Chozo.ChozoStatueVisualsVerification;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Enemies.Chozo;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Gameplay;
namespace SuperMetroid.Verification.Enemies.Chozo;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Rooms;

/// <summary>Checks Lower Norfair Chozo hand admission, acid draining, and completed-room re-entry.</summary>
internal static class LowerNorfairHandVerification
{
    /// <summary>Drives retail hand collision and the full statue sequence with PLM data reads blocked.</summary>
    internal static void VerifyLowerNorfairHand()
    {
        var source = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        var bus = NewChozoProgramAndDrawReadGuard(source);
        var runtime = CreateRetailRuntimeFixture(playerInvincibilityEnabled: true);
        var handEntries = ChozoStatuePlmDrawDefinitions.All.Select(draw =>
            new RoomPlmVisualEntry(
                ChozoStatuePlmDrawDefinitions.VisualId(draw.Pointer),
                draw.VisualWords())).ToArray();
        handEntries.Single(entry => entry.Id == "lower-norfair-cleared-hand")
            .Blocks[0] = 0x0053;
        runtime.ShotBlockVisuals.RoomPlmChozoStatueVisuals =
            new RoomPlmChozoStatueVisualCatalog(handEntries);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.AcidStatue);
        runtime.StepFrame(0);
        var level = runtime.Room.LoadedLevelData;
        var hand = level.GetCollisionBlock(new(4, 8));
        AssertTrue(runtime.Plms.BlockReactions.TrySpawnProjectileShotBlock(level, hand.Index,
            hand.Bts, new SamusProjectileTypeWord(0x0300), true), "power bomb breaks the retail hand cover");
        for (int frame = 0; frame < 30; frame++) runtime.StepFrame(0);
        hand = level.GetCollisionBlock(new(4, 8));
        AssertEqual(RoomCollisionType.SpecialBlock, hand.CollisionType, "destroyed cover becomes solid hand");
        AssertEqual(0x83, hand.Bts.Value, "Norfair hand activation BTS");
        var samus = runtime.Player.IntroducedSamus;
        foreach (bool hasSpaceJump in new[] { false, true })
        {
            samus.CollectedItems = hasSpaceJump ? SamusEquipmentFlags.SpaceJump : 0;
            samus.PoseId = hasSpaceJump ? SamusPoseId.FacingRightNormalPose : SamusPoseId.MorphBallGroundRightPose;
            SamusPresentation.InitializeAnimation(samus);
            samus.Position = samus.Position.WithWhole(new(4 * 16 + 8, (ushort)(8 * 16 - samus.Kinematics.Radius.Y - 1)));
            var rejected = SamusBlockCollision.MoveVertical(new SamusTerrain { Level = level, Plms = runtime.Plms }, samus.Kinematics, samus, new VerticalBlockMove { Displacement = 2 << 16, ScanLeftToRight = true, CanBreakBombBlocks = false, IncludeSolidEnemies = false });
            AssertTrue(rejected.Collided, "hand remains solid for ineligible contact");
            AssertTrue(!runtime.System.HasEvent(EventNumber.LowerNorfairChozoLoweredAcid),
                "standing or missing Space Jump must not activate statue");
        }
        samus.CollectedItems |= SamusEquipmentFlags.SpaceJump;
        samus.PoseId = SamusPoseId.MorphBallGroundRightPose;
        SamusPresentation.InitializeAnimation(samus);
        samus.Position = samus.Position.WithWhole(new(4 * 16 + 8, (ushort)(8 * 16 - samus.Kinematics.Radius.Y - 1)));
        var collision = SamusBlockCollision.MoveVertical(new SamusTerrain { Level = level, Plms = runtime.Plms }, samus.Kinematics, samus, new VerticalBlockMove { Displacement = 2 << 16, ScanLeftToRight = true, CanBreakBombBlocks = false, IncludeSolidEnemies = false });
        // Samus's whole width lies in the hand's block column, so the downward probe's
        // only candidate is the hand block at (4, 8).
        int leftColumn = (samus.Kinematics.Position.X.Whole - samus.Kinematics.Radius.X) >> 4;
        int rightColumn = (samus.Kinematics.Position.X.Whole + samus.Kinematics.Radius.X - 1) >> 4;
        AssertTrue(collision.Collided && leftColumn == hand.Index % level.Size.X &&
            rightColumn == hand.Index % level.Size.X,
            "downward morph contact lands on the actual hand tile");
        AssertTrue(runtime.System.HasEvent(EventNumber.LowerNorfairChozoLoweredAcid), "hand sets native acid event");
        AssertTrue((runtime.Enemies.SlotTable.Slots[0].SpeciesState<ChozoStatueState>() ?? throw new InvalidOperationException("Verification requires the statue state.")).HandTriggered, "hand wakes statue");
        AssertTrue(!runtime.Player.GroundedSamusMovementEnabled, "statue takes control on hand contact");
        ushort startingAcidY = runtime.RoomLayer3Fx.BaseY.Whole;
        bool sawAcidDrain = false;
        var displayedHeights = new HashSet<ushort>();
        bool sawStartCommand = false;
        ushort previousBaseY = startingAcidY;
        for (int frame = 0; frame < 3000 && !runtime.Player.GroundedSamusMovementEnabled; frame++)
        {
            runtime.StepFrame(0);
            if (!sawStartCommand && runtime.Enemies.ChozoStatueBehavior.ChozoStatueAcidLoweringStarted)
            {
                sawStartCommand = true;
                AssertEqual((ushort)32, runtime.RoomLayer3Fx.Timer.RawValue, "start instruction writes live delay once");
                AssertEqual((ushort)64, runtime.RoomLayer3Fx.YVelocity.RawValue, "start instruction writes live 8.8 velocity");
            }
            sawAcidDrain |= runtime.RoomLayer3Fx.BaseY.Whole > startingAcidY &&
                runtime.RoomLayer3Fx.BaseY.Whole < 0x2d2;
            if (!runtime.Player.GroundedSamusMovementEnabled)
            {
                AssertTrue(runtime.RoomLayer3Fx.BaseY.Whole - previousBaseY is 0 or 1,
                    "live acid moves gradually rather than teleporting before release");
                if (runtime.Display.DisplayedRoomLayer3Fx is { } displayed &&
                    displayed.Surface.Y > startingAcidY && displayed.Surface.Y < 0x2d2)
                    displayedHeights.Add(displayed.Surface.Y);
            }
            previousBaseY = runtime.RoomLayer3Fx.BaseY.Whole;
        }
        AssertTrue(runtime.Player.GroundedSamusMovementEnabled, "statue finishes and releases Samus");
        AssertTrue(sawAcidDrain, "acid surface must visibly descend during the live statue sequence");
        AssertEqual((ushort)0x2d2, runtime.RoomLayer3Fx.BaseY.Whole,
            "statue completes acid lowering before leaving the room");
        AssertTrue(displayedHeights.Count > 50, "accepted display snapshots contain the progressing acid surface");
        // Enemy AI publishes the final base after this frame's FX handler. The next
        // handler updates the surface registers, then the following NMI displays them.
        runtime.StepFrame(0);
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        AssertEqual((ushort)0x2d2, (runtime.Display.DisplayedRoomLayer3Fx ?? throw new InvalidOperationException("Verification requires runtime.Display.DisplayedRoomLayer3Fx.")).Surface.Y,
            "final drained height reaches display without a room transition");
        runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.AcidStatue);
        runtime.StepFrame(0);
        AssertEqual((ushort)0x2d2, runtime.RoomLayer3Fx.BaseY.Whole, "re-entry restores drained acid");
        for (int frame = 0; frame < 30; frame++) runtime.StepFrame(0);
        AssertTrue(level != runtime.Room.LevelData, "re-entry uses newly loaded room terrain");
        AssertTrue(runtime.Room.LoadedLevelData.GetCollisionBlock(new(4, 8)).CollisionType != RoomCollisionType.SpecialBlock,
            "completed statue does not install another hand trigger");
        RoomLevelData reentered = runtime.Room.LoadedLevelData;
        reentered.SetBlockDefinitionWord(0x53 * 4, 0x0053);
        AssertEqual((ushort)0x0053,
            reentered.DisplayedBlockTiles(0x1d * reentered.Size.X + 0x0c).TopLeft.Raw,
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
