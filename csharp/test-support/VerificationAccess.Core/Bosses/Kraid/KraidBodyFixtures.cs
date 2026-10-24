using SuperMetroid.Core.Bosses.Kraid.Artwork;
using SuperMetroid.Core.Bosses.Artwork;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Kraid's body as tests observe it (#1352): his composed working map and the BG2 tilemap a body frame shows.</summary>
internal static class KraidBodyFixtures
{
    /// <summary>The working map <paramref name="artwork"/>'s Kraid BG2 sources compose.</summary>
    internal static ushort[] WorkingMap(BossArtworkCatalog artwork) => KraidBodyTilemap.ComposeWorkingMap(Art(artwork));

    /// <summary>The BG2 tilemap words <paramref name="state"/>'s body frame shows with <paramref name="artwork"/>.</summary>
    internal static ushort[] Shown(BossArtworkCatalog artwork, KraidEnemyState state) =>
        [.. KraidBodyTilemap.Resolve(state.DisplayedBody, WorkingMap(artwork), Art(artwork)).Words];

    /// <summary>
    /// Kraid's 64x64 BG2 tilemap words with <paramref name="top"/> on the upper-left screen, <paramref name="head"/> over
    /// its first rows and <paramref name="bottom"/> on the lower-left screen; every other word is the blank tile.
    /// </summary>
    internal static ushort[] Tilemap(ReadOnlySpan<ushort> top, ReadOnlySpan<ushort> head, ReadOnlySpan<ushort> bottom)
    {
        var words = new ushort[KraidBackgroundRomData.BodyPlaneSize.X * KraidBackgroundRomData.BodyPlaneSize.Y];
        words.AsSpan().Fill(KraidBackgroundRomData.BlankTile);
        top.CopyTo(words);
        head.CopyTo(words);
        bottom.CopyTo(words.AsSpan(2 * KraidBackgroundRomData.VisiblePageWords));
        return words;
    }

    /// <summary>The installed Kraid BG2 artwork in <paramref name="artwork"/>.</summary>
    private static KraidBackgroundArtwork Art(BossArtworkCatalog artwork) =>
        artwork.KraidBackground ?? throw new InvalidOperationException("The boss artwork has no Kraid BG2 sources.");
}
