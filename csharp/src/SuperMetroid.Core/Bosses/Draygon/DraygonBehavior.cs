using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>The Draygon species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class DraygonBehavior : IEnemySpecies, IEnemyEntityFactory
{

    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [.. DraygonPartDefinitions.Parts.Keys];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new DraygonEntity(this, slot, common, _bossArtwork);

    /// <summary>Provides the active room bus, random source, and persistent boss state.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Supplies frame timing for the body and linked-part AI behaviours.</summary>
    private readonly EnemyFrameContext _frameContext;
    /// <summary>Draygon's body, eye and tail: their setup, animations and cues.</summary>
    internal DraygonParts Parts { get; }

    /// <summary>The wall turrets' firing cadence during Draygon's fight.</summary>
    internal DraygonTurrets Turrets { get; }

    /// <summary>Draygon's entrance: the opening delay and its dance.</summary>
    internal DraygonIntro Intro { get; }

    /// <summary>Draygon's swoops across the room and its climb straight up.</summary>
    internal DraygonSwoops Swoops { get; }

    /// <summary>How Draygon moves along an angle or horizontally, and the breath bubbles it trails.</summary>
    internal DraygonMovement Movement { get; }

    /// <summary>Draygon's goop passes and the goop and turret shots as projectiles.</summary>
    internal DraygonGoop Goop { get; }

    /// <summary>Draygon grabbing Samus: the chase, the spiral carry, the tail whips and the release.</summary>
    internal DraygonGrab Grab { get; }

    /// <summary>Draygon's death: drifting to its burial point, the Evirs and sinking below the room.</summary>
    internal DraygonDeath Death { get; }

    /// <summary>Draygon's hitboxes, touch and hurt, and what Samus's weapons do to it.</summary>
    internal DraygonCombat Combat { get; }
    /// <summary>The session's installed boss artwork.</summary>
    private readonly BossArtworkBinding _bossArtwork;



    /// <summary>Binds the encounter's room, projectile, presentation, and interaction owners.</summary>
    internal DraygonBehavior(DraygonBehaviorDependencies dependencies)
    {
        _bossArtwork = dependencies.BossArtwork;
        _room = dependencies.Room;
        _frameContext = dependencies.FrameContext;
        Parts = new DraygonParts(new DraygonPartsDependencies
        {
            Room = dependencies.Room, Projectiles = dependencies.Projectiles,
            Earthquake = dependencies.Earthquake, SpriteObjects = dependencies.SpriteObjects,
            Presentation = dependencies.Presentation, FrameContext = dependencies.FrameContext, Audio = dependencies.Audio,
        });
        Turrets = new DraygonTurrets(_room, dependencies.Projectiles);
        Intro = new DraygonIntro(dependencies.SpriteObjects, Turrets);
        Movement = new DraygonMovement(dependencies.SpriteObjects);
        Swoops = new DraygonSwoops(_room, Parts, Turrets, Movement);
        Goop = new DraygonGoop(_room, _frameContext, dependencies.DeathsAndDrops, Parts, Turrets, Movement);
        Grab = new DraygonGrab(dependencies.FrameResults, _room, dependencies.SpriteObjects, dependencies.ScrollCap, Parts, Turrets);
        Death = new DraygonDeath(_room, dependencies.SpriteObjects, dependencies.DeathsAndDrops, Parts);
        Combat = new DraygonCombat(dependencies.Projectiles, dependencies.Collision, Parts, Grab);
    }

    /// <summary>Runs Draygon's body per-frame behaviour.</summary>
    private void RunDraygonBodyMain(RoomEnemySlot body, SamusState? samus, int frameCounter)
    {
        DraygonEnemyState state = Combat.RequireCompleteDraygonState(body);
        switch (state.Phase)
        {
            case DraygonPhase.IntroInitialDelay:
                Intro.RunDraygonIntroInitialDelay(state, samus, frameCounter);
                break;
            case DraygonPhase.IntroDance:
                Intro.RunDraygonIntroDance(state, samus, frameCounter);
                break;
            case DraygonPhase.SwoopRightSetup:
                Swoops.SetupDraygonRightSwoop(state, samus, frameCounter);
                break;
            case DraygonPhase.SwoopRightDescending:
                Swoops.DescendDraygonSwoop(state, samus, frameCounter, RightSwoop);
                break;
            case DraygonPhase.SwoopRightApex:
                Swoops.CrossDraygonRightSwoopApex(state, samus, frameCounter);
                break;
            case DraygonPhase.SwoopRightAscending:
                Swoops.AscendDraygonSwoop(state, samus, frameCounter, RightSwoop);
                break;
            case DraygonPhase.SwoopLeftSetup:
                Swoops.SetupDraygonLeftSwoop(state, samus, frameCounter);
                break;
            case DraygonPhase.SwoopLeftDescending:
                Swoops.DescendDraygonSwoop(state, samus, frameCounter, LeftSwoop);
                break;
            case DraygonPhase.SwoopLeftApex:
                Swoops.CrossDraygonLeftSwoopApex(state, samus, frameCounter);
                break;
            case DraygonPhase.SwoopLeftAscending:
                Swoops.AscendDraygonSwoop(state, samus, frameCounter, LeftSwoop);
                break;
            case DraygonPhase.GoopRightSetup:
                Goop.SetupDraygonGoopPass(state, direction: HorizontalDirection.Right, samus, frameCounter);
                break;
            case DraygonPhase.GoopRight:
                Goop.ApproachSamusForDraygonGoop(state, direction: HorizontalDirection.Right, samus, frameCounter);
                break;
            case DraygonPhase.GoopRightTail:
                Goop.FireDraygonGoop(state, direction: HorizontalDirection.Right, samus, frameCounter);
                break;
            case DraygonPhase.GoopRightRecovery:
                Goop.ExitDraygonGoopPass(state, direction: HorizontalDirection.Right);
                break;
            case DraygonPhase.GoopLeftSetup:
                Goop.SetupDraygonGoopPass(state, direction: HorizontalDirection.Left, samus, frameCounter);
                break;
            case DraygonPhase.GoopLeft:
                Goop.ApproachSamusForDraygonGoop(state, direction: HorizontalDirection.Left, samus, frameCounter);
                break;
            case DraygonPhase.GoopLeftTail:
                Goop.FireDraygonGoop(state, direction: HorizontalDirection.Left, samus, frameCounter);
                break;
            case DraygonPhase.GoopLeftRecovery:
                Goop.ExitDraygonGoopPass(state, direction: HorizontalDirection.Left, samus);
                break;
            case DraygonPhase.TryGrabSamus:
                Grab.ChaseAndTryToGrabSamus(state, samus, frameCounter);
                break;
            case DraygonPhase.GrabbedSamus:
                DraygonGrab.RepelDraygonWithGrapple(state, samus);
                break;
            case DraygonPhase.CarrySamus:
                Grab.CarrySamusToDraygonSpiral(state, samus);
                break;
            case DraygonPhase.FlailWithSamus:
                Grab.CarrySamusInDraygonSpiral(state, samus);
                break;
            case DraygonPhase.TailWhipWithSamus:
                Grab.RunDraygonTailWhip(state, samus);
                break;
            case DraygonPhase.FinalTailWhips:
                Grab.BeginDraygonFinalTailWhips(state, samus);
                break;
            case DraygonPhase.FinalTailWhipsWait:
                Grab.WaitForDraygonFinalTailWhips(state, samus);
                break;
            case DraygonPhase.ReleaseSamus:
                Grab.ReleaseSamusFromDraygon(state, samus);
                break;
            case DraygonPhase.FlyStraightUp:
                Swoops.FlyDraygonStraightUp(state, samus, frameCounter);
                break;
            case DraygonPhase.Dying:
                Death.DriftDyingDraygonToBurialPoint(state);
                break;
            case DraygonPhase.DyingSink:
                Death.WaitForDraygonBurialEvirs(state, frameCounter);
                break;
            case DraygonPhase.DyingFinish:
                Death.SinkDraygonBelowTheRoom(state, frameCounter);
                break;
            case DraygonPhase.None:
            default:
                throw new InvalidDataException(
                    $"Draygon body has no behaviour for phase {state.Phase}.");
        }

        // These are four physical enemy records, but Draygon's main behaviour makes the body coordinate
        // authoritative after its phase runs. Copying here preserves that ordering for
        // the eye AI and each part's later animation step in the same enemy frame.
        state.Arms.Position = state.Arms.Position.WithWholeX(body.Position.X.Whole);
        state.Tail.Position = state.Tail.Position.WithWholeX(body.Position.X.Whole);
        state.Eye.Position = state.Eye.Position.WithWholeX(body.Position.X.Whole);
        state.Arms.Position = state.Arms.Position.WithWholeY(body.Position.Y.Whole);
        state.Tail.Position = state.Tail.Position.WithWholeY(body.Position.Y.Whole);
        state.Eye.Position = state.Eye.Position.WithWholeY(body.Position.Y.Whole);
    }

    /// <summary>One side of Draygon's swoop: the direction it travels and the arm animations and phases that side uses.</summary>
    /// <param name="Direction">The way the swoop travels; rightward adds its velocity to X.</param>
    /// <param name="ArmsNearApex">The arms animation started as the path nears its lowest point.</param>
    /// <param name="ArmsIdle">The arms animation restored as the ascent passes the same point.</param>
    /// <param name="Apex">The phase that recalculates velocity at the bottom of the swoop.</param>
    /// <param name="Ascending">The phase that climbs back up after the apex.</param>
    internal sealed record DraygonSwoopSide(HorizontalDirection Direction, DraygonSegment ArmsNearApex, DraygonSegment ArmsIdle,
        DraygonPhase Apex, DraygonPhase Ascending);

    /// <summary>The swoop from the left edge to the right.</summary>
    internal static readonly DraygonSwoopSide RightSwoop = new(HorizontalDirection.Right, DraygonSegment.ArmsFacingRightNearSwoopApex,
        DraygonSegment.ArmsFacingRightIdle, DraygonPhase.SwoopRightApex, DraygonPhase.SwoopRightAscending);

    /// <summary>The mirrored swoop from the right edge to the left.</summary>
    internal static readonly DraygonSwoopSide LeftSwoop = new(HorizontalDirection.Left, DraygonSegment.ArmsFacingLeftNearSwoopApex,
        DraygonSegment.ArmsFacingLeftIdle, DraygonPhase.SwoopLeftApex, DraygonPhase.SwoopLeftAscending);

    /// <summary>
    /// The X Draygon parks at off the left edge (-80, wrapped into the coordinate range); a goop pass leftwards ends once
    /// the body crosses it.
    /// </summary>
    internal static readonly int DraygonOffscreenLeftX = CoordinateAxis.Wrap(-80);

    /// <summary>Distance from the left-side reset point to the right-side one: 672 pixels to the right.</summary>
    internal static readonly Offset16<RoomPixels> DraygonRightSideResetDistance = new(672, 0);

    /// <summary>The swoop acceleration a fight starts with and a tail-whip hit restores: 24/256 pixel per frame per frame downward.</summary>
    internal static readonly EightEightVector<RoomPixels> DraygonInitialSwoopAcceleration = EightEightVector<RoomPixels>.FromTwoHundredFiftySixthsOfPixel(0, 24);

    /// <summary>The swoop acceleration each eye hit adds, 8/256 pixel per frame per frame, while below the limit.</summary>
    internal static readonly EightEightVector<RoomPixels> DraygonShotSwoopAccelerationStep = EightEightVector<RoomPixels>.FromTwoHundredFiftySixthsOfPixel(0, 8);

    /// <summary>The swoop acceleration, 160/256 pixel per frame per frame, an eye hit may not reach.</summary>
    internal static readonly SignedEightEight DraygonSwoopAccelerationLimit = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(160);

    /// <summary>The swoop path entry at which the arms reach for the swoop's apex, and return to rest on the climb.</summary>
    internal const int DraygonSwoopApexArmsEntry = 26;

    /// <summary>Advances Draygon's typed animation.</summary>
    internal void AdvanceDraygonTypedAnimation(RoomEnemySlot slot, EnemyAnimationCueContext context)
    {
        Parts.AdvanceDraygonPartAnimation(slot, context.Samus);
    }

    /// <summary>Runs DraygonBody's main AI.</summary>
    internal void RunDraygonBodyMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        // Draygon times turrets and smoke from the frame counter, never the separate
        // counter that wraps every 256 frames; only its place in a short repeating cycle is tested.
        RunDraygonBodyMain(slot, frame.Samus, _frameContext.FrameCounter);
    }

    /// <summary>Runs DraygonEye's main AI.</summary>
    internal void RunDraygonEyeMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        Parts.RunDraygonPartMain(slot, frame.Samus);
    }

    /// <summary>Runs DraygonTail's main AI.</summary>
    internal void RunDraygonTailMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        Parts.RunDraygonPartMain(slot, frame.Samus);
    }
    /// <summary>Arena X coordinate to which Draygon drifts before burial.</summary>
    internal const int DraygonDeathTargetX = 256;
    /// <summary>Arena Y coordinate at which the death drift hands off to burial.</summary>
    internal const int DraygonDeathTargetY = 480;
    /// <summary>Y threshold below the room where Draygon's burial is complete.</summary>
    internal const int DraygonBurialFinishedY = 576;
    /// <summary>Wait before the burial Evirs and sinking phase proceed.</summary>
    internal const int DraygonDeathWaitFramesUs = 416;
    /// <summary>Whole-pixel speed of Draygon's direct chase before the carry spiral.</summary>
    internal const int DraygonChaseSpeed = 2;
    /// <summary>The carry spiral's target and initial center, (256, 384).</summary>
    internal static readonly Position16<RoomPixels> DraygonSpiralTarget = new(256, 384);
    /// <summary>Initial angular position used when the Samus-carry spiral begins: three quarters of a turn.</summary>
    internal static readonly RotationAngle DraygonInitialSpiralAngle = RotationAngle.ThreeQuarterTurn;
    /// <summary>Initial per-frame angular step of the Samus-carry spiral: eight table units, slowed by 1/256 unit each frame.</summary>
    internal static readonly RotationAngle DraygonInitialSpiralAngleDelta = RotationAngle.FromSteps(8);

    /// <summary>How much the spiral's turn each frame slows every frame: a 256th of a step.</summary>
    internal static readonly RotationAngle DraygonSpiralTurnSlowing = RotationAngle.FromSteps(0, 1);
    /// <summary>The carry spiral's per-frame radius growth: one eighth of a pixel.</summary>
    internal static readonly SignedSixteenSixteen DraygonSpiralRadiusGrowth = SignedSixteenSixteen.FromParts(0, 8192);
    /// <summary>Largest radius reached by the expanding carry spiral.</summary>
    internal const int DraygonMaximumSpiralRadius = 160;
    /// <summary>Upper room boundary for the spiral center's vertical lift.</summary>
    internal const int DraygonMinimumSpiralCenterY = 64;
    /// <summary>The spiral center's per-frame 16.16 lift, a quarter pixel upward.</summary>
    internal static readonly SignedSixteenSixteen DraygonSpiralCenterLift = -SignedSixteenSixteen.FromSixteenthsOfPixel(4);
    /// <summary>Number of AI calls in Draygon's tail-whip phase.</summary>
    internal const int DraygonTailWhipDuration = 64;
    /// <summary>Countdown value that triggers the first tail-whip animation step.</summary>
    internal const int DraygonTailWhipStartFrame = 63;

    /// <summary>
    /// Disables a wall cannon for the room object that names it. Cannon setup names one of
    /// Draygon's wall cannons, then the cannon's damage step disables it here.
    /// </summary>
    public void DisableDraygonCannon(DraygonWallCannon cannon)
    {
        // The defeated room still has cannon room objects even though its enemy population no
        // longer includes Draygon; only a live Draygon reads the disabled cannons.
        _room.EnsureLoaded();
        Parts.Draygon?.DisabledCannons.Add(cannon);
    }

    /// <summary>
    /// Registers Draygon's hurt AI: flashes body, eye, and tail together, while connected
    /// grapple electrocution may drain another 256 health every eighth frame.
    /// </summary>
    internal void AddDraygonHurtAi(EnemySharedReactions table) =>
        table.AddHurt(slot => slot.DefinitionId == EnemyDefinitionId.DraygonBody,
            (slot, frame) => Combat.ApplyDraygonHurt(slot, Combat.RequireCompleteDraygonState(slot), frame.Samus));
}
