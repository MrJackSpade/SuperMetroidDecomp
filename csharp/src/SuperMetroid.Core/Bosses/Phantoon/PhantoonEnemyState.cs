using SuperMetroid.Core.Enemies;
namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>
/// Phantoon's encounter: his four parts, body, eye, tentacles and mouth, each its own enemy made with the encounter by his
/// body's setup and keeping its own animation (#2252), and the fight's state grouped by what it does (#1933): the phase
/// and its timer, the figure-eight, the swoop, the round of vulnerability, the casual flames, the fade on his body, the
/// wave, whether his body shows, and his death.
/// </summary>
[ArchitectureReview("context-bag", "Phantoon's encounter state: every member is a part or sub-state of his own fight that he owns for the room (his four parts, phases, flames, fades, wave and visibility, #2252), not a service carried through.")]
public sealed class PhantoonEnemyState : SuperMetroid.Core.Enemies.BossBodies.IBossBodyShowing
{
    /// <summary>
    /// The body the boss shows its parts' frames on (#1353, #1466, #1515, #2554): its own, made as it sets up and attached
    /// to the room's display then, nothing showing at first.
    /// </summary>
    internal SuperMetroid.Core.Enemies.BossBodies.BossBodyParts BodyParts => field ??= new SuperMetroid.Core.Enemies.BossBodies.BossBodyParts(SuperMetroid.Core.Enemies.BossBodies.BossBodyOwner.Phantoon, this);

    /// <inheritdoc />
    Position16<RoomPixels>? SuperMetroid.Core.Enemies.BossBodies.IBossBodyShowing.BodyPlacement => Body.Position.Whole;

    /// <inheritdoc />
    bool SuperMetroid.Core.Enemies.BossBodies.IBossBodyShowing.BodyHidden => false;

    /// <summary>Begins the encounter of the body at <paramref name="body"/> and the parts his room lists with it.</summary>
    /// <param name="bodyOccupant">The enemy holding <paramref name="body"/>.</param>
    /// <param name="body">The place his body occupies.</param>
    /// <param name="eye">The place his eye occupies.</param>
    /// <param name="tentacles">The place his tentacles occupy.</param>
    /// <param name="mouth">The place his mouth occupies.</param>
    internal PhantoonEnemyState(RoomEnemyOccupant bodyOccupant, RoomEnemyPlace body, RoomEnemyPlace eye, RoomEnemyPlace tentacles, RoomEnemyPlace mouth)
    {
        BodyOccupant = bodyOccupant;
        BodyPart = new PhantoonPart(this, body, PhantoonPartSegment.InvulnerableBody);
        EyePart = new PhantoonPart(this, eye, PhantoonPartSegment.EyeClosed);
        TentaclesPart = new PhantoonPart(this, tentacles, PhantoonPartSegment.Tentacles);
        MouthPart = new PhantoonPart(this, mouth, PhantoonPartSegment.MouthInitial);
    }

    /// <summary>Phantoon's body's and his room's colours (#1512).</summary>
    internal PhantoonColours Colours { get; } = new();

    /// <summary>His body, whose health and position are his; it runs first.</summary>
    public PhantoonPart BodyPart { get; }

    /// <summary>His eye, which shares the body's position while keeping its own animation and colour counters.</summary>
    public PhantoonPart EyePart { get; }

    /// <summary>His tentacles, positioned from the body after its AI pass.</summary>
    public PhantoonPart TentaclesPart { get; }

    /// <summary>His mouth, positioned from the body after its AI pass.</summary>
    public PhantoonPart MouthPart { get; }

    /// <summary>Gets the place his body occupies.</summary>
    public RoomEnemyPlace Body => BodyPart.Place;

    /// <summary>The enemy holding his body's place, whose sheet his flames draw from (#2558).</summary>
    public RoomEnemyOccupant BodyOccupant { get; }

    /// <summary>Gets the place his eye occupies.</summary>
    public RoomEnemyPlace Eye => EyePart.Place;

    /// <summary>Gets the place his tentacles occupy.</summary>
    public RoomEnemyPlace Tentacles => TentaclesPart.Place;

    /// <summary>Gets the place his mouth occupies.</summary>
    public RoomEnemyPlace Mouth => MouthPart.Place;

    /// <summary>
    /// The encounter phase his body runs; the encounter starts by spawning the flames that circle the room before he
    /// appears.
    /// </summary>
    public PhantoonPhase Phase { get; internal set; } = PhantoonPhase.SpawnStartingFlames;

    /// <summary>
    /// The wait the current timed phase runs (#2538): the phase's start gives it its duration, and the phase moves on on
    /// its last update; the Wrecked Ship's power starts to come on on its last.
    /// </summary>
    public BossCountdown PhaseWait { get; } = new();

    /// <summary>The pattern his eye-close animation's pattern choice picks next: his figure-eight, or the first flame rain.</summary>
    public PhantoonPatternChoice NextPattern { get; internal set; }

    /// <summary>What follows his vulnerable window when it closes: his eye closing, or a swoop once a shot hit him in it.</summary>
    public PhantoonWindowEnd WindowEnd { get; internal set; }

    /// <summary>The flames that circle the room before he appears.</summary>
    public PhantoonStartingFlamesState StartingFlames { get; } = new();

    /// <summary>His figure-eight.</summary>
    public PhantoonFigureEightState FigureEight { get; } = new();

    /// <summary>His swoop at Samus.</summary>
    public PhantoonSwoopState Swoop { get; } = new();

    /// <summary>The current round of his vulnerability.</summary>
    public PhantoonRoundState Round { get; } = new();

    /// <summary>The burst of casual flames his mouth spits.</summary>
    public PhantoonCasualFlameBurst CasualFlames { get; } = new();

    /// <summary>The flame waves his rage has sent, up to <see cref="PhantoonRageDefinitions.WaveCount"/>; each rage starts from none.</summary>
    public int RageWaveCount { get; internal set => field = Ensure.AtLeastZero(value); }

    /// <summary>How far the fade on his body has run.</summary>
    public PhantoonFadeProgress BodyFade { get; } = new();

    /// <summary>How far the Wrecked Ship's power coming on after his death has run, its own fade (#2251).</summary>
    public PhantoonFadeProgress ShipPowerOn { get; } = new();

    /// <summary>The wave his body ripples with.</summary>
    public PhantoonWaveState Wave { get; } = new();

    /// <summary>
    /// Whether his body shows while he is not translucent. Every part's start asks to hide it before
    /// <see cref="PhantoonBodyVisibilityEffect"/> first reads it.
    /// </summary>
    public PhantoonBodyVisibilityRequest BodyVisibilityRequest { get; internal set; } = PhantoonBodyVisibilityRequest.Hide;

    /// <summary>His death.</summary>
    public PhantoonDeathState Death { get; } = new();


    /// <summary>
    /// Whether Phantoon's body shows translucent; <see cref="PhantoonBodyVisibilityEffect"/> shows him so whatever
    /// <see cref="BodyVisibilityRequest"/> asks.
    /// </summary>
    public bool Translucent { get; internal set; }

    /// <summary>
    /// The sound his next materialization queues among <see cref="PhantoonSoundDefinitions.MaterializationSounds"/>: the
    /// first, then each in turn, the first again after the last (#2258).
    /// </summary>
    public ListStep<SoundEffectId> NextMaterializationSound { get; internal set; } =
        ListStep<SoundEffectId>.FirstOf(PhantoonSoundDefinitions.MaterializationSounds);

    /// <summary>Gets whether the final death step has published the persistent area-boss defeat flag.</summary>
    public bool BossDefeatPersisted { get; internal set; }

    /// <summary>Gets the encounter's lazily created wave of his body, which advances separately from display latching.</summary>
    public PhantoonBodyWaveEffect BodyWave => field ??= new();

    /// <summary>Gets the encounter's lazily created visibility effect: whether his body shows, hidden or translucent, live and as the display latched it with his body's pixelation.</summary>
    public PhantoonBodyVisibilityEffect BodyVisibility => field ??= new();
}
