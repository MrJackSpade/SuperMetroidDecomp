namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>
/// How many updates a <see cref="DachoraStride"/> shows each of its six poses for (#2553): a pace names one count per pose,
/// so a stride is never timed by more or fewer counts than it has poses.
/// </summary>
/// <param name="FarLegBack">The updates the far leg's back pose shows.</param>
/// <param name="FarLegPassing">The updates the far leg's passing pose shows.</param>
/// <param name="FarLegReaching">The updates the far leg's reaching pose shows.</param>
/// <param name="NearLegBack">The updates the near leg's back pose shows.</param>
/// <param name="NearLegPassing">The updates the near leg's passing pose shows.</param>
/// <param name="NearLegReaching">The updates the near leg's reaching pose shows.</param>
internal sealed record DachoraStridePace(int FarLegBack, int FarLegPassing, int FarLegReaching, int NearLegBack, int NearLegPassing, int NearLegReaching)
{
    /// <summary>A pace showing every pose for <paramref name="updates"/> updates.</summary>
    internal static DachoraStridePace Even(int updates) => new(updates, updates, updates, updates, updates, updates);

    /// <summary>The counts in the order the stride shows its poses (<see cref="DachoraStride.Poses"/>).</summary>
    internal IReadOnlyList<int> Updates => [FarLegBack, FarLegPassing, FarLegReaching, NearLegBack, NearLegPassing, NearLegReaching];
}
