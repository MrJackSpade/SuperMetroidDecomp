using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>Crocomire's body state.</summary>
/// <remarks>
/// The game keeps the queued steps, the death-phase delay, the rumble step and the vertical acceleration in
/// one value; the reaction timer, the vertical speed and the skeleton speed's fraction in a second; and the
/// projectile counter, the vertical subpixel and the skeleton speed's whole pixels in a third (#1476). Each is held on
/// its own here because no phase's behaviour depends on what another meaning left behind: every phase starts its
/// values (the bridge collapse and each melt pass start the sink from rest, the pause loads its delay, the rumble its
/// first step, the skeleton its speed) except where one phase hands the same meaning to the next (a hop's sink
/// continues the rise's speed and acceleration). The one value a phase reads under another meaning is a rise's
/// acceleration, which starts as the death-phase delay the sink left: zero after a submerged pause, which counts its
/// delay out, but forty-eight 256ths after a melt, which skips the pause. A rise reads only the acceleration's whole
/// pixels, which the leftover fraction never reaches before the acceleration's ceiling discards it, and every melt's
/// rise is followed by a new sink from rest, so a rise here starts from no acceleration.
/// </remarks>
public sealed class CrocomireEnemyState
{
    /// <summary>Creates the state for Crocomire's body slot.</summary>
    internal CrocomireEnemyState(RoomEnemySlot body) => Body = body;

    /// <summary>Crocomire's body's and sprites' colours (#1512).</summary>
    internal CrocomireColours Colours { get; } = new();

    /// <summary>Physical body record owning the fight and death state and room-pixel coordinates; his body layer follows this actor.</summary>
    public RoomEnemySlot Body { get; }
    /// <summary>The body's playing animation and where it stands; before the first start, the initial animation.</summary>
    internal AnimationPlayback<CrocomireBodySegment> BodyPlayback { get; set; } =
        new(CrocomireBodyAnimations.StartingWith(CrocomireBodySegment.Initial), AnimationPlace.StartOfAnimation);

    /// <summary>The body frame shown last, or null before the first frame.</summary>
    internal CrocomireBodyFrame? BodyFrame { get; set; }

    /// <summary>
    /// The segment the body's animation rests in. Resting after the last beat of a segment that runs on into the next, it
    /// already rests in the next, as the body's animation position then lies inside it.
    /// </summary>
    internal CrocomireBodySegment BodyResidentAnimation =>
        ResidentSegment(BodyPlayback);

    /// <summary>The segment <paramref name="playback"/>, a body animation, rests in.</summary>
    internal static CrocomireBodySegment ResidentSegment(AnimationPlayback<CrocomireBodySegment> playback) =>
        AnimationResidence.ResidentSegment(CrocomireBodyAnimations.Segments, CrocomireBodyAnimations.StartingAt, CrocomireBodyAnimations.RunsOnInto, playback);

    /// <summary>
    /// The camera position the last main update saw: it decides whether his body is near enough the screen to show, the row
    /// below which his sinking hides it and where the defeated room pins it.
    /// </summary>
    internal Position16<RoomPixels> Camera { get; set; }

    /// <summary>The tongue's playing animation and where it stands; before the first start, the fight loop.</summary>
    internal AnimationPlayback<CrocomireTongueSegment> TonguePlayback { get; set; } =
        new(CrocomireAnimations.TongueStartingWith(CrocomireTongueSegment.Fight), AnimationPlace.StartOfAnimation);

    /// <summary>Physical tongue record, attached during initialization; bridge collapse sleeps and hides it, while later melting phases reuse its actor.</summary>
    public RoomEnemySlot? Tongue { get; internal set; }

    /// <summary>The phase of his death sequence; <see cref="CrocomireDeathPhase.Fighting"/> runs the fight and bridge checks, and <see cref="CrocomireDeathPhase.DefeatedRoomAdvance"/> displays an already-defeated corpse.</summary>
    public CrocomireDeathPhase DeathPhase { get; internal set; }

    /// <summary>Whether a mouth hit is waiting for the fight AI to start a backward step.</summary>
    public bool DamageReactionPending { get; internal set; }

    /// <summary>The fight's phase, run when the body animation's fight-AI cue fires rather than once per main-AI frame.</summary>
    public CrocomirePhase Phase { get; internal set; }

    /// <summary>
    /// During combat: queued backward steps. Producers store an authored count of at most three or add at most three per
    /// mouth hit while stepping back consumes them, so the count stays far
    /// inside <see cref="MaximumStepCount"/>. A store outside that range is rejected rather than wrapped, because the
    /// power-bomb charge's signed comparison is exact only inside it.
    /// </summary>
    public int StepCounter
    {
        get;
        internal set => field = Ensure.BetweenInclusive(0, MaximumStepCount, value);
    }

    /// <summary>The largest queued step count the fight AI's signed comparisons read as the game does.</summary>
    private const int MaximumStepCount = short.MaxValue;

    /// <summary>
    /// In the submerged pause and while the skeleton is bared behind the wall: the frames left in that death phase, counted down only while
    /// it is above zero.
    /// </summary>
    public int DeathPhaseDelay { get; internal set; }

    /// <summary>
    /// While the hidden wall rumbles: the step of the current target in <see cref="CrocomireRumbleDefinitions.All"/>,
    /// up to the schedule's length once its last target completes.
    /// </summary>
    public int RumbleStep
    {
        get;
        internal set => field = Ensure.BetweenInclusive(0, CrocomireRumbleDefinitions.All.Length, value);
    }

    /// <summary>
    /// While the body sinks or rises: the vertical acceleration per frame. The sink clamps its whole part at forty-eight
    /// pixels and the rise at thirty-one, and both start from none, so it never reaches the type's wrap.
    /// </summary>
    public SignedEightEight VerticalAcceleration { get; internal set; }

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
    /// While the body sinks or rises: the vertical position below a whole pixel carried between frames, in 256ths of a
    /// pixel, less than one pixel.
    /// </summary>
    public int VerticalSubpixel
    {
        get;
        internal set => field = Ensure.BetweenInclusive(0, EightEight.FromWhole(1) - 1, value);
    }

    /// <summary>
    /// While the skeleton breaks out and collapses: its 1/65536-pixel horizontal speed in room pixels per frame, zero when the
    /// skeleton breaks through the wall.
    /// </summary>
    internal SignedSixteenSixteen SkeletonVelocity { get; set; }

    /// <summary>
    /// Starts a sink from rest: clears the vertical speed and subpixel, then sets the starting acceleration of
    /// <paramref name="acceleration"/> per frame per frame.
    /// </summary>
    internal void StartVerticalMotion(SignedEightEight acceleration)
    {
        VerticalSpeed = SignedEightEight.Zero;
        VerticalSubpixel = 0;
        VerticalAcceleration = acceleration;
    }
}
