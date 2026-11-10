namespace SuperMetroid.Core.Programs;

/// <summary>Where an animation stands (#1600): its phase, the phase's next beat, and how many plays of the phase remain.</summary>
/// <param name="Phase">The phase playing.</param>
/// <param name="Beat">The phase's next beat, counted from the phase's first; past its last, the phase's play is done.</param>
/// <param name="PlaysLeft">How many plays of the phase remain, this one included.</param>
internal readonly record struct AnimationPlace(int Phase, int Beat, int PlaysLeft)
{
    /// <summary>The start of phase <paramref name="phase"/>, with all <paramref name="plays"/> of it to come.</summary>
    internal static AnimationPlace StartOf(int phase, int plays) => new(phase, 0, plays);
}
