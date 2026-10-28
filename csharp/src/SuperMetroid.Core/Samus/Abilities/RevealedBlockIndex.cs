using System.Collections.Immutable;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>
/// Every way the X-ray reveal lays out a room's block definitions, by the four tiles it writes for each (#1392): a
/// copied block as the level shows it, a reveal metatile as defined, an overlay with its rows swapped. A buffer's block is
/// found from its tiles; definitions laid out to the same tiles show the same picture.
/// </summary>
public sealed class RevealedBlockIndex
{
    /// <summary>The layouts by their four tiles.</summary>
    private readonly Dictionary<ExpandedBlockTiles, RevealedBlock> layouts = [];

    /// <summary>Lays out each of <paramref name="definitions"/> every way, the layouts as defined taking precedence.</summary>
    public RevealedBlockIndex(ImmutableArray<BackgroundTile> definitions)
    {
        Definitions = definitions;
        int count = definitions.Length / RoomLevelPlane.DefinitionTiles;
        foreach (RevealedBlockArrangement arrangement in Enum.GetValues<RevealedBlockArrangement>())
        {
            for (int definition = 0; definition < count; definition++)
                layouts.TryAdd(Tiles(definitions.AsSpan(), definition, arrangement), new RevealedBlock(definition, arrangement));
        }
    }

    /// <summary>The definitions laid out.</summary>
    public ImmutableArray<BackgroundTile> Definitions { get; }

    /// <summary>
    /// The block whose layout writes the four tiles, top left to bottom right; null for a block the copy took from
    /// beyond the room, where BG1 shows nothing and the copy writes blank tiles that lay out no definition.
    /// </summary>
    /// <exception cref="InvalidDataException">No definition laid out any of the reveal's ways writes those tiles.</exception>
    public RevealedBlock? Find(ExpandedBlockTiles tiles)
    {
        if (layouts.TryGetValue(tiles, out RevealedBlock block))
            return block;
        if (tiles == default)
            return null;
        throw new InvalidDataException(
            $"The X-ray reveal writes {tiles.TopLeft} {tiles.TopRight} {tiles.BottomLeft} {tiles.BottomRight}, which lays out no block definition.");
    }

    /// <summary>The four tiles <paramref name="arrangement"/> lays definition <paramref name="definition"/> out as.</summary>
    private static ExpandedBlockTiles Tiles(ReadOnlySpan<BackgroundTile> definitions, int definition, RevealedBlockArrangement arrangement)
    {
        if (arrangement == RevealedBlockArrangement.RowsSwapped)
        {
            // $91:D0A6 swaps the two tile rows without toggling any tile's mirroring.
            ReadOnlySpan<BackgroundTile> tiles = definitions.Slice(definition * RoomLevelPlane.DefinitionTiles, RoomLevelPlane.DefinitionTiles);
            return new(tiles[2], tiles[3], tiles[0], tiles[1]);
        }
        LevelBlockFlipFlags flips = arrangement switch
        {
            RevealedBlockArrangement.AsDefined => LevelBlockFlipFlags.None,
            RevealedBlockArrangement.MirroredHorizontally => LevelBlockFlipFlags.Horizontal,
            RevealedBlockArrangement.MirroredVertically => LevelBlockFlipFlags.Vertical,
            RevealedBlockArrangement.MirroredBoth => LevelBlockFlipFlags.Horizontal | LevelBlockFlipFlags.Vertical,
            RevealedBlockArrangement.RowsSwapped => throw new InvalidOperationException("Swapped rows were laid out above."),
            _ => throw new InvalidDataException($"Unknown revealed block arrangement {arrangement}."),
        };
        return LevelBlockTilemapExpander.Expand(RoomLevelWord.Create(checked((ushort)definition), flips, RoomCollisionType.Air), definitions);
    }
}
