using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies;
namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>
/// Phantoon's encounter: his four parts, body, eye, tentacles and mouth, each its own enemy attached by reference when it
/// initializes and keeping only its animation here, and the fight's state grouped by what it does (#1933): the phase and
/// its timer, the figure-eight, the swoop, the round of vulnerability, the casual flames, the fade on his body, the wave,
/// whether his body shows, and his death.
/// </summary>
public sealed class PhantoonEnemyState
{
    /// <summary>Attaches shared encounter state to the body's place.</summary>
    internal PhantoonEnemyState(RoomEnemyPlace body) => Body = body;

    /// <summary>Phantoon's body's and his room's colours (#1512).</summary>
    internal PhantoonColours Colours { get; } = new();

    /// <summary>Gets the body, whose health and position are his; it runs first.</summary>
    public RoomEnemyPlace Body { get; }

    /// <summary>
    /// The encounter phase his body runs, or null while it runs none: while his eye's opening or closing animation
    /// decides what comes next, the body does nothing on its turn (#1870).
    /// </summary>
    public PhantoonPhase? Phase { get; internal set; }

    /// <summary>The updates before the current phase moves on.</summary>
    public int PhaseTimer { get; internal set; }

    /// <summary>Whether the eye-close animation's pattern choice takes the flame-rain branch.</summary>
    public bool FlameRainRequested { get; internal set; }

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

    /// <summary>The flame waves his rage has sent, zero through eight; each rage starts from none.</summary>
    public int RageWaveCount { get; internal set => field = Ensure.AtLeastZero(value); }

    /// <summary>How far the fade on his body has run.</summary>
    public PhantoonBodyFadeProgress BodyFade { get; } = new();

    /// <summary>The wave his body ripples with.</summary>
    public PhantoonWaveState Wave { get; } = new();

    /// <summary>
    /// Whether his body shows while he is not translucent. Every part's start asks to hide it before
    /// <see cref="PhantoonBodyVisibilityEffect"/> first reads it.
    /// </summary>
    public PhantoonBodyVisibilityRequest BodyVisibilityRequest { get; internal set; } = PhantoonBodyVisibilityRequest.Hide;

    /// <summary>His death.</summary>
    public PhantoonDeathState Death { get; } = new();

    /// <summary>The eye, once attached (<see cref="Eye"/>).</summary>
    private RoomEnemyPlace? _eye;

    /// <summary>The tentacles, once attached (<see cref="Tentacles"/>).</summary>
    private RoomEnemyPlace? _tentacles;

    /// <summary>The mouth, once attached (<see cref="Mouth"/>).</summary>
    private RoomEnemyPlace? _mouth;

    /// <summary>Gets the eye, attached by part initialization and sharing the body's position while retaining its own animation/colour counters.</summary>
    public RoomEnemyPlace Eye { get => _eye ?? throw new InvalidOperationException("Phantoon's eye part is not attached; part initialization attaches it."); internal set => _eye = value; }
    /// <summary>Gets the tentacles, attached by part initialization and positioned from the body after its AI pass.</summary>
    public RoomEnemyPlace Tentacles { get => _tentacles ?? throw new InvalidOperationException("Phantoon's tentacle part is not attached; part initialization attaches it."); internal set => _tentacles = value; }
    /// <summary>Gets the mouth, attached by part initialization and positioned from the body after its AI pass.</summary>
    public RoomEnemyPlace Mouth { get => _mouth ?? throw new InvalidOperationException("Phantoon's mouth part is not attached; part initialization attaches it."); internal set => _mouth = value; }

    /// <summary>
    /// The body's animation playback (#641, #1600). Every part's playback is the invulnerable body's until the part's
    /// initialization installs its own.
    /// </summary>
    private AnimationPlayback<PhantoonPartSegment> _bodyPlayback = Starting(PhantoonPartSegment.InvulnerableBody);

    /// <summary>The eye's animation playback.</summary>
    private AnimationPlayback<PhantoonPartSegment> _eyePlayback = Starting(PhantoonPartSegment.InvulnerableBody);

    /// <summary>The tentacles' animation playback.</summary>
    private AnimationPlayback<PhantoonPartSegment> _tentaclesPlayback = Starting(PhantoonPartSegment.InvulnerableBody);

    /// <summary>The mouth's animation playback.</summary>
    private AnimationPlayback<PhantoonPartSegment> _mouthPlayback = Starting(PhantoonPartSegment.InvulnerableBody);

    /// <summary>The animation starting at <paramref name="segment"/>, before its first play.</summary>
    internal static AnimationPlayback<PhantoonPartSegment> Starting(PhantoonPartSegment segment) =>
        new(PhantoonAnimations.StartingWith(segment), AnimationPlace.StartOfAnimation);

    /// <summary>The animation playback of <paramref name="part"/>, which is Phantoon's body or one of his attached parts.</summary>
    /// <exception cref="InvalidOperationException"><paramref name="part"/> is not one of Phantoon's parts.</exception>
    internal ref AnimationPlayback<PhantoonPartSegment> PartPlayback(RoomEnemyPlace part)
    {
        Ensure.NotNull(part);
        if (ReferenceEquals(part, Body))
            return ref _bodyPlayback;
        if (ReferenceEquals(part, _eye))
            return ref _eyePlayback;
        if (ReferenceEquals(part, _tentacles))
            return ref _tentaclesPlayback;
        if (ReferenceEquals(part, _mouth))
            return ref _mouthPlayback;
        throw new InvalidOperationException($"Enemy {part.DefinitionId} in place {part.Ordinal} is not one of Phantoon's parts.");
    }

    /// <summary>
    /// Whether Phantoon's body shows translucent; <see cref="PhantoonBodyVisibilityEffect"/> shows him so whatever
    /// <see cref="BodyVisibilityRequest"/> asks.
    /// </summary>
    public bool Translucent { get; internal set; }

    /// <summary>Position zero through two in the repeating three-sound materialization cycle; the next materialization plays this one and advances it.</summary>
    public int MaterializationSoundIndex { get; internal set; }

    /// <summary>
    /// Gets the live pixelation block size of his body in screen pixels before display latching: one (no quantization) until
    /// the dying fade grows it one pixel every sixteenth frame up to sixteen, and one again once the body is cleared.
    /// </summary>
    public int PixelationBlockSize { get; internal set; } = 1;

    /// <summary>Gets whether the final death step has published the persistent area-boss defeat flag.</summary>
    public bool BossDefeatPersisted { get; internal set; }

    /// <summary>Gets the encounter's lazily created wave of his body, which advances separately from display latching.</summary>
    public PhantoonBodyWaveEffect BodyWave => field ??= new();

    /// <summary>Gets the encounter's lazily created visibility effect: whether his body shows, hidden or translucent, live and as the display latched it with his body's pixelation.</summary>
    public PhantoonBodyVisibilityEffect BodyVisibility => field ??= new();
}
