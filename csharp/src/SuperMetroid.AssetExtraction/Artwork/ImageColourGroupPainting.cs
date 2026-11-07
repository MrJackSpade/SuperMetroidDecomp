using System.Collections.Immutable;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>
/// How the import pipeline records the colour groups it paints an installed image with (#1464): the group each ink was
/// coloured from and the groups a frame's pixels use.
/// </summary>
internal static class ImageColourGroupPainting
{
    /// <summary>Every group, in the order the image keeps their colours.</summary>
    private static readonly ImmutableArray<ImageColourGroup> EveryGroup =
        [.. Enumerable.Range(0, ImageColourGroup.Count).Select(number => new ImageColourGroup(number))];

    /// <summary>The groups an image is coloured in, and the group an ink belongs to.</summary>
    extension(ImageColourGroup)
    {
        /// <summary>Every group, in the order the image keeps their colours.</summary>
        internal static ImmutableArray<ImageColourGroup> Every => EveryGroup;

        /// <summary>The group whose colours the image keeps at place <paramref name="number"/>.</summary>
        /// <exception cref="ArgumentOutOfRangeException">The place is not one of the image's groups.</exception>
        internal static ImageColourGroup Numbered(int number) => EveryGroup[Ensure.BetweenInclusive(0, ImageColourGroup.Count - 1, number)];

        /// <summary>The group a pixel inked <paramref name="paletteIndex"/> among a sixteen-colour image's colours was coloured from.</summary>
        /// <exception cref="ArgumentOutOfRangeException">The index lies past the image's groups.</exception>
        internal static ImageColourGroup OfInk(int paletteIndex) => ImageColourGroup.Numbered(paletteIndex / ImageColourGroup.Colours);
    }

    /// <summary>Reading which groups a set holds.</summary>
    extension(ImageColourGroups groups)
    {
        /// <summary>Whether the set holds <paramref name="group"/>.</summary>
        internal bool Contains(ImageColourGroup group) => (groups.ManifestMembers & (1 << group.Number)) != 0;
    }

    /// <summary>Building the set of groups a frame's pixels use.</summary>
    extension(ImageColourGroups)
    {
        /// <summary>No group.</summary>
        internal static ImageColourGroups None => default;

        /// <summary>The set the manifest stores as <paramref name="members"/>.</summary>
        /// <exception cref="ArgumentOutOfRangeException">The number names a group past the image's.</exception>
        internal static ImageColourGroups FromManifest(int members) => new(members);
    }

    /// <summary>Adding groups to a set.</summary>
    extension(ImageColourGroups groups)
    {
        /// <summary>This set with <paramref name="group"/> added.</summary>
        internal ImageColourGroups With(ImageColourGroup group) => new(groups.ManifestMembers | (1 << group.Number));

        /// <summary>The groups either this set or <paramref name="other"/> holds.</summary>
        internal ImageColourGroups Union(ImageColourGroups other) => new(groups.ManifestMembers | other.ManifestMembers);
    }
}
