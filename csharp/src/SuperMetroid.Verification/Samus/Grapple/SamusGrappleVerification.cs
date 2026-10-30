using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Assets;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using static SuperMetroid.Verification.Samus.Artwork.SamusRenderingFixtureVerification;
using static SuperMetroid.Verification.Harness.SyntheticSamusCartridgeVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Grapple;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.AssetExtraction.Samus;
namespace SuperMetroid.Verification.Samus.Grapple;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Audio;

/// <summary>Checks grapple firing, swing geometry, wall contacts, beam drawing, and release poses.</summary>
internal static class SamusGrappleVerification
{

/// <summary>Checks attached swing motion, collision branches, wall jump, dropped pose, OAM drawing, and release.</summary>
internal static void VerifySamusGrappleSwingAndRelease()
{
    var bus = new TestAddressSpace();
    void BindGrapplePresentation(SamusState state)
    {
        BindSyntheticSamusRendering(bus, state);
        // Capture the fixture's currently authored visual offsets at construction time.
        state.Grapple.FlarePlacement = ChargeFlarePlacements.Load(
            new MemoryStream(GrappleFlarePlacementExtractor.Extract(bus)));
        state.Grapple.SwingFrames = InstalledGrappleSwingFrames.Load(
            new MemoryStream(GrappleSwingFrameExtractor.Extract(bus)));
    }
    SamusState CreateSamus(SamusPoseId pose, Position16<RoomPixels> position)
    {
        SamusState state = VerificationHelpersVerification.CreateSamus(pose, position);
        BindGrapplePresentation(state);
        return state;
    }

    // Engine trigonometry is compiled, not a synthetic presentation asset. Leave
    // the sine region absent: geometry must use native vectors, not patched waves.

    // Pose definitions are literal eight-byte records. Only X direction, movement type,
    // graphics offset, and radii matter to this isolated route. $B2 is right-facing/type
    // $16; $B3 is its left-facing mirror. Release poses $51/$52 return to type two.
    WritePoseDefinition(bus, (int)SamusPoseId.GrappleSwingRightPose,
        [0x08, 0x16, 0xff, 0x02, 0x00, 0x00, 0x05, 0x15]);
    WritePoseDefinition(bus, (int)SamusPoseId.GrappleSwingLeftPose,
        [0x04, 0x16, 0xff, 0x07, 0x00, 0x00, 0x05, 0x15]);
    WritePoseDefinition(bus, (int)SamusPoseId.NormalJumpForwardRightPose,
        [0x08, 0x02, 0xff, 0x02, 0x00, 0x00, 0x05, 0x15]);
    WritePoseDefinition(bus, (int)SamusPoseId.NormalJumpForwardLeftPose,
        [0x04, 0x02, 0xff, 0x07, 0x00, 0x00, 0x05, 0x15]);

    // The close-collision routes use a second family of type-$16 records. These bytes are
    // the retail pose definitions, not convenient test metadata: `$B6/$B7` are locked
    // crouching-down poses, `$B8/$B9` are the two wall contacts, and `$83/$84` are the
    // ordinary wall-jump launch poses selected one function call later. The dropped route
    // below uses compact diagonal-down `$74`; locked cancellation uses stable crouch `$27`.
    WritePoseDefinition(bus, (int)SamusPoseId.GrappleCrouchingDownRightPose,
        [0x08, 0x16, 0x27, 0x03, 0x00, 0x00, 0x10, 0x00]);
    WritePoseDefinition(bus, (int)SamusPoseId.GrappleCrouchingDownLeftPose,
        [0x04, 0x16, 0x28, 0x06, 0x00, 0x00, 0x10, 0x00]);
    WritePoseDefinition(bus, (int)SamusPoseId.GrappleWallContactLeftPose,
        [0x08, 0x16, 0xff, 0x03, 0x00, 0x00, 0x10, 0x00]);
    WritePoseDefinition(bus, (int)SamusPoseId.GrappleWallContactRightPose,
        [0x04, 0x16, 0xff, 0x06, 0x00, 0x00, 0x10, 0x00]);
    WritePoseDefinition(bus, (int)SamusPoseId.WallJumpRightPose,
        [0x08, 0x14, 0x19, 0xff, 0x08, 0x00, 0x13, 0x00]);
    WritePoseDefinition(bus, (int)SamusPoseId.WallJumpLeftPose,
        [0x04, 0x14, 0x1a, 0xff, 0x08, 0x00, 0x13, 0x00]);
    WritePoseDefinition(bus, (int)SamusPoseId.CrouchingRightPose,
        [0x08, 0x05, 0x27, 0x02, 0x00, 0x00, 0x10, 0x00]);
    WritePoseDefinition(bus, (int)SamusPoseId.CrouchingAimDiagonalDownLeftPose,
        [0x04, 0x05, 0x28, 0x06, 0x00, 0x00, 0x10, 0x00]);

    // `$9B:B9D9-$BA29` can install every one of these six additional type-$16 poses when
    // a stationary firing beam connects. These are literal retail pose-definition bytes:
    // `$A8-$AB` are full-height standing locks, while `$B4/$B5` are the two crouching
    // diagonal-up locks. `$B6/$B7` above complete the crouching family.
    WritePoseDefinition(bus, 0xa8,
        [0x08, 0x16, 0x01, 0x02, 0x06, 0x00, 0x15, 0x00]);
    WritePoseDefinition(bus, 0xa9,
        [0x04, 0x16, 0x02, 0x07, 0x06, 0x00, 0x15, 0x00]);
    WritePoseDefinition(bus, 0xaa,
        [0x08, 0x16, 0x07, 0x03, 0x06, 0x00, 0x15, 0x00]);
    WritePoseDefinition(bus, 0xab,
        [0x04, 0x16, 0x08, 0x06, 0x06, 0x00, 0x15, 0x00]);
    WritePoseDefinition(bus, 0xb4,
        [0x08, 0x16, 0x27, 0x02, 0x00, 0x00, 0x10, 0x00]);
    WritePoseDefinition(bus, 0xb5,
        [0x04, 0x16, 0x28, 0x07, 0x00, 0x00, 0x10, 0x00]);

    // All four poses point at a harmless ordinary delay list so the public connection and
    // release initializers can execute their real animation initialization seam.
    foreach (SamusPoseId pose in new SamusPoseId[]
    {
        SamusPoseId.GrappleSwingRightPose,
        SamusPoseId.GrappleSwingLeftPose,
        SamusPoseId.NormalJumpForwardRightPose,
        SamusPoseId.NormalJumpForwardLeftPose,
        SamusPoseId.GrappleCrouchingDownRightPose,
        SamusPoseId.GrappleCrouchingDownLeftPose,
        SamusPoseId.GrappleWallContactLeftPose,
        SamusPoseId.GrappleWallContactRightPose,
        SamusPoseId.WallJumpRightPose,
        SamusPoseId.WallJumpLeftPose,
        SamusPoseId.CrouchingRightPose,
        SamusPoseId.CrouchingAimDiagonalDownLeftPose,
        SamusPoseId.GrappleStandingRightPose,
        SamusPoseId.GrappleStandingLeftPose,
        SamusPoseId.GrappleStandingDownRightPose,
        SamusPoseId.GrappleStandingDownLeftPose,
        SamusPoseId.GrappleCrouchingRightPose,
        SamusPoseId.GrappleCrouchingLeftPose,
    })
    {
        WritePoseAnimationDelayPointer(bus, pose, 0xbf00);
    }
    WriteAnimationDelayStream(bus, 0xbf00, [0x05, 0xff]);

    // Dry-air wall-jump table zero is 4.A000. A grapple wall jump reaches the same
    // `$90:9949` initializer as an ordinary spin wall jump after its reversed pose choice.
    SyntheticSamusCartridge.WriteDryAirWallJumpSpeed(bus, whole: 0x0004, subspeed: 0xa000);

    // Firing source pose $29 is a retail right-facing fall with shot direction two. The
    // compiled mechanics supply +11.F4 X velocity, zero Y velocity, angle $C000,
    // and (+2,+2) hand origin minus the native eight-pixel pose correction.
    // Flare presentation is deliberately synthetic.
    WritePoseDefinition(bus, (int)SamusPoseId.FallingRightPose,
        [0x08, 0x06, 0xff, 0x02, 0x00, 0x00, 0x05, 0x15]);
    WriteGrappleFlareOrigin(bus, direction: 2, new Offset16<RoomPixels>(2, 2));

    // Compiled native connection pairs include the crouching table's intentional
    // standing `$AB` entries. Routing assertions below still cover all thirty records.

    // A type-$E/BTS-$00 block is persistent grapple PLM $D0D8 and returns flags $41.
    // Put it at (3,3): the first frame's four 16.16 substeps end at X=45, then frame two's
    // first substep reaches X=48 and must center the accepted endpoint at (56,56).
    var firingBlocks = new RoomTerrainBlock[8 * 8];
    firingBlocks[3 * 8 + 3] = RoomTerrainBlock.Of(0, BlockMirroring.None, RoomCollisionType.GrappleBlock);
    RoomLevelData firingLevel = CreateRoom(new(8, 8), firingBlocks, new RoomBlockBehavior[firingBlocks.Length]);

    // Samus at Y=56 puts the corrected firing endpoint at Y=50 inside the target.
    // Samus minus its centered anchor is (-24,0), exactly angle $C0.
    WriteGrappleSwingFrame(bus, angle: 0xc0, frame: 5);

    SamusState firingSamus = CreateSamus(SamusPoseId.FallingRightPose, new(32, 56));
    firingSamus.Kinematics.YSpeed = firingSamus.Kinematics.YSpeed.WithWhole(1); // selects moving-vertically connection table $C3EE
    SamusGrappleMovement.BeginFiring(firingSamus);
    AssertEqual(GrapplePhase.Firing, firingSamus.Grapple.Phase, "grapple firing phase");
    AssertEqual(0x0bf4, firingSamus.Grapple.ExtensionVelocity.X.SignedRawValue,
        "grapple X extension velocity comes from ROM");
    AssertEqual(34, firingSamus.Grapple.Anchor.X, "grapple initial endpoint X");
    AssertEqual(50, firingSamus.Grapple.Anchor.Y, "grapple initial endpoint Y");

    GrappleMovementResult extending = SamusGrappleMovement.StepFiring(
firingLevel, firingSamus, ControllerButton.X);
    AssertTrue(extending.Fired && extending.Phase == GrapplePhase.Firing && !extending.OwnsMovement,
        "unobstructed firing remains live without stealing ordinary body movement");
    AssertEqual(12, firingSamus.Grapple.RopeLength, "firing length grows by twelve");
    AssertEqual(45, firingSamus.Grapple.Anchor.X,
        "four fractional collision substeps publish the exact first-frame endpoint");

    GrappleMovementResult connected = SamusGrappleMovement.StepFiring(
firingLevel, firingSamus, ControllerButton.X);
    AssertTrue(connected.Phase is GrapplePhase.ConnectedSwinging or GrapplePhase.ConnectedLocked && connected.OwnsMovement,
        "persistent grapple block establishes connected movement");
    AssertEqual(GrapplePhase.ConnectedSwinging, firingSamus.Grapple.Phase,
        "block acquisition installs swinging function");
    AssertEqual(55, firingSamus.Grapple.Anchor.X,
        "accepted grapple block centers X then applies negative-rope side bias");
    AssertEqual(56, firingSamus.Grapple.Anchor.Y, "accepted grapple block applies nonnegative Y side bias");
    AssertEqual(0xc000, firingSamus.Grapple.Angle.RawValue,
        "connection angle uses bank-$A0 integer octant calculation");
    AssertEqual(SamusPoseId.GrappleSwingRightPose, firingSamus.Pose,
        "right-half airborne shot selects clockwise grapple pose $B2");
    AssertEqual(31, firingSamus.Grapple.RopeStart.X,
        "accepted connection publishes native rope Start X");
    AssertEqual(56, firingSamus.Grapple.RopeStart.Y,
        "accepted connection publishes native rope Start Y");
    AssertEqual(firingSamus.Grapple.RopeStart.X, firingSamus.Grapple.BeamStart.X,
        "swing command copies rope Start X into flare/draw X");
    AssertEqual(firingSamus.Grapple.RopeStart.Y, firingSamus.Grapple.BeamStart.Y,
        "swing command copies rope Start Y into flare/draw Y");
    AssertEqual(0, firingSamus.Kinematics.YSpeed.Whole,
        "connection common tail clears whole Y speed");
    AssertEqual(0, firingSamus.Position.X.Whole, "native frame $18 offsets body thirty-one pixels left of rope start");
    AssertEqual(12, (connected.CameraPrevious ?? throw new InvalidOperationException("Verification requires connected.CameraPrevious.")).X,
        "connection common tail clamps previous X to twelve pixels from native body");
    AssertEqual(59, connected.CameraPrevious.Value.Y,
        "connection common tail clamps previous Y to twelve pixels from native body");

    // The Grapple-speedkeep movie connects on the same frame that hit-interruption
    // expiry publishes a super-special pose. Native frame 412 already installs the
    // swinging function and rope geometry, but UpdateSamusPose suppresses Grapple's
    // lower-priority command nine. Recreate the relevant frame-411 values here: the
    // ordinary `$52` body and its 1.4000 + 0.B000 running momentum must survive until
    // the following connected-function call.
    SamusState deferredConnectionSamus = CreateSamus(
        SamusPoseId.FallingRightPose,
        new(32, 56));
    deferredConnectionSamus.Kinematics.YSpeed = deferredConnectionSamus.Kinematics.YSpeed.WithWhole(1);
    SamusGrappleMovement.BeginFiring(deferredConnectionSamus);
    SamusGrappleMovement.StepFiring(

        firingLevel,
        deferredConnectionSamus,
        ControllerButton.X);
    deferredConnectionSamus.Pose = SamusPoseId.NormalJumpForwardLeftPose;
    // Native frame 411 is already aimed left while the previously launched endpoint
    // reaches its block. Keep that accepted direction in sync so the firing-pose-change
    // pre-handler does not legitimately restart the beam before collision dispatch.
    deferredConnectionSamus.Grapple.FireDirection = SuperMetroid.Core.Samus.Projectiles.SamusProjectileDirection.Left;
    deferredConnectionSamus.HorizontalSpeed.BaseSpeed = new UnsignedSixteenSixteen(1, 0x4000);
    deferredConnectionSamus.HorizontalSpeed.ExtraRunSpeed = new UnsignedSixteenSixteen(0, 0xb000);
    deferredConnectionSamus.Kinematics.YSpeed = new UnsignedSixteenSixteen(3, 0x6070);

    GrappleMovementResult deferredConnection = SamusGrappleMovement.StepFiring(

        firingLevel,
        deferredConnectionSamus,
        ControllerButton.X,
        connectionPoseChange: GrapplePoseChange.Deferred);
    AssertTrue(deferredConnection.Phase is GrapplePhase.ConnectedSwinging or GrapplePhase.ConnectedLocked && !deferredConnection.OwnsMovement,
        "connection publishes its pose without prematurely taking beta movement");
    AssertEqual(GrapplePhase.ConnectedSwinging, deferredConnectionSamus.Grapple.Phase,
        "deferred connection still installs the native Grapple function");
    AssertEqual(SamusPoseId.NormalJumpForwardLeftPose, deferredConnectionSamus.Pose,
        "higher-priority pose owner can retain the ordinary body");
    AssertEqual(1, deferredConnectionSamus.HorizontalSpeed.BaseSpeed.Whole,
        "suppressed Grapple pose retains base whole speed");
    AssertEqual(0x4000, deferredConnectionSamus.HorizontalSpeed.BaseSpeed.Fraction,
        "suppressed Grapple pose retains base fractional speed");
    AssertEqual(0xb000, deferredConnectionSamus.HorizontalSpeed.ExtraRunSpeed.Fraction,
        "suppressed Grapple pose retains extra running momentum");
    AssertEqual(3, deferredConnectionSamus.Kinematics.YSpeed.Whole,
        "suppressed Grapple pose retains vertical whole speed");
    AssertEqual(0x6070, deferredConnectionSamus.Kinematics.YSpeed.Fraction,
        "suppressed Grapple pose retains vertical fractional speed");
    AssertTrue(deferredConnection.PendingConnection is
        { Pose: SamusPoseId.GrappleSwingLeftPose, Swinging: true },
        "connection exposes the lower-priority command-nine pose for late dispatch");
    AssertTrue(!deferredConnection.CameraPrevious.HasValue,
        "suppressed connection does not run command-nine camera correction");

    // BTS one must use the same accepted-connection path, but setup CFB5 also creates an
    // independent bank-$84 object and clears BTS before returning flags $41. This direct
    // firing test guards the integration seam; the complete ROM instruction timeline is
    // verified separately below.
    var breakableFiringBlocks = new RoomTerrainBlock[8 * 8];
    var breakableFiringBts = new RoomBlockBehavior[breakableFiringBlocks.Length];
    breakableFiringBlocks[3 * 8 + 3] = RoomTerrainBlock.Of(0, BlockMirroring.None, RoomCollisionType.GrappleBlock);
    breakableFiringBts[3 * 8 + 3] = new RoomBlockBehavior(1);
    RoomLevelData breakableFiringLevel = CreateRoom(new(8, 8), breakableFiringBlocks, breakableFiringBts);
    SamusState breakableFiringSamus = CreateSamus(SamusPoseId.FallingRightPose, new(32, 56));
    breakableFiringSamus.Kinematics.YSpeed = breakableFiringSamus.Kinematics.YSpeed.WithWhole(1);
    var breakableFiringPlms = PlmFixtures.Unbound();
    SamusGrappleMovement.BeginFiring(breakableFiringSamus);
    SamusGrappleMovement.StepFiring(

        breakableFiringLevel,
        breakableFiringSamus,
        ControllerButton.X,
        breakableFiringPlms);
    GrappleMovementResult breakableConnection = SamusGrappleMovement.StepFiring(

        breakableFiringLevel,
        breakableFiringSamus,
        ControllerButton.X,
        breakableFiringPlms);
    AssertTrue(breakableConnection.Phase is GrapplePhase.ConnectedSwinging or GrapplePhase.ConnectedLocked,
        "BTS-one grapple firing connects through PLM setup");
    AssertEqual(1, breakableFiringPlms.SlotTable.ActiveCount,
        "BTS-one acquisition installs one independent room PLM");
    AssertEqual(0, breakableFiringLevel.GetCollisionBlock(new(3, 3)).Behavior.Value,
        "BTS-one acquisition synchronously clears low BTS byte");

    // Bank $94 does not treat extension blocks as collision results of their own. Instead,
    // type $5 adds signed BTS directly to the linear block index and dispatches the block
    // found there. Keep the beam physically inside (3,3), but make that cell point right to
    // a persistent type-$E target at (4,3). Connecting proves the indirection is followed;
    // ending in the physical (3,3) cell, rather than (4,3), proves the visible endpoint stays in the
    // extension cell exactly as GrappleCollision_XBlock/YBlock do in the original routine.
    var horizontalExtensionBlocks = new RoomTerrainBlock[8 * 8];
    var horizontalExtensionBts = new RoomBlockBehavior[horizontalExtensionBlocks.Length];
    horizontalExtensionBlocks[3 * 8 + 3] = RoomTerrainBlock.Of(0, BlockMirroring.None, RoomCollisionType.HorizontalExtension);
    horizontalExtensionBts[3 * 8 + 3] = new RoomBlockBehavior(1);
    horizontalExtensionBlocks[3 * 8 + 4] = RoomTerrainBlock.Of(0, BlockMirroring.None, RoomCollisionType.GrappleBlock);
    RoomLevelData horizontalExtensionLevel = CreateRoom(new(8, 8), horizontalExtensionBlocks, horizontalExtensionBts);
    SamusState horizontalExtensionSamus = CreateSamus(SamusPoseId.FallingRightPose, new(32, 56));
    horizontalExtensionSamus.Kinematics.YSpeed = horizontalExtensionSamus.Kinematics.YSpeed.WithWhole(1);
    SamusGrappleMovement.BeginFiring(horizontalExtensionSamus);
    SamusGrappleMovement.StepFiring(
horizontalExtensionLevel, horizontalExtensionSamus, ControllerButton.X);
    GrappleMovementResult horizontalExtensionConnection = SamusGrappleMovement.StepFiring(
horizontalExtensionLevel, horizontalExtensionSamus, ControllerButton.X);
    AssertTrue(horizontalExtensionConnection.Phase is GrapplePhase.ConnectedSwinging or GrapplePhase.ConnectedLocked,
        "horizontal extension BTS dispatches its referenced grapple block");
    AssertEqual(55, horizontalExtensionSamus.Grapple.Anchor.X,
        "horizontal extension keeps physical endpoint block X");

    // Type $D uses the same signed byte but multiplies it by RoomWidthBlocks. A +1 BTS at
    // (3,3) therefore dispatches (3,4), while the accepted endpoint still centers in (3,3).
    // This test is intentionally separate from the horizontal case: confusing the two
    // formulas is easy and can appear correct in rooms whose nearby cells happen to be air.
    var verticalExtensionBlocks = new RoomTerrainBlock[8 * 8];
    var verticalExtensionBts = new RoomBlockBehavior[verticalExtensionBlocks.Length];
    verticalExtensionBlocks[3 * 8 + 3] = RoomTerrainBlock.Of(0, BlockMirroring.None, RoomCollisionType.VerticalExtension);
    verticalExtensionBts[3 * 8 + 3] = new RoomBlockBehavior(1);
    verticalExtensionBlocks[4 * 8 + 3] = RoomTerrainBlock.Of(0, BlockMirroring.None, RoomCollisionType.GrappleBlock);
    RoomLevelData verticalExtensionLevel = CreateRoom(new(8, 8), verticalExtensionBlocks, verticalExtensionBts);
    SamusState verticalExtensionSamus = CreateSamus(SamusPoseId.FallingRightPose, new(32, 56));
    verticalExtensionSamus.Kinematics.YSpeed = verticalExtensionSamus.Kinematics.YSpeed.WithWhole(1);
    SamusGrappleMovement.BeginFiring(verticalExtensionSamus);
    SamusGrappleMovement.StepFiring(
verticalExtensionLevel, verticalExtensionSamus, ControllerButton.X);
    GrappleMovementResult verticalExtensionConnection = SamusGrappleMovement.StepFiring(
verticalExtensionLevel, verticalExtensionSamus, ControllerButton.X);
    AssertTrue(verticalExtensionConnection.Phase is GrapplePhase.ConnectedSwinging or GrapplePhase.ConnectedLocked,
        "vertical extension BTS dispatches its referenced grapple block");
    AssertEqual(56, verticalExtensionSamus.Grapple.Anchor.Y,
        "vertical extension keeps physical endpoint block Y");

    // Ordinary solid-family blocks return carry with overflow clear. That is not a rope
    // connection: firing moves to the one-call cancellation function, matching $94:A8E5.
    var solidBlocks = new RoomTerrainBlock[8 * 8];
    solidBlocks[3 * 8 + 3] = RoomTerrainBlock.Of(0, BlockMirroring.None, RoomCollisionType.SolidBlock);
    RoomLevelData solidLevel = CreateRoom(new(8, 8), solidBlocks, new RoomBlockBehavior[solidBlocks.Length]);
    SamusState solidCollisionSamus = CreateSamus(SamusPoseId.FallingRightPose, new(32, 56));
    SamusGrappleMovement.BeginFiring(solidCollisionSamus);
    SamusGrappleMovement.StepFiring(
solidLevel, solidCollisionSamus, ControllerButton.X);
    GrappleMovementResult solidCancellation = SamusGrappleMovement.StepFiring(
solidLevel, solidCollisionSamus, ControllerButton.X);
    AssertTrue(solidCancellation.Phase == GrapplePhase.CancelPending,
        "solid block queues grapple firing cancellation");

    // Type `$A` does not use the generic solid result during grapple firing. `$94:A7FD`
    // spawns one of sixteen bank-$84 entries: ordinary nonnegative BTS values run the
    // carry-set/overflow-clear setup and cancel, while BTS three is Draygon's broken turret
    // and queues one whole periodic-damage unit before returning carry+overflow to connect.
    foreach ((byte behavior, bool expectedConnection, bool expectedCancellation, ushort expectedDamage) in new[]
    {
        new VerifySamusGrappleSwingAndReleaseCase(0x00, false, true, 0),
        new VerifySamusGrappleSwingAndReleaseCase(0x03, true, false, 1),
        new VerifySamusGrappleSwingAndReleaseCase(0x83, false, false, 0),
    })
    {
        var spikeBlocks = new RoomTerrainBlock[8 * 8];
        var spikeBts = new RoomBlockBehavior[spikeBlocks.Length];
        spikeBlocks[3 * 8 + 3] = RoomTerrainBlock.Of(0, BlockMirroring.None, RoomCollisionType.SpikeBlock);
        spikeBts[3 * 8 + 3] = new RoomBlockBehavior(behavior);
        RoomLevelData spikeLevel = CreateRoom(new(8, 8), spikeBlocks, spikeBts);
        SamusState spikeSamus = CreateSamus(SamusPoseId.FallingRightPose, new(32, 56));
        spikeSamus.Kinematics.YSpeed = spikeSamus.Kinematics.YSpeed.WithWhole(1);
        SamusGrappleMovement.BeginFiring(spikeSamus);
        SamusGrappleMovement.StepFiring(
spikeLevel, spikeSamus, ControllerButton.X);
        GrappleMovementResult spikeReaction = SamusGrappleMovement.StepFiring(
spikeLevel, spikeSamus, ControllerButton.X);

        AssertEqual(expectedConnection, spikeReaction.Phase is GrapplePhase.ConnectedSwinging or GrapplePhase.ConnectedLocked,
            $"firing spike BTS ${behavior:X2} connection flag");
        AssertEqual(expectedCancellation, spikeReaction.Phase == GrapplePhase.CancelPending,
            $"firing spike BTS ${behavior:X2} cancellation flag");
        AssertEqual(expectedDamage, spikeSamus.LiquidPhysics.PeriodicDamage,
            $"firing spike BTS ${behavior:X2} periodic damage");
    }

    // The remaining four bank-$94 endpoint entries share their projectile/bomb PLM setup
    // with ordinary weapons, but grapple itself has projectile family zero.  Types four
    // and seven therefore remain air; their solid twins C and F cancel the live beam.
    // BTS C selects one of the retail no-op projectile-shot PLMs for types four/C, proving
    // that the shared reaction side effect is retained independently of collision carry.
    foreach ((RoomCollisionType collisionKind, bool expectedCancellation, int expectedPlms) in new[]
    {
        // `$94:A85B` runs all four fractional probes unless a carry+overflow connection
        // is accepted. Both air type four and solid type C therefore allocate four
        // one-handler native no-op slots; only the fourth reaction decides cancellation.
        new SamusGrappleVerifySamusGrappleSwingAndReleaseCase2(RoomCollisionType.ShootableAir, false, 4),
        new SamusGrappleVerifySamusGrappleSwingAndReleaseCase2(RoomCollisionType.BombableAir, false, 0),
        new SamusGrappleVerifySamusGrappleSwingAndReleaseCase2(RoomCollisionType.ShootableBlock, true, 4),
        new SamusGrappleVerifySamusGrappleSwingAndReleaseCase2(RoomCollisionType.BombableBlock, true, 0),
    })
    {
        var endpointBlocks = new RoomTerrainBlock[8 * 8];
        var endpointBts = new RoomBlockBehavior[endpointBlocks.Length];
        endpointBlocks[3 * 8 + 3] = RoomTerrainBlock.Of(0, BlockMirroring.None, collisionKind);
        endpointBts[3 * 8 + 3] = new RoomBlockBehavior(0x0c);
        RoomLevelData endpointLevel = CreateRoom(new(8, 8), endpointBlocks, endpointBts);
        var endpointPlms = PlmFixtures.Unbound();
        SamusState endpointSamus = CreateSamus(SamusPoseId.FallingRightPose, new(32, 56));
        SamusGrappleMovement.BeginFiring(endpointSamus);
        SamusGrappleMovement.StepFiring(
endpointLevel, endpointSamus, ControllerButton.X, endpointPlms);
        GrappleMovementResult endpointReaction = SamusGrappleMovement.StepFiring(
endpointLevel, endpointSamus, ControllerButton.X, endpointPlms);

        AssertEqual(expectedCancellation, endpointReaction.Phase == GrapplePhase.CancelPending,
            $"grapple endpoint {collisionKind} cancellation topology");
        AssertEqual(!expectedCancellation, endpointReaction.Fired,
            $"grapple endpoint {collisionKind} live-beam topology");
        AssertEqual(expectedPlms, endpointPlms.SlotTable.ActiveCount,
            $"grapple endpoint {collisionKind} shared PLM side effect");
    }

    // Length grows before collision checks. Values 12..120 receive their four probes, but
    // the next addition produces 132 and queues cancellation without moving the endpoint.
    var emptyWideBlocks = new RoomTerrainBlock[16 * 8];
    RoomLevelData emptyWideLevel = CreateRoom(new(16, 8), emptyWideBlocks, new RoomBlockBehavior[emptyWideBlocks.Length]);
    SamusState rangeLimitedSamus = CreateSamus(SamusPoseId.FallingRightPose, new(32, 56));
    SamusGrappleMovement.BeginFiring(rangeLimitedSamus);
    for (int firingFrame = 0; firingFrame < 10; firingFrame++)
    {
        GrappleMovementResult liveRange = SamusGrappleMovement.StepFiring(
emptyWideLevel, rangeLimitedSamus, ControllerButton.X);
        AssertTrue(liveRange.Fired, $"grapple range frame {firingFrame} remains live");
    }
    AssertEqual(120, rangeLimitedSamus.Grapple.RopeLength,
        "last collision-tested grapple firing length");
    ushort endpointBeforeRangeCancellation = rangeLimitedSamus.Grapple.Anchor.X;
    GrappleMovementResult rangeCancellation = SamusGrappleMovement.StepFiring(
emptyWideLevel, rangeLimitedSamus, ControllerButton.X);
    AssertTrue(rangeCancellation.Phase == GrapplePhase.CancelPending,
        "grapple queues cancellation when pre-collision length reaches 128");
    AssertEqual(endpointBeforeRangeCancellation, rangeLimitedSamus.Grapple.Anchor.X,
        "range cancellation performs no endpoint substep");

    // Release-of-Shoot is checked before extension. Cancellation remains queued for one
    // function call, mirroring the bank-$9B pointer change rather than disappearing early.
    var cancelledSamus = new SamusState
    {
        Pose = SamusPoseId.FallingRightPose,
        Position = FixedPosition<RoomPixels>.FromWhole(new(32, 48)),
    };
    BindGrapplePresentation(cancelledSamus);
    SamusGrappleMovement.BeginFiring(cancelledSamus);
    GrappleMovementResult cancelQueued = SamusGrappleMovement.StepFiring(
firingLevel, cancelledSamus, controllerInput: 0);
    AssertTrue(cancelQueued.Phase == GrapplePhase.CancelPending,
        "released firing queues cancellation");
    GrappleMovementResult cancelled =
        SamusGrappleMovement.CompleteFiringCancellation(firingLevel, cancelledSamus);
    AssertTrue(cancelled.Phase == GrapplePhase.Inactive && cancelledSamus.Grapple.Phase == GrapplePhase.Inactive,
        "queued firing cancellation clears on following call");

    // Exercise all three native connection tables through the public BeginFiring/StepFiring
    // route. Filling this isolated room with persistent type-$E blocks makes every
    // endpoint connect on its first substep, while each authored direction still
    // selects its own four-byte {function, handler} record and pose.
    var connectionBlocks = Enumerable.Repeat(RoomTerrainBlock.Of(0x000, BlockMirroring.None, RoomCollisionType.GrappleBlock), 8 * 8).ToArray();
    RoomLevelData connectionLevel = CreateRoom(new(8, 8), connectionBlocks, new RoomBlockBehavior[connectionBlocks.Length]);
    byte[][] expectedConnectionPoses =
    [
        // Default stationary table `$C3C6`.
        [0xb2, 0xb2, 0xa8, 0xaa, 0xab, 0xab, 0xab, 0xa9, 0xb3, 0xb3],
        // Crouching stationary table `$C416`; directions four/five literally use `$AB`.
        [0xb2, 0xb2, 0xb4, 0xb6, 0xab, 0xab, 0xb7, 0xb5, 0xb3, 0xb3],
        // Any nonzero vertical speed half overrides posture and selects `$C3EE`.
        [0xb2, 0xb2, 0xb2, 0xb2, 0xb2, 0xb3, 0xb3, 0xb3, 0xb3, 0xb3],
    ];

    // Native physical origins remain fixed. Give Flare distinct presentation values so
    // command 10 cannot pass by confusing the visual flare with physical rope Start.
    short[] nativeOriginX = [2, 10, 2, 10, 3, -4, -10, -2, -10, -2];
    short[] nativeOriginY = [-16, -12, 2, 0, 6, 6, 0, 2, -12, -16];
    for (int direction = 0; direction < 10; direction++)
    {
        WriteGrappleFlareOrigin(bus, direction,
            new Offset16<RoomPixels>(checked((short)(direction + 20)), checked((short)(direction + 30))));
    }

    for (int family = 0; family < expectedConnectionPoses.Length; family++)
    {
        for (byte direction = 0; direction < 10; direction++)
        {
            SamusMovementType sourceMovementType = family == 1
                ? SamusMovementType.Crouching
                : SamusMovementType.Falling;
            // Exercise the whole connection-table cross-product through real authored
            // aim/movement combinations. Immutable mechanics metadata is not test input.
            int sourcePoseIndex = Enumerable.Range(0, 253).FirstOrDefault(pose =>
                SamusPoseDispatch.ReadMovement((SamusPoseId)pose) == sourceMovementType &&
                SamusPoseAimRestrictions.Read((SamusPoseId)pose) == direction,
                -1);
            if (sourcePoseIndex < 0)
                continue;
            SamusPoseId sourcePose = (SamusPoseId)sourcePoseIndex;

            var connectionSamus = new SamusState
            {
                Pose = sourcePose,
                Position = FixedPosition<RoomPixels>.FromWhole(new(40, 40)),
            };

            BindGrapplePresentation(connectionSamus);
            if (family == 2)
            {
                // A nonzero fractional half alone must select the vertical table and must
                // be cleared by the shared special-pose-command tail after connection.
                connectionSamus.Kinematics.YSpeed = connectionSamus.Kinematics.YSpeed.WithFraction(1);
            }
            connectionSamus.HorizontalSpeed.AccelerationMode = SamusAccelerationMode.Turning;
            connectionSamus.HorizontalSpeed.BaseSpeed = new UnsignedSixteenSixteen(1, 2);
            connectionSamus.HorizontalSpeed.ExtraRunSpeed = new UnsignedSixteenSixteen(3, 4);

            SamusGrappleMovement.BeginFiring(connectionSamus);
            GrappleMovementResult tableConnection = SamusGrappleMovement.StepFiring(

                connectionLevel,
                connectionSamus,
                ControllerButton.X);

            SamusPoseId expectedPose = (SamusPoseId)expectedConnectionPoses[family][direction];
            bool expectedLocked = expectedPose is not (SamusPoseId.GrappleSwingRightPose or SamusPoseId.GrappleSwingLeftPose);
            AssertTrue(tableConnection.Phase is GrapplePhase.ConnectedSwinging or GrapplePhase.ConnectedLocked,
                $"connection family {family} direction {direction} connects through runtime path");
            AssertEqual(expectedPose, connectionSamus.Pose,
                $"connection family {family} direction {direction} pose");
            AssertEqual(
                expectedLocked ? GrapplePhase.ConnectedLocked : GrapplePhase.ConnectedSwinging,
                connectionSamus.Grapple.Phase,
                $"connection family {family} direction {direction} function phase");
            AssertEqual(expectedLocked, tableConnection.Phase == GrapplePhase.ConnectedLocked,
                $"connection family {family} direction {direction} locked result");
            AssertTrue(tableConnection.CameraPrevious.HasValue,
                $"connection family {family} direction {direction} publishes camera clamp");

            AssertEqual(0, connectionSamus.HorizontalSpeed.BaseSpeed.Whole,
                "connection clears whole X base speed");
            AssertEqual(0, connectionSamus.HorizontalSpeed.BaseSpeed.Fraction,
                "connection clears fractional X base speed");
            AssertEqual(0, connectionSamus.HorizontalSpeed.ExtraRunSpeed.Whole,
                "connection clears whole extra run speed");
            AssertEqual(0, connectionSamus.HorizontalSpeed.ExtraRunSpeed.Fraction,
                "connection clears fractional extra run speed");
            AssertEqual(SamusAccelerationMode.Turning, connectionSamus.HorizontalSpeed.AccelerationMode,
                "connection leaves acceleration mode untouched");
            AssertEqual(0, connectionSamus.Kinematics.YSpeed.Whole,
                "connection clears whole Y speed");
            AssertEqual(0, connectionSamus.Kinematics.YSpeed.Fraction,
                "connection clears fractional Y speed");

            if (expectedLocked)
            {
                short rawOriginX = nativeOriginX[direction];
                short rawOriginY = nativeOriginY[direction];
                short rawFlareX = unchecked((short)(direction + 20));
                short rawFlareY = unchecked((short)(direction + 30));
                AssertEqual(
                    unchecked((ushort)(connectionSamus.Grapple.RopeStart.X - rawOriginX)),
                    connectionSamus.Position.X.Whole,
                    "locked command positions Samus X from physical rope Start");
                AssertEqual(
                    unchecked((ushort)(connectionSamus.Grapple.RopeStart.Y - rawOriginY)),
                    connectionSamus.Position.Y.Whole,
                    "locked command positions Samus Y from physical rope Start");
                AssertEqual(
                    unchecked((ushort)(connectionSamus.Position.X.Whole + rawFlareX)),
                    connectionSamus.Grapple.BeamStart.X,
                    "locked command independently publishes flare/draw X");
                AssertEqual(
                    unchecked((ushort)(connectionSamus.Position.Y.Whole + rawFlareY)),
                    connectionSamus.Grapple.BeamStart.Y,
                    "locked command independently publishes flare/draw Y");
            }
            else
            {
                AssertEqual(connectionSamus.Grapple.RopeStart.X, connectionSamus.Grapple.BeamStart.X,
                    "swing command aliases physical Start and flare X");
                AssertEqual(connectionSamus.Grapple.RopeStart.Y, connectionSamus.Grapple.BeamStart.Y,
                    "swing command aliases physical Start and flare Y");
            }
        }
    }

    // Preserve the original fixture record for the remaining grapple tests in this method.
    WritePoseDefinition(bus, (int)SamusPoseId.FallingRightPose,
        [0x08, 0x06, 0xff, 0x02, 0x00, 0x00, 0x05, 0x15]);

    // Native $80 points straight down (X=0,Y=256); $81 is (-6,+255).
    // At length 50 those offsets become (0,50) and (-1,49). Negative X also
    // changes the block-anchor low nibble from eight to seven.

    // Override displayed frames independently of the native physical body offsets.
    // Both angles retain right-facing body offset (0,+46), despite art frames three/four.
    WriteGrappleSwingFrame(bus, angle: 0x80, frame: 3);
    WriteGrappleSwingFrame(bus, angle: 0x81, frame: 4);

    // The already-connected pendulum fixture uses an intentionally empty 32x16 room. Its
    // six-point sweep must stay in bounds while proving that no invented terrain response
    // disturbs the pre-existing unobstructed numbers.
    var swingBlocks = new RoomTerrainBlock[32 * 16];
    RoomLevelData swingLevel = CreateRoom(new(32, 16), swingBlocks, new RoomBlockBehavior[swingBlocks.Length]);

    var samus = new SamusState { Position = FixedPosition<RoomPixels>.FromWhole(new(10, 20)) };

    BindGrapplePresentation(samus);
    SamusGrappleMovement.ConnectUnobstructedSwing(
        bus,
        samus,
        anchor: new Position16<RoomPixels>(200, 100),
        ropeLength: 50,
        angle: RotationAngle.HalfTurn,
        angularVelocity: 0,
        faceRight: true);

    AssertEqual(SamusPoseId.GrappleSwingRightPose, samus.Pose, "grapple connection pose");
    AssertEqual(
        SamusMovementType.Grappling,
        SamusPoseMetadata.ReadMovementType(samus),
        "grapple movement type from pose record");
    AssertEqual(200, samus.Grapple.BeamStart.X, "initial grapple beam-start X");
    AssertEqual(154, samus.Grapple.BeamStart.Y, "initial grapple beam-start Y");
    AssertEqual(200, samus.Position.X.Whole, "initial grapple native body X");
    AssertEqual(200, samus.Position.Y.Whole, "initial grapple native body Y");
    AssertEqual(3, samus.AnimationFrame, "initial grapple angle art frame");

    // Left held at exact $8000 first applies the native +$0100 kick, then +12 input.
    // Gravity is exactly zero on that axis, so angle advances by $010C to $810C.
    GrappleMovementResult swung = SamusGrappleMovement.Step(new SamusTerrain { Level = swingLevel }, samus, (ControllerButton.X | ControllerButton.Left), newlyPressedInput: 0);
    AssertEqual(GrapplePhase.ConnectedSwinging, swung.Phase, "held-shot grapple phase");
    AssertEqual(0x010c, samus.Grapple.AngularVelocity, "bottom kick plus input acceleration");
    AssertEqual(0x810c, samus.Grapple.Angle.RawValue, "unobstructed angle integration");
    AssertEqual(198, samus.Grapple.BeamStart.X, "advanced grapple beam-start X");
    AssertEqual(153, samus.Grapple.BeamStart.Y, "advanced grapple beam-start Y");
    AssertEqual(198, samus.Position.X.Whole, "advanced grapple native body X");
    AssertEqual(199, samus.Position.Y.Whole, "advanced grapple native body Y");
    AssertEqual(4, samus.AnimationFrame, "advanced grapple angle art frame");

    // `$90:EB86` remains installed for every noninactive host phase, but its signed native
    // function-pointer test selects beam-specific graphics only through `$9B:C832`. The
    // later one-frame functions must therefore suppress both ordinary charge flare and rope
    // while still falling through to the normal atmosphere/body/cannon/echo subroutine.
    AssertTrue(SamusGrappleBeamDrawing.UsesGrappleDrawingHandler(GrapplePhase.Firing),
        "grapple firing owns replacement draw handler");
    AssertTrue(SamusGrappleBeamDrawing.UsesBeamSpecificDrawingPath(GrapplePhase.WallGrabRelease),
        "grapple wall-release remains inside beam-specific pointer range");
    AssertTrue(SamusGrappleBeamDrawing.UsesGrappleDrawingHandler(GrapplePhase.CancelPending),
        "grapple cancel keeps replacement draw handler for pending frame");
    AssertTrue(!SamusGrappleBeamDrawing.UsesBeamSpecificDrawingPath(GrapplePhase.CancelPending),
        "grapple cancel takes ordinary body-and-echo fallback");
    AssertTrue(!SamusGrappleBeamDrawing.UsesGrappleDrawingHandler(GrapplePhase.Inactive),
        "inactive grapple restores default draw handler");

    // Give bank-$93 table index 16 one deliberately recognizable one-piece spritemap.
    // The first flare call must force frame 16/timer three, decrement the timer to two,
    // and draw at the connected Flare origin before any rope or Samus objects are appended.
    foreach (ushort pointer in ChargeFlareSpriteDefinitions.NativePointers)
    {
        SyntheticSamusCartridge.WriteSingleObjectProjectileSpritemap(
            bus, pointer, x: 2, y: 0xfd, tileAttributes: 0x3456);
    }
    var grappleFlareOamFrame = new SpriteFrameBuffer();
    grappleFlareOamFrame.BeginFrame();
    AssertTrue(SamusGrappleBeamDrawing.DrawFlareBeforeSamus(
            samus, grappleFlareOamFrame, layer1: new(100, 50), InstalledFixtures.ProjectileArtwork()),
        "connected grapple flare passes unsigned screen-Y gate");
    OamBuffer grappleFlareOam = grappleFlareOamFrame.FinalizeAsOam();
    AssertEqual(1, samus.Grapple.FlareCounter,
        "grapple flare counter increments only in post-Samus tile pass");
    AssertEqual(16, samus.Grapple.FlareAnimationFrame,
        "first grapple flare selects shared main frame sixteen");
    AssertEqual(2, samus.Grapple.FlareAnimationTimer.RawValue,
        "first grapple flare performs decrement-before-test");
    var grappleFlare = (ImageSpriteDraw)grappleFlareOamFrame.Draws.Single();
    AssertEqual(InstalledFixtures.SpriteSheets.Get(SpriteSheetId.ProjectilesPower).Frame(ProjectileSheetFrames.FlareFrame(
            ChargeFlareSpriteSelection.Selectors[16 + ChargeFlareSpriteDefinitions.MainFlareSelectorOffset])), grappleFlare.Image,
        "grapple flare shows main flare frame sixteen from the loaded beams' projectile sheet");
    AssertEqual(new Position16<ScreenPixels>(98, 103), grappleFlare.Origin, "grapple flare is placed at the flare origin");

    // Selector provenance is now compiled even without installed artwork. The
    // independent all-angle cartridge comparison covers its exact sector boundaries.
    int segmentSource = GrappleTileSelection.TransferFor(
        GrappleTileSelection.SegmentAssetFor(samus.Grapple.Angle)).SourceAddress;

    // $94:AFBA recalculates the angle from endpoint minus flare (1,-49).
    // Coarse octant division selects angle zero, giving a (0,-8) segment step.

    var grappleOamFrame = new SpriteFrameBuffer();
    grappleOamFrame.BeginFrame();
    SamusGrappleBeamDrawing.DrawConnectedBeam(
        samus.Grapple,
        grappleOamFrame,
        layer1: new(100, 50),
        InstalledFixtures.GrappleSheet);
    OamBuffer grappleOam = grappleOamFrame.FinalizeAsOam();

    AssertEqual(0x9a8200, GrapplePointAnimationDefinitions.SourceAddress(samus.Grapple.PointAnimationFrame),
        "grapple selects the native first endpoint tile");
    AssertEqual(segmentSource, GrappleTileSelection.TransferFor(GrappleTileSelection.SegmentAssetFor(samus.Grapple.Angle)).SourceAddress,
        "grapple selects the native angle-selected segment tiles");
    AssertTrue(grappleOamFrame.Draws.All(draw => ReferenceEquals(InstalledFixtures.GrappleSheet.Image, ((ImageSpriteDraw)draw).Image.Sheet)),
        "grapple draws show frames of the grapple sheet");
    AssertEqual(2, samus.Grapple.FlareCounter,
        "post-Samus grapple tile pass increments flare counter");

    // Fifty pixels yields six body pieces because $94:AFBA uses (length / 8) before drawing
    // the endpoint. Instruction slots descend 15..10, so GrappleFunc_AF87's phases are
    // $24,$23,$22,$21,$24,$23 rather than one shared guessed animation tile.
    AssertEqual(28, (grappleOam.LastFinalizedSpriteCount * 4), "six grapple segments plus endpoint OAM bytes");
    int[] expectedTiles = [0x24, 0x23, 0x22, 0x21, 0x24, 0x23];
    for (int segment = 0; segment < expectedTiles.Length; segment++)
    {
        OamEntry entry = grappleOam.GetEntry(segment);
        AssertOamEntry(
            new OamEntry(
                new Position16<SpritePixels>(94, (ushort)(99 - segment * 8)),
                expectedTiles[segment],
                5,
                3,
                FlipX: true,
                FlipY: false,
                IsLarge: false),
            entry,
            $"grapple segment {segment} complete OBJ");
    }
    OamEntry grappleEndpoint = grappleOam.GetEntry(6);
    AssertEqual(95, grappleEndpoint.Position.X, "grapple endpoint screen X");
    AssertEqual(50, grappleEndpoint.Position.Y, "grapple endpoint screen Y");
    AssertEqual(0x20, grappleEndpoint.TileNumber, "grapple endpoint tile");

    // `$9B:BFA5` still uploads both graphics blocks and increments flare time when length
    // is zero; `$90:EB86` tests length only before calling the bank-$94 OAM rope renderer.
    var zeroLengthGrapple = new SamusGrappleState
    {
        Phase = GrapplePhase.Firing,
        RopeLength = 0,
        Angle = samus.Grapple.Angle,
        FlareCounter = 1,
    };
    var zeroLengthOamFrame = new SpriteFrameBuffer();
    zeroLengthOamFrame.BeginFrame();
    SamusGrappleBeamDrawing.DrawConnectedBeam(
        zeroLengthGrapple,
        zeroLengthOamFrame,
        layer1: new(100, 50),
        InstalledFixtures.GrappleSheet);
    OamBuffer zeroLengthOam = zeroLengthOamFrame.FinalizeAsOam();
    AssertEqual(2, zeroLengthGrapple.FlareCounter,
        "zero-length grapple still advances flare counter");
    AssertEqual(0, (zeroLengthOam.LastFinalizedSpriteCount * 4),
        "zero-length grapple emits no rope or endpoint OAM");

    // Releasing Shoot runs $9B:CA65 now but queues $9B:CB8B for the next call. With signed
    // cosine -6 and doubled angular velocity 536, vertical magnitude is $00000C90.
    // The phase-shifted horizontal sample is +254, giving $000213D0.
    GrappleMovementResult queued = SamusGrappleMovement.Step(new SamusTerrain { Level = swingLevel }, samus, 0, 0);
    AssertTrue(queued.Phase == GrapplePhase.ReleaseFromSwing, "grapple release is one-frame queued");
    AssertEqual(GrapplePhase.ReleaseFromSwing, samus.Grapple.Phase, "release function pointer phase");
    AssertEqual(0, samus.Kinematics.YSpeed.Whole, "grapple release whole Y speed");
    AssertEqual(0x0c90, samus.Kinematics.YSpeed.Fraction, "grapple release fractional Y speed");
    AssertEqual(SamusVerticalDirection.Up, samus.Kinematics.YDirection, "positive swing with negative cosine launches up");
    AssertEqual(2, samus.HorizontalSpeed.BaseSpeed.Whole, "grapple release whole X speed");
    AssertEqual(0x13d0, samus.HorizontalSpeed.BaseSpeed.Fraction, "grapple release fractional X speed");
    AssertEqual(SamusAccelerationMode.Decelerating, samus.HorizontalSpeed.AccelerationMode, "release selects deceleration mode");

    GrappleMovementResult released = SamusGrappleMovement.Step(new SamusTerrain { Level = swingLevel }, samus, 0, 0);
    AssertTrue(released.Phase == GrapplePhase.Inactive, "queued grapple release completes");
    AssertEqual(GrapplePhase.Inactive, samus.Grapple.Phase, "completed release clears grapple phase");
    AssertEqual(SamusPoseId.NormalJumpForwardLeftPose, samus.Pose,
        "nonnegative angular velocity selects left-facing release pose $52");
    AssertEqual(0, samus.Kinematics.YSpeed.Whole, "release pose preserves whole Y velocity");
    AssertEqual(0x0c90, samus.Kinematics.YSpeed.Fraction, "release pose preserves fractional Y velocity");
    AssertEqual(0, samus.Grapple.FlareCounter,
        "completed grapple release clears shared flare counter");
    AssertEqual(0, samus.Grapple.FlareAnimationFrame,
        "completed grapple release clears flare frame");
    AssertEqual(0, samus.Grapple.FlareAnimationTimer.RawValue,
        "completed grapple release clears flare timer");

    // Angle $40 is exactly rightward; the next sample $41 is (+255,+6).
    // Keep those native values and place terrain at the actual nearest probe.
    WriteGrappleSwingFrame(bus, angle: 0x40, frame: 0);

    // Anchor (128,128) is biased to (136,136). With length 32, candidate angle $41's
    // nearest probe is 40 pixels from the anchor at (175,136), block (10,8). Gravity adds
    // $18 to initial velocity $100, so collision must preserve last-safe angle $40.80 and
    // transform velocity $118 into -($118 >> 1) = -$8C while opening a 16-frame kick gate.
    var angularCollisionBlocks = new RoomTerrainBlock[16 * 16];
    angularCollisionBlocks[8 * 16 + 10] = RoomTerrainBlock.Of(0, BlockMirroring.None, RoomCollisionType.SolidBlock);
    RoomLevelData angularCollisionLevel = CreateRoom(new(16, 16), angularCollisionBlocks, new RoomBlockBehavior[angularCollisionBlocks.Length]);
    var angularCollisionSamus = new SamusState();
    BindGrapplePresentation(angularCollisionSamus);
    SamusGrappleMovement.ConnectUnobstructedSwing(
        bus,
        angularCollisionSamus,
        anchor: new Position16<RoomPixels>(128, 128),
        ropeLength: 32,
        angle: RotationAngle.QuarterTurn,
        angularVelocity: 0x0100,
        faceRight: true);
    GrappleMovementResult angularCollision = SamusGrappleMovement.Step(new SamusTerrain { Level = angularCollisionLevel }, angularCollisionSamus, ControllerButton.X, newlyPressedInput: 0);
    AssertTrue(angularCollision.TerrainCollided, "grapple angular sweep reports terrain collision");
    AssertEqual(6, angularCollision.CollisionDistanceFromFeet,
        "nearest of six grapple body probes collides first");
    AssertEqual(0x4080, angularCollisionSamus.Grapple.Angle.RawValue,
        "grapple collision restores last-safe angle plus half fraction");
    AssertEqual(-0x008c, angularCollisionSamus.Grapple.AngularVelocity,
        "grapple collision negates arithmetic half velocity");
    AssertEqual(16, angularCollisionSamus.Grapple.CollisionBounceTimer.RawValue,
        "grapple collision opens sixteen-frame kick window");

    // The same six-point radial sweep has two damage-producing dispatcher entries. Solid
    // spike BTS zero/one queue `$003C/$0010` and still collide. Every other table entry is
    // literal zero, while a negative BTS skips the table. Use a fresh Samus for each row so
    // `$18A8` does not mask the next fixture's first contact.
    foreach ((byte behavior, ushort expectedDamage) in new[]
    {
        new SamusGrappleVerifySamusGrappleSwingAndReleaseCase(0x00, 0x003c),
        new SamusGrappleVerifySamusGrappleSwingAndReleaseCase(0x01, 0x0010),
        new SamusGrappleVerifySamusGrappleSwingAndReleaseCase(0x02, 0x0000),
        new SamusGrappleVerifySamusGrappleSwingAndReleaseCase(0x80, 0x0000),
    })
    {
        var spikeBlockWords = new RoomTerrainBlock[16 * 16];
        var spikeBlockBts = new RoomBlockBehavior[spikeBlockWords.Length];
        spikeBlockWords[8 * 16 + 10] = RoomTerrainBlock.Of(0, BlockMirroring.None, RoomCollisionType.SpikeBlock);
        spikeBlockBts[8 * 16 + 10] = new RoomBlockBehavior(behavior);
        RoomLevelData spikeBlockLevel = CreateRoom(new(16, 16), spikeBlockWords, spikeBlockBts);
        var spikeBlockSamus = new SamusState();
        BindGrapplePresentation(spikeBlockSamus);
        SamusGrappleMovement.ConnectUnobstructedSwing(
            bus,
            spikeBlockSamus,
            anchor: new Position16<RoomPixels>(128, 128),
            ropeLength: 32,
            angle: RotationAngle.QuarterTurn,
            angularVelocity: 0x0100,
            faceRight: true);
        GrappleMovementResult spikeBlockContact = SamusGrappleMovement.Step(new SamusTerrain { Level = spikeBlockLevel }, spikeBlockSamus, ControllerButton.X, newlyPressedInput: 0);

        AssertTrue(spikeBlockContact.TerrainCollided,
            $"solid grapple spike BTS ${behavior:X2} remains collision");
        AssertEqual(expectedDamage, spikeBlockSamus.LiquidPhysics.PeriodicDamage,
            $"solid grapple spike BTS ${behavior:X2} damage table");
        AssertEqual(expectedDamage == 0 ? (ushort)0 : (ushort)0x003c,
            spikeBlockSamus.InvincibilityTimer.RawValue,
            $"solid grapple spike BTS ${behavior:X2} invincibility publication");
        AssertEqual(expectedDamage == 0 ? (ushort)0 : (ushort)0x000a,
            spikeBlockSamus.KnockbackTimer.RawValue,
            $"solid grapple spike BTS ${behavior:X2} knockback-timer publication");
    }

    // Spike-air never stops the pendulum. Only BTS two has nonzero damage; because the
    // near-horizontal sweep samples block (11,8) more than once, an exact `$0010` result also
    // proves the first sample's invincibility timer suppresses later samples in this frame.
    foreach ((byte behavior, ushort expectedDamage) in new[]
    {
        new SamusGrappleVerifySamusGrappleSwingAndReleaseCase(0x00, 0x0000),
        new SamusGrappleVerifySamusGrappleSwingAndReleaseCase(0x02, 0x0010),
        new SamusGrappleVerifySamusGrappleSwingAndReleaseCase(0x82, 0x0000),
    })
    {
        var spikeAirWords = new RoomTerrainBlock[16 * 16];
        var spikeAirBts = new RoomBlockBehavior[spikeAirWords.Length];
        spikeAirWords[8 * 16 + 11] = RoomTerrainBlock.Of(0, BlockMirroring.None, RoomCollisionType.SpikeAir);
        spikeAirBts[8 * 16 + 11] = new RoomBlockBehavior(behavior);
        RoomLevelData spikeAirLevel = CreateRoom(new(16, 16), spikeAirWords, spikeAirBts);
        var spikeAirSamus = new SamusState();
        BindGrapplePresentation(spikeAirSamus);
        SamusGrappleMovement.ConnectUnobstructedSwing(
            bus,
            spikeAirSamus,
            anchor: new Position16<RoomPixels>(128, 128),
            ropeLength: 32,
            angle: RotationAngle.QuarterTurn,
            angularVelocity: 0x0100,
            faceRight: true);
        GrappleMovementResult spikeAirContact = SamusGrappleMovement.Step(new SamusTerrain { Level = spikeAirLevel }, spikeAirSamus, ControllerButton.X, newlyPressedInput: 0);

        AssertTrue(!spikeAirContact.TerrainCollided,
            $"grapple spike-air BTS ${behavior:X2} remains noncollision");
        AssertEqual(expectedDamage, spikeAirSamus.LiquidPhysics.PeriodicDamage,
            $"grapple spike-air BTS ${behavior:X2} damage table");
        AssertEqual(expectedDamage == 0 ? (ushort)0 : (ushort)0x003c,
            spikeAirSamus.InvincibilityTimer.RawValue,
            $"grapple spike-air BTS ${behavior:X2} invincibility publication");
        AssertEqual(expectedDamage == 0 ? (ushort)0 : (ushort)0x000a,
            spikeAirSamus.KnockbackTimer.RawValue,
            $"grapple spike-air BTS ${behavior:X2} knockback-timer publication");
        if (expectedDamage != 0)
        {
            spikeAirSamus.DecrementHurtTimers();
            AssertEqual(0x003b, spikeAirSamus.InvincibilityTimer.RawValue,
                "gameplay tail ages grapple-created invincibility timer");
            AssertEqual(0x0009, spikeAirSamus.KnockbackTimer.RawValue,
                "gameplay tail ages grapple-created knockback timer");
        }
    }

    // Move the reflected pendulum into empty terrain and press Jump during the kick window.
    // Gravity/correction update -$8C to -$6F, then $9B:BD44 adds -$300 extra velocity.
    // Total -$36F moves $40.80 to $3D.11; success ages the timer and damps only the extra
    // word from -$300 to -$2FA. This is the real collision-assisted grapple kick, not an
    // ordinary aerial jump applied to Samus's X/Y velocity.
    GrappleMovementResult afterAngularCollision = SamusGrappleMovement.Step(new SamusTerrain { Level = swingLevel }, angularCollisionSamus, (ControllerButton.X | ControllerButton.A), newlyPressedInput: ControllerButton.A);
    AssertTrue(!afterAngularCollision.TerrainCollided,
        "collision kick crosses three clear angle-byte terrain sweeps");
    AssertEqual(0x3d11, angularCollisionSamus.Grapple.Angle.RawValue,
        "grapple collision kick advances exact reflected-plus-extra angle");
    AssertEqual(-0x006f, angularCollisionSamus.Grapple.AngularVelocity,
        "grapple kick keeps gravity-corrected base angular velocity");
    AssertEqual(-0x02fa, angularCollisionSamus.Grapple.JumpImpulse,
        "successful grapple kick damps extra angular velocity by six");
    AssertEqual(15, angularCollisionSamus.Grapple.CollisionBounceTimer.RawValue,
        "successful post-bounce movement ages kick window");

    // Rope growth uses a different radial frontier: candidate length 33 plus 56 pixels puts
    // the probe at X=225, block 14. Make that cell a horizontal extension to solid block 15.
    // Correct per-pixel length handling must reject 33, retain 32, and leave +2 active to
    // retry next frame; treating the extension itself as air would incorrectly grow the rope.
    var ropeCollisionBlocks = new RoomTerrainBlock[16 * 16];
    var ropeCollisionBts = new RoomBlockBehavior[ropeCollisionBlocks.Length];
    ropeCollisionBlocks[8 * 16 + 14] = RoomTerrainBlock.Of(0, BlockMirroring.None, RoomCollisionType.HorizontalExtension);
    ropeCollisionBts[8 * 16 + 14] = new RoomBlockBehavior(1);
    ropeCollisionBlocks[8 * 16 + 15] = RoomTerrainBlock.Of(0, BlockMirroring.None, RoomCollisionType.SolidBlock);
    RoomLevelData ropeCollisionLevel = CreateRoom(new(16, 16), ropeCollisionBlocks, ropeCollisionBts);
    var ropeCollisionSamus = new SamusState();
    BindGrapplePresentation(ropeCollisionSamus);
    SamusGrappleMovement.ConnectUnobstructedSwing(
        bus,
        ropeCollisionSamus,
        anchor: new Position16<RoomPixels>(128, 128),
        ropeLength: 32,
        angle: RotationAngle.QuarterTurn,
        angularVelocity: 0,
        faceRight: true);
    GrappleMovementResult ropeCollision = SamusGrappleMovement.Step(new SamusTerrain { Level = ropeCollisionLevel }, ropeCollisionSamus, (ControllerButton.X | ControllerButton.Down), newlyPressedInput: ControllerButton.Down);
    AssertTrue(ropeCollision.RopeLengthBlocked,
        "grapple rope growth follows extension BTS to solid collision");
    AssertEqual(32, ropeCollisionSamus.Grapple.RopeLength,
        "blocked grapple rope keeps last accepted length");
    AssertEqual(2, ropeCollisionSamus.Grapple.RopeLengthDelta,
        "blocked grapple rope retains signed retry delta");

    // The already-connected public seam normally skips block validation. Opt this fixture
    // into the same flag installed by real firing, then present air at the stored anchor.
    // Gravity creates nonzero momentum, selecting the translated release path rather than
    // the still-untranslated zero-speed dropped handler.
    var disconnectedAnchorSamus = new SamusState();
    BindGrapplePresentation(disconnectedAnchorSamus);
    SamusGrappleMovement.ConnectUnobstructedSwing(
        bus,
        disconnectedAnchorSamus,
        anchor: new Position16<RoomPixels>(128, 128),
        ropeLength: 32,
        angle: RotationAngle.QuarterTurn,
        angularVelocity: 0,
        faceRight: true);
    disconnectedAnchorSamus.Grapple.ValidateAnchorBlock = true;
    GrappleMovementResult disconnectedAnchor = SamusGrappleMovement.Step(new SamusTerrain { Level = CreateEmptyRoom(new(16, 16)) }, disconnectedAnchorSamus, ControllerButton.X, newlyPressedInput: 0);
    AssertTrue(disconnectedAnchor.Phase == GrapplePhase.ReleaseFromSwing &&
        disconnectedAnchorSamus.Grapple.ReleasedMovementActive,
        "air replacing a validated grapple anchor queues moving release");

    // Retail $6A has components (+131,+219), placing the eight-pixel rope at
    // (140,142). Candidate $6B is (+126,+225): its first 16-pixel body probe
    // reaches (143,150), solid block (8,9), rather than an invented horizontal wall.
    // Bank $94 must stop at `$6A80`; bank $9B must then consume record four's literal
    // `$B9`, (+24,+16), `$C814` tuple rather than inventing a host-side angle range.
    WriteGrappleSwingFrame(bus, angle: 0x6a, frame: 0);

    var specialBlocks = new RoomTerrainBlock[16 * 16];
    specialBlocks[8 * 16 + 8] = RoomTerrainBlock.Of(0, BlockMirroring.None, RoomCollisionType.SolidBlock); // `$B9`'s later 16-pixel left wall probe
    specialBlocks[9 * 16 + 8] = RoomTerrainBlock.Of(0, BlockMirroring.None, RoomCollisionType.SolidBlock); // candidate `$6B`, nearest radial probe
    specialBlocks[10 * 16 + 8] = RoomTerrainBlock.Of(0, BlockMirroring.None, RoomCollisionType.SolidBlock);
    RoomLevelData specialLevel = CreateRoom(new(16, 16), specialBlocks, new RoomBlockBehavior[specialBlocks.Length]);
    var wallGrabSamus = new SamusState();
    BindGrapplePresentation(wallGrabSamus);
    SamusGrappleMovement.ConnectUnobstructedSwing(
        bus,
        wallGrabSamus,
        anchor: new Position16<RoomPixels>(128, 128),
        ropeLength: 8,
        angle: RotationAngle.FromRaw(0x6a00),
        angularVelocity: 0x0200,
        faceRight: true);
    GrappleMovementResult wallGrab = SamusGrappleMovement.Step(new SamusTerrain { Level = specialLevel }, wallGrabSamus, ControllerButton.X, newlyPressedInput: 0);
    AssertTrue(wallGrab.TerrainCollided && wallGrab.CameraPrevious.HasValue && wallGrab.Phase == GrapplePhase.WallGrab,
        "close grapple collision enters ROM-selected wall-grab function");
    AssertEqual(6, wallGrab.CollisionDistanceFromFeet,
        "wall-grab special route requires nearest radial probe");
    AssertEqual(GrapplePhase.WallGrab, wallGrabSamus.Grapple.Phase,
        "special record installs `$C814` wall-grab phase");
    AssertEqual(SamusPoseId.GrappleWallContactRightPose, wallGrabSamus.Pose,
        "special record installs literal wall-contact pose `$B9`");
    AssertEqual(160, wallGrabSamus.Position.X.Whole,
        "wall-grab snap applies anchor-relative +24 X");
    AssertEqual(152, wallGrabSamus.Position.Y.Whole,
        "wall-grab snap applies anchor-relative +16 Y");
    AssertEqual(156, wallGrab.CameraPrevious?.X,
        "special snap retains native pre-snap body X within twelve pixels");
    AssertEqual(164, wallGrab.CameraPrevious?.Y,
        "special snap clamps native pre-snap body Y to twelve pixels");

    GrappleMovementResult heldWall = SamusGrappleMovement.Step(new SamusTerrain { Level = specialLevel }, wallGrabSamus, ControllerButton.X, newlyPressedInput: 0);
    AssertTrue(heldWall.Phase == GrapplePhase.WallGrab && wallGrabSamus.Grapple.Phase == GrapplePhase.WallGrab,
        "held Shoot retains frozen wall-grab pose");
    GrappleMovementResult wallReleased = SamusGrappleMovement.Step(new SamusTerrain { Level = specialLevel }, wallGrabSamus, controllerInput: 0, newlyPressedInput: 0);
    AssertTrue(wallReleased.Phase == GrapplePhase.WallGrabRelease,
        "wall-grab release opens native thirty-check wall-jump window");
    AssertEqual(30, wallGrabSamus.Grapple.WallJumpTimer.RawValue,
        "wall-grab release seeds decimal thirty before decrementing");

    // `$B9` faces left (pose direction four), so `$90:9CAC` probes left even though the
    // later launch selects right-facing wall-jump pose `$83`. Jump must be a fresh A edge;
    // the grapple function queues `$C9CE` now and performs the launch on the next call.
    GrappleMovementResult wallJumpQueued = SamusGrappleMovement.Step(new SamusTerrain { Level = specialLevel }, wallGrabSamus, controllerInput: ControllerButton.A, newlyPressedInput: ControllerButton.A);
    AssertTrue(wallJumpQueued.Phase == GrapplePhase.WallJumping,
        "fresh Jump plus wall probe queues grapple wall jump");
    AssertEqual(29, wallGrabSamus.Grapple.WallJumpTimer.RawValue,
        "first eligible wall-jump check decrements timer to twenty-nine");

    // `$9B:C9CE` mirrors the ordinary route's selective cleanup: it zeros base speed but
    // preserves the Dash pair and `$0B3C`. Seed the state after the queueing call so this
    // assertion isolates the launch function itself from earlier grapple-swing behavior.
    wallGrabSamus.HorizontalSpeed.ExtraRunSpeed = new UnsignedSixteenSixteen(1, 0x7000);
    wallGrabSamus.HorizontalSpeed.HasRunningMomentum = true;
    wallGrabSamus.ProjectileFlareCounter = 0x003c;
    wallGrabSamus.LiquidPhysics.BeginFrameSoundRequests();
    GrappleMovementResult wallJumpStarted = SamusGrappleMovement.Step(new SamusTerrain { Level = specialLevel }, wallGrabSamus, controllerInput: 0, newlyPressedInput: 0);
    AssertTrue(wallJumpStarted.WallJumpStarted,
        "queued grapple wall jump starts on following function call");
    AssertEqual(GrapplePhase.Inactive, wallGrabSamus.Grapple.Phase,
        "grapple wall jump clears connected function");
    AssertEqual(SamusPoseId.WallJumpRightPose, wallGrabSamus.Pose,
        "left-facing `$B9` contact reverses to wall-jump pose `$83`");
    AssertEqual(4, wallGrabSamus.Kinematics.YSpeed.Whole,
        "grapple wall jump reads dry whole speed from ROM");
    AssertEqual(0xa000, wallGrabSamus.Kinematics.YSpeed.Fraction,
        "grapple wall jump reads dry fractional speed from ROM");
    AssertEqual(SamusVerticalDirection.Up, wallGrabSamus.Kinematics.YDirection,
        "grapple wall jump launches upward");
    AssertEqual(1, wallGrabSamus.HorizontalSpeed.ExtraRunSpeed.Whole,
        "grapple wall jump preserves Dash whole speed");
    AssertEqual(0x7000, wallGrabSamus.HorizontalSpeed.ExtraRunSpeed.Fraction,
        "grapple wall jump preserves Dash fraction");
    AssertTrue(wallGrabSamus.HorizontalSpeed.HasRunningMomentum,
        "grapple wall jump preserves Dash momentum flag");
    AssertEqual(0, wallGrabSamus.ProjectileFlareCounter,
        "grapple wall jump clears the active projectile flare counter");
    AssertEqual(new SoundRequest(Library1Sound.GrappleEnd, SoundQueueLimit.Max15), wallGrabSamus.LiquidPhysics.SoundRequests.Single(),
        "grapple wall jump queues generic library-one sound seven");
    AssertEqual(0, wallGrabSamus.Grapple.RopeLength,
        "grapple wall jump removes rope state");

    // Repeat the same authentic entry but do not press Jump. DEC/BPL permits exactly thirty
    // wall checks (timer 29 through zero); the thirty-first call wraps to `$FFFF` and queues
    // `$C8C5`. Pose `$B9` has direction six and radius sixteen, so table `$C9C4[6]` must
    // choose compact diagonal-down-left `$74`, not a generic falling approximation.
    var expiredWallGrabSamus = new SamusState();
    BindGrapplePresentation(expiredWallGrabSamus);
    SamusGrappleMovement.ConnectUnobstructedSwing(
        bus,
        expiredWallGrabSamus,
        anchor: new Position16<RoomPixels>(128, 128),
        ropeLength: 8,
        angle: RotationAngle.FromRaw(0x6a00),
        angularVelocity: 0x0200,
        faceRight: true);
    SamusGrappleMovement.Step(new SamusTerrain { Level = specialLevel }, expiredWallGrabSamus, ControllerButton.X, newlyPressedInput: 0);
    SamusGrappleMovement.Step(new SamusTerrain { Level = specialLevel }, expiredWallGrabSamus, controllerInput: 0, newlyPressedInput: 0);
    for (int eligibleCheck = 0; eligibleCheck < 30; eligibleCheck++)
    {
        GrappleMovementResult grace = SamusGrappleMovement.Step(new SamusTerrain { Level = specialLevel }, expiredWallGrabSamus, controllerInput: 0, newlyPressedInput: 0);
        AssertEqual(GrapplePhase.WallGrabRelease, grace.Phase,
            $"wall-grab grace check {eligibleCheck + 1} remains live");
    }
    GrappleMovementResult dropQueued = SamusGrappleMovement.Step(new SamusTerrain { Level = specialLevel }, expiredWallGrabSamus, controllerInput: 0, newlyPressedInput: 0);
    AssertTrue(dropQueued.Phase == GrapplePhase.Dropped,
        "wall-grab grace underflow queues dropped function");
    GrappleMovementResult dropped = SamusGrappleMovement.Step(new SamusTerrain { Level = specialLevel }, expiredWallGrabSamus, controllerInput: 0, newlyPressedInput: 0);
    AssertTrue(dropped.Phase == GrapplePhase.Inactive && expiredWallGrabSamus.Grapple.Phase == GrapplePhase.Inactive,
        "queued dropped handler clears grapple on following call");
    AssertEqual(SamusPoseId.CrouchingAimDiagonalDownLeftPose, expiredWallGrabSamus.Pose,
        "compact dropped table preserves `$B9` diagonal-down aim");
    AssertEqual(0, expiredWallGrabSamus.Kinematics.YSpeed.Whole,
        "dropped handler clears whole vertical speed");
    AssertEqual(0, expiredWallGrabSamus.Kinematics.YSpeed.Fraction,
        "dropped handler clears fractional vertical speed");

    // Record zero is the locked `$D680 -> $B6` route. Candidate `$D7` points up-left
    // into block (7,7); a two-byte angular step guarantees that sample is visited after
    // quadrant gravity. Releasing Shoot queues `$C856`, whose movement-type-$16` fallback
    // byte in the literal `$B6` definition selects stable crouch `$27` one call later.
    WriteGrappleSwingFrame(bus, angle: 0xd6, frame: 0);
    var lockedBlocks = new RoomTerrainBlock[16 * 16];
    lockedBlocks[7 * 16 + 7] = RoomTerrainBlock.Of(0, BlockMirroring.None, RoomCollisionType.SolidBlock);
    RoomLevelData lockedLevel = CreateRoom(new(16, 16), lockedBlocks, new RoomBlockBehavior[lockedBlocks.Length]);
    var lockedSamus = new SamusState();
    BindGrapplePresentation(lockedSamus);
    SamusGrappleMovement.ConnectUnobstructedSwing(
        bus,
        lockedSamus,
        anchor: new Position16<RoomPixels>(128, 128),
        ropeLength: 8,
        angle: RotationAngle.FromRaw(0xd600),
        angularVelocity: 0x0200,
        faceRight: true);
    GrappleMovementResult locked = SamusGrappleMovement.Step(new SamusTerrain { Level = lockedLevel }, lockedSamus, ControllerButton.X, newlyPressedInput: 0);
    AssertTrue(locked.CameraPrevious.HasValue && locked.Phase == GrapplePhase.ConnectedLocked,
        "close collision enters ROM-selected locked function");
    AssertEqual(GrapplePhase.ConnectedLocked, lockedSamus.Grapple.Phase,
        "special record installs `$C77E` locked phase");
    AssertEqual(SamusPoseId.GrappleCrouchingDownRightPose, lockedSamus.Pose,
        "locked special record installs pose `$B6`");
    AssertEqual(105, lockedSamus.Position.X.Whole,
        "locked snap applies signed -30 X offset");
    AssertEqual(111, lockedSamus.Position.Y.Whole,
        "locked snap applies signed -24 Y offset");
    GrappleMovementResult lockedHeld = SamusGrappleMovement.Step(new SamusTerrain { Level = lockedLevel }, lockedSamus, ControllerButton.X, newlyPressedInput: 0);
    AssertTrue(lockedHeld.Phase == GrapplePhase.ConnectedLocked &&
        lockedSamus.Position.X.Whole == 105 && lockedSamus.Position.Y.Whole == 111,
        "held Shoot preserves special locked body without pendulum integration");
    GrappleMovementResult lockedCancelQueued = SamusGrappleMovement.Step(new SamusTerrain { Level = lockedLevel }, lockedSamus, controllerInput: 0, newlyPressedInput: 0);
    AssertTrue(lockedCancelQueued.Phase == GrapplePhase.CancelPending && lockedCancelQueued.OwnsMovement,
        "locked release queues connected-pose cancellation");
    GrappleMovementResult lockedCancelled =
        SamusGrappleMovement.CompleteFiringCancellation(lockedLevel, lockedSamus);
    AssertTrue(lockedCancelled.Phase == GrapplePhase.Inactive && lockedCancelled.OwnsMovement,
        "connected cancellation owns pose-fallback frame");
    AssertEqual(SamusPoseId.CrouchingRightPose, lockedSamus.Pose,
        "locked `$B6` cancellation follows definition fallback `$27`");
    AssertEqual(0, lockedSamus.Grapple.RopeLength,
        "locked cancellation clears rope state");

    AssertThrows<ArgumentOutOfRangeException>(
        () => SamusGrappleMovement.ConnectUnobstructedSwing(
            bus, new SamusState(), Position16<RoomPixels>.Zero, ropeLength: 7, angle: RotationAngle.Zero, angularVelocity: 0, faceRight: true),
        "grapple rejects a rope shorter than retail connected minimum");

    Console.WriteLine("  Samus grapple: firing, swing collision, locked/wall-grab specials, wall jump, dropped pose, beam OAM, and release agree.");
}

/// <summary>
/// Sets direction <paramref name="direction"/>'s entries of the non-running grapple flare
/// origin tables <c>$9B:C14A</c> (X) and <c>$9B:C15E</c> (Y), one signed word per direction.
/// </summary>
static void WriteGrappleFlareOrigin(TestAddressSpace bus, int direction, Offset16<RoomPixels> origin)
{
    Ensure.AtLeastZero(direction);
    ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(direction, 10);
    WriteTestWord(bus, 0x9bc14a + direction * 2, unchecked((ushort)origin.X));
    WriteTestWord(bus, 0x9bc15e + direction * 2, unchecked((ushort)origin.Y));
}

/// <summary>
/// Sets the <c>$9B:C1C2</c> swing-frame table entry selected by swing-angle high byte
/// <paramref name="angle"/> to animation frame <paramref name="frame"/>.
/// </summary>
static void WriteGrappleSwingFrame(TestAddressSpace bus, byte angle, byte frame) =>
    WriteTestByte(bus, 0x9bc1c2 + angle, frame);

    /// <summary>Runs the focused Samus grapple movement and presentation check.</summary>
    [VerificationCommand("--samus-grapple")]
    internal static int SamusGrappleCommand()
    {
        Suite(nameof(VerifySamusGrappleSwingAndRelease), () => VerifySamusGrappleSwingAndRelease());
        return 0;
    }
}
