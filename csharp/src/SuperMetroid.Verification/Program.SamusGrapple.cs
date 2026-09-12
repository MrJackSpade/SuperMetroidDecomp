using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{

/// <summary>Grapple swing, collision, release, and drawing verification.</summary>
static void VerifySamusGrappleSwingAndRelease()
{
    var bus = new TestAddressSpace();

    // Engine trigonometry is compiled, not a synthetic presentation asset. Leave
    // the sine region absent: geometry must use native vectors, not patched waves.

    // Pose definitions are literal eight-byte records. Only X direction, movement type,
    // graphics offset, and radii matter to this isolated route. $B2 is right-facing/type
    // $16; $B3 is its left-facing mirror. Release poses $51/$52 return to type two.
    WritePoseDefinition(bus, SamusPoseIds.GrappleSwingRightPose,
        [0x08, 0x16, 0xff, 0x02, 0x00, 0x00, 0x05, 0x15]);
    WritePoseDefinition(bus, SamusPoseIds.GrappleSwingLeftPose,
        [0x04, 0x16, 0xff, 0x07, 0x00, 0x00, 0x05, 0x15]);
    WritePoseDefinition(bus, SamusPoseIds.NormalJumpForwardRightPose,
        [0x08, 0x02, 0xff, 0x02, 0x00, 0x00, 0x05, 0x15]);
    WritePoseDefinition(bus, SamusPoseIds.NormalJumpForwardLeftPose,
        [0x04, 0x02, 0xff, 0x07, 0x00, 0x00, 0x05, 0x15]);

    // The close-collision routes use a second family of type-$16 records. These bytes are
    // the retail pose definitions, not convenient test metadata: `$B6/$B7` are locked
    // crouching-down poses, `$B8/$B9` are the two wall contacts, and `$83/$84` are the
    // ordinary wall-jump launch poses selected one function call later. The dropped route
    // below uses compact diagonal-down `$74`; locked cancellation uses stable crouch `$27`.
    WritePoseDefinition(bus, SamusPoseIds.GrappleCrouchingDownRightPose,
        [0x08, 0x16, 0x27, 0x03, 0x00, 0x00, 0x10, 0x00]);
    WritePoseDefinition(bus, SamusPoseIds.GrappleCrouchingDownLeftPose,
        [0x04, 0x16, 0x28, 0x06, 0x00, 0x00, 0x10, 0x00]);
    WritePoseDefinition(bus, SamusPoseIds.GrappleWallContactLeftPose,
        [0x08, 0x16, 0xff, 0x03, 0x00, 0x00, 0x10, 0x00]);
    WritePoseDefinition(bus, SamusPoseIds.GrappleWallContactRightPose,
        [0x04, 0x16, 0xff, 0x06, 0x00, 0x00, 0x10, 0x00]);
    WritePoseDefinition(bus, SamusPoseIds.WallJumpRightPose,
        [0x08, 0x14, 0x19, 0xff, 0x08, 0x00, 0x13, 0x00]);
    WritePoseDefinition(bus, SamusPoseIds.WallJumpLeftPose,
        [0x04, 0x14, 0x1a, 0xff, 0x08, 0x00, 0x13, 0x00]);
    WritePoseDefinition(bus, SamusPoseIds.CrouchingRightPose,
        [0x08, 0x05, 0x27, 0x02, 0x00, 0x00, 0x10, 0x00]);
    WritePoseDefinition(bus, SamusPoseIds.CrouchingAimDiagonalDownLeftPose,
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
    foreach (byte pose in new byte[]
    {
        SamusPoseIds.GrappleSwingRightPose,
        SamusPoseIds.GrappleSwingLeftPose,
        SamusPoseIds.NormalJumpForwardRightPose,
        SamusPoseIds.NormalJumpForwardLeftPose,
        SamusPoseIds.GrappleCrouchingDownRightPose,
        SamusPoseIds.GrappleCrouchingDownLeftPose,
        SamusPoseIds.GrappleWallContactLeftPose,
        SamusPoseIds.GrappleWallContactRightPose,
        SamusPoseIds.WallJumpRightPose,
        SamusPoseIds.WallJumpLeftPose,
        SamusPoseIds.CrouchingRightPose,
        SamusPoseIds.CrouchingAimDiagonalDownLeftPose,
        0xa8,
        0xa9,
        0xaa,
        0xab,
        0xb4,
        0xb5,
    })
    {
        WriteTestWord(bus, 0x91b010 + pose * 2, 0xbf00);
    }
    bus.WriteBytes(0x91bf00, [0x05, 0xff]);

    // Dry-air wall-jump table zero is 4.A000. A grapple wall jump reaches the same
    // `$90:9949` initializer as an ordinary spin wall jump after its reversed pose choice.
    WriteTestWord(bus, 0x909ed1, 0x0004);
    WriteTestWord(bus, 0x909ed7, 0xa000);

    // Firing source pose $29 is a retail right-facing fall with shot direction two. The
    // compiled mechanics supply +11.F4 X velocity, zero Y velocity, angle $C000,
    // and (+2,+2) physical origin. Flare presentation is deliberately synthetic.
    WritePoseDefinition(bus, SamusPoseIds.FallingRightPose,
        [0x08, 0x06, 0xff, 0x02, 0x00, 0x00, 0x05, 0x15]);
    WriteTestWord(bus, 0x9bc14a + 2 * 2, 0x0002);
    WriteTestWord(bus, 0x9bc15e + 2 * 2, 0x0002);

    // Compiled native connection pairs include the crouching table's intentional
    // standing `$AB` entries. Routing assertions below still cover all thirty records.

    // A type-$E/BTS-$00 block is persistent grapple PLM $D0D8 and returns flags $41.
    // Put it at (3,3): the first frame's four 16.16 substeps end at X=45, then frame two's
    // first substep reaches X=48 and must center the accepted endpoint at (56,56).
    var firingBlocks = new ushort[8 * 8];
    firingBlocks[3 * 8 + 3] = 0xe000;
    RoomLevelData firingLevel = CreateRoom(
        8, 8, firingBlocks, new byte[firingBlocks.Length]);

    // Samus minus anchor is (-24,-8), which $A0:C0B1 approximates as angle byte $CA.
    // Native components are X=-248 and Y=-62; only the displayed frame is synthetic.
    bus.WriteByte(0x9bc1c2 + 0xca, 5);

    SamusState firingSamus = CreateSamus(SamusPoseIds.FallingRightPose, 32, 48);
    firingSamus.Kinematics.YSpeed = 1; // selects moving-vertically connection table $C3EE
    SamusGrappleMovement.BeginFiring(bus, firingSamus);
    AssertEqual(GrapplePhase.Firing, firingSamus.Grapple.Phase, "grapple firing phase");
    AssertEqual(0x0bf4, firingSamus.Grapple.ExtensionXVelocity,
        "grapple X extension velocity comes from ROM");
    AssertEqual(34, firingSamus.Grapple.AnchorX, "grapple initial endpoint X");
    AssertEqual(50, firingSamus.Grapple.AnchorY, "grapple initial endpoint Y");

    GrappleMovementResult extending = SamusGrappleMovement.StepFiring(
        bus, firingLevel, firingSamus, (ushort)SnesButton.X);
    AssertTrue(extending.Fired && !extending.Connected && !extending.OwnsMovement,
        "unobstructed firing remains live without stealing ordinary body movement");
    AssertEqual(12, firingSamus.Grapple.RopeLength, "firing length grows by twelve");
    AssertEqual(45, firingSamus.Grapple.AnchorX,
        "four fractional collision substeps publish the exact first-frame endpoint");

    GrappleMovementResult connected = SamusGrappleMovement.StepFiring(
        bus, firingLevel, firingSamus, (ushort)SnesButton.X);
    AssertTrue(connected.Connected && connected.OwnsMovement,
        "persistent grapple block establishes connected movement");
    AssertEqual(GrapplePhase.ConnectedSwinging, firingSamus.Grapple.Phase,
        "block acquisition installs swinging function");
    AssertEqual(55, firingSamus.Grapple.AnchorX,
        "accepted grapple block centers X then applies negative-rope side bias");
    AssertEqual(55, firingSamus.Grapple.AnchorY, "accepted grapple block applies negative Y side bias");
    AssertEqual(0xca00, firingSamus.Grapple.Angle.RawValue,
        "connection angle uses bank-$A0 integer octant calculation");
    AssertEqual(SamusPoseIds.GrappleSwingRightPose, firingSamus.Pose,
        "right-half airborne shot selects clockwise grapple pose $B2");
    AssertEqual(32, firingSamus.Grapple.RopeStartX,
        "accepted connection publishes native rope Start X");
    AssertEqual(50, firingSamus.Grapple.RopeStartY,
        "accepted connection publishes native rope Start Y");
    AssertEqual(firingSamus.Grapple.RopeStartX, firingSamus.Grapple.BeamStartX,
        "swing command copies rope Start X into flare/draw X");
    AssertEqual(firingSamus.Grapple.RopeStartY, firingSamus.Grapple.BeamStartY,
        "swing command copies rope Start Y into flare/draw Y");
    AssertEqual(0, firingSamus.Kinematics.YSpeed,
        "connection common tail clears whole Y speed");
    AssertEqual(2, firingSamus.XPosition, "native frame $19 offsets body thirty pixels left of rope start");
    AssertEqual(14, connected.CameraPreviousX!.Value,
        "connection common tail clamps previous X to twelve pixels from native body");
    AssertEqual(48, connected.CameraPreviousY!.Value,
        "connection common tail retains in-range camera previous Y");

    // BTS one must use the same accepted-connection path, but setup CFB5 also creates an
    // independent bank-$84 object and clears BTS before returning flags $41. This direct
    // firing test guards the integration seam; the complete ROM instruction timeline is
    // verified separately below.
    var breakableFiringBlocks = new ushort[8 * 8];
    var breakableFiringBts = new byte[breakableFiringBlocks.Length];
    breakableFiringBlocks[3 * 8 + 3] = 0xe000;
    breakableFiringBts[3 * 8 + 3] = 1;
    RoomLevelData breakableFiringLevel = CreateRoom(
        8, 8, breakableFiringBlocks, breakableFiringBts);
    SamusState breakableFiringSamus = CreateSamus(SamusPoseIds.FallingRightPose, 32, 48);
    breakableFiringSamus.Kinematics.YSpeed = 1;
    var breakableFiringPlms = new RoomPlmSystem();
    SamusGrappleMovement.BeginFiring(bus, breakableFiringSamus);
    SamusGrappleMovement.StepFiring(
        bus,
        breakableFiringLevel,
        breakableFiringSamus,
        (ushort)SnesButton.X,
        breakableFiringPlms);
    GrappleMovementResult breakableConnection = SamusGrappleMovement.StepFiring(
        bus,
        breakableFiringLevel,
        breakableFiringSamus,
        (ushort)SnesButton.X,
        breakableFiringPlms);
    AssertTrue(breakableConnection.Connected,
        "BTS-one grapple firing connects through PLM setup");
    AssertEqual(1, breakableFiringPlms.ActiveCount,
        "BTS-one acquisition installs one independent room PLM");
    AssertEqual(0, breakableFiringLevel.GetCollisionBlock(3, 3).Behavior,
        "BTS-one acquisition synchronously clears low BTS byte");

    // Bank $94 does not treat extension blocks as collision results of their own. Instead,
    // type $5 adds signed BTS directly to the linear block index and dispatches the block
    // found there. Keep the beam physically inside (3,3), but make that cell point right to
    // a persistent type-$E target at (4,3). Connecting proves the indirection is followed;
    // ending in the physical (3,3) cell, rather than (4,3), proves the visible endpoint stays in the
    // extension cell exactly as GrappleCollision_XBlock/YBlock do in the original routine.
    var horizontalExtensionBlocks = new ushort[8 * 8];
    var horizontalExtensionBts = new byte[horizontalExtensionBlocks.Length];
    horizontalExtensionBlocks[3 * 8 + 3] = 0x5000;
    horizontalExtensionBts[3 * 8 + 3] = 1;
    horizontalExtensionBlocks[3 * 8 + 4] = 0xe000;
    RoomLevelData horizontalExtensionLevel = CreateRoom(
        8, 8, horizontalExtensionBlocks, horizontalExtensionBts);
    SamusState horizontalExtensionSamus = CreateSamus(SamusPoseIds.FallingRightPose, 32, 48);
    horizontalExtensionSamus.Kinematics.YSpeed = 1;
    SamusGrappleMovement.BeginFiring(bus, horizontalExtensionSamus);
    SamusGrappleMovement.StepFiring(
        bus, horizontalExtensionLevel, horizontalExtensionSamus, (ushort)SnesButton.X);
    GrappleMovementResult horizontalExtensionConnection = SamusGrappleMovement.StepFiring(
        bus, horizontalExtensionLevel, horizontalExtensionSamus, (ushort)SnesButton.X);
    AssertTrue(horizontalExtensionConnection.Connected,
        "horizontal extension BTS dispatches its referenced grapple block");
    AssertEqual(55, horizontalExtensionSamus.Grapple.AnchorX,
        "horizontal extension keeps physical endpoint block X");

    // Type $D uses the same signed byte but multiplies it by RoomWidthBlocks. A +1 BTS at
    // (3,3) therefore dispatches (3,4), while the accepted endpoint still centers in (3,3).
    // This test is intentionally separate from the horizontal case: confusing the two
    // formulas is easy and can appear correct in rooms whose nearby cells happen to be air.
    var verticalExtensionBlocks = new ushort[8 * 8];
    var verticalExtensionBts = new byte[verticalExtensionBlocks.Length];
    verticalExtensionBlocks[3 * 8 + 3] = 0xd000;
    verticalExtensionBts[3 * 8 + 3] = 1;
    verticalExtensionBlocks[4 * 8 + 3] = 0xe000;
    RoomLevelData verticalExtensionLevel = CreateRoom(
        8, 8, verticalExtensionBlocks, verticalExtensionBts);
    SamusState verticalExtensionSamus = CreateSamus(SamusPoseIds.FallingRightPose, 32, 48);
    verticalExtensionSamus.Kinematics.YSpeed = 1;
    SamusGrappleMovement.BeginFiring(bus, verticalExtensionSamus);
    SamusGrappleMovement.StepFiring(
        bus, verticalExtensionLevel, verticalExtensionSamus, (ushort)SnesButton.X);
    GrappleMovementResult verticalExtensionConnection = SamusGrappleMovement.StepFiring(
        bus, verticalExtensionLevel, verticalExtensionSamus, (ushort)SnesButton.X);
    AssertTrue(verticalExtensionConnection.Connected,
        "vertical extension BTS dispatches its referenced grapple block");
    AssertEqual(55, verticalExtensionSamus.Grapple.AnchorY,
        "vertical extension keeps physical endpoint block Y");

    // Ordinary solid-family blocks return carry with overflow clear. That is not a rope
    // connection: firing moves to the one-call cancellation function, matching $94:A8E5.
    var solidBlocks = new ushort[8 * 8];
    solidBlocks[3 * 8 + 3] = 0x8000;
    RoomLevelData solidLevel = CreateRoom(
        8, 8, solidBlocks, new byte[solidBlocks.Length]);
    SamusState solidCollisionSamus = CreateSamus(SamusPoseIds.FallingRightPose, 32, 48);
    SamusGrappleMovement.BeginFiring(bus, solidCollisionSamus);
    SamusGrappleMovement.StepFiring(
        bus, solidLevel, solidCollisionSamus, (ushort)SnesButton.X);
    GrappleMovementResult solidCancellation = SamusGrappleMovement.StepFiring(
        bus, solidLevel, solidCollisionSamus, (ushort)SnesButton.X);
    AssertTrue(solidCancellation.CancelQueued && !solidCancellation.Connected,
        "solid block queues grapple firing cancellation");

    // Type `$A` does not use the generic solid result during grapple firing. `$94:A7FD`
    // spawns one of sixteen bank-$84 entries: ordinary nonnegative BTS values run the
    // carry-set/overflow-clear setup and cancel, while BTS three is Draygon's broken turret
    // and queues one whole periodic-damage unit before returning carry+overflow to connect.
    foreach ((byte behavior, bool expectedConnection, bool expectedCancellation, ushort expectedDamage) in new[]
    {
        ((byte)0x00, false, true, (ushort)0),
        ((byte)0x03, true, false, (ushort)1),
        ((byte)0x83, false, false, (ushort)0),
    })
    {
        var spikeBlocks = new ushort[8 * 8];
        var spikeBts = new byte[spikeBlocks.Length];
        spikeBlocks[3 * 8 + 3] = 0xa000;
        spikeBts[3 * 8 + 3] = behavior;
        RoomLevelData spikeLevel = CreateRoom(8, 8, spikeBlocks, spikeBts);
        SamusState spikeSamus = CreateSamus(SamusPoseIds.FallingRightPose, 32, 48);
        spikeSamus.Kinematics.YSpeed = 1;
        SamusGrappleMovement.BeginFiring(bus, spikeSamus);
        SamusGrappleMovement.StepFiring(
            bus, spikeLevel, spikeSamus, (ushort)SnesButton.X);
        GrappleMovementResult spikeReaction = SamusGrappleMovement.StepFiring(
            bus, spikeLevel, spikeSamus, (ushort)SnesButton.X);

        AssertEqual(expectedConnection, spikeReaction.Connected,
            $"firing spike BTS ${behavior:X2} connection flag");
        AssertEqual(expectedCancellation, spikeReaction.CancelQueued,
            $"firing spike BTS ${behavior:X2} cancellation flag");
        AssertEqual(expectedDamage, spikeSamus.LiquidPhysics.PeriodicDamage,
            $"firing spike BTS ${behavior:X2} periodic damage");
    }

    // The remaining four bank-$94 endpoint entries share their projectile/bomb PLM setup
    // with ordinary weapons, but grapple itself has projectile family zero.  Types four
    // and seven therefore remain air; their solid twins C and F cancel the live beam.
    // BTS C selects one of the retail no-op projectile-shot PLMs for types four/C, proving
    // that the shared reaction side effect is retained independently of collision carry.
    foreach ((ushort collisionWord, bool expectedCancellation, int expectedPlms) in new[]
    {
        // A clear-carry type-four endpoint survives all four fractional probes, so each
        // probe allocates its own one-handler native no-op slot.  Solid type C stops at
        // the first carry-set probe and therefore allocates exactly one.
        ((ushort)0x4000, false, 4),
        ((ushort)0x7000, false, 0),
        ((ushort)0xc000, true, 1),
        ((ushort)0xf000, true, 0),
    })
    {
        var endpointBlocks = new ushort[8 * 8];
        var endpointBts = new byte[endpointBlocks.Length];
        endpointBlocks[3 * 8 + 3] = collisionWord;
        endpointBts[3 * 8 + 3] = 0x0c;
        RoomLevelData endpointLevel = CreateRoom(8, 8, endpointBlocks, endpointBts);
        var endpointPlms = new RoomPlmSystem();
        SamusState endpointSamus = CreateSamus(SamusPoseIds.FallingRightPose, 32, 48);
        SamusGrappleMovement.BeginFiring(bus, endpointSamus);
        SamusGrappleMovement.StepFiring(
            bus, endpointLevel, endpointSamus, (ushort)SnesButton.X, endpointPlms);
        GrappleMovementResult endpointReaction = SamusGrappleMovement.StepFiring(
            bus, endpointLevel, endpointSamus, (ushort)SnesButton.X, endpointPlms);

        AssertEqual(expectedCancellation, endpointReaction.CancelQueued,
            $"grapple endpoint type ${(collisionWord >> 12):X1} cancellation topology");
        AssertEqual(!expectedCancellation, endpointReaction.Fired,
            $"grapple endpoint type ${(collisionWord >> 12):X1} live-beam topology");
        AssertEqual(expectedPlms, endpointPlms.ActiveCount,
            $"grapple endpoint type ${(collisionWord >> 12):X1} shared PLM side effect");
    }

    // Length grows before collision checks. Values 12..120 receive their four probes, but
    // the next addition produces 132 and queues cancellation without moving the endpoint.
    var emptyWideBlocks = new ushort[16 * 8];
    RoomLevelData emptyWideLevel = CreateRoom(
        16, 8, emptyWideBlocks, new byte[emptyWideBlocks.Length]);
    SamusState rangeLimitedSamus = CreateSamus(SamusPoseIds.FallingRightPose, 32, 48);
    SamusGrappleMovement.BeginFiring(bus, rangeLimitedSamus);
    for (int firingFrame = 0; firingFrame < 10; firingFrame++)
    {
        GrappleMovementResult liveRange = SamusGrappleMovement.StepFiring(
            bus, emptyWideLevel, rangeLimitedSamus, (ushort)SnesButton.X);
        AssertTrue(liveRange.Fired, $"grapple range frame {firingFrame} remains live");
    }
    AssertEqual(120, rangeLimitedSamus.Grapple.RopeLength,
        "last collision-tested grapple firing length");
    ushort endpointBeforeRangeCancellation = rangeLimitedSamus.Grapple.AnchorX;
    GrappleMovementResult rangeCancellation = SamusGrappleMovement.StepFiring(
        bus, emptyWideLevel, rangeLimitedSamus, (ushort)SnesButton.X);
    AssertTrue(rangeCancellation.CancelQueued,
        "grapple queues cancellation when pre-collision length reaches 128");
    AssertEqual(endpointBeforeRangeCancellation, rangeLimitedSamus.Grapple.AnchorX,
        "range cancellation performs no endpoint substep");

    // Release-of-Shoot is checked before extension. Cancellation remains queued for one
    // function call, mirroring the bank-$9B pointer change rather than disappearing early.
    var cancelledSamus = new SamusState
    {
        Pose = SamusPoseIds.FallingRightPose,
        XPosition = 32,
        YPosition = 48,
    };
    SamusGrappleMovement.BeginFiring(bus, cancelledSamus);
    GrappleMovementResult cancelQueued = SamusGrappleMovement.StepFiring(
        bus, firingLevel, cancelledSamus, controllerInput: 0);
    AssertTrue(cancelQueued.CancelQueued && !cancelQueued.Cancelled,
        "released firing queues cancellation");
    GrappleMovementResult cancelled =
        SamusGrappleMovement.CompleteFiringCancellation(bus, firingLevel, cancelledSamus);
    AssertTrue(cancelled.Cancelled && cancelledSamus.Grapple.Phase == GrapplePhase.Inactive,
        "queued firing cancellation clears on following call");

    // Exercise all three native connection tables through the public BeginFiring/StepFiring
    // route. Filling this isolated room with persistent type-$E blocks makes every
    // endpoint connect on its first substep, while each authored direction still
    // selects its own four-byte {function, handler} record and pose.
    var connectionBlocks = Enumerable.Repeat((ushort)0xe000, 8 * 8).ToArray();
    RoomLevelData connectionLevel = CreateRoom(
        8, 8, connectionBlocks, new byte[connectionBlocks.Length]);
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
        int tableOffset = direction * 2;
        WriteTestWord(bus, 0x9bc14a + tableOffset, unchecked((ushort)(direction + 20)));
        WriteTestWord(bus, 0x9bc15e + tableOffset, unchecked((ushort)(direction + 30)));
    }

    for (int family = 0; family < expectedConnectionPoses.Length; family++)
    {
        for (byte direction = 0; direction < 10; direction++)
        {
            byte sourceMovementType = family == 1 ? (byte)5 : (byte)6;
            WritePoseDefinition(bus, SamusPoseIds.FallingRightPose,
                [0x08, sourceMovementType, 0xff, direction, 0x00, 0x00, 0x05, 0x15]);

            var connectionSamus = new SamusState
            {
                Pose = SamusPoseIds.FallingRightPose,
                XPosition = 40,
                YPosition = 40,
            };
            if (family == 2)
            {
                // A nonzero fractional half alone must select the vertical table and must
                // be cleared by the shared special-pose-command tail after connection.
                connectionSamus.Kinematics.YSubspeed = 1;
            }
            connectionSamus.HorizontalSpeed.AccelerationMode = 7;
            connectionSamus.HorizontalSpeed.BaseSpeed = 1;
            connectionSamus.HorizontalSpeed.BaseSubspeed = 2;
            connectionSamus.HorizontalSpeed.ExtraRunSpeed = 3;
            connectionSamus.HorizontalSpeed.ExtraRunSubspeed = 4;

            SamusGrappleMovement.BeginFiring(bus, connectionSamus);
            GrappleMovementResult tableConnection = SamusGrappleMovement.StepFiring(
                bus,
                connectionLevel,
                connectionSamus,
                (ushort)SnesButton.X);

            byte expectedPose = expectedConnectionPoses[family][direction];
            bool expectedLocked = expectedPose is not (0xb2 or 0xb3);
            AssertTrue(tableConnection.Connected,
                $"connection family {family} direction {direction} connects through runtime path");
            AssertEqual(expectedPose, connectionSamus.Pose,
                $"connection family {family} direction {direction} pose");
            AssertEqual(
                expectedLocked ? GrapplePhase.ConnectedLocked : GrapplePhase.ConnectedSwinging,
                connectionSamus.Grapple.Phase,
                $"connection family {family} direction {direction} function phase");
            AssertEqual(expectedLocked, tableConnection.LockedInPlace,
                $"connection family {family} direction {direction} locked result");
            AssertTrue(tableConnection.CameraPreviousX.HasValue && tableConnection.CameraPreviousY.HasValue,
                $"connection family {family} direction {direction} publishes camera clamp");

            AssertEqual(0, connectionSamus.HorizontalSpeed.BaseSpeed,
                "connection clears whole X base speed");
            AssertEqual(0, connectionSamus.HorizontalSpeed.BaseSubspeed,
                "connection clears fractional X base speed");
            AssertEqual(0, connectionSamus.HorizontalSpeed.ExtraRunSpeed,
                "connection clears whole extra run speed");
            AssertEqual(0, connectionSamus.HorizontalSpeed.ExtraRunSubspeed,
                "connection clears fractional extra run speed");
            AssertEqual(7, connectionSamus.HorizontalSpeed.AccelerationMode,
                "connection leaves acceleration mode untouched");
            AssertEqual(0, connectionSamus.Kinematics.YSpeed,
                "connection clears whole Y speed");
            AssertEqual(0, connectionSamus.Kinematics.YSubspeed,
                "connection clears fractional Y speed");

            if (expectedLocked)
            {
                short rawOriginX = nativeOriginX[direction];
                short rawOriginY = nativeOriginY[direction];
                short rawFlareX = unchecked((short)(direction + 20));
                short rawFlareY = unchecked((short)(direction + 30));
                AssertEqual(
                    unchecked((ushort)(connectionSamus.Grapple.RopeStartX - rawOriginX)),
                    connectionSamus.XPosition,
                    "locked command positions Samus X from physical rope Start");
                AssertEqual(
                    unchecked((ushort)(connectionSamus.Grapple.RopeStartY - rawOriginY)),
                    connectionSamus.YPosition,
                    "locked command positions Samus Y from physical rope Start");
                AssertEqual(
                    unchecked((ushort)(connectionSamus.XPosition + rawFlareX)),
                    connectionSamus.Grapple.BeamStartX,
                    "locked command independently publishes flare/draw X");
                AssertEqual(
                    unchecked((ushort)(connectionSamus.YPosition + rawFlareY)),
                    connectionSamus.Grapple.BeamStartY,
                    "locked command independently publishes flare/draw Y");
            }
            else
            {
                AssertEqual(connectionSamus.Grapple.RopeStartX, connectionSamus.Grapple.BeamStartX,
                    "swing command aliases physical Start and flare X");
                AssertEqual(connectionSamus.Grapple.RopeStartY, connectionSamus.Grapple.BeamStartY,
                    "swing command aliases physical Start and flare Y");
            }
        }
    }

    // Preserve the original fixture record for the remaining grapple tests in this method.
    WritePoseDefinition(bus, SamusPoseIds.FallingRightPose,
        [0x08, 0x06, 0xff, 0x02, 0x00, 0x00, 0x05, 0x15]);

    // Native $80 points straight down (X=0,Y=256); $81 is (-6,+255).
    // At length 50 those offsets become (0,50) and (-1,49). Negative X also
    // changes the block-anchor low nibble from eight to seven.

    // Override displayed frames independently of the native physical body offsets.
    // Both angles retain right-facing body offset (0,+46), despite art frames three/four.
    bus.WriteByte(0x9bc1c2 + 0x80, 3);
    bus.WriteByte(0x9bc1c2 + 0x81, 4);

    // The already-connected pendulum fixture uses an intentionally empty 32x16 room. Its
    // six-point sweep must stay in bounds while proving that no invented terrain response
    // disturbs the pre-existing unobstructed numbers.
    var swingBlocks = new ushort[32 * 16];
    RoomLevelData swingLevel = CreateRoom(
        32, 16, swingBlocks, new byte[swingBlocks.Length]);

    var samus = new SamusState { XPosition = 10, YPosition = 20 };
    SamusGrappleMovement.ConnectUnobstructedSwing(
        bus,
        samus,
        anchorX: 200,
        anchorY: 100,
        ropeLength: 50,
        angle: SnesAngle.HalfTurn,
        angularVelocity: 0,
        faceRight: true);

    AssertEqual(SamusPoseIds.GrappleSwingRightPose, samus.Pose, "grapple connection pose");
    AssertEqual(
        SamusMovementType.Grappling,
        samus.ReadMovementType(bus),
        "grapple movement type from pose record");
    AssertEqual(200, samus.Grapple.BeamStartX, "initial grapple beam-start X");
    AssertEqual(154, samus.Grapple.BeamStartY, "initial grapple beam-start Y");
    AssertEqual(200, samus.XPosition, "initial grapple native body X");
    AssertEqual(200, samus.YPosition, "initial grapple native body Y");
    AssertEqual(3, samus.AnimationFrame, "initial grapple angle art frame");

    // Left held at exact $8000 first applies the native +$0100 kick, then +12 input.
    // Gravity is exactly zero on that axis, so angle advances by $010C to $810C.
    GrappleMovementResult swung = SamusGrappleMovement.Step(
        bus,
        swingLevel,
        samus,
        (ushort)(SnesButton.X | SnesButton.Left),
        newlyPressedInput: 0);
    AssertEqual(GrapplePhase.ConnectedSwinging, swung.Phase, "held-shot grapple phase");
    AssertEqual(0x010c, samus.Grapple.AngularVelocity, "bottom kick plus input acceleration");
    AssertEqual(0x810c, samus.Grapple.Angle.RawValue, "unobstructed angle integration");
    AssertEqual(198, samus.Grapple.BeamStartX, "advanced grapple beam-start X");
    AssertEqual(153, samus.Grapple.BeamStartY, "advanced grapple beam-start Y");
    AssertEqual(198, samus.XPosition, "advanced grapple native body X");
    AssertEqual(199, samus.YPosition, "advanced grapple native body Y");
    AssertEqual(4, samus.AnimationFrame, "advanced grapple angle art frame");

    // `$90:EB86` remains installed for every noninactive host phase, but its signed native
    // function-pointer test selects beam-specific graphics only through `$9B:C832`. The
    // later one-frame functions must therefore suppress both ordinary charge flare and rope
    // while still falling through to the normal atmosphere/body/cannon/echo subroutine.
    AssertTrue(SamusGrappleMovement.UsesGrappleDrawingHandler(GrapplePhase.Firing),
        "grapple firing owns replacement draw handler");
    AssertTrue(SamusGrappleMovement.UsesBeamSpecificDrawingPath(GrapplePhase.WallGrabRelease),
        "grapple wall-release remains inside beam-specific pointer range");
    AssertTrue(SamusGrappleMovement.UsesGrappleDrawingHandler(GrapplePhase.CancelPending),
        "grapple cancel keeps replacement draw handler for pending frame");
    AssertTrue(!SamusGrappleMovement.UsesBeamSpecificDrawingPath(GrapplePhase.CancelPending),
        "grapple cancel takes ordinary body-and-echo fallback");
    AssertTrue(!SamusGrappleMovement.UsesGrappleDrawingHandler(GrapplePhase.Inactive),
        "inactive grapple restores default draw handler");

    // Give bank-$93 table index 16 one deliberately recognizable one-piece spritemap.
    // The first flare call must force frame 16/timer three, decrement the timer to two,
    // and draw at the connected Flare origin before any rope or Samus objects are appended.
    WriteTestWord(bus, 0x93a1a1 + 16 * 2, 0x9000);
    WriteTestWord(bus, 0x939000, 1);
    WriteTestWord(bus, 0x939002, 2);
    bus.WriteByte(0x939004, 0xfd);
    WriteTestWord(bus, 0x939005, 0x3456);
    var grappleFlareOam = new OamBuffer();
    grappleFlareOam.BeginFrame();
    AssertTrue(SamusGrappleMovement.DrawFlareBeforeSamus(
            bus, samus, grappleFlareOam, layer1X: 100, layer1Y: 50),
        "connected grapple flare passes unsigned screen-Y gate");
    AssertEqual(1, samus.Grapple.FlareCounter,
        "grapple flare counter increments only in post-Samus tile pass");
    AssertEqual(16, samus.Grapple.FlareAnimationFrame,
        "first grapple flare selects shared main frame sixteen");
    AssertEqual(2, samus.Grapple.FlareAnimationTimer,
        "first grapple flare performs decrement-before-test");
    AssertEqual(4, grappleFlareOam.NextByteOffset,
        "grapple flare appends one ROM-authored OAM entry");
    OamEntry grappleFlare = grappleFlareOam.GetEntry(0);
    AssertEqual(100, grappleFlare.X, "grapple flare spritemap X offset");
    AssertEqual(100, grappleFlare.Y, "grapple flare signed Y offset");
    AssertEqual(0x56, grappleFlare.TileNumber, "grapple flare retains ROM tile bits");
    AssertEqual(2, grappleFlare.Palette, "grapple flare retains ROM palette bits");
    AssertEqual(3, grappleFlare.Priority, "grapple flare retains ROM priority bits");

    // UpdateGrappleBeamTiles reads one 32-byte endpoint source and one angle-selected
    // 128-byte segment source from bank-$9B pointer tables, but queues bank $9A as the DMA
    // source. Give this angle unique pointers so a hard-coded host tile cannot pass.
    WriteTestWord(bus, 0x9bc342, 0x1234);
    WriteTestWord(bus, 0x9bc344, 0x1434);
    int foldedAngleOffset = (samus.Grapple.Angle.RawValue >> 9) & 0xfe;
    WriteTestWord(bus, 0x9bc346 + foldedAngleOffset, 0x5678);

    // $94:AFBA recalculates the angle from endpoint minus flare (1,-49).
    // Coarse octant division selects angle zero, giving a (0,-8) segment step.

    var grappleOam = new OamBuffer();
    var grappleVramWrites = new VramWriteQueue();
    grappleOam.BeginFrame();
    SamusGrappleMovement.DrawConnectedBeam(
        bus,
        samus.Grapple,
        grappleOam,
        grappleVramWrites,
        layer1X: 100,
        layer1Y: 50);

    AssertEqual(2, grappleVramWrites.Entries.Count, "grapple queues endpoint and segment tiles");
    AssertEqual(new VramWriteEntry(0x20, 0x9a1234, 0x6200), grappleVramWrites.Entries[0],
        "grapple endpoint tile DMA record");
    AssertEqual(new VramWriteEntry(0x80, 0x9a5678, 0x6210), grappleVramWrites.Entries[1],
        "grapple angle-selected segment DMA record");
    AssertEqual(2, samus.Grapple.FlareCounter,
        "post-Samus grapple tile pass increments flare counter");

    // Fifty pixels yields six body pieces because $94:AFBA uses (length / 8) before drawing
    // the endpoint. Instruction slots descend 15..10, so GrappleFunc_AF87's phases are
    // $24,$23,$22,$21,$24,$23 rather than one shared guessed animation tile.
    AssertEqual(28, grappleOam.NextByteOffset, "six grapple segments plus endpoint OAM bytes");
    int[] expectedTiles = [0x24, 0x23, 0x22, 0x21, 0x24, 0x23];
    for (int segment = 0; segment < expectedTiles.Length; segment++)
    {
        OamEntry entry = grappleOam.GetEntry(segment);
        AssertOamEntry(
            new OamEntry(
                94,
                (byte)(99 - segment * 8),
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
    AssertEqual(95, grappleEndpoint.X, "grapple endpoint screen X");
    AssertEqual(50, grappleEndpoint.Y, "grapple endpoint screen Y");
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
    var zeroLengthOam = new OamBuffer();
    var zeroLengthWrites = new VramWriteQueue();
    zeroLengthOam.BeginFrame();
    SamusGrappleMovement.DrawConnectedBeam(
        bus,
        zeroLengthGrapple,
        zeroLengthOam,
        zeroLengthWrites,
        layer1X: 100,
        layer1Y: 50);
    AssertEqual(2, zeroLengthWrites.Entries.Count,
        "zero-length grapple still queues endpoint and segment tiles");
    AssertEqual(2, zeroLengthGrapple.FlareCounter,
        "zero-length grapple still advances flare counter");
    AssertEqual(0, zeroLengthOam.NextByteOffset,
        "zero-length grapple emits no rope or endpoint OAM");

    // Releasing Shoot runs $9B:CA65 now but queues $9B:CB8B for the next call. With signed
    // cosine -6 and doubled angular velocity 536, vertical magnitude is $00000C90.
    // The phase-shifted horizontal sample is +254, giving $000213D0.
    GrappleMovementResult queued = SamusGrappleMovement.Step(bus, swingLevel, samus, 0, 0);
    AssertTrue(queued.ReleaseQueued && !queued.Released, "grapple release is one-frame queued");
    AssertEqual(GrapplePhase.ReleaseFromSwing, samus.Grapple.Phase, "release function pointer phase");
    AssertEqual(0, samus.Kinematics.YSpeed, "grapple release whole Y speed");
    AssertEqual(0x0c90, samus.Kinematics.YSubspeed, "grapple release fractional Y speed");
    AssertEqual(1, samus.Kinematics.YDirection, "positive swing with negative cosine launches up");
    AssertEqual(2, samus.HorizontalSpeed.BaseSpeed, "grapple release whole X speed");
    AssertEqual(0x13d0, samus.HorizontalSpeed.BaseSubspeed, "grapple release fractional X speed");
    AssertEqual(2, samus.HorizontalSpeed.AccelerationMode, "release selects deceleration mode");

    GrappleMovementResult released = SamusGrappleMovement.Step(bus, swingLevel, samus, 0, 0);
    AssertTrue(released.Released && !released.ReleaseQueued, "queued grapple release completes");
    AssertEqual(GrapplePhase.Inactive, samus.Grapple.Phase, "completed release clears grapple phase");
    AssertEqual(SamusPoseIds.NormalJumpForwardLeftPose, samus.Pose,
        "nonnegative angular velocity selects left-facing release pose $52");
    AssertEqual(0, samus.Kinematics.YSpeed, "release pose preserves whole Y velocity");
    AssertEqual(0x0c90, samus.Kinematics.YSubspeed, "release pose preserves fractional Y velocity");
    AssertEqual(0, samus.Grapple.FlareCounter,
        "completed grapple release clears shared flare counter");
    AssertEqual(0, samus.Grapple.FlareAnimationFrame,
        "completed grapple release clears flare frame");
    AssertEqual(0, samus.Grapple.FlareAnimationTimer,
        "completed grapple release clears flare timer");

    // Angle $40 is exactly rightward; the next sample $41 is (+255,+6).
    // Keep those native values and place terrain at the actual nearest probe.
    bus.WriteByte(0x9bc1c2 + 0x40, 0);

    // Anchor (128,128) is biased to (136,136). With length 32, candidate angle $41's
    // nearest probe is 40 pixels from the anchor at (175,136), block (10,8). Gravity adds
    // $18 to initial velocity $100, so collision must preserve last-safe angle $40.80 and
    // transform velocity $118 into -($118 >> 1) = -$8C while opening a 16-frame kick gate.
    var angularCollisionBlocks = new ushort[16 * 16];
    angularCollisionBlocks[8 * 16 + 10] = 0x8000;
    RoomLevelData angularCollisionLevel = CreateRoom(
        16, 16, angularCollisionBlocks, new byte[angularCollisionBlocks.Length]);
    var angularCollisionSamus = new SamusState();
    SamusGrappleMovement.ConnectUnobstructedSwing(
        bus,
        angularCollisionSamus,
        anchorX: 128,
        anchorY: 128,
        ropeLength: 32,
        angle: SnesAngle.QuarterTurn,
        angularVelocity: 0x0100,
        faceRight: true);
    GrappleMovementResult angularCollision = SamusGrappleMovement.Step(
        bus,
        angularCollisionLevel,
        angularCollisionSamus,
        (ushort)SnesButton.X,
        newlyPressedInput: 0);
    AssertTrue(angularCollision.TerrainCollided, "grapple angular sweep reports terrain collision");
    AssertEqual(6, angularCollision.CollisionDistanceFromFeet,
        "nearest of six grapple body probes collides first");
    AssertEqual(0x4080, angularCollisionSamus.Grapple.Angle.RawValue,
        "grapple collision restores last-safe angle plus half fraction");
    AssertEqual(-0x008c, angularCollisionSamus.Grapple.AngularVelocity,
        "grapple collision negates arithmetic half velocity");
    AssertEqual(16, angularCollisionSamus.Grapple.CollisionBounceTimer,
        "grapple collision opens sixteen-frame kick window");

    // The same six-point radial sweep has two damage-producing dispatcher entries. Solid
    // spike BTS zero/one queue `$003C/$0010` and still collide. Every other table entry is
    // literal zero, while a negative BTS skips the table. Use a fresh Samus for each row so
    // `$18A8` does not mask the next fixture's first contact.
    foreach ((byte behavior, ushort expectedDamage) in new[]
    {
        ((byte)0x00, (ushort)0x003c),
        ((byte)0x01, (ushort)0x0010),
        ((byte)0x02, (ushort)0x0000),
        ((byte)0x80, (ushort)0x0000),
    })
    {
        var spikeBlockWords = new ushort[16 * 16];
        var spikeBlockBts = new byte[spikeBlockWords.Length];
        spikeBlockWords[8 * 16 + 10] = 0xa000;
        spikeBlockBts[8 * 16 + 10] = behavior;
        RoomLevelData spikeBlockLevel = CreateRoom(
            16, 16, spikeBlockWords, spikeBlockBts);
        var spikeBlockSamus = new SamusState();
        SamusGrappleMovement.ConnectUnobstructedSwing(
            bus,
            spikeBlockSamus,
            anchorX: 128,
            anchorY: 128,
            ropeLength: 32,
            angle: SnesAngle.QuarterTurn,
            angularVelocity: 0x0100,
            faceRight: true);
        GrappleMovementResult spikeBlockContact = SamusGrappleMovement.Step(
            bus,
            spikeBlockLevel,
            spikeBlockSamus,
            (ushort)SnesButton.X,
            newlyPressedInput: 0);

        AssertTrue(spikeBlockContact.TerrainCollided,
            $"solid grapple spike BTS ${behavior:X2} remains collision");
        AssertEqual(expectedDamage, spikeBlockSamus.LiquidPhysics.PeriodicDamage,
            $"solid grapple spike BTS ${behavior:X2} damage table");
        AssertEqual(expectedDamage == 0 ? (ushort)0 : (ushort)0x003c,
            spikeBlockSamus.InvincibilityTimer,
            $"solid grapple spike BTS ${behavior:X2} invincibility publication");
        AssertEqual(expectedDamage == 0 ? (ushort)0 : (ushort)0x000a,
            spikeBlockSamus.KnockbackTimer,
            $"solid grapple spike BTS ${behavior:X2} knockback-timer publication");
    }

    // Spike-air never stops the pendulum. Only BTS two has nonzero damage; because the
    // near-horizontal sweep samples block (11,8) more than once, an exact `$0010` result also
    // proves the first sample's invincibility timer suppresses later samples in this frame.
    foreach ((byte behavior, ushort expectedDamage) in new[]
    {
        ((byte)0x00, (ushort)0x0000),
        ((byte)0x02, (ushort)0x0010),
        ((byte)0x82, (ushort)0x0000),
    })
    {
        var spikeAirWords = new ushort[16 * 16];
        var spikeAirBts = new byte[spikeAirWords.Length];
        spikeAirWords[8 * 16 + 11] = 0x2000;
        spikeAirBts[8 * 16 + 11] = behavior;
        RoomLevelData spikeAirLevel = CreateRoom(16, 16, spikeAirWords, spikeAirBts);
        var spikeAirSamus = new SamusState();
        SamusGrappleMovement.ConnectUnobstructedSwing(
            bus,
            spikeAirSamus,
            anchorX: 128,
            anchorY: 128,
            ropeLength: 32,
            angle: SnesAngle.QuarterTurn,
            angularVelocity: 0x0100,
            faceRight: true);
        GrappleMovementResult spikeAirContact = SamusGrappleMovement.Step(
            bus,
            spikeAirLevel,
            spikeAirSamus,
            (ushort)SnesButton.X,
            newlyPressedInput: 0);

        AssertTrue(!spikeAirContact.TerrainCollided,
            $"grapple spike-air BTS ${behavior:X2} remains noncollision");
        AssertEqual(expectedDamage, spikeAirSamus.LiquidPhysics.PeriodicDamage,
            $"grapple spike-air BTS ${behavior:X2} damage table");
        AssertEqual(expectedDamage == 0 ? (ushort)0 : (ushort)0x003c,
            spikeAirSamus.InvincibilityTimer,
            $"grapple spike-air BTS ${behavior:X2} invincibility publication");
        AssertEqual(expectedDamage == 0 ? (ushort)0 : (ushort)0x000a,
            spikeAirSamus.KnockbackTimer,
            $"grapple spike-air BTS ${behavior:X2} knockback-timer publication");
        if (expectedDamage != 0)
        {
            spikeAirSamus.DecrementHurtTimers();
            AssertEqual(0x003b, spikeAirSamus.InvincibilityTimer,
                "gameplay tail ages grapple-created invincibility timer");
            AssertEqual(0x0009, spikeAirSamus.KnockbackTimer,
                "gameplay tail ages grapple-created knockback timer");
        }
    }

    // Move the reflected pendulum into empty terrain and press Jump during the kick window.
    // Gravity/correction update -$8C to -$6F, then $9B:BD44 adds -$300 extra velocity.
    // Total -$36F moves $40.80 to $3D.11; success ages the timer and damps only the extra
    // word from -$300 to -$2FA. This is the real collision-assisted grapple kick, not an
    // ordinary aerial jump applied to Samus's X/Y velocity.
    GrappleMovementResult afterAngularCollision = SamusGrappleMovement.Step(
        bus,
        swingLevel,
        angularCollisionSamus,
        (ushort)(SnesButton.X | SnesButton.A),
        newlyPressedInput: (ushort)SnesButton.A);
    AssertTrue(!afterAngularCollision.TerrainCollided,
        "collision kick crosses three clear angle-byte terrain sweeps");
    AssertEqual(0x3d11, angularCollisionSamus.Grapple.Angle.RawValue,
        "grapple collision kick advances exact reflected-plus-extra angle");
    AssertEqual(-0x006f, angularCollisionSamus.Grapple.AngularVelocity,
        "grapple kick keeps gravity-corrected base angular velocity");
    AssertEqual(-0x02fa, angularCollisionSamus.Grapple.JumpImpulse,
        "successful grapple kick damps extra angular velocity by six");
    AssertEqual(15, angularCollisionSamus.Grapple.CollisionBounceTimer,
        "successful post-bounce movement ages kick window");

    // Rope growth uses a different radial frontier: candidate length 33 plus 56 pixels puts
    // the probe at X=225, block 14. Make that cell a horizontal extension to solid block 15.
    // Correct per-pixel length handling must reject 33, retain 32, and leave +2 active to
    // retry next frame; treating the extension itself as air would incorrectly grow the rope.
    var ropeCollisionBlocks = new ushort[16 * 16];
    var ropeCollisionBts = new byte[ropeCollisionBlocks.Length];
    ropeCollisionBlocks[8 * 16 + 14] = 0x5000;
    ropeCollisionBts[8 * 16 + 14] = 1;
    ropeCollisionBlocks[8 * 16 + 15] = 0x8000;
    RoomLevelData ropeCollisionLevel = CreateRoom(
        16, 16, ropeCollisionBlocks, ropeCollisionBts);
    var ropeCollisionSamus = new SamusState();
    SamusGrappleMovement.ConnectUnobstructedSwing(
        bus,
        ropeCollisionSamus,
        anchorX: 128,
        anchorY: 128,
        ropeLength: 32,
        angle: SnesAngle.QuarterTurn,
        angularVelocity: 0,
        faceRight: true);
    GrappleMovementResult ropeCollision = SamusGrappleMovement.Step(
        bus,
        ropeCollisionLevel,
        ropeCollisionSamus,
        (ushort)(SnesButton.X | SnesButton.Down),
        newlyPressedInput: (ushort)SnesButton.Down);
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
    SamusGrappleMovement.ConnectUnobstructedSwing(
        bus,
        disconnectedAnchorSamus,
        anchorX: 128,
        anchorY: 128,
        ropeLength: 32,
        angle: SnesAngle.QuarterTurn,
        angularVelocity: 0,
        faceRight: true);
    disconnectedAnchorSamus.Grapple.ValidateAnchorBlock = true;
    GrappleMovementResult disconnectedAnchor = SamusGrappleMovement.Step(
        bus,
        CreateEmptyRoom(16, 16),
        disconnectedAnchorSamus,
        (ushort)SnesButton.X,
        newlyPressedInput: 0);
    AssertTrue(disconnectedAnchor.AnchorDisconnected && disconnectedAnchor.ReleaseQueued,
        "air replacing a validated grapple anchor queues moving release");

    // Retail $6A has components (+131,+219), placing the eight-pixel rope at
    // (140,142). Candidate $6B is (+126,+225): its first 16-pixel body probe
    // reaches (143,150), solid block (8,9), rather than an invented horizontal wall.
    // Bank $94 must stop at `$6A80`; bank $9B must then consume record four's literal
    // `$B9`, (+24,+16), `$C814` tuple rather than inventing a host-side angle range.
    bus.WriteByte(0x9bc1c2 + 0x6a, 0);

    var specialBlocks = new ushort[16 * 16];
    specialBlocks[8 * 16 + 8] = 0x8000; // `$B9`'s later 16-pixel left wall probe
    specialBlocks[9 * 16 + 8] = 0x8000; // candidate `$6B`, nearest radial probe
    specialBlocks[10 * 16 + 8] = 0x8000;
    RoomLevelData specialLevel = CreateRoom(
        16, 16, specialBlocks, new byte[specialBlocks.Length]);
    var wallGrabSamus = new SamusState();
    SamusGrappleMovement.ConnectUnobstructedSwing(
        bus,
        wallGrabSamus,
        anchorX: 128,
        anchorY: 128,
        ropeLength: 8,
        angle: SnesAngle.FromRaw(0x6a00),
        angularVelocity: 0x0200,
        faceRight: true);
    GrappleMovementResult wallGrab = SamusGrappleMovement.Step(
        bus,
        specialLevel,
        wallGrabSamus,
        (ushort)SnesButton.X,
        newlyPressedInput: 0);
    AssertTrue(wallGrab.TerrainCollided && wallGrab.SpecialAngleHandled && wallGrab.WallGrabEntered,
        "close grapple collision enters ROM-selected wall-grab function");
    AssertEqual(6, wallGrab.CollisionDistanceFromFeet,
        "wall-grab special route requires nearest radial probe");
    AssertEqual(GrapplePhase.WallGrab, wallGrabSamus.Grapple.Phase,
        "special record installs `$C814` wall-grab phase");
    AssertEqual(SamusPoseIds.GrappleWallContactRightPose, wallGrabSamus.Pose,
        "special record installs literal wall-contact pose `$B9`");
    AssertEqual(160, wallGrabSamus.XPosition,
        "wall-grab snap applies anchor-relative +24 X");
    AssertEqual(152, wallGrabSamus.YPosition,
        "wall-grab snap applies anchor-relative +16 Y");
    AssertEqual(156, wallGrab.CameraPreviousX,
        "special snap retains native pre-snap body X within twelve pixels");
    AssertEqual(164, wallGrab.CameraPreviousY,
        "special snap clamps native pre-snap body Y to twelve pixels");

    GrappleMovementResult heldWall = SamusGrappleMovement.Step(
        bus, specialLevel, wallGrabSamus, (ushort)SnesButton.X, newlyPressedInput: 0);
    AssertTrue(heldWall.WallGrabEntered && wallGrabSamus.Grapple.Phase == GrapplePhase.WallGrab,
        "held Shoot retains frozen wall-grab pose");
    GrappleMovementResult wallReleased = SamusGrappleMovement.Step(
        bus, specialLevel, wallGrabSamus, controllerInput: 0, newlyPressedInput: 0);
    AssertTrue(wallReleased.WallJumpWindowOpened,
        "wall-grab release opens native thirty-check wall-jump window");
    AssertEqual(30, wallGrabSamus.Grapple.WallJumpTimer,
        "wall-grab release seeds decimal thirty before decrementing");

    // `$B9` faces left (pose direction four), so `$90:9CAC` probes left even though the
    // later launch selects right-facing wall-jump pose `$83`. Jump must be a fresh A edge;
    // the grapple function queues `$C9CE` now and performs the launch on the next call.
    GrappleMovementResult wallJumpQueued = SamusGrappleMovement.Step(
        bus,
        specialLevel,
        wallGrabSamus,
        controllerInput: (ushort)SnesButton.A,
        newlyPressedInput: (ushort)SnesButton.A);
    AssertTrue(wallJumpQueued.WallProbeCollided && wallJumpQueued.WallJumpQueued,
        "fresh Jump plus wall probe queues grapple wall jump");
    AssertEqual(29, wallGrabSamus.Grapple.WallJumpTimer,
        "first eligible wall-jump check decrements timer to twenty-nine");

    // `$9B:C9CE` mirrors the ordinary route's selective cleanup: it zeros base speed but
    // preserves the Dash pair and `$0B3C`. Seed the state after the queueing call so this
    // assertion isolates the launch function itself from earlier grapple-swing behavior.
    wallGrabSamus.HorizontalSpeed.ExtraRunSpeed = 1;
    wallGrabSamus.HorizontalSpeed.ExtraRunSubspeed = 0x7000;
    wallGrabSamus.HorizontalSpeed.HasRunningMomentum = true;
    wallGrabSamus.ProjectileFlareCounter = 0x003c;
    wallGrabSamus.LiquidPhysics.BeginFrameSoundRequests();
    GrappleMovementResult wallJumpStarted = SamusGrappleMovement.Step(
        bus, specialLevel, wallGrabSamus, controllerInput: 0, newlyPressedInput: 0);
    AssertTrue(wallJumpStarted.WallJumpStarted,
        "queued grapple wall jump starts on following function call");
    AssertEqual(GrapplePhase.Inactive, wallGrabSamus.Grapple.Phase,
        "grapple wall jump clears connected function");
    AssertEqual(SamusPoseIds.WallJumpRightPose, wallGrabSamus.Pose,
        "left-facing `$B9` contact reverses to wall-jump pose `$83`");
    AssertEqual(4, wallGrabSamus.Kinematics.YSpeed,
        "grapple wall jump reads dry whole speed from ROM");
    AssertEqual(0xa000, wallGrabSamus.Kinematics.YSubspeed,
        "grapple wall jump reads dry fractional speed from ROM");
    AssertEqual(1, wallGrabSamus.Kinematics.YDirection,
        "grapple wall jump launches upward");
    AssertEqual(1, wallGrabSamus.HorizontalSpeed.ExtraRunSpeed,
        "grapple wall jump preserves Dash whole speed");
    AssertEqual(0x7000, wallGrabSamus.HorizontalSpeed.ExtraRunSubspeed,
        "grapple wall jump preserves Dash fraction");
    AssertTrue(wallGrabSamus.HorizontalSpeed.HasRunningMomentum,
        "grapple wall jump preserves Dash momentum flag");
    AssertEqual(0, wallGrabSamus.ProjectileFlareCounter,
        "grapple wall jump clears the active projectile flare counter");
    AssertEqual(new SamusSoundRequest(SoundEffectId.FromCartridge(SoundEffectLibrary.Library1, 0x07), 15), wallGrabSamus.LiquidPhysics.SoundRequests.Single(),
        "grapple wall jump queues generic library-one sound seven");
    AssertEqual(0, wallGrabSamus.Grapple.RopeLength,
        "grapple wall jump removes rope state");

    // Repeat the same authentic entry but do not press Jump. DEC/BPL permits exactly thirty
    // wall checks (timer 29 through zero); the thirty-first call wraps to `$FFFF` and queues
    // `$C8C5`. Pose `$B9` has direction six and radius sixteen, so table `$C9C4[6]` must
    // choose compact diagonal-down-left `$74`, not a generic falling approximation.
    var expiredWallGrabSamus = new SamusState();
    SamusGrappleMovement.ConnectUnobstructedSwing(
        bus,
        expiredWallGrabSamus,
        anchorX: 128,
        anchorY: 128,
        ropeLength: 8,
        angle: SnesAngle.FromRaw(0x6a00),
        angularVelocity: 0x0200,
        faceRight: true);
    SamusGrappleMovement.Step(
        bus, specialLevel, expiredWallGrabSamus, (ushort)SnesButton.X, newlyPressedInput: 0);
    SamusGrappleMovement.Step(
        bus, specialLevel, expiredWallGrabSamus, controllerInput: 0, newlyPressedInput: 0);
    for (int eligibleCheck = 0; eligibleCheck < 30; eligibleCheck++)
    {
        GrappleMovementResult grace = SamusGrappleMovement.Step(
            bus, specialLevel, expiredWallGrabSamus, controllerInput: 0, newlyPressedInput: 0);
        AssertEqual(GrapplePhase.WallGrabRelease, grace.Phase,
            $"wall-grab grace check {eligibleCheck + 1} remains live");
    }
    GrappleMovementResult dropQueued = SamusGrappleMovement.Step(
        bus, specialLevel, expiredWallGrabSamus, controllerInput: 0, newlyPressedInput: 0);
    AssertTrue(dropQueued.DropQueued && dropQueued.Phase == GrapplePhase.Dropped,
        "wall-grab grace underflow queues dropped function");
    GrappleMovementResult dropped = SamusGrappleMovement.Step(
        bus, specialLevel, expiredWallGrabSamus, controllerInput: 0, newlyPressedInput: 0);
    AssertTrue(dropped.Dropped && expiredWallGrabSamus.Grapple.Phase == GrapplePhase.Inactive,
        "queued dropped handler clears grapple on following call");
    AssertEqual(SamusPoseIds.CrouchingAimDiagonalDownLeftPose, expiredWallGrabSamus.Pose,
        "compact dropped table preserves `$B9` diagonal-down aim");
    AssertEqual(0, expiredWallGrabSamus.Kinematics.YSpeed,
        "dropped handler clears whole vertical speed");
    AssertEqual(0, expiredWallGrabSamus.Kinematics.YSubspeed,
        "dropped handler clears fractional vertical speed");

    // Record zero is the locked `$D680 -> $B6` route. Candidate `$D7` points up-left
    // into block (7,7); a two-byte angular step guarantees that sample is visited after
    // quadrant gravity. Releasing Shoot queues `$C856`, whose movement-type-$16` fallback
    // byte in the literal `$B6` definition selects stable crouch `$27` one call later.
    bus.WriteByte(0x9bc1c2 + 0xd6, 0);
    var lockedBlocks = new ushort[16 * 16];
    lockedBlocks[7 * 16 + 7] = 0x8000;
    RoomLevelData lockedLevel = CreateRoom(16, 16, lockedBlocks, new byte[lockedBlocks.Length]);
    var lockedSamus = new SamusState();
    SamusGrappleMovement.ConnectUnobstructedSwing(
        bus,
        lockedSamus,
        anchorX: 128,
        anchorY: 128,
        ropeLength: 8,
        angle: SnesAngle.FromRaw(0xd600),
        angularVelocity: 0x0200,
        faceRight: true);
    GrappleMovementResult locked = SamusGrappleMovement.Step(
        bus, lockedLevel, lockedSamus, (ushort)SnesButton.X, newlyPressedInput: 0);
    AssertTrue(locked.SpecialAngleHandled && locked.LockedInPlace,
        "close collision enters ROM-selected locked function");
    AssertEqual(GrapplePhase.ConnectedLocked, lockedSamus.Grapple.Phase,
        "special record installs `$C77E` locked phase");
    AssertEqual(SamusPoseIds.GrappleCrouchingDownRightPose, lockedSamus.Pose,
        "locked special record installs pose `$B6`");
    AssertEqual(105, lockedSamus.XPosition,
        "locked snap applies signed -30 X offset");
    AssertEqual(111, lockedSamus.YPosition,
        "locked snap applies signed -24 Y offset");
    GrappleMovementResult lockedHeld = SamusGrappleMovement.Step(
        bus, lockedLevel, lockedSamus, (ushort)SnesButton.X, newlyPressedInput: 0);
    AssertTrue(lockedHeld.LockedInPlace,
        "held Shoot preserves special locked body without pendulum integration");
    GrappleMovementResult lockedCancelQueued = SamusGrappleMovement.Step(
        bus, lockedLevel, lockedSamus, controllerInput: 0, newlyPressedInput: 0);
    AssertTrue(lockedCancelQueued.CancelQueued && lockedCancelQueued.OwnsMovement,
        "locked release queues connected-pose cancellation");
    GrappleMovementResult lockedCancelled =
        SamusGrappleMovement.CompleteFiringCancellation(bus, lockedLevel, lockedSamus);
    AssertTrue(lockedCancelled.Cancelled && lockedCancelled.OwnsMovement,
        "connected cancellation owns pose-fallback frame");
    AssertEqual(SamusPoseIds.CrouchingRightPose, lockedSamus.Pose,
        "locked `$B6` cancellation follows definition fallback `$27`");
    AssertEqual(0, lockedSamus.Grapple.RopeLength,
        "locked cancellation clears rope state");

    AssertThrows<ArgumentOutOfRangeException>(
        () => SamusGrappleMovement.ConnectUnobstructedSwing(
            bus, new SamusState(), 0, 0, ropeLength: 7, angle: SnesAngle.Zero, angularVelocity: 0, faceRight: true),
        "grapple rejects a rope shorter than retail connected minimum");

    Console.WriteLine("  Samus grapple: firing, swing collision, locked/wall-grab specials, wall jump, dropped pose, beam OAM, and release agree.");
}

}
