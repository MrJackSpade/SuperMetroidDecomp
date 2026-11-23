namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// The Baby Metroid's horizontal lock on its target (#2561): it builds by <see cref="BabyMetroidFightDefinitions.LockRise"/> on each
/// update the Baby Metroid keeps within reach of its target's X and decays by <see cref="BabyMetroidFightDefinitions.LockDecay"/>,
/// never below none, on each update it strays; its hover chases Samus and its follow leaves once the lock has built to
/// their thresholds.
/// </summary>
public sealed partial class BabyMetroidHorizontalLock
{
    /// <summary>How far the lock has built: none until the Baby Metroid first keeps within reach.</summary>
    public int Built { get; internal set; }

    /// <summary>Builds or decays the lock for one update.</summary>
    /// <param name="withinReach">Whether the Baby Metroid keeps within reach of its target's X on this update.</param>
    internal void Update(bool withinReach) =>
        Built = withinReach
            ? Built + BabyMetroidFightDefinitions.LockRise
            : Math.Max(0, Built - BabyMetroidFightDefinitions.LockDecay);

    /// <summary>Whether the lock has built to <paramref name="threshold"/>.</summary>
    internal bool HasBuiltTo(int threshold) => Built >= threshold;
}
