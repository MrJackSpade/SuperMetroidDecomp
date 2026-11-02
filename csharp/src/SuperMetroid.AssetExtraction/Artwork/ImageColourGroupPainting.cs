using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>
/// How the import pipeline records the colour groups it paints an installed image with (#1464): the group each ink was
/// coloured from, the groups a frame's pixels use, and whether a part follows a drawn colouring or keeps its own.
/// </summary>
internal static class ImageColourGroupPainting
{
    /// <summary>The group an ink belongs to.</summary>
    extension(ImageColourGroup)
    {
        /// <summary>The group a pixel inked <paramref name="paletteIndex"/> among a sixteen-colour image's colours was coloured from.</summary>
        /// <exception cref="ArgumentOutOfRangeException">The index lies past the image's groups.</exception>
        internal static ImageColourGroup OfInk(int paletteIndex) => ImageColourGroup.Numbered(paletteIndex / ImageColourGroup.Colours);
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

    /// <summary>The part group a painted pixel belongs to.</summary>
    extension(PartColourGroup)
    {
        /// <summary>A part of colour group <paramref name="group"/> that follows its draw's colouring.</summary>
        internal static PartColourGroup Following(ImageColourGroup group) => PartColourGroup.FromCode(group.Number);

        /// <summary>A part of colour group <paramref name="group"/> that keeps its own colours under any drawn colouring.</summary>
        internal static PartColourGroup Keeping(ImageColourGroup group) => PartColourGroup.FromCode(group.Number + ImageColourGroup.Count);
    }
}
