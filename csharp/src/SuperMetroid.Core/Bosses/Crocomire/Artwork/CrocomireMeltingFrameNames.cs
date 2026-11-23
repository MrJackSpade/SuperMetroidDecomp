namespace SuperMetroid.Core.Bosses.Crocomire.Artwork;

/// <summary>
/// The names Crocomire's melt images take in his installed body sheet (#1466, #1513, #1652, #2401): one frame per image,
/// which his body shows at every step of its dissolve through the step's hidden columns. The import names the frames it
/// composes with these, and Crocomire draws them by them. Each name is the installed sheet's own key, written out, so
/// renaming an illustration's identity never changes a key on disk.
/// </summary>
internal static partial class CrocomireMeltingFrameNames
{
    /// <summary>Each melt image's installed body sheet frame.</summary>
    private static readonly IdentityTable<CrocomireMeltingIllustration, string> Keys = new(new Dictionary<CrocomireMeltingIllustration, string>
        {
            [CrocomireMeltingIllustration.Flesh] = "MeltingFlesh",
            [CrocomireMeltingIllustration.BareSkull] = "MeltingBareSkull",
        });

    /// <summary>The body sheet frame of melt image <paramref name="illustration"/> before its dissolve erases any column.</summary>
    internal static string Of(CrocomireMeltingIllustration illustration) => Keys[illustration];
}
