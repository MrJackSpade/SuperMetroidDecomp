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
            new RidleyFlightState(RidleyFacing.Right, new Position16<RoomPixels>(64, 64), new Position16<RoomPixels>(224, 416), wingBeatStep: 0),
            new RidleyTailState(damage: 120))
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

    /// <summary>Independent swoop phase countdown used by the swoop phases.</summary>
    public int SwoopPhaseTimer { get; internal set; }

    /// <summary>Gets his health band, updated from his health after each of his updates; he starts unhurt.</summary>
    public NorfairRidleyHealthBand HealthBand { get; internal set; } = NorfairRidleyHealthBandDefinitions.Unhurt;
    /// <summary>Gets the grabbed-Samus state controlling claw attachment and release: carrying Samus while Ridley holds her, otherwise released.</summary>
    public RidleyGrabState GrabState { get; internal set; }
    /// <summary>Gets the countdown keeping the body intangible after a grab release.</summary>
    public int IntangibilityTimer { get; internal set; }
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
    /// <summary>Gets the small-explosion countdown, reloaded to four for one explosion every five calls.</summary>
    public int DeathExplosionTimer { get; internal set; }
    /// <summary>Gets the index, zero through nine, cycling through ten authored death-explosion placements.</summary>
    public int DeathExplosionCount { get; internal set; }
    /// <summary>Gets whether his twelve independently moving breakup actors have already been allocated.</summary>
    public bool DeathBreakupSpawned { get; internal set; }

    /// <summary>Whether he carries Samus in his claws.</summary>
    internal override bool CarriesSamus => GrabState != RidleyGrabState.Released;
}
