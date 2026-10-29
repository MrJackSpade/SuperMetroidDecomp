using System.Text.Json.Serialization;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// The colour groups an installed image's frame uses (#1464): those its pixels were coloured from, so the groups whose
/// shown colours a plain draw of it resolves.
/// </summary>
/// <remarks>
/// The manifest stores the set as one number with a bit per group's place, which
/// <see cref="ImageColourGroupsJsonConverter"/> reads and writes; nothing else reads the bits.
/// </remarks>
[JsonConverter(typeof(ImageColourGroupsJsonConverter))]
public readonly record struct ImageColourGroups
{
    /// <summary>The highest number the manifest stores a set as: every group.</summary>
    internal const int EveryMember = (1 << ImageColourGroup.Count) - 1;

    /// <summary>One bit per group the set holds, at the group's place.</summary>
    private readonly int members;

    /// <summary>Keeps the groups whose bits <paramref name="members"/> sets.</summary>
    private ImageColourGroups(int members) => this.members = members;

    /// <summary>No group.</summary>
    public static ImageColourGroups None => default;

    /// <summary>The set the manifest stores as <paramref name="members"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The number names a group past the image's.</exception>
    internal static ImageColourGroups FromManifest(int members) => new(Ensure.BetweenInclusive(0, EveryMember, members));

    /// <summary>The number the manifest stores the set as.</summary>
    internal int ManifestMembers => members;

    /// <summary>Whether the set holds <paramref name="group"/>.</summary>
    public bool Contains(ImageColourGroup group) => (members & Bit(group)) != 0;

    /// <summary>This set with <paramref name="group"/> added.</summary>
    internal ImageColourGroups With(ImageColourGroup group) => new(members | Bit(group));

    /// <summary>The groups either this set or <paramref name="other"/> holds.</summary>
    internal ImageColourGroups Union(ImageColourGroups other) => new(members | other.members);

    /// <summary>The bit standing for <paramref name="group"/>.</summary>
    private static int Bit(ImageColourGroup group) => 1 << group.Number;
}
