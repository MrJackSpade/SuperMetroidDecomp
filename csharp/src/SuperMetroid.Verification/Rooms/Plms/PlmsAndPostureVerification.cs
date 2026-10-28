using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Samus.Drawing;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Harness.SyntheticRoomCartridgeFixturesVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
namespace SuperMetroid.Verification.Rooms.Plms;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.World;

/// <summary>Focused PLM terrain and Samus posture transition checks over constructed cartridge data.</summary>
internal static class PlmsAndPostureVerification
{

/// <summary>Checks breakable grapple and collision-bomb PLM timing, terrain, tilemap redraws, sound, and respawn.</summary>
internal static void VerifyBreakableGrapplePlms()
{
    TestAddressSpace bus = CreateBreakableGrappleCartridge();

    const int width = 8;
    const int height = 8;
    const int blockIndex = 3 * width + 3;
    static RoomLevelData CreateLevel(byte behavior)
    {
        var foreground = new ushort[width * height];
        var bts = new byte[foreground.Length];
        foreground[blockIndex] = 0xe123;
        bts[blockIndex] = behavior;
        return CreateRoom(new(width, height), foreground, bts);
    }

    static void StepMany(
        RoomPlmSystem plms,
        RoomLevelData level,
        int count)
    {
        for (int frame = 0; frame < count; frame++)
            plms.Step(level);
    }

    // BTS one waits 240 handler calls, breaks through four exact visual words, reverses
    // through the same words, restores both the saved level word and BTS one, then deletes
    // on the following pass because DrawPLMBlock deliberately seeded timer one.
    RoomLevelData respawning = CreateLevel(1);
    var respawningPlms = PlmFixtures.Unbound();
    AssertTrue(respawningPlms.TrySpawnBreakableGrappleBlock(respawning, blockIndex, 1),
        "respawning grapple PLM occupies a native slot");
    AssertEqual(0, respawning.GetCollisionBlockByIndex(blockIndex).Bts.Value,
        "CFB5 clears breakable grapple BTS immediately");
    AssertEqual(0xe123, respawning.GetCollisionBlockByIndex(blockIndex).PackedWord.Raw,
        "CFB5 retains original level word until handler");

    respawningPlms.Step(respawning);
    AssertEqual(0xe0b7, respawning.GetCollisionBlockByIndex(blockIndex).PackedWord.Raw,
        "connection-frame PLM pass draws grapple frame zero");
    AssertEqual(0x00b7, respawning.DisplayedVisualWord(blockIndex),
        "the drawn block shows its ROM-selected block, unmirrored");

    StepMany(respawningPlms,respawning, 239);
    AssertEqual(0xe0b7, respawning.GetCollisionBlockByIndex(blockIndex).PackedWord.Raw,
        "respawning block remains grapple terrain through timer 240 minus one");
    respawningPlms.Step(respawning);
    AssertEqual(0x0053, respawning.GetCollisionBlockByIndex(blockIndex).PackedWord.Raw,
        "timer 240 expiry draws first air frame");
    AssertEqual(1, respawningPlms.Sounds.SoundRequests.Count,
        "break transition queues one sound request");
    AssertEqual(new SoundRequest(Library2Sound.BlockCrumble, SoundQueueLimit.Max6), respawningPlms.Sounds.SoundRequests[0],
        "break transition uses library two sound $0A with maximum six");

    StepMany(respawningPlms,respawning, 4);
    AssertEqual(0x0054, respawning.GetCollisionBlockByIndex(blockIndex).PackedWord.Raw,
        "respawning break frame one advances after four");
    StepMany(respawningPlms,respawning, 4);
    AssertEqual(0x0055, respawning.GetCollisionBlockByIndex(blockIndex).PackedWord.Raw,
        "respawning break frame two advances after four");
    StepMany(respawningPlms,respawning, 6);
    AssertEqual(0x00ff, respawning.GetCollisionBlockByIndex(blockIndex).PackedWord.Raw,
        "respawning break reaches blank frame after six");
    StepMany(respawningPlms,respawning, 4 + 4 + 4 + 4);
    AssertEqual(0xe123, respawning.GetCollisionBlockByIndex(blockIndex).PackedWord.Raw,
        "DrawPLMBlock restores the complete saved grapple word");
    AssertEqual(1, respawning.GetCollisionBlockByIndex(blockIndex).Bts.Value,
        "respawning instruction restores BTS one before terrain");
    AssertEqual(1, respawningPlms.SlotTable.ActiveCount,
        "restoration pass retains PLM for timer-one delete delay");
    respawningPlms.Step(respawning);
    AssertEqual(0, respawningPlms.SlotTable.ActiveCount,
        "restored grapple PLM deletes on following handler pass");

    // BTS two uses the shorter 120-frame delay and never restores the saved word/BTS.
    RoomLevelData permanent = CreateLevel(2);
    var permanentPlms = PlmFixtures.Unbound();
    AssertTrue(permanentPlms.TrySpawnBreakableGrappleBlock(permanent, blockIndex, 2),
        "nonrespawning grapple PLM occupies a native slot");
    permanentPlms.Step(permanent);
    StepMany(permanentPlms,permanent, 119);
    AssertEqual(0xe0b7, permanent.GetCollisionBlockByIndex(blockIndex).PackedWord.Raw,
        "nonrespawning block retains grapple terrain through timer 120 minus one");
    permanentPlms.Step(permanent);
    AssertEqual(0x0053, permanent.GetCollisionBlockByIndex(blockIndex).PackedWord.Raw,
        "nonrespawning timer 120 begins break sequence");
    StepMany(permanentPlms,permanent, 4 + 4 + 4);
    AssertEqual(0x00ff, permanent.GetCollisionBlockByIndex(blockIndex).PackedWord.Raw,
        "nonrespawning sequence ends on blank-air visual word");
    permanentPlms.Step(permanent);
    AssertEqual(0, permanentPlms.SlotTable.ActiveCount,
        "nonrespawning sequence deletes one frame after timer-one blank draw");
    AssertEqual(0, permanent.GetCollisionBlockByIndex(blockIndex).Bts.Value,
        "nonrespawning sequence leaves cleared BTS");

    SeedBombBlockPlmPrograms(bus);

    static RoomLevelData CreateBombLevel(byte behavior)
    {
        var foreground = new ushort[width * height];
        var bts = new byte[foreground.Length];
        foreground[blockIndex] = 0xf321;
        bts[blockIndex] = behavior;
        return CreateRoom(new(width, height), foreground, bts);
    }

    RoomLevelData respawning2x2 = CreateBombLevel(3);
    var respawning2x2Plms = PlmFixtures.Unbound();
    AssertTrue(respawning2x2Plms.TrySpawnCollisionBombBlock(
        respawning2x2, blockIndex, behavior: 3),
        "BTS-3 collision setup occupies a native PLM slot");
    AssertEqual(0x0058, respawning2x2.GetCollisionBlockByIndex(blockIndex).PackedWord.Raw,
        "CE83 synchronously installs the native air-type bomb-parent visual");
    respawning2x2Plms.Step(respawning2x2);
    AssertEqual(new SoundRequest(Library2Sound.ContactBlockBreak, SoundQueueLimit.Max3), respawning2x2Plms.Sounds.SoundRequests[0],
        "collision break queues library-two sound six with native maximum three");
    AssertEqual(0x0053,
        respawning2x2.GetCollisionBlock(new(3, 3)).PackedWord.Raw,
        "2x2 first row begins at PLM origin");
    AssertEqual(0x0053,
        respawning2x2.GetCollisionBlock(new(4, 4)).PackedWord.Raw,
        "signed {0,+1} record draws the 2x2 lower-right block");

    StepMany(respawning2x2Plms,respawning2x2, 12);
    AssertEqual(0x00ff, respawning2x2.GetCollisionBlock(new(4, 4)).PackedWord.Raw,
        "BTS-3 reaches its four-block blank frame after three four-frame transitions");
    StepMany(respawning2x2Plms,respawning2x2, 384 + 12);
    AssertEqual(0xf058, respawning2x2.GetCollisionBlock(new(3, 3)).PackedWord.Raw,
        "BTS-3 restores the type-F parent after the exact 384-frame blank hold");
    AssertEqual(0x5058, respawning2x2.GetCollisionBlock(new(4, 3)).PackedWord.Raw,
        "BTS-3 restores the type-5 horizontal extension");
    AssertEqual(0xd058, respawning2x2.GetCollisionBlock(new(3, 4)).PackedWord.Raw,
        "BTS-3 restores the type-D vertical extension row");
    AssertEqual(1, respawning2x2Plms.SlotTable.ActiveCount,
        "timer-one restored frame keeps BTS-3 PLM alive through this handler pass");
    respawning2x2Plms.Step(respawning2x2);
    AssertEqual(0, respawning2x2Plms.SlotTable.ActiveCount,
        "BTS-3 deletes on the handler pass after linked-block restoration");

    RoomLevelData permanent1x2 = CreateBombLevel(6);
    var permanent1x2Plms = PlmFixtures.Unbound();
    AssertTrue(permanent1x2Plms.TrySpawnCollisionBombBlock(
        permanent1x2, blockIndex, behavior: 6),
        "BTS-6 collision setup occupies a native PLM slot");
    permanent1x2Plms.Step(permanent1x2);
    AssertEqual(0x0053, permanent1x2.GetCollisionBlock(new(3, 3)).PackedWord.Raw,
        "vertical draw writes BTS-6 origin");
    AssertEqual(0x0053, permanent1x2.GetCollisionBlock(new(3, 4)).PackedWord.Raw,
        "vertical draw advances its second word by one room row");
    AssertEqual(0, permanent1x2.GetCollisionBlock(new(4, 3)).PackedWord.Raw,
        "vertical draw does not accidentally advance into the neighboring column");
    StepMany(permanent1x2Plms,permanent1x2, 12);
    AssertEqual(0x00ff, permanent1x2.GetCollisionBlock(new(3, 4)).PackedWord.Raw,
        "permanent BTS-6 finishes on two vertical blank-air words");
    permanent1x2Plms.Step(permanent1x2);
    AssertEqual(0, permanent1x2Plms.SlotTable.ActiveCount,
        "permanent BTS-6 deletes one frame after its timer-one blank draw");

    Console.WriteLine("  Movement PLMs: grapple and collision-bomb ROM timing, multi-block terrain, sound, VRAM, and respawn agree.");
}

/// <summary>Checks native crouch and stand transition timing, collision radii, floor clipping, and grounded movement.</summary>
internal static void VerifySamusPostureMovement()
{
    TestAddressSpace bus = CreateSamusPostureCartridge();

    // These twelve literal records prove the aimed transition art retains command seven's
    // radius semantics: `$F1-$F6` already carry radius 16, while `$F7-$FC` carry radius 21.
    SamusPoseId[] aimedCrouchTransitions = [SamusPoseId.CrouchingTransitionAimUpRightPose, SamusPoseId.CrouchingTransitionAimUpLeftPose, SamusPoseId.CrouchingTransitionAimDiagonalUpRightPose, SamusPoseId.CrouchingTransitionAimDiagonalUpLeftPose, SamusPoseId.CrouchingTransitionAimDiagonalDownRightPose, SamusPoseId.CrouchingTransitionAimDiagonalDownLeftPose];
    SamusPoseId[] aimedStandTransitions = [SamusPoseId.StandingTransitionAimUpRightPose, SamusPoseId.StandingTransitionAimUpLeftPose, SamusPoseId.StandingTransitionAimDiagonalUpRightPose, SamusPoseId.StandingTransitionAimDiagonalUpLeftPose, SamusPoseId.StandingTransitionAimDiagonalDownRightPose, SamusPoseId.StandingTransitionAimDiagonalDownLeftPose];
    byte[] transitionDirections = [0x08, 0x04, 0x08, 0x04, 0x08, 0x04];
    byte[] transitionShots = [0x00, 0x09, 0x01, 0x08, 0x03, 0x06];
    for (int index = 0; index < 6; index++)
    {
        SeedAimedTransitionPoseDefinitions(bus, aimedCrouchTransitions[index], aimedStandTransitions[index],
            transitionDirections[index], transitionShots[index]);
    }
    // Animation-delay pointers and command bytes are compiled cartridge
    // mechanics. The real $35/$3B and $F1-$FC streams each spend three ticks
    // before FD; the pose-definition bytes above remain mutable fixture data.

    SeedDryAirJumpTableZero(bus);

    // The literal target arrays mirror the compiled `$91:B518-$91:B53B`
    // command operands. They are not fake-bus stream overrides.
    SamusPoseId[] aimedCrouchTargets = [SamusPoseId.CrouchingAimUpRightPose, SamusPoseId.CrouchingAimUpLeftPose, SamusPoseId.CrouchingAimDiagonalUpRightPose, SamusPoseId.CrouchingAimDiagonalUpLeftPose, SamusPoseId.CrouchingAimDiagonalDownRightPose, SamusPoseId.CrouchingAimDiagonalDownLeftPose];
    SamusPoseId[] aimedStandTargets = [SamusPoseId.StandingAimUpRightPose, SamusPoseId.StandingAimUpLeftPose, SamusPoseId.StandingAimDiagonalUpRightPose, SamusPoseId.StandingAimDiagonalUpLeftPose, SamusPoseId.StandingAimDiagonalDownRightPose, SamusPoseId.StandingAimDiagonalDownLeftPose];
    for (int index = 0; index < 6; index++)
    {
        AssertEqual((byte)3,
            SamusAnimationDelayDefinitions.ReadAnimationByte(
                null, SamusAnimationDelayDefinitions.PointerForPose(aimedCrouchTransitions[index]), 0),
            "aimed crouch starts with native three-tick delay");
        AssertEqual((byte)3,
            SamusAnimationDelayDefinitions.ReadAnimationByte(
                null, SamusAnimationDelayDefinitions.PointerForPose(aimedStandTransitions[index]), 0),
            "aimed stand starts with native three-tick delay");
    }
    // These poses already have compiled retail delay streams; no synthetic
    // timing override is required to exercise the actual posture transitions.

    const int width = 8;
    const int height = 8;
    var floor = new ushort[width * height];
    for (int x = 0; x < width; x++)
        floor[4 * width + x] = 0x8000;
    RoomLevelData level = CreateRoom(new(width, height), floor, new byte[floor.Length]);

    var samus = new SamusState
    {
        Pose = SamusPoseId.FacingRightNormalPose,
        Position = FixedPosition<RoomPixels>.FromWhole(new(48, 43)), // standing bottom is pixel 63, immediately above row-four floor
    };
    SamusPoseMetadata.RefreshCollisionRadii(samus);
    SamusPresentation.InitializeAnimation(samus);
    AssertTrue(
        SamusPosturePoseTransitions.TryApplyPostureTransition(samus, 
level, SamusPoseId.CrouchingTransitionRightPose, frameCounter: 0),
        "standing begins crouch transition");
    AssertEqual(16, samus.Kinematics.Radius.Y, "crouch transition radius");
    AssertEqual(48, samus.Position.Y.Whole, "command seven moves crouch center down five");

    // #1269: command seven clips its five-pixel request through $94:96AB with the new
    // radius. Descending a slope leaves the standing body overlapping the surface, so the
    // crouch moves center down only to the floor. Model that overlap with a body sunk into
    // the row-four floor: the radius-16 probe from bottom pixel 60 stops at 63, so center
    // moves three pixels rather than the unconditional five.
    var sunkSamus = new SamusState
    {
        Pose = SamusPoseId.FacingRightNormalPose,
        Position = FixedPosition<RoomPixels>.FromWhole(new(48, 45)), // standing bottom pixel 65 overlaps the floor starting at 64
    };
    SamusPoseMetadata.RefreshCollisionRadii(sunkSamus);
    SamusPresentation.InitializeAnimation(sunkSamus);
    AssertTrue(
        SamusPosturePoseTransitions.TryApplyPostureTransition(sunkSamus, 
level, SamusPoseId.CrouchingTransitionRightPose, frameCounter: 0),
        "overlapping stand begins crouch transition");
    AssertEqual(48, sunkSamus.Position.Y.Whole, "command seven clips its crouch descent at the floor");

    for (int tick = 0; tick < 3; tick++)
        SamusPresentation.AnimateNoFx(samus, new SamusAnimationStep { Random = new SharedRandom() });
    AssertTrue(SamusPoseTransitions.ApplyPendingVerifiedAnimationTransition(samus), "crouch FD applies");
    AssertEqual(SamusPoseId.CrouchingRightPose, samus.Pose, "crouch transition target");
    samus.HorizontalSpeed.HasRunningMomentum = true;
    samus.HorizontalSpeed.SpeedBoostCounter = new SamusSpeedBoostCounter(0x0401);
    samus.HorizontalSpeed.BaseSpeed = samus.HorizontalSpeed.BaseSpeed.WithWhole(1);
    GroundedMovementResult crouchFrame = SamusPostureMovement.StepCrouching(
level, samus, frameCounter: 0);
    AssertTrue(crouchFrame.Vertical.Collided, "crouch performs grounded probe");
    AssertEqual(0, samus.HorizontalSpeed.BaseSpeed.Whole, "crouch clears numeric horizontal velocity");
    AssertEqual(0x0401, samus.HorizontalSpeed.SpeedBoostCounter.RawValue, "crouch movement preserves boost stage");
    AssertTrue(samus.HorizontalSpeed.HasRunningMomentum, "crouch movement preserves momentum bookkeeping");
    samus.HorizontalSpeed.ApplyStoppedInputFallback(SamusPoseMetadata.ReadFacingDirection(samus));
    AssertEqual(0, samus.HorizontalSpeed.SpeedBoostCounter.RawValue, "crouch input fallback independently cancels boost");
    AssertTrue(!samus.HorizontalSpeed.HasRunningMomentum, "fallback cancels momentum bookkeeping");

    AssertTrue(
        SamusPosturePoseTransitions.TryApplyPostureTransition(samus, 
level, SamusPoseId.StandingTransitionRightPose, frameCounter: 1),
        "crouch begins standing transition");
    AssertEqual(16, samus.Kinematics.Radius.Y, "standing commit retains crouch radius");
    AssertEqual(43, samus.Position.Y.Whole, "floor-constrained expansion moves center up five");
    SamusPoseMetadata.RefreshCollisionRadii(samus);
    AssertEqual(21, samus.Kinematics.Radius.Y, "next alpha publishes standing transition radius");
    for (int tick = 0; tick < 3; tick++)
        SamusPresentation.AnimateNoFx(samus, new SamusAnimationStep { Random = new SharedRandom() });
    AssertTrue(SamusPoseTransitions.ApplyPendingVerifiedAnimationTransition(samus), "standing FD applies");
    AssertEqual(SamusPoseId.FacingRightNormalPose, samus.Pose, "standing transition target");

    // Exercise all six facing/direction variants through both radius-changing halves. This
    // is intentionally a table test because the retail transition tables route every one
    // through the same two native collision commands but different `$FD` targets.
    for (int index = 0; index < 6; index++)
    {
        bool facesLeft = (index & 1) != 0;
        var aimedSamus = new SamusState
        {
            Pose = facesLeft ? SamusPoseId.FacingLeftNormalPose : SamusPoseId.FacingRightNormalPose,
            Position = FixedPosition<RoomPixels>.FromWhole(new(48, 43)),
        };
        SamusPoseMetadata.RefreshCollisionRadii(aimedSamus);
        SamusPresentation.InitializeAnimation(aimedSamus);
        AssertTrue(
            SamusPosturePoseTransitions.TryApplyPostureTransition(aimedSamus, 
level, aimedCrouchTransitions[index], unchecked((ushort)index)),
            $"aimed crouch transition ${(int)(aimedCrouchTransitions[index]):X2} begins");
        AssertEqual(16, aimedSamus.Kinematics.Radius.Y, "aimed crouch transition radius");
        AssertEqual(48, aimedSamus.Position.Y.Whole, "aimed crouch keeps feet aligned");
        SamusPostureMovement.StepCrouchStandTransition(
level, aimedSamus, unchecked((ushort)index));
        for (int tick = 0; tick < 3; tick++)
            SamusPresentation.AnimateNoFx(aimedSamus, new SamusAnimationStep { Random = new SharedRandom() });
        AssertTrue(SamusPoseTransitions.ApplyPendingVerifiedAnimationTransition(aimedSamus), "aimed crouch FD applies");
        AssertEqual(aimedCrouchTargets[index], aimedSamus.Pose, "aimed crouch FD target");
        AssertEqual(
            facesLeft ? SamusPoseId.CrouchingLeftPose : SamusPoseId.CrouchingRightPose,
            SamusPoseMetadata.ReadNoInputFallbackPose(aimedSamus),
            "aimed crouch definition fallback");
        GroundedMovementResult aimedCrouchFrame = SamusPostureMovement.StepCrouching(
level, aimedSamus, unchecked((ushort)index));
        AssertTrue(aimedCrouchFrame.Vertical.Collided, "aimed crouch remains grounded");

        AssertTrue(
            SamusPosturePoseTransitions.TryApplyPostureTransition(aimedSamus, 
level, aimedStandTransitions[index], unchecked((ushort)index)),
            $"aimed stand transition ${(int)(aimedStandTransitions[index]):X2} begins");
        AssertEqual(16, aimedSamus.Kinematics.Radius.Y, "aimed standing commit retains crouch radius");
        AssertEqual(43, aimedSamus.Position.Y.Whole, "aimed stand keeps feet aligned");
        SamusPoseMetadata.RefreshCollisionRadii(aimedSamus);
        AssertEqual(21, aimedSamus.Kinematics.Radius.Y, "aimed stand next alpha radius");
        for (int tick = 0; tick < 3; tick++)
            SamusPresentation.AnimateNoFx(aimedSamus, new SamusAnimationStep { Random = new SharedRandom() });
        AssertTrue(SamusPoseTransitions.ApplyPendingVerifiedAnimationTransition(aimedSamus), "aimed stand FD applies");
        AssertEqual(aimedStandTargets[index], aimedSamus.Pose, "aimed stand FD target");
    }

    // Equal-radius live shoulder changes never route through pose-change collision. Verify
    // both facing families and the definition-byte-two return to ordinary crouch.
    var crouchAim = new SamusState
    {
        Pose = SamusPoseId.CrouchingRightPose,
        Position = FixedPosition<RoomPixels>.FromWhole(new(48, 48)),
    };
    SamusPoseMetadata.RefreshCollisionRadii(crouchAim);
    SamusPresentation.InitializeAnimation(crouchAim);
    foreach (SamusPoseId target in new SamusPoseId[] {
        SamusPoseId.CrouchingAimUpRightPose,
        SamusPoseId.CrouchingAimDiagonalUpRightPose,
        SamusPoseId.CrouchingAimDiagonalDownRightPose,
        SamusPoseId.CrouchingRightPose,
    })
    {
        SamusGroundedPoseTransitions.ApplyGroundedAimTransition(crouchAim, target);
        AssertEqual(16, crouchAim.Kinematics.Radius.Y, $"right crouch aim ${(int)target:X2} radius");
    }
    AssertThrows<InvalidOperationException>(
        () => SamusGroundedPoseTransitions.ApplyGroundedAimTransition(crouchAim, SamusPoseId.CrouchingAimUpLeftPose),
        "crouch aim cannot cross facing families");

    // Releasing Down while retaining the facing direction matches `$91:A6A0`'s direct
    // `$27 -> $01` record. This is not the `$3B` standing animation: expansion collision
    // and floor alignment happen at the post-input seam, but `$91:FDAE` leaves the old
    // live radius published until ordinary alpha begins the following frame.
    var directStand = new SamusState
    {
        Pose = SamusPoseId.CrouchingRightPose,
        Position = FixedPosition<RoomPixels>.FromWhole(new(48, 48)),
    };
    SamusPoseMetadata.RefreshCollisionRadii(directStand);
    SamusPresentation.InitializeAnimation(directStand);
    AssertTrue(
        SamusPosturePoseTransitions.TryApplyDirectCrouchToStandingTransition(directStand, 
level, SamusPoseId.FacingRightNormalPose, frameCounter: 0),
        "direct crouch-to-standing record applies");
    AssertEqual(SamusPoseId.FacingRightNormalPose, directStand.Pose, "direct crouch exit target");
    AssertEqual(16, directStand.Kinematics.Radius.Y, "direct crouch exit commit retains live crouch radius");
    AssertEqual(43, directStand.Position.Y.Whole, "direct crouch exit keeps feet aligned");
    SamusPoseMetadata.RefreshCollisionRadii(directStand);
    AssertEqual(21, directStand.Kinematics.Radius.Y, "next alpha publishes direct crouch exit radius");
    AssertEqual(43, directStand.Position.Y.Whole, "alpha does not repeat direct crouch exit center correction");

    var directStandLeft = new SamusState
    {
        Pose = SamusPoseId.CrouchingLeftPose,
        Position = FixedPosition<RoomPixels>.FromWhole(new(48, 48)),
    };
    SamusPoseMetadata.RefreshCollisionRadii(directStandLeft);
    SamusPresentation.InitializeAnimation(directStandLeft);
    AssertTrue(
        SamusPosturePoseTransitions.TryApplyDirectCrouchToStandingTransition(directStandLeft, 
level, SamusPoseId.FacingLeftNormalPose, frameCounter: 1),
        "mirrored direct crouch-to-standing record applies");
    AssertEqual(SamusPoseId.FacingLeftNormalPose, directStandLeft.Pose, "mirrored direct crouch exit target");
    AssertEqual(16, directStandLeft.Kinematics.Radius.Y, "mirrored direct crouch exit retains live radius");
    AssertEqual(43, directStandLeft.Position.Y.Whole, "mirrored direct crouch exit alignment");

    // `$91:FC66` first accepts the 16 -> 19 radius expansion, moving the center up three
    // against the floor, then subtracts ten more pixels only when PreviousPose is exactly
    // ordinary crouch `$27/$28`. Make_Samus_Jump follows with the ROM's 4.E000 velocity.
    var crouchJump = new SamusState
    {
        Pose = SamusPoseId.CrouchingRightPose,
        Position = FixedPosition<RoomPixels>.FromWhole(new(48, 48)),
    };
    SamusPoseMetadata.RefreshCollisionRadii(crouchJump);
    SamusPresentation.InitializeAnimation(crouchJump);
    AssertTrue(
        SamusPosturePoseTransitions.TryApplyCrouchJumpTransition(crouchJump, 
level, SamusPoseId.NeutralJumpTransitionRightPose, frameCounter: 0),
        "ordinary crouch jump applies");
    AssertEqual(SamusPoseId.NeutralJumpTransitionRightPose, crouchJump.Pose, "ordinary crouch jump transition pose");
    AssertEqual(16, crouchJump.Kinematics.Radius.Y, "crouch jump commit retains live crouch radius");
    AssertEqual(35, crouchJump.Position.Y.Whole, "ordinary crouch jump collision plus FC8A offset");
    SamusPoseMetadata.RefreshCollisionRadii(crouchJump);
    AssertEqual(19, crouchJump.Kinematics.Radius.Y, "next alpha publishes crouch jump radius");
    AssertEqual(35, crouchJump.Position.Y.Whole, "alpha does not repeat crouch jump center correction");
    AssertEqual(4, crouchJump.Kinematics.YSpeed.Whole, "ordinary crouch jump Y speed");
    AssertEqual(0xe000, crouchJump.Kinematics.YSpeed.Fraction, "ordinary crouch jump Y subspeed");
    AssertEqual(SamusVerticalDirection.Up, crouchJump.Kinematics.YDirection, "ordinary crouch jump rises");

    var crouchJumpLeft = new SamusState
    {
        Pose = SamusPoseId.CrouchingLeftPose,
        Position = FixedPosition<RoomPixels>.FromWhole(new(48, 48)),
    };
    SamusPoseMetadata.RefreshCollisionRadii(crouchJumpLeft);
    SamusPresentation.InitializeAnimation(crouchJumpLeft);
    AssertTrue(
        SamusPosturePoseTransitions.TryApplyCrouchJumpTransition(crouchJumpLeft, 
level, SamusPoseId.NeutralJumpTransitionLeftPose, frameCounter: 1),
        "mirrored ordinary crouch jump applies");
    AssertEqual(SamusPoseId.NeutralJumpTransitionLeftPose, crouchJumpLeft.Pose, "mirrored crouch jump transition pose");
    AssertEqual(35, crouchJumpLeft.Position.Y.Whole, "mirrored crouch jump Y adjustment");

    // The native literal-pose comparison intentionally excludes aimed crouches. They use
    // the same `$4B` art and jump velocity but receive only the three-pixel floor-alignment
    // adjustment from pose-change collision.
    var aimedCrouchJump = new SamusState
    {
        Pose = SamusPoseId.CrouchingAimDiagonalUpRightPose,
        Position = FixedPosition<RoomPixels>.FromWhole(new(48, 48)),
    };
    SamusPoseMetadata.RefreshCollisionRadii(aimedCrouchJump);
    SamusPresentation.InitializeAnimation(aimedCrouchJump);
    AssertTrue(
        SamusPosturePoseTransitions.TryApplyCrouchJumpTransition(aimedCrouchJump, 
level, SamusPoseId.NeutralJumpTransitionRightPose, frameCounter: 1),
        "aimed crouch jump applies");
    AssertEqual(45, aimedCrouchJump.Position.Y.Whole, "aimed crouch jump omits FC8A offset");

    // Ceiling row one ends at pixel 31. A crouched body occupies 32..63 exactly, while a
    // standing body would need 27..63. Both five-pixel probes collide, so native pose
    // collision rejects the larger pose and retains crouch.
    var tunnelBlocks = (ushort[])floor.Clone();
    for (int x = 0; x < width; x++)
        tunnelBlocks[1 * width + x] = 0x8000;
    RoomLevelData tunnel = CreateRoom(new(width, height), tunnelBlocks, new byte[tunnelBlocks.Length]);
    var tunnelSamus = new SamusState
    {
        Pose = SamusPoseId.CrouchingRightPose,
        Position = FixedPosition<RoomPixels>.FromWhole(new(48, 48)),
    };
    SamusPoseMetadata.RefreshCollisionRadii(tunnelSamus);
    SamusPresentation.InitializeAnimation(tunnelSamus);
    AssertTrue(
        !SamusPosturePoseTransitions.TryApplyPostureTransition(tunnelSamus, 
tunnel, SamusPoseId.StandingTransitionRightPose, frameCounter: 0),
        "low tunnel rejects standing radius expansion");
    AssertEqual(SamusPoseId.CrouchingRightPose, tunnelSamus.Pose, "rejected stand retains crouch pose");
    AssertEqual(48, tunnelSamus.Position.Y.Whole, "rejected stand preserves center Y");

    var tunnelJump = new SamusState
    {
        Pose = SamusPoseId.CrouchingAimUpRightPose,
        Position = FixedPosition<RoomPixels>.FromWhole(new(48, 48)),
    };
    SamusPoseMetadata.RefreshCollisionRadii(tunnelJump);
    SamusPresentation.InitializeAnimation(tunnelJump);
    AssertTrue(
        !SamusPosturePoseTransitions.TryApplyCrouchJumpTransition(tunnelJump, 
tunnel, SamusPoseId.NeutralJumpTransitionRightPose, frameCounter: 1),
        "low tunnel rejects crouch-jump radius expansion");
    AssertEqual(SamusPoseId.CrouchingRightPose, tunnelJump.Pose, "boxed aimed jump falls back to ordinary crouch");
    AssertEqual(16, tunnelJump.Kinematics.Radius.Y, "boxed aimed jump retains crouch radius");
    AssertEqual(0, tunnelJump.Kinematics.YSpeed.Whole, "boxed aimed jump does not call Make_Samus_Jump");

    Console.WriteLine("  Samus posture: animated/direct exits, crouch jumps, radii, movement, FD targets, and low-ceiling fallback agree.");
}

/// <summary>Synthetic cartridge with the literal grapple-block PLM streams and their one-block draw records.</summary>
static TestAddressSpace CreateBreakableGrappleCartridge()
{
    var bus = new TestAddressSpace();

    // These bytes are the literal $84:CD6A and $84:CDA9 instruction streams. Production
    // code interprets these ROM words; the fixture does not inject a friendly C# timeline.
    SeedPlmProgramBytes(bus, 0xcd6a, [
        0xf0, 0x00, 0xf9, 0xa4,
        0x10, 0x8c, 0x0a,
        0x04, 0x00, 0xff, 0xa4,
        0x04, 0x00, 0x05, 0xa5,
        0x04, 0x00, 0x0b, 0xa5,
        0x06, 0x00, 0x11, 0xa5,
        0x04, 0x00, 0x0b, 0xa5,
        0x04, 0x00, 0x05, 0xa5,
        0x04, 0x00, 0xff, 0xa4,
        0x93, 0xcd,
        0x17, 0x8b,
        0xbc, 0x86,
    ]);
    SeedPlmProgramBytes(bus, 0xcda9, [
        0x78, 0x00, 0xf9, 0xa4,
        0x10, 0x8c, 0x0a,
        0x04, 0x00, 0xff, 0xa4,
        0x04, 0x00, 0x05, 0xa5,
        0x04, 0x00, 0x0b, 0xa5,
        0x01, 0x00, 0x11, 0xa5,
        0xbc, 0x86,
    ]);

    // Each native draw instruction is `{one block, complete level word, terminator}`.
    SeedPlmProgramBytes(bus, 0xa4f9, [0x01, 0x00, 0xb7, 0xe0, 0x00, 0x00]);
    SeedPlmProgramBytes(bus, 0xa4ff, [0x01, 0x00, 0x53, 0x00, 0x00, 0x00]);
    SeedPlmProgramBytes(bus, 0xa505, [0x01, 0x00, 0x54, 0x00, 0x00, 0x00]);
    SeedPlmProgramBytes(bus, 0xa50b, [0x01, 0x00, 0x55, 0x00, 0x00, 0x00]);
    SeedPlmProgramBytes(bus, 0xa511, [0x01, 0x00, 0xff, 0x00, 0x00, 0x00]);
    return bus;
}

/// <summary>Seeds BTS-3 and BTS-6 bomb-block heads, shared bank-$84 tails, and draw records for collision checks.</summary>
static void SeedBombBlockPlmPrograms(TestAddressSpace bus)
{
    // The following bytes are the exact BTS-3/BTS-6 collision heads and their shared
    // bank-$84 tails. Together they exercise every structural feature that the bomb-block
    // family adds over grapple: queue-cap three, GotoY, a signed-offset second row, a true
    // vertical record, linked-block restoration, and the respawning/permanent split.
    SeedPlmProgramBytes(bus, 0xccb7, [0x46, 0x8c, 0x06, 0x24, 0x87, 0xc1, 0xcc]);
    SeedPlmProgramBytes(bus, 0xccc1, [
        0x04, 0x00, 0x9d, 0xa3,
        0x04, 0x00, 0xad, 0xa3,
        0x04, 0x00, 0xbd, 0xa3,
        0x80, 0x01, 0xcd, 0xa3,
        0x04, 0x00, 0xbd, 0xa3,
        0x04, 0x00, 0xad, 0xa3,
        0x04, 0x00, 0x9d, 0xa3,
        0x01, 0x00, 0xd7, 0xa4,
        0xbc, 0x86,
    ]);
    SeedPlmProgramBytes(bus, 0xcd1b, [0x46, 0x8c, 0x06, 0x24, 0x87, 0x25, 0xcd]);
    SeedPlmProgramBytes(bus, 0xcd25, [
        0x04, 0x00, 0x7d, 0xa3,
        0x04, 0x00, 0x85, 0xa3,
        0x04, 0x00, 0x8d, 0xa3,
        0x01, 0x00, 0x95, 0xa3,
        0xbc, 0x86,
    ]);

    // `$A39D-$A3DC`: 2x2 animation draw records. Each begins with a two-word horizontal
    // row, then signed offset bytes `{0,+1}`, another two-word row, and a zero terminator.
    static void Write2x2Draw(TestAddressSpace fixtureBus, int address, ushort levelWord)
    {
        fixtureBus.WriteBytes(address, [
            0x02, 0x00,
            unchecked((byte)levelWord), LittleEndianBytes.HighByte(levelWord),
            unchecked((byte)levelWord), LittleEndianBytes.HighByte(levelWord),
            0x00, 0x01,
            0x02, 0x00,
            unchecked((byte)levelWord), LittleEndianBytes.HighByte(levelWord),
            unchecked((byte)levelWord), LittleEndianBytes.HighByte(levelWord),
            0x00, 0x00,
        ]);
    }
    Write2x2Draw(bus, 0x84a39d, 0x0053);
    Write2x2Draw(bus, 0x84a3ad, 0x0054);
    Write2x2Draw(bus, 0x84a3bd, 0x0055);
    Write2x2Draw(bus, 0x84a3cd, 0x00ff);
    SeedPlmProgramBytes(bus, 0xa4d7, [
        0x02, 0x00, 0x58, 0xf0, 0x58, 0x50,
        0x00, 0x01,
        0x02, 0x00, 0x58, 0xd0, 0x58, 0xd0,
        0x00, 0x00,
    ]);

    // `$A37D-$A39C`: vertical two-word records used by 1x2 blocks. The high count bit is
    // significant—the second level word advances by room width, never by one column.
    static void Write1x2Draw(TestAddressSpace fixtureBus, int address, ushort levelWord)
    {
        fixtureBus.WriteBytes(address, [
            0x02, 0x80,
            unchecked((byte)levelWord), LittleEndianBytes.HighByte(levelWord),
            unchecked((byte)levelWord), LittleEndianBytes.HighByte(levelWord),
            0x00, 0x00,
        ]);
    }
    Write1x2Draw(bus, 0x84a37d, 0x0053);
    Write1x2Draw(bus, 0x84a385, 0x0054);
    Write1x2Draw(bus, 0x84a38d, 0x0055);
    Write1x2Draw(bus, 0x84a395, 0x00ff);
}

/// <summary>Synthetic cartridge with the literal pose definitions the posture transitions consult.</summary>
static TestAddressSpace CreateSamusPostureCartridge()
{
    var bus = new TestAddressSpace();
    bus.WriteBytes(0x91b631, [0x08, 0x00, 0xff, 0x02, 0x06, 0x00, 0x15, 0x00]); // $01
    bus.WriteBytes(0x91b639, [0x04, 0x00, 0xff, 0x07, 0x06, 0x00, 0x15, 0x00]); // $02
    bus.WriteBytes(0x91b641, [0x08, 0x00, 0x01, 0x00, 0x06, 0x00, 0x15, 0x00]); // $03
    bus.WriteBytes(0x91b649, [0x04, 0x00, 0x02, 0x09, 0x06, 0x00, 0x15, 0x00]); // $04
    bus.WriteBytes(0x91b651, [0x08, 0x00, 0x01, 0x01, 0x06, 0x00, 0x15, 0x00]); // $05
    bus.WriteBytes(0x91b659, [0x04, 0x00, 0x02, 0x08, 0x06, 0x00, 0x15, 0x00]); // $06
    bus.WriteBytes(0x91b661, [0x08, 0x00, 0x01, 0x03, 0x06, 0x00, 0x15, 0x00]); // $07
    bus.WriteBytes(0x91b669, [0x04, 0x00, 0x02, 0x06, 0x06, 0x00, 0x15, 0x00]); // $08
    bus.WriteBytes(0x91b761, [0x08, 0x05, 0x27, 0x02, 0x00, 0x00, 0x10, 0x00]); // $27
    bus.WriteBytes(0x91b769, [0x04, 0x05, 0x28, 0x07, 0x00, 0x00, 0x10, 0x00]); // $28
    bus.WriteBytes(0x91b7d1, [0x08, 0x0f, 0xff, 0x02, 0x00, 0x00, 0x10, 0x00]); // $35
    bus.WriteBytes(0x91b801, [0x08, 0x0f, 0xff, 0x02, 0x06, 0x00, 0x15, 0x00]); // $3B
    bus.WriteBytes(0x91b881, [0x08, 0x02, 0xff, 0x02, 0x03, 0x00, 0x13, 0x00]); // $4B
    bus.WriteBytes(0x91b889, [0x04, 0x02, 0xff, 0x07, 0x03, 0x00, 0x13, 0x00]); // $4C
    bus.WriteBytes(0x91b9b1, [0x08, 0x05, 0x27, 0x01, 0x00, 0x00, 0x10, 0x00]); // $71
    bus.WriteBytes(0x91b9b9, [0x04, 0x05, 0x28, 0x08, 0x00, 0x00, 0x10, 0x00]); // $72
    bus.WriteBytes(0x91b9c1, [0x08, 0x05, 0x27, 0x03, 0x00, 0x00, 0x10, 0x00]); // $73
    bus.WriteBytes(0x91b9c9, [0x04, 0x05, 0x28, 0x06, 0x00, 0x00, 0x10, 0x00]); // $74
    bus.WriteBytes(0x91ba51, [0x08, 0x05, 0x27, 0x00, 0x00, 0x00, 0x10, 0x00]); // $85
    bus.WriteBytes(0x91ba59, [0x04, 0x05, 0x28, 0x09, 0x00, 0x00, 0x10, 0x00]); // $86
    return bus;
}

/// <summary>Installs the two complete aimed crouch/stand transition pose definitions for one aim.</summary>
static void SeedAimedTransitionPoseDefinitions(
    TestAddressSpace bus, SamusPoseId crouchPose, SamusPoseId standPose, byte direction, byte shot)
{
    int crouchAddress = 0x91b629 + (int)crouchPose * 8;
    bus.WriteBytes(crouchAddress, [
        direction, 0x0f, 0xff, shot,
        0x08, 0x00, 0x10, 0x00,
    ]);
    int standAddress = 0x91b629 + (int)standPose * 8;
    bus.WriteBytes(standAddress, [
        direction, 0x0f, 0xff, shot,
        0x03, 0x00, 0x15, 0x00,
    ]);
}

/// <summary>Seeds native dry-air jump and gravity table-zero words for posture movement checks.</summary>
static void SeedDryAirJumpTableZero(TestAddressSpace bus)
{
    // Dry-air table-zero values used by Make_Samus_Jump and normal-air gravity. Keeping
    // these as literal ROM words makes a crouch jump observable beyond merely changing pose.
    bus.WriteBytes(0x909eb9, [0x04, 0x00]);
    bus.WriteBytes(0x909ebf, [0x00, 0xe0]);
    bus.WriteBytes(0x909ea1, [0x00, 0x1c]);
    bus.WriteBytes(0x909ea7, [0x00, 0x00]);
}

    /// <summary>Runs the focused Samus posture and grounded-movement fixture.</summary>
    [VerificationCommand("--samus-posture-movement")]
    internal static int SamusPostureMovementCommand()
    {
        Suite(nameof(VerifySamusPostureMovement), () => VerifySamusPostureMovement());
        return 0;
    }
}
