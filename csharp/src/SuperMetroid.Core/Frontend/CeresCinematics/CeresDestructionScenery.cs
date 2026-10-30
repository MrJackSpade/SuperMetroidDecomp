using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Frontend.CeresCinematics.Artwork;

namespace SuperMetroid.Core.Frontend.CeresCinematics;

/// <summary>What the Ceres destruction's Zebes reveal shows (#1404, #1467): its planet tilemap once loaded.</summary>
internal static class CeresDestructionScenery
{
    /// <summary>The Zebes reveal's tilemap once <paramref name="zebesRevealed"/>; an empty plane before.</summary>
    internal static BackgroundTile[] ZebesTilemap(IntroCinematicArtwork artwork, bool zebesRevealed)
    {
        var tiles = new BackgroundTile[CeresDestructionArtworkFormat.ZebesMapByteCount / sizeof(ushort)];
        if (!zebesRevealed)
            return tiles;
        artwork.CeresDestruction.ZebesMap.Tiles.Span[..tiles.Length].CopyTo(tiles);
        return tiles;
    }
}
