namespace SuperMetroid.AssetExtraction.Bosses;

/// <summary>
/// The installed boss and intro overread file (#1500): what Kraid's fingernails, Crocomire's volley and the opening
/// cinematic's egg shell read past their data, decoded from the player's cartridge and keyed by the game situation that
/// reaches each read, never by a cartridge place.
/// </summary>
/// <param name="Version">The schema revision, <see cref="BossAndIntroOverreadFormat.Version"/>.</param>
/// <param name="KraidNailPastLedges">The contour record a fingernail higher than Kraid's four ledges is pushed back at.</param>
/// <param name="CrocomireTenthShotGradient">The vertical component of the aim of Crocomire's last spit shot, signed.</param>
/// <param name="ShellFragmentFallPastCurve">The steps an egg-shell fragment falls by on each update past its gravity curve.</param>
internal sealed record BossAndIntroOverreadDocument(
    int Version,
    BossAndIntroOverreadDocument.ContourRecord KraidNailPastLedges,
    int CrocomireTenthShotGradient,
    IReadOnlyList<BossAndIntroOverreadDocument.FallStep> ShellFragmentFallPastCurve)
{
    /// <summary>A contour record: where a nail is pushed back, and the bound it must lie below.</summary>
    /// <param name="LeftOffset">The offset from Kraid's body's X, signed.</param>
    /// <param name="Bound">The bound on the nail's height relative to the body, signed.</param>
    internal sealed record ContourRecord(int LeftOffset, int Bound);

    /// <summary>One update's fall.</summary>
    /// <param name="WholePixels">The signed whole pixels.</param>
    /// <param name="Fraction">The unsigned fraction of a pixel, in 65536ths.</param>
    internal sealed record FallStep(int WholePixels, int Fraction);
}
