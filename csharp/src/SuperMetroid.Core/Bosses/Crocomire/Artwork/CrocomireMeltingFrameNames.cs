namespace SuperMetroid.Core.Bosses.Crocomire.Artwork;

/// <summary>
/// The names Crocomire's melt images take in his installed body sheet (#1466, #1513, #1652): one frame per image, which his
/// body shows at every step of its dissolve through the step's hidden columns. The import names the frames it
/// composes with these, and Crocomire draws them by them.
/// </summary>
internal static class CrocomireMeltingFrameNames
{
    /// <summary>The body sheet frame of melt image <paramref name="illustration"/> before its dissolve erases any column.</summary>
    internal static string Of(CrocomireMeltingIllustration illustration) => $"Melting{illustration}";
}
