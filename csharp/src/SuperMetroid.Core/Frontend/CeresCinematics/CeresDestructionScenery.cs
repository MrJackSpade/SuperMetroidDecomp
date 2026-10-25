using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Frontend.CeresCinematics.Artwork;

namespace SuperMetroid.Core.Frontend.CeresCinematics;

/// <summary>What the Ceres destruction's uploads leave on screen (#1404): the floor's cells and the Zebes reveal's tilemap.</summary>
internal static class CeresDestructionScenery
{
    /// <summary>The floor's cells <paramref name="uploads"/> leave over the installed maps, in the order the scene made them.</summary>
    internal static byte[] FloorMap(IntroCinematicArtworkCatalog artwork, IReadOnlyList<CeresDestructionUpload> uploads, bool withSamus)
    {
        byte[] maps = Tilemaps(artwork);
        var cells = new byte[AffineFloor.CellCount];
        foreach (CeresDestructionUpload upload in uploads)
        {
            switch (upload)
            {
                case CeresDestructionUpload.CeresScene:
                    // C11B selects bytes $600-$BFF: the third/fourth 24-row Ceres map pair, over the initial fill.
                    cells.AsSpan(0, AffineFloor.CellCount).Fill(CeresDestructionSceneDefinitions.Floor.BlankCharacter);
                    maps.AsSpan(CeresDestructionSceneDefinitions.Floor.CeresSceneStart, CeresDestructionSceneDefinitions.Floor.CeresSceneCells).CopyTo(cells);
                    break;
                case CeresDestructionUpload.FinalExplosionMaps:
                    // State $22 shows the front gunship in the upper rows; state $25 has no gunship and clears them too.
                    int upperSource = withSamus ? CeresDestructionSceneDefinitions.Floor.BlankViewStart : 0;
                    maps.AsSpan(upperSource, CeresDestructionSceneDefinitions.Floor.ViewCells).CopyTo(cells);
                    maps.AsSpan(CeresDestructionSceneDefinitions.Floor.BlankViewStart, CeresDestructionSceneDefinitions.Floor.ViewCells)
                        .CopyTo(cells.AsSpan(CeresDestructionSceneDefinitions.Floor.LowerViewFirstCell));
                    break;
                case CeresDestructionUpload.ZebesReveal:
                    maps.AsSpan(CeresDestructionSceneDefinitions.Floor.ViewCells, CeresDestructionSceneDefinitions.Floor.ViewCells).CopyTo(cells);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(uploads), upload, "Undefined Ceres destruction upload.");
            }
        }
        return cells;
    }

    /// <summary>The Zebes reveal's tilemap once <paramref name="uploads"/> include it; an empty plane before.</summary>
    internal static ushort[] ZebesTilemap(IntroCinematicArtworkCatalog artwork, IReadOnlyList<CeresDestructionUpload> uploads)
    {
        var words = new ushort[CeresDestructionArtworkFormat.ZebesMapByteCount / sizeof(ushort)];
        if (!uploads.Contains(CeresDestructionUpload.ZebesReveal))
            return words;
        ReadOnlySpan<byte> bytes = artwork.CeresDestruction.ZebesMap.Bytes.Span[..CeresDestructionArtworkFormat.ZebesMapByteCount];
        for (int word = 0; word < words.Length; word++)
            words[word] = SnesLittleEndian.Word(bytes[word * 2], bytes[word * 2 + 1]);
        return words;
    }

    /// <summary>The installed Ceres flight and destruction maps joined into the native contiguous transfer source.</summary>
    internal static byte[] Tilemaps(IntroCinematicArtworkCatalog artwork)
    {
        var maps = new byte[CeresDestructionSceneDefinitions.Floor.MapCells];
        artwork.CeresFlight.FloorMaps.Span.CopyTo(maps);
        artwork.CeresDestruction.CeresMaps.Span.CopyTo(maps.AsSpan(artwork.CeresFlight.FloorMaps.Length));
        return maps;
    }
}
