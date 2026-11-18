using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>Crocomire's body state.</summary>
/// <remarks>
/// Every phase starts its own values: the bridge collapse and each melt pass start the sink from rest, the pause loads
/// its delay, the rumble its first step and the skeleton its speed. Only a hop's sink carries on from the phase before
/// it, continuing the rise's speed and acceleration. A rise starts from no acceleration (#1476).
/// </remarks>
public sealed class CrocomireEnemyState : SuperMetroid.Core.Enemies.BossBodies.IBossBodyShowing
{
    /// <inheritdoc />
    Position16<RoomPixels>? SuperMetroid.Core.Enemies.BossBodies.IBossBodyShowing.BodyPlacement => Body.Position.Whole;

    /// <inheritdoc />
    bool SuperMetroid.Core.Enemies.BossBodies.IBossBodyShowing.BodyHidden => false;

    /// <summary>Creates the state for Crocomire's body place.</summary>
    internal CrocomireEnemyState(RoomEnemyPlace body) => Body = body;

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
        new(CrocomireBodyAnimations.StartingWith(CrocomireBodySegment.IdleSway), AnimationPlace.StartOfAnimation);

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

    /// <summary>The enemy that joined as his tongue, or none before the tongue set up.</summary>
    private RoomEnemyPlace? _tongue;

    /// <summary>
    /// Crocomire's tongue (#2400), which joins the encounter as it sets up, right after his body, in a living room and a
    /// defeated one alike; bridge collapse rests and hides it, while later melting phases reuse its actor.
    /// </summary>
    /// <exception cref="InvalidOperationException">The tongue has not set up.</exception>
    public RoomEnemyPlace Tongue => _tongue ?? throw new InvalidOperationException("Crocomire's tongue is reached before it set up.");

    /// <summary>Joins <paramref name="tongue"/> as his tongue, as it sets up.</summary>
    /// <exception cref="InvalidDataException">Another enemy already set up as his tongue.</exception>
    internal void JoinTongue(RoomEnemyPlace tongue)
    {
        Ensure.NotNull(tongue);
        if (_tongue is { } joined && !ReferenceEquals(joined, tongue))
            throw new InvalidDataException($"Crocomire's tongue set up as enemy {tongue.Ordinal}, but enemy {joined.Ordinal} already is.");
        _tongue = tongue;
    }

    /// <summary>The phase of his death sequence; <see cref="CrocomireDeathPhase.Fighting"/> runs the fight and bridge checks, and <see cref="CrocomireDeathPhase.InertCorpse"/> is a corpse, an already-defeated room's from its setup.</summary>
    public CrocomireDeathPhase DeathPhase { get; internal set; }

    /// <summary>Whether a mouth hit is waiting for the fight AI to start a backward step.</summary>
    public bool DamageReactionPending { get; internal set; }

    /// <summary>
    /// The fight's phase, run when the body animation's fight-AI cue fires rather than once per main-AI frame (#1939). The
    /// fight starts dormant; a defeated room's corpse never runs it.
    /// </summary>
    public CrocomirePhase Phase { get; internal set; } = CrocomirePhase.Dormant;

    /// <summary>
    /// During combat: queued backward steps. Producers store an authored count of at most three or add at most three per
    /// mouth hit while stepping back consumes them, so the count stays a few steps from none and never nears a wrap
    /// (#1940).
    /// </summary>
    public int StepCounter
    {
        get;
        internal set => field = Ensure.AtLeastZero(value);
    }

    /// <summary>
    /// In the submerged pause and while the skeleton is bared behind the wall: the frames left in that death phase, counted down only while
    /// it is above zero.
    /// </summary>
    public int DeathPhaseDelay { get; internal set; }

    /// <summary>
    /// While the hidden wall rumbles: the oscillation under way in <see cref="CrocomireRumbleDefinitions.Oscillations"/>,
    /// up to their count once the last has finished.
    /// </summary>
    public int RumbleOscillation
    {
        get;
        internal set => field = Ensure.BetweenInclusive(0, CrocomireRumbleDefinitions.Oscillations.Length, value);
    }

    /// <summary>Whether the rumbling wall is heading for its oscillation's extreme rather than back to its return offset.</summary>
    public bool RumbleTowardExtreme { get; internal set; }

    /// <summary>
    /// While the body sinks or rises: the vertical acceleration, in pixels an update an update (#2299). The sink stops its
    /// 256ths at forty-eight and the rise at thirty-one, and both start from none.
    /// </summary>
    public SignedSixteenSixteen VerticalAcceleration { get; internal set; }

    /// <summary>
    /// While the body sinks or rises: the vertical speed per frame, down while sinking and up while rising. The sink clamps
    /// its whole part at three pixels and the rise stops it at zero, so it stays below four pixels per frame.
    /// </summary>
    public SignedEightEight VerticalSpeed { get; internal set; }

    /// <summary>
    /// During the projectile attack: how many of the volley's shots Crocomire has spat, cleared when the attack starts and
    /// counted up to <see cref="CrocomireProjectileDefinitions.VolleyShotCount"/>.
    /// </summary>
    public int ShotsSpat { get; internal set; }

    /// <summary>
    /// While the body sinks or rises: the part of a pixel his travel has carried between updates, less than one pixel
    /// (#2181). The sinks and rises keep it apart from his position's own fraction, which they never move and his
    /// skeleton's later arc moves from.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The value set is negative or reaches a whole pixel.</exception>
    public SignedEightEight VerticalFraction
    {
        get;
        internal set => field = value.IsNegative || value.WholePixels != 0
            ? throw new ArgumentOutOfRangeException(nameof(value), value, "Crocomire's carried vertical fraction is less than one pixel and never negative.")
            : value;
    }

    /// <summary>
    /// While the skeleton breaks out and collapses: its 1/65536-pixel horizontal speed in room pixels per frame, zero when the
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
        VerticalFraction = SignedEightEight.Zero;
        VerticalAcceleration = acceleration;
    }
}
