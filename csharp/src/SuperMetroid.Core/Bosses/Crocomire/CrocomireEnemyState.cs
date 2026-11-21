using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>Crocomire's body state.</summary>
/// <remarks>
/// Every death stage starts its own values: the bridge collapse and each melt pass start the sink from rest, the sink starts
/// the pause's wait, the rumble its first step and the skeleton its speed. Only a hop's sink carries on from the stage before
/// it, continuing the rise's speed and acceleration. A rise starts from no acceleration (#1476).
/// </remarks>
public sealed class CrocomireEnemyState : SuperMetroid.Core.Enemies.BossBodies.IBossBodyShowing
{
    /// <inheritdoc />
    Position16<RoomPixels>? SuperMetroid.Core.Enemies.BossBodies.IBossBodyShowing.BodyPlacement => Body.Position.Whole;

    /// <inheritdoc />
    bool SuperMetroid.Core.Enemies.BossBodies.IBossBodyShowing.BodyHidden => false;

    /// <summary>Creates the state for Crocomire's body place.</summary>
    /// <param name="bodyOccupant">The enemy holding his body's place.</param>
    /// <param name="body">His body's place.</param>
    /// <param name="tongue">The place of Crocomire's tongue, which his body offered its creation as he was placed (#2554).</param>
    internal CrocomireEnemyState(RoomEnemyOccupant bodyOccupant, RoomEnemyPlace body, RoomEnemyPlace tongue)
    {
        BodyOccupant = bodyOccupant;
        Body = body;
        Tongue = Ensure.NotNull(tongue);
        BodyParts = new(SuperMetroid.Core.Enemies.BossBodies.BossBodyOwner.Crocomire, this);
    }

    /// <summary>
    /// The parts his body layer shows (#1466, #1515), which his setup attaches to the room's display and his parts' frames
    /// show on: nothing at first.
    /// </summary>
    internal SuperMetroid.Core.Enemies.BossBodies.BossBodyParts BodyParts { get; }

    /// <summary>The enemy holding his body's place, whose sheet the objects he spawns draw from (#2558).</summary>
    public RoomEnemyOccupant BodyOccupant { get; }

    /// <summary>Crocomire's body's and sprites' colours (#1512).</summary>
    internal CrocomireColours Colours { get; } = new();

    /// <summary>
    /// The state of his death (#2177): the bridge's collapse, his melting in the acid and his skeleton's return, which his
    /// body's death sequence runs and the room's main update and his body layer read.
    /// </summary>
    public CrocomireDeathState Death { get; } = new();

    /// <summary>Crocomire's body, owning the fight and death state and room-pixel coordinates; his body layer follows this actor.</summary>
    public RoomEnemyPlace Body { get; }
    /// <summary>The body's playing animation and where it stands; before the first start, the initial animation.</summary>
    internal AnimationPlayback<CrocomireBodySegment> BodyPlayback { get; set; } =
        new(CrocomireBodyAnimations.StartingWith(CrocomireBodyAnimation.IdleSway), AnimationPlace.StartOfAnimation);

    /// <summary>The body frame shown last, or null before the first frame.</summary>
    internal EnemyFrameId? BodyFrame { get; set; }

    /// <summary>
    /// Whether his skeleton's collapse has shown its last picture (#2180, #2301), as the collapse marks it; he collapses
    /// once, so it stays set through the stable skeleton the collapse rests on after. The death waits on this before it
    /// lays his dead skeleton down.
    /// </summary>
    internal bool SkeletonHasCollapsed { get; private set; }

    /// <summary>His skeleton's collapse shows its last picture, as its animation's cue marks.</summary>
    internal void MarkSkeletonCollapsed() => SkeletonHasCollapsed = true;

    /// <summary>The tongue's playing animation and where it stands; before the first start, the fight loop.</summary>
    internal AnimationPlayback<CrocomireTongueSegment> TonguePlayback { get; set; } =
        new(CrocomireAnimations.TongueStartingWith(CrocomireTongueSegment.Fight), AnimationPlace.StartOfAnimation);

    /// <summary>
    /// The place of Crocomire's tongue (#2400), which his room lists right after his body, found as his body is placed; the
    /// tongue joins the encounter there as it sets up, in a living room and a defeated one alike. Bridge collapse rests
    /// and hides it, while later melting phases reuse its actor.
    /// </summary>
    public RoomEnemyPlace Tongue { get; }

    /// <summary>
    /// Where he stands in his course (<see cref="CrocomireDeathStageDefinitions.Stages"/>, #2540): the fight, from his setup
    /// until the bridge gives way under him, then each stage of his death in turn to his inert corpse.
    /// </summary>
    internal ListStep<CrocomireDeathStage> DeathStage { get; private set; } = ListStep<CrocomireDeathStage>.FirstOf(CrocomireDeathStageDefinitions.Stages);

    /// <summary>Hands his death over to the stage after the one he is at; his course rests at his inert corpse.</summary>
    internal void AdvanceDeathStage() => DeathStage = DeathStage.FollowingToRest;

    /// <summary>His course stands at his inert corpse from now on, as an already-defeated room's setup leaves him.</summary>
    internal void RestAsCorpse() =>
        DeathStage = ListStep<CrocomireDeathStage>.At(CrocomireDeathStageDefinitions.Stages, CrocomireDeathStageDefinitions.Stages.Count - 1);

    /// <summary>The step back a mouth hit asks of his fight, waiting until the fight takes it.</summary>
    public CrocomireAwaitedStepBack StepBack { get; } = new();

    /// <summary>
    /// The fight's phase, run when the body animation's fight-AI cue fires rather than once per main-AI frame (#1939). The
    /// fight starts dormant; a defeated room's corpse never runs it.
    /// </summary>
    public CrocomirePhase Phase { get; internal set; } = CrocomirePhase.Dormant;

    /// <summary>
    /// During combat: his queued backward steps (#1940). A setup queues an authored count, each mouth hit adds to it
    /// and stepping back consumes it.
    /// </summary>
    public int StepCounter
    {
        get;
        internal set => field = Ensure.AtLeastZero(value);
    }

    /// <summary>
    /// The wait of a timed death stage (#2463): each sink starts it to last
    /// <see cref="CrocomireBridgeCollapseDefinitions.SubmergedPauseUpdates"/>, the submerged pause after it rising as it
    /// runs out, and the wall's rumble starts it to last <see cref="CrocomireSkeletonDefinitions.SpikedWallUpdates"/>, his
    /// skeleton breaking through as it runs out.
    /// </summary>
    public BossCountdown DeathStageWait { get; } = new();

    /// <summary>
    /// While the hidden wall rumbles: the oscillation under way in <see cref="CrocomireRumbleDefinitions.Oscillations"/>,
    /// or none once the last has finished.
    /// </summary>
    internal ListStep<CrocomireRumbleOscillation>? RumbleOscillation { get; set; }

    /// <summary>Whether the rumbling wall is heading for its oscillation's extreme rather than back to its return offset.</summary>
    public bool RumbleTowardExtreme { get; internal set; }

    /// <summary>
    /// While the body sinks or rises: the vertical acceleration (#2299). A sink grows it to at most
    /// <see cref="CrocomireBridgeCollapseDefinitions.MaximumSinkAcceleration"/>, from
    /// <see cref="CrocomireBridgeCollapseDefinitions.SinkStartingAcceleration"/> after the bridge's collapse or a melt and
    /// otherwise from where the rise before it left it; a rise grows it from none to at most
    /// <see cref="CrocomireBridgeCollapseDefinitions.MaximumRiseAcceleration"/>.
    /// </summary>
    public SignedSixteenSixteen VerticalAcceleration { get; internal set; }

    /// <summary>
    /// While the body sinks or rises: the vertical speed per frame, down while sinking and up while rising. The sink clamps
    /// its whole part at <see cref="CrocomireBridgeCollapseDefinitions.MaximumSinkSpeed"/> and the rise stops it at zero.
    /// </summary>
    public SignedEightEight VerticalSpeed { get; internal set; }

    /// <summary>
    /// During the projectile attack: how many of the volley's shots Crocomire has spat, cleared when the attack starts and
    /// counted up to <see cref="CrocomireProjectileDefinitions.VolleyShotCount"/>.
    /// </summary>
    public int ShotsSpat { get; internal set; }

    /// <summary>
    /// While the body sinks or rises: the part of a pixel his travel has carried between updates (#2181, #2555). The sinks
    /// and rises move his position by the whole pixels it crosses and keep it apart from his position's own fraction,
    /// which they never move and his skeleton's later arc moves from.
    /// </summary>
    public PixelFractionCarry VerticalTravelCarry { get; internal set; }

    /// <summary>
    /// While the skeleton breaks out and collapses: its horizontal speed in room pixels per frame, zero when the
    /// skeleton breaks through the wall.
    /// </summary>
    internal SignedSixteenSixteen SkeletonVelocity { get; set; }

    /// <summary>
    /// Starts a sink from rest: clears the vertical speed and carried fraction, then sets the starting acceleration of
    /// <paramref name="acceleration"/>.
    /// </summary>
    internal void StartVerticalMotion(SignedSixteenSixteen acceleration)
    {
        VerticalSpeed = SignedEightEight.Zero;
        VerticalTravelCarry = PixelFractionCarry.None;
        VerticalAcceleration = acceleration;
    }
}
