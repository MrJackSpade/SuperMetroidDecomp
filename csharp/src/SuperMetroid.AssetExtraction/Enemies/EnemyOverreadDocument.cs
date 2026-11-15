namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// The installed enemy overread file (#1500): what each enemy read past its authored data yields, decoded from the
/// player's cartridge and keyed by the game situation that reaches it, never by a cartridge place.
/// </summary>
/// <param name="Version">The schema revision, <see cref="EnemyOverreadFormat.Version"/>.</param>
/// <param name="MagdolliteApexGrowthBySlot">Whether a Magdollite arm grows a final segment at its full height, by the enemy slot it occupies.</param>
/// <param name="FallingSparkUnauthoredLaunch">The drift a falling spark takes when its launch selection lies past the authored launches.</param>
internal sealed record EnemyOverreadDocument(
    int Version,
    IReadOnlyList<bool> MagdolliteApexGrowthBySlot,
    EnemyOverreadDocument.Drift FallingSparkUnauthoredLaunch)
{
    /// <summary>A signed horizontal drift per update.</summary>
    /// <param name="WholePixels">The signed whole pixels.</param>
    /// <param name="Fraction">The sixteen-bit fraction of a pixel, 0 through 65535.</param>
    internal sealed record Drift(int WholePixels, int Fraction);
}
