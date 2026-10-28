using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Kraid.Artwork;
using SuperMetroid.Core.Bosses.Artwork;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// Kraid's body as tests observe it (#1352): his composed working map and the BG2 tilemap a body frame shows, each tile
/// encoded through the codec as the cartridge's tilemap entry so checks compare them with the cartridge's words.
/// </summary>
internal static class KraidBodyFixtures
{
    /// <summary>The working map <paramref name="artwork"/>'s Kraid BG2 sources compose, as tilemap entries.</summary>
    internal static ushort[] WorkingMap(BossArtworkCatalog artwork) =>
        BackgroundTileWordBytes.Words(KraidBodyTilemap.ComposeWorkingMap(Art(artwork)));

    /// <summary>The BG2 tilemap entries <paramref name="state"/>'s body frame shows with <paramref name="artwork"/>.</summary>
    internal static ushort[] Shown(BossArtworkCatalog artwork, KraidEnemyState state) => BackgroundTileWordBytes.Words(
        KraidBodyTilemap.Resolve(state.DisplayedBody, KraidBodyTilemap.ComposeWorkingMap(Art(artwork)), Art(artwork)).Tiles.AsSpan());

    /// <summary>
    /// Kraid's 64x64 BG2 tilemap entries with <paramref name="top"/> on the upper-left screen, <paramref name="head"/> over
    /// its first rows and <paramref name="bottom"/> on the lower-left screen; every other entry is the blank tile's.
    /// </summary>
    internal static ushort[] Tilemap(ReadOnlySpan<ushort> top, ReadOnlySpan<ushort> head, ReadOnlySpan<ushort> bottom)
    {
        var words = new ushort[KraidBackgroundDefinitions.BodyPlaneSize.X * KraidBackgroundDefinitions.BodyPlaneSize.Y];
        words.AsSpan().Fill(KraidBackgroundDefinitions.BlankTile.PackedWord);
        top.CopyTo(words);
        head.CopyTo(words);
        bottom.CopyTo(words.AsSpan(2 * KraidBackgroundDefinitions.PageCells));
        return words;
    }

    /// <summary>The installed Kraid BG2 artwork in <paramref name="artwork"/>.</summary>
    private static KraidBackgroundArtwork Art(BossArtworkCatalog artwork) =>
        artwork.KraidBackground ?? throw new InvalidOperationException("The boss artwork has no Kraid BG2 sources.");
}
