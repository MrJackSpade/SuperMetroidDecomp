namespace SuperMetroid.Core.Animation;

/// <summary>
/// Where an animation stands (#1600): its phase; whether the play has not entered the phase yet, plays it, or holds in it;
/// the beats of the phase it has passed; and, in a repeated phase, the plays the phase has left (#2006).
/// </summary>
internal readonly partial record struct AnimationPlace
{
    /// <summary>
    /// Playing phase <paramref name="phase"/>, its next beat <paramref name="beat"/>, counted from the phase's first; past
    /// its last, the phase's play is done.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="beat"/> is negative.</exception>
    internal AnimationPlace(int phase, int beat)
        : this(phase, AnimationPlaceStage.Playing, Ensure.AtLeastZero(beat))
    {
    }

    /// <summary>Standing in phase <paramref name="phase"/> at <paramref name="stage"/>, its first <paramref name="beat"/> beats passed.</summary>
    private AnimationPlace(int phase, AnimationPlaceStage stage, int beat)
    {
        Phase = phase;
        Stage = stage;
        Beat = beat;
    }

    /// <summary>The phase the animation stands in.</summary>
    internal int Phase { get; }

    /// <summary>Whether the play has not entered the phase yet, plays it, or holds in it.</summary>
    private AnimationPlaceStage Stage { get; }

    /// <summary>
    /// The beats of the phase passed: while the phase plays, its next beat counted from its first; none before the play
    /// enters it; and every one of them where the animation holds in it.
    /// </summary>
    internal int Beat { get; }

    /// <summary>Whether the play has not entered the phase yet, and starts counting its plays when it does.</summary>
    internal bool HasNotEntered => Stage == AnimationPlaceStage.BeforePhase;

    /// <summary>Whether the animation holds in the phase: its play is done for good and it rests on its last frame.</summary>
    internal bool IsHolding => Stage == AnimationPlaceStage.Holding;

    /// <summary>
    /// The plays a repeated phase has left, the one playing included: entering the phase sets them to its plays, and the end
    /// of each play counts one off and replays the phase while any are left. Zero in a phase played once, which counts
    /// nothing, before a phase is entered and where the animation holds. Only the playback counts them.
    /// </summary>
    internal int PlaysLeft { get; private init; }

    /// <summary>The start of phase <paramref name="phase"/>, entered.</summary>
    internal static AnimationPlace StartOf(int phase) => new(phase, 0);

    /// <summary>
    /// The start of any animation: before its first phase, which its first play enters, counting that phase's plays then
    /// when it is repeated.
    /// </summary>
    internal static AnimationPlace StartOfAnimation => Before(0);

    /// <summary>Holding in phase <paramref name="phase"/>, of <paramref name="beatCount"/> beats: its play done for good.</summary>
    internal static AnimationPlace HoldingIn(int phase, int beatCount) => new(phase, AnimationPlaceStage.Holding, beatCount);

    /// <summary>Before phase <paramref name="phase"/>, which the next play enters.</summary>
    internal static AnimationPlace Before(int phase) => new(phase, AnimationPlaceStage.BeforePhase, 0);

    /// <summary>This place, with <paramref name="playsLeft"/> plays of its repeated phase left, the one playing included.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="playsLeft"/> is negative.</exception>
    internal AnimationPlace WithPlaysLeft(int playsLeft) => this with { PlaysLeft = Ensure.AtLeastZero(playsLeft) };
}
