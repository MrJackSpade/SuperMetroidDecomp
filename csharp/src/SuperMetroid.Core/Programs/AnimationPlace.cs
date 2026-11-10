namespace SuperMetroid.Core.Programs;

/// <summary>Where an animation stands (#1600): its phase, and the phase's next beat.</summary>
/// <param name="Phase">The phase playing.</param>
/// <param name="Beat">The phase's next beat, counted from the phase's first; past its last, the phase's play is done.</param>
internal readonly record struct AnimationPlace(int Phase, int Beat)
{
    /// <summary>The start of phase <paramref name="phase"/>.</summary>
    internal static AnimationPlace StartOf(int phase) => new(phase, 0);
}
