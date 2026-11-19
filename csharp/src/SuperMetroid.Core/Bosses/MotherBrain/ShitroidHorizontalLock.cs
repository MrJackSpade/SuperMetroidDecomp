namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// The Shitroid's horizontal lock on its target (#2561): it builds by <see cref="ShitroidFightDefinitions.LockRise"/> on each
/// update the Shitroid keeps within reach of its target's X and decays by <see cref="ShitroidFightDefinitions.LockDecay"/>,
/// never below none, on each update it strays; its hover chases Samus and its follow leaves once the lock has built to
/// their thresholds.
/// </summary>
public sealed class ShitroidHorizontalLock
{
    /// <summary>How far the lock has built: none until the Shitroid first keeps within reach.</summary>
    [IntegerQuantity("how far the lock has built")]
    public int Built { get; private set; }

    /// <summary>Builds or decays the lock for one update.</summary>
    /// <param name="withinReach">Whether the Shitroid keeps within reach of its target's X on this update.</param>
    internal void Update(bool withinReach) =>
        Built = withinReach
            ? Built + ShitroidFightDefinitions.LockRise
            : Math.Max(0, Built - ShitroidFightDefinitions.LockDecay);

    /// <summary>Whether the lock has built to <paramref name="threshold"/>.</summary>
    internal bool HasBuiltTo(int threshold) => Built >= threshold;
}
