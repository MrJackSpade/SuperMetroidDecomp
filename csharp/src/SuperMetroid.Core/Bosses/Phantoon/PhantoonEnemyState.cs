using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms.RoomObjects;
namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>
/// State shared by Phantoon's four parts: body, eye, tentacles and mouth. Each part is its own enemy, attached by
/// reference when it initializes, and keeps its own typed state here, because the parts' behaviour reads one another's
/// values, and keeping the values per part keeps those relationships visible.
/// </summary>
public sealed class PhantoonEnemyState
{
    /// <summary>Attaches shared encounter state to the body's slot.</summary>
    internal PhantoonEnemyState(RoomEnemySlot body) => Body = body;

    /// <summary>Phantoon's body's and his room's colours (#1512).</summary>
    internal PhantoonColours Colours { get; } = new();

    /// <summary>Gets the body, owner of the phases, health, motion, and shared encounter values; it runs first.</summary>
    public RoomEnemySlot Body { get; }

    /// <summary>The body's own state.</summary>
    public PhantoonBodyState BodyState { get; } = new();

    /// <summary>The eye's own state.</summary>
    public PhantoonEyeState EyeState { get; } = new();

    /// <summary>The tentacles' own state.</summary>
    public PhantoonTentaclesState TentaclesState { get; } = new();

    /// <summary>The mouth's own state, which every part's initialization also writes.</summary>
    public PhantoonMouthState MouthState { get; } = new();

    /// <summary>The eye, once attached (<see cref="Eye"/>).</summary>
    private RoomEnemySlot? _eye;

    /// <summary>The tentacles, once attached (<see cref="Tentacles"/>).</summary>
    private RoomEnemySlot? _tentacles;

    /// <summary>The mouth, once attached (<see cref="Mouth"/>).</summary>
    private RoomEnemySlot? _mouth;

    /// <summary>Gets the eye, attached by part initialization and sharing the body's position while retaining its own animation/palette counters.</summary>
    public RoomEnemySlot Eye { get => _eye ?? throw new InvalidOperationException("Phantoon's eye part is not attached; part initialization attaches it."); internal set => _eye = value; }
    /// <summary>Gets the tentacles, attached by part initialization and positioned from the body after its AI pass.</summary>
    public RoomEnemySlot Tentacles { get => _tentacles ?? throw new InvalidOperationException("Phantoon's tentacle part is not attached; part initialization attaches it."); internal set => _tentacles = value; }
    /// <summary>Gets the mouth, attached by part initialization and also retaining the wave/blending controls.</summary>
    public RoomEnemySlot Mouth { get => _mouth ?? throw new InvalidOperationException("Phantoon's mouth part is not attached; part initialization attaches it."); internal set => _mouth = value; }

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
    internal ref AnimationPlayback<PhantoonPartSegment> PartPlayback(RoomEnemySlot part)
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
        throw new InvalidOperationException($"Enemy {part.DefinitionId} in slot {part.SlotIndex} is not one of Phantoon's parts.");
    }

    /// <summary>
    /// Where Phantoon's body layer is placed, written after the body has moved: camera minus body plus forty whole
    /// pixels on each axis when ordinary synchronization runs. His room shows his body layer as its background layer.
    /// </summary>
    public Position16<BackgroundPixels> BodyLayerPlacement { get; internal set; }

    /// <summary>
    /// Whether Phantoon's body shows translucent; <see cref="PhantoonBodyVisibilityEffect"/> shows him so whatever his
    /// mouth's <see cref="PhantoonMouthState.BodyLayer"/> asks.
    /// </summary>
    public bool Translucent { get; internal set; }

    /// <summary>Position zero through two in the repeating three-sound materialization cycle; the next materialization plays this one and advances it.</summary>
    public int MaterializationSoundIndex { get; internal set; }

    /// <summary>
    /// Gets the live pixelation block size of his body in screen pixels before display latching: one (no quantization) until
    /// the dying fade grows it one pixel every sixteenth frame up to sixteen, and one again once the body is cleared.
    /// </summary>
    public int MosaicBlockSize { get; internal set; } = 1;

    /// <summary>Gets whether the final death step has published the persistent area-boss defeat flag.</summary>
    public bool BossDefeatPersisted { get; internal set; }

    /// <summary>Gets the encounter's lazily created wave of his body, which advances separately from display latching.</summary>
    public PhantoonBodyWaveEffect BodyWave => field ??= new();

    /// <summary>Gets the encounter's lazily created visibility effect: whether his body shows, hidden or translucent, live and as the display latched it with his body's pixelation.</summary>
    public PhantoonBodyVisibilityEffect BodyVisibility => field ??= new();
}
