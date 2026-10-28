namespace SuperMetroid.Core.Enemies.PipeBugs;

/// <summary>The Norfair Pipe Bug formation's release counters and movement roles, by formation slot.</summary>
internal static class PipeBugFormation
{
    /// <summary>The release counter of formation slot <paramref name="member"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="member"/> is outside the five slots.</exception>
    internal static ushort NorfairStaggerTarget(int member) =>
        (uint)member < (uint)PipeBugDefinitions.NorfairStaggerTargets.Length
            ? PipeBugDefinitions.NorfairStaggerTargets[member]
            : throw new ArgumentOutOfRangeException(nameof(member));

    /// <summary>The movement function of formation slot <paramref name="member"/> after it rises.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="member"/> is outside the five slots.</exception>
    internal static PipeBugEnemyFunction NorfairPostRiseFunction(int member) =>
        (uint)member < (uint)PipeBugDefinitions.NorfairPostRiseFunctions.Length
            ? PipeBugDefinitions.NorfairPostRiseFunctions[member]
            : throw new ArgumentOutOfRangeException(nameof(member));
}
