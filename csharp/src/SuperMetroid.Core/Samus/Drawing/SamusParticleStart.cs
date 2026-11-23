namespace SuperMetroid.Core.Samus.Drawing;

/// <summary>
/// How one of Samus's particles begins: shown at once for a number of updates on its current animation step, or hidden for
/// a number of updates and then shown, on the last of them, for its step's full duration.
/// </summary>
public abstract partial record SamusParticleStart
{
    /// <summary>Only the two starts derive from it (#2560).</summary>
    private SamusParticleStart()
    {
    }

    /// <summary>A particle shown at once, its current step lasting <paramref name="stepUpdates"/> updates.</summary>
    /// <param name="stepUpdates">One or more.</param>
    public static SamusParticleStart Shown(int stepUpdates) => new ShownAtOnce(stepUpdates > 0
        ? stepUpdates
        : throw new ArgumentOutOfRangeException(nameof(stepUpdates), stepUpdates, "A step shows for one update or more."));

    /// <summary>
    /// A particle hidden until its <paramref name="updates"/>th update, which reloads its step's duration and shows it.
    /// </summary>
    /// <param name="updates">One or more.</param>
    public static SamusParticleStart Hidden(int updates) => new HiddenFirst(updates > 0
        ? updates
        : throw new ArgumentOutOfRangeException(nameof(updates), updates, "A hidden start lasts one update or more."));

    /// <summary>A particle shown at once.</summary>
    /// <param name="StepUpdates">The updates its current step shows.</param>
    public sealed partial record ShownAtOnce(int StepUpdates) : SamusParticleStart;

    /// <summary>A particle that waits hidden before it shows.</summary>
    /// <param name="Updates">The updates it waits, counting the update it first shows on.</param>
    public sealed partial record HiddenFirst(int Updates) : SamusParticleStart;
}
