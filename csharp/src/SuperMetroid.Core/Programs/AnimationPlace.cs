namespace SuperMetroid.Core.Programs;

/// <summary>Where an animation stands (#1600): its phase, and the phase's next beat.</summary>
/// <param name="Phase">The phase playing.</param>
/// <param name="Beat">
/// The phase's next beat, counted from the phase's first; past its last, the phase's play is done. Before its first,
/// <see cref="NotEntered"/>: the play has not reached the phase yet, and loads its repeats when it does.
/// </param>
internal readonly record struct AnimationPlace(int Phase, int Beat)
{
    /// <summary>The beat of a phase the play has not entered yet.</summary>
    internal const int NotEntered = -1;

    /// <summary>The start of phase <paramref name="phase"/>, entered.</summary>
    internal static AnimationPlace StartOf(int phase) => new(phase, 0);

    /// <summary>
    /// The start of any animation: before its first phase, which its first play enters, loading the owner's repeats then
    /// when that phase is played more than once.
    /// </summary>
    internal static AnimationPlace StartOfAnimation => Before(0);

    /// <summary>Before phase <paramref name="phase"/>, which the next play enters.</summary>
    internal static AnimationPlace Before(int phase) => new(phase, NotEntered);
}
