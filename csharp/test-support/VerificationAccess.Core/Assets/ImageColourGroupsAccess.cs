namespace SuperMetroid.Core.Assets;

/// <summary>Verification access to <see cref="ImageColourGroups"/>: fixture frames name the groups they use directly.</summary>
internal static class ImageColourGroupsAccess
{
    /// <summary>Builds a fixture frame's set of colour groups.</summary>
    extension(ImageColourGroups)
    {
        /// <summary>The set of <paramref name="groups"/>.</summary>
        internal static ImageColourGroups Of(params ReadOnlySpan<ImageColourGroup> groups)
        {
            ImageColourGroups set = ImageColourGroups.None;
            foreach (ImageColourGroup group in groups)
                set = set.With(group);
            return set;
        }
    }
}
