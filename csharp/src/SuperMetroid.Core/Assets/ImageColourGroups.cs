namespace SuperMetroid.Core.Assets;

/// <summary>
/// The colour groups an installed image's frame uses (#1464): those its pixels were coloured from, so the groups whose
/// shown colours a plain draw of it resolves.
/// </summary>
/// <remarks>
/// The manifest stores the set as one number with a bit per group's place, which the sheet's frame document holds; the
/// import pipeline builds a set from that number and <see cref="ManifestMembers"/> gives it back. Nothing else reads the
/// bits.
/// </remarks>
public readonly record struct ImageColourGroups
{
    /// <summary>The highest number the manifest stores a set as: every group.</summary>
    internal const int EveryMember = (1 << ImageColourGroup.Count) - 1;

    /// <summary>One bit per group the set holds, at the group's place.</summary>
    private readonly int members;

    /// <summary>The set the manifest stores as <paramref name="members"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The number names a group past the image's.</exception>
    internal ImageColourGroups(int members) => this.members = Ensure.BetweenInclusive(0, EveryMember, members);

    /// <summary>The number the manifest stores the set as.</summary>
    internal int ManifestMembers => members;
}
