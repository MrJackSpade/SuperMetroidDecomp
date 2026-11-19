namespace SuperMetroid.Core.Animation;

/// <summary>
/// Where an animation stands (#1600): its phase, the phase's next beat, and, in a repeated phase, the plays the phase has
/// left (#2006).
/// </summary>
/// <param name="Phase">The phase playing.</param>
/// <param name="Beat">
/// The phase's next beat, counted from the phase's first; past its last, the phase's play is done. Before its first
/// (<see cref="HasNotEntered"/>), the play has not reached the phase yet, and starts counting its plays when it does. One
/// past its last (<see cref="HoldsIn"/>), the animation holds there: its play is done for good and it rests on its last
/// frame.
/// </param>
internal readonly record struct AnimationPlace(int Phase, int Beat)
{
    /// <summary>
    /// The beat counter of a phase the play has not entered yet, one before its first beat: the playback counts beats from
    /// it, so entering the phase is the count's step to the first (#2558). Callers ask <see cref="HasNotEntered"/>.
    /// </summary>
    private const int BeforeFirstBeat = -1;

    /// <summary>Whether the play has not entered the phase yet: it stands before the phase's first beat.</summary>
    internal bool HasNotEntered => Beat == BeforeFirstBeat;

    /// <summary>
    /// Whether the animation holds in the phase, of <paramref name="beatCount"/> beats: its play done for good, resting on
    /// its last frame, where <see cref="HoldingIn"/> leaves it.
    /// </summary>
    internal bool HoldsIn(int beatCount) => Beat > beatCount;

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
    internal static AnimationPlace HoldingIn(int phase, int beatCount) => new(phase, beatCount + 1);

    /// <summary>Before phase <paramref name="phase"/>, which the next play enters.</summary>
    internal static AnimationPlace Before(int phase) => new(phase, BeforeFirstBeat);

    /// <summary>This place, with <paramref name="playsLeft"/> plays of its repeated phase left, the one playing included.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="playsLeft"/> is negative.</exception>
    internal AnimationPlace WithPlaysLeft(int playsLeft) => this with { PlaysLeft = Ensure.AtLeastZero(playsLeft) };
}
