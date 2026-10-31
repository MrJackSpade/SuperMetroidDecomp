using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>The Draygon species owner: its per-room state, AI routines and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class DraygonBehavior : IEnemySpecies, IEnemyEntityFactory
{

    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [.. DraygonPartDefinitions.Parts.Keys];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new DraygonEntity(this, slot, common, _bossArtwork);

    /// <summary>Provides the active room bus, random source, and persistent boss state.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Supplies frame timing for the body and linked-part AI callbacks.</summary>
    private readonly EnemyFrameContext _frameContext;
    /// <summary>Draygon's body, eye and tail: their setup, programs and cues.</summary>
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
        Parts = new DraygonParts(dependencies);
        Turrets = new DraygonTurrets(_room, dependencies.Projectiles);
        Intro = new DraygonIntro(dependencies.SpriteObjects, dependencies.Presentation, Turrets);
        Movement = new DraygonMovement(dependencies.SpriteObjects);
        Swoops = new DraygonSwoops(_room, Parts, Turrets, Movement);
        Goop = new DraygonGoop(_room, dependencies.Audio, dependencies.DeathsAndDrops, Parts, Turrets, Movement);
        Grab = new DraygonGrab(dependencies.FrameResults, _room, dependencies.SpriteObjects, dependencies.ScrollCap, Parts, Turrets);
        Death = new DraygonDeath(_room, dependencies.SpriteObjects, dependencies.DeathsAndDrops, Parts);
        Combat = new DraygonCombat(dependencies.Projectiles, dependencies.Presentation, dependencies.Collision, Parts, Grab, _bossArtwork);
    }

    /// <summary>Ports <c>MainAI_DraygonBody</c> at <c>$A5:86FC</c>.</summary>
    private void RunDraygonBodyMain(RoomEnemySlot body, SamusState? samus, int frameCounterLow)
    {
        DraygonEnemyState state = Combat.RequireCompleteDraygonState(body);
        switch (state.Function)
        {
            case DraygonAiFunction.IntroInitialDelay:
                Intro.RunDraygonIntroInitialDelay(state, samus, frameCounterLow);
                break;
            case DraygonAiFunction.IntroDance:
                Intro.RunDraygonIntroDance(state, samus, frameCounterLow);
                break;
            case DraygonAiFunction.SwoopRightSetup:
                Swoops.SetupDraygonRightSwoop(state, samus, frameCounterLow);
                break;
            case DraygonAiFunction.SwoopRightDescending:
                Swoops.DescendDraygonSwoop(state, samus, frameCounterLow, RightSwoop);
                break;
            case DraygonAiFunction.SwoopRightApex:
                Swoops.CrossDraygonRightSwoopApex(state, samus, frameCounterLow);
                break;
            case DraygonAiFunction.SwoopRightAscending:
                Swoops.AscendDraygonSwoop(state, samus, frameCounterLow, RightSwoop);
                break;
            case DraygonAiFunction.SwoopLeftSetup:
                Swoops.SetupDraygonLeftSwoop(state, samus, frameCounterLow);
                break;
            case DraygonAiFunction.SwoopLeftDescending:
                Swoops.DescendDraygonSwoop(state, samus, frameCounterLow, LeftSwoop);
                break;
            case DraygonAiFunction.SwoopLeftApex:
                Swoops.CrossDraygonLeftSwoopApex(state, samus, frameCounterLow);
                break;
            case DraygonAiFunction.SwoopLeftAscending:
                Swoops.AscendDraygonSwoop(state, samus, frameCounterLow, LeftSwoop);
                break;
            case DraygonAiFunction.GoopRightSetup:
                Goop.SetupDraygonGoopPass(state, direction: HorizontalDirection.Right, samus, frameCounterLow);
                break;
            case DraygonAiFunction.GoopRight:
                Goop.ApproachSamusForDraygonGoop(state, direction: HorizontalDirection.Right, samus, frameCounterLow);
                break;
            case DraygonAiFunction.GoopRightTail:
                Goop.FireDraygonGoop(state, direction: HorizontalDirection.Right, samus, frameCounterLow);
                break;
            case DraygonAiFunction.GoopRightRecovery:
                Goop.ExitDraygonGoopPass(state, direction: HorizontalDirection.Right);
                break;
            case DraygonAiFunction.GoopLeftSetup:
                Goop.SetupDraygonGoopPass(state, direction: HorizontalDirection.Left, samus, frameCounterLow);
                break;
            case DraygonAiFunction.GoopLeft:
                Goop.ApproachSamusForDraygonGoop(state, direction: HorizontalDirection.Left, samus, frameCounterLow);
                break;
            case DraygonAiFunction.GoopLeftTail:
                Goop.FireDraygonGoop(state, direction: HorizontalDirection.Left, samus, frameCounterLow);
                break;
            case DraygonAiFunction.GoopLeftRecovery:
                Goop.ExitDraygonGoopPass(state, direction: HorizontalDirection.Left, samus);
                break;
            case DraygonAiFunction.TryGrabSamus:
                Grab.ChaseAndTryToGrabSamus(state, samus, frameCounterLow);
                break;
            case DraygonAiFunction.GrabbedSamus:
                DraygonGrab.RepelDraygonWithGrapple(state, samus);
                break;
            case DraygonAiFunction.CarrySamus:
                Grab.CarrySamusToDraygonSpiral(state, samus);
                break;
            case DraygonAiFunction.FlailWithSamus:
                Grab.CarrySamusInDraygonSpiral(state, samus);
                break;
            case DraygonAiFunction.TailWhipWithSamus:
                Grab.RunDraygonTailWhip(state, samus);
                break;
            case DraygonAiFunction.FinalTailWhips:
                Grab.BeginDraygonFinalTailWhips(state, samus);
                break;
            case DraygonAiFunction.FinalTailWhipsWait:
                Grab.WaitForDraygonFinalTailWhips(state, samus);
                break;
            case DraygonAiFunction.ReleaseSamus:
                Grab.ReleaseSamusFromDraygon(state, samus);
                break;
            case DraygonAiFunction.FlyStraightUp:
                Swoops.FlyDraygonStraightUp(state, samus, frameCounterLow);
                break;
            case DraygonAiFunction.Dying:
                Death.DriftDyingDraygonToBurialPoint(state);
                break;
            case DraygonAiFunction.DyingSink:
                Death.WaitForDraygonBurialEvirs(state, frameCounterLow);
                break;
            case DraygonAiFunction.DyingFinish:
                Death.SinkDraygonBelowTheRoom(state, frameCounterLow);
                break;
            case DraygonAiFunction.None:
            default:
                throw new InvalidDataException(
                    $"Draygon body function {state.Function} is not translated.");
        }

        // These are four physical enemy records, but Draygon_Main makes the body coordinate
        // authoritative after its function returns. Copying here preserves that ordering for
        // the eye AI and each part's later instruction tick in the same enemy frame.
        state.Arms.Position = state.Arms.Position.WithWholeX(body.Position.X.Whole);
        state.Tail.Position = state.Tail.Position.WithWholeX(body.Position.X.Whole);
        state.Eye.Position = state.Eye.Position.WithWholeX(body.Position.X.Whole);
        state.Arms.Position = state.Arms.Position.WithWholeY(body.Position.Y.Whole);
        state.Tail.Position = state.Tail.Position.WithWholeY(body.Position.Y.Whole);
        state.Eye.Position = state.Eye.Position.WithWholeY(body.Position.Y.Whole);
    }

    /// <summary>One side of Draygon's swoop: the direction it travels and the arm programs and functions that side uses.</summary>
    /// <param name="Direction">The way the swoop travels; rightward adds its velocity to X.</param>
    /// <param name="ArmsNearApex">The arms program started as the path nears its lowest point.</param>
    /// <param name="ArmsIdle">The arms program restored as the ascent passes the same point.</param>
    /// <param name="Apex">The function that recalculates velocity at the bottom of the swoop.</param>
    /// <param name="Ascending">The function that climbs back up after the apex.</param>
    internal sealed record DraygonSwoopSide(HorizontalDirection Direction, DraygonProgram ArmsNearApex, DraygonProgram ArmsIdle,
        DraygonAiFunction Apex, DraygonAiFunction Ascending);

    /// <summary>The swoop from the left edge to the right, $A5:88B1-$8951.</summary>
    internal static readonly DraygonSwoopSide RightSwoop = new(HorizontalDirection.Right, DraygonProgram.ArmsFacingRightNearSwoopApex,
        DraygonProgram.ArmsFacingRightIdle, DraygonAiFunction.SwoopRightApex, DraygonAiFunction.SwoopRightAscending);

    /// <summary>The mirrored swoop from the right edge to the left, $A5:8A00-$8A90.</summary>
    internal static readonly DraygonSwoopSide LeftSwoop = new(HorizontalDirection.Left, DraygonProgram.ArmsFacingLeftNearSwoopApex,
        DraygonProgram.ArmsFacingLeftIdle, DraygonAiFunction.SwoopLeftApex, DraygonAiFunction.SwoopLeftAscending);

    /// <summary>
    /// <c>$FFB0</c>: the X Draygon parks at off the left edge (-80 as a signed word); a goop pass leftwards ends once
    /// the body crosses it.
    /// </summary>
    internal const ushort DraygonOffscreenLeftX = unchecked((ushort)-80);

    /// <summary>Distance from the left-side reset point to the right-side one: 672 pixels to the right.</summary>
    internal static readonly Offset16<RoomPixels> DraygonRightSideResetDistance = new(672, 0);

    /// <summary>The swoop acceleration a fight starts with and a tail-whip hit restores: 24/256 pixel per frame per frame downward.</summary>
    internal static readonly EightEightVector<RoomPixels> DraygonInitialSwoopAcceleration = new(0, 24);

    /// <summary>The swoop acceleration each eye hit adds, 8/256 pixel per frame per frame, while below the limit.</summary>
    internal static readonly EightEightVector<RoomPixels> DraygonShotSwoopAccelerationStep = new(0, 8);

    /// <summary>The swoop acceleration, in 1/256 pixel per frame per frame, an eye hit may not reach (a signed compare of the difference).</summary>
    internal const int DraygonSwoopAccelerationLimit = 160;

    /// <summary>The swoop path entry at which the arms reach for the swoop's apex, and return to rest on the climb.</summary>
    internal const int DraygonSwoopApexArmsEntry = 26;

    /// <summary>Advances Draygon's typed animation.</summary>
    internal void AdvanceDraygonTypedAnimation(RoomEnemySlot slot, EnemyInstructionContext context)
    {
        Parts.AdvanceDraygonPartAnimation(slot, context.Samus);
    }

    /// <summary>Ports DraygonBody's main AI.</summary>
    internal void RunDraygonBodyMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        // Draygon times turrets and smoke from the frame counter, never the separate
        // byte-wide frame counter; only its low bits are tested.
        RunDraygonBodyMain(slot, frame.Samus, unchecked((byte)_frameContext.FrameCounter));
    }

    /// <summary>Ports DraygonEye's main AI.</summary>
    internal void RunDraygonEyeMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        Parts.RunDraygonPartMain(slot, frame.Samus);
    }

    /// <summary>Ports DraygonTail's main AI.</summary>
    internal void RunDraygonTailMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        Parts.RunDraygonPartMain(slot, frame.Samus);
    }
    /// <summary>Arena X coordinate to which Draygon drifts before burial.</summary>
    internal const ushort DraygonDeathTargetX = 256;
    /// <summary>Arena Y coordinate at which the death drift hands off to burial.</summary>
    internal const ushort DraygonDeathTargetY = 480;
    /// <summary>Y threshold below the room where Draygon's burial is complete.</summary>
    internal const ushort DraygonBurialFinishedY = 576;
    /// <summary>Native wait before the burial Evirs and sinking phase proceed.</summary>
    internal const ushort DraygonDeathWaitFramesUs = 416;
    /// <summary>Whole-pixel speed of Draygon's direct chase before the carry spiral.</summary>
    internal const ushort DraygonChaseSpeed = 2;
    /// <summary>The carry spiral's target and initial center, ($100, $180).</summary>
    internal static readonly Position16<RoomPixels> DraygonSpiralTarget = new(256, 384);
    /// <summary>Initial angular position used when the Samus-carry spiral begins: three quarters of a turn.</summary>
    internal static readonly RotationAngle DraygonInitialSpiralAngle = RotationAngle.ThreeQuarterTurn;
    /// <summary>Initial per-frame angular step of the Samus-carry spiral: eight table units, slowed by 1/256 unit each frame.</summary>
    internal static readonly RotationAngle DraygonInitialSpiralAngleDelta = RotationAngle.FromTableIndex(8);
    /// <summary>The carry spiral's per-frame radius growth: one eighth of a pixel.</summary>
    internal static readonly SignedSixteenSixteen DraygonSpiralRadiusGrowth = SignedSixteenSixteen.FromParts(0, 8192);
    /// <summary>Largest radius reached by the expanding carry spiral.</summary>
    internal const ushort DraygonMaximumSpiralRadius = 160;
    /// <summary>Upper room boundary for the spiral center's vertical lift.</summary>
    internal const ushort DraygonMinimumSpiralCenterY = 64;
    /// <summary>The spiral center's per-frame 16.16 lift, -$0000.4000.</summary>
    internal static readonly SignedSixteenSixteen DraygonSpiralCenterLift = SignedSixteenSixteen.FromRaw(-16384);
    /// <summary>Number of AI calls in Draygon's tail-whip phase.</summary>
    internal const ushort DraygonTailWhipDuration = 64;
    /// <summary>Countdown value that triggers the first tail-whip animation step.</summary>
    internal const ushort DraygonTailWhipStartFrame = 63;

    /// <summary>
    /// Implements bank $84's write through <c>PLM_Variable</c>. Cannon setup names one of
    /// Draygon's wall cannons, then the damage instruction disables it here.
    /// </summary>
    public void DisableDraygonCannon(DraygonWallCannon cannon)
    {
        // The defeated room still has cannon PLMs even though its enemy population no
        // longer includes Draygon; only a live Draygon reads the disabled words.
        _room.EnsureLoaded();
        Parts.Draygon?.DisabledCannons.Add(cannon);
    }

    /// <summary>
    /// Registers $A5:954D hurt AI: flashes body, eye, and tail together, while connected
    /// grapple electrocution may drain another 256 health every eighth frame.
    /// </summary>
    internal void AddDraygonHurtAi(EnemySharedReactions table) =>
        table.AddHurt(slot => slot.DefinitionId == EnemyDefinitionId.DraygonBody,
            (slot, frame) => Combat.ApplyDraygonHurt(slot, Combat.RequireCompleteDraygonState(slot), frame.Samus));
}
