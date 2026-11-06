namespace SuperMetroid.Core.Bosses.Crocomire.Artwork;

/// <summary>
/// The names Crocomire's melt images take in his installed body sheet (#1466, #1513). The import names the frames it
/// composes with these, and Crocomire draws them by them.
/// </summary>
internal static class CrocomireMeltingFrameNames
{
    /// <summary>The body sheet frame of melt image <paramref name="illustration"/> before its dissolve erases any column.</summary>
    internal static string Of(CrocomireMeltingIllustration illustration) => $"Melting{illustration}";

    /// <summary>
    /// The body sheet frame of melt image <paramref name="illustration"/> once its dissolve has erased
    /// <paramref name="erased"/> columns: the whole image, each band of it as the dissolve last showed it (#1513).
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="erased"/> is not a count of the dissolve's columns.</exception>
    internal static string Of(CrocomireMeltingIllustration illustration, int erased) =>
        (uint)erased > CrocomireMeltingDefinitions.ColumnCount
            ? throw new ArgumentOutOfRangeException(nameof(erased), erased, "Crocomire's dissolve has no such step.")
            : erased == 0 ? Of(illustration) : $"Melting{illustration}.{erased}";
}
