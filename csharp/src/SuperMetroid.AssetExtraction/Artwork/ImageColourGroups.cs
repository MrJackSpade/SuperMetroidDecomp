using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>
/// The colour groups a composed sheet's frame uses (#1464): those its pixels were coloured from, which the import uses to
/// decide which pictures can share a sheet's colouring. The installed sheet does not keep them (#1544).
/// </summary>
public readonly partial record struct ImageColourGroups
{
    /// <summary>The members of the set of every group, one bit per group.</summary>
    internal const int EveryMember = (1 << ImageColourGroup.Count) - 1;

    /// <summary>One bit per group the set holds, at the group's place.</summary>
    private readonly int members;

    /// <summary>The set whose members are <paramref name="members"/>, one bit per group at the group's place.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The number names a group past the half's.</exception>
    internal ImageColourGroups(int members) => this.members = Ensure.BetweenInclusive(0, EveryMember, members);

    /// <summary>The set's members, one bit per group at the group's place, for the set's own operations.</summary>
    internal int Members => members;
}
