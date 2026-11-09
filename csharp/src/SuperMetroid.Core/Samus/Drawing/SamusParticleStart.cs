namespace SuperMetroid.Core.Samus.Drawing;

/// <summary>
/// How one of Samus's particles begins: shown at once for a number of updates on its current animation step, or hidden for
/// a number of updates and then shown, on the last of them, for its step's full duration.
/// </summary>
public readonly record struct SamusParticleStart
{
    /// <summary>The updates the current step shows when the particle is shown at once; zero when it starts hidden.</summary>
    public int StepUpdates { get; private init; }

    /// <summary>
    /// The updates the particle waits before it shows, counting the update it first shows on; zero when it shows at once.
    /// </summary>
    public int HiddenUpdates { get; private init; }

    /// <summary>A particle shown at once, its current step lasting <paramref name="stepUpdates"/> updates.</summary>
    /// <param name="stepUpdates">One or more.</param>
    public static SamusParticleStart Shown(int stepUpdates) => new()
    {
        StepUpdates = stepUpdates > 0
            ? stepUpdates
            : throw new ArgumentOutOfRangeException(nameof(stepUpdates), stepUpdates, "A step shows for one update or more."),
    };

    /// <summary>
    /// A particle hidden until its <paramref name="updates"/>th update, which reloads its step's duration and shows it.
    /// </summary>
    /// <param name="updates">One or more.</param>
    public static SamusParticleStart Hidden(int updates) => new()
    {
        HiddenUpdates = updates > 0
            ? updates
            : throw new ArgumentOutOfRangeException(nameof(updates), updates, "A hidden start lasts one update or more."),
    };
}
