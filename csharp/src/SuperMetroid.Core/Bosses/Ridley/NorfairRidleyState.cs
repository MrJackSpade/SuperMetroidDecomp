using SuperMetroid.Core.Enemies.Corpses;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// Lower Norfair Ridley's encounter (#2228): the shared flight and tail with his Lower Norfair phase, his arena's reveal,
/// his health band, his grab and carry of Samus, his pogo and his death. The combat values carry the meanings their
/// consumers establish.
/// </summary>
public sealed class NorfairRidleyState : RidleyEncounterState
{
    /// <summary>Begins Lower Norfair Ridley's encounter below his arena, facing right.</summary>
    internal NorfairRidleyState()
        : base(
            new RidleyFlightState(RidleyFacing.Right, NorfairRidleyArenaDefinitions.MovementMinimum, NorfairRidleyArenaDefinitions.MovementMaximum,
                firstWingPose: RidleyWingDefinitions.NorfairFirstPose),
            new RidleyTailState(damage: RidleyTailDefinitions.NorfairTipDamage))
    {
    }

    /// <summary>The phase Lower Norfair Ridley runs once he has lifted off.</summary>
    /// <exception cref="InvalidOperationException">He has not lifted off.</exception>
    public NorfairRidleyPhase Phase
    {
        get => LiftedOff ? field : throw NotLiftedOff();
        internal set
        {
            field = value;
            LiftOff();
        }
    }

    /// <summary>How far his arena's reveal, which he holds his roar through, has gone (#2228); it starts at its first step.</summary>
    public NorfairArenaRevealProgress ArenaReveal { get; internal set; }

    /// <summary>The countdown of the swoop phase he flies, apart from his phase timer: each stage of his swoop lasts its own updates.</summary>
    public BossCountdown SwoopPhaseTimer { get; } = new();

    /// <summary>Gets his health band, updated from his health after each of his updates; he starts unhurt.</summary>
    public NorfairRidleyHealthBand HealthBand { get; internal set; } = NorfairRidleyHealthBandDefinitions.Unhurt;
    /// <summary>Gets the grabbed-Samus state controlling claw attachment and release: carrying Samus while Ridley holds her, otherwise released.</summary>
    public RidleyGrabState GrabState { get; internal set; }
    /// <summary>
    /// The countdown of his body's intangibility after he releases Samus, which lets her touch him again as it runs out;
    /// null while none runs.
    /// </summary>
    public BossCountdown? ReleaseIntangibility { get; internal set; }
    /// <summary>Gets the current movement target's whole-pixel room position.</summary>
    public Position16<RoomPixels> Target { get; internal set; }
    /// <summary>Gets the pogo bounce counter, reset after its two-bounce threshold.</summary>
    public int PogoBounceCount { get; internal set; }
    /// <summary>Gets the positive velocity increment the health band's pogo launch gives downward pogo motion.</summary>
    public SignedEightEight PogoDownwardAcceleration { get; internal set; }
    /// <summary>Gets the positive velocity increment the health band's pogo launch gives upward pogo motion.</summary>
    public SignedEightEight PogoUpwardAcceleration { get; internal set; }
    /// <summary>Gets Samus's signed whole-pixel displacement from the claw anchor; carry updates decay each axis toward zero.</summary>
    public Offset16<RoomPixels> GrabOffset { get; internal set; }
    /// <summary>
    /// Gets the hit-pressure count, zero through eight: each hurt frame raises it by one and each ordinary AI frame lowers
    /// it by four; at eight a hurt frame also carries a grabbed Samus.
    /// </summary>
    public int HurtMovementClamp { get; internal set; }
    /// <summary>Gets the number of zero-health lunges completed before the authored death transition.</summary>
    public int ZeroHealthLungeCount { get; internal set; }
    /// <summary>The wait before his death's next small explosion, one every five updates.</summary>
    public BossCountdown DeathExplosionTimer { get; } = new();
    /// <summary>
    /// Where his death's next small explosion bursts: each takes the next point of the scatter shared with the Baby
    /// Metroid's death, the first after the last, starting from the scatter's first.
    /// </summary>
    public ListStep<Offset16<RoomPixels>> NextDeathExplosion { get; internal set; } = FirstDeathExplosion;
    /// <summary>Where his death's first small explosion bursts: the shared scatter's first point.</summary>
    internal static ListStep<Offset16<RoomPixels>> FirstDeathExplosion => ListStep<Offset16<RoomPixels>>.FirstOf(DeathExplosionScatterDefinitions.Offsets);

    /// <summary>Whether he carries Samus in his claws.</summary>
    internal override bool CarriesSamus => GrabState != RidleyGrabState.Released;
}
