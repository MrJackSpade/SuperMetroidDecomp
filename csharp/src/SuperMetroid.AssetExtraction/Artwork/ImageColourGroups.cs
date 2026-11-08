using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>
/// The colour groups a composed sheet's frame uses (#1464): those its pixels were coloured from, which the import uses to
/// decide which pictures can share a sheet's colouring. The installed sheet does not keep them (#1544).
/// </summary>
/// <remarks>
/// A composed frame document stores the set as one number with a bit per group's place; <see cref="ManifestMembers"/>
/// gives it back.
/// </remarks>
public readonly record struct ImageColourGroups
{
    /// <summary>The highest number a frame document stores a set as: every group.</summary>
    internal const int EveryMember = (1 << ImageColourGroup.Count) - 1;

    /// <summary>One bit per group the set holds, at the group's place.</summary>
    private readonly int members;

    /// <summary>The set a frame document stores as <paramref name="members"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The number names a group past the half's.</exception>
    internal ImageColourGroups(int members) => this.members = Ensure.BetweenInclusive(0, EveryMember, members);

    /// <summary>The number a frame document stores the set as.</summary>
    internal int ManifestMembers => members;
}
