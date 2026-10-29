using System.Collections.Immutable;
using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// The bosses' installed BG2 frame documents: loading and validating their named frames, which the import composes the
/// bosses' body sheets from (#1466), and their identity.
/// </summary>
internal static class EnemyBg2Frames
{
    /// <summary>Canonical frames retain command order, destinations and each tile-run boundary.</summary>
    internal static string ContentIdentity(Dictionary<EnemyFrameId, EnemyBg2TilemapWrite[]> frames) =>
        SelectedPresentationHash.Create("enemy-bg2-frames-v3", content =>
        {
            foreach ((EnemyFrameId frame, EnemyBg2TilemapWrite[] writes) in frames.OrderBy(pair => EnemyFrameSheetKeys.Of(pair.Key), StringComparer.Ordinal))
            {
                content.AppendName("frame", EnemyFrameSheetKeys.Of(frame));
                content.Append("writes", writes.Length);
                foreach (EnemyBg2TilemapWrite write in writes)
                {
                    content.Append("destination", write.FirstCell);
                    content.AppendTiles("tiles", write.Tiles.Span);
                }
            }
        });

    /// <summary>Validates a complete named frame document and compiles its horizontal tile runs to BG2 destinations.</summary>
    internal static EnemyBg2FrameCatalog Load(Stream json,
        ImmutableArray<EnemySpritemapDefinition> definitions, int version, string family)
    {
        Ensure.NotNull(json);
        EnemyBg2FrameDocument document;
        document = JsonAssetDocument.Read<EnemyBg2FrameDocument>(json, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow, }, $"{family} BG2 frame");
        if (document.Version != version || document.Frames is null ||
            document.Frames.Count != definitions.Length)
            throw new InvalidDataException(
                $"{family} BG2 frames require the current version and every named frame.");

        var frames = new Dictionary<EnemyFrameId, EnemyBg2TilemapWrite[]>();
        foreach (EnemySpritemapDefinition frame in definitions)
        {
            if (!document.Frames.TryGetValue(frame.Name, out EnemyBg2WriteDocument[]? source) ||
                source is null || source.Length is < 1 or >
                    EnemyBg2FrameLayout.MaximumCommandsPerStream)
                throw new InvalidDataException(
                    $"{family} BG2 frame {frame.Name} is missing or empty.");
            var writes = new EnemyBg2TilemapWrite[source.Length];
            for (int index = 0; index < writes.Length; index++)
            {
                EnemyBg2WriteDocument? write = source[index];
                if (write is null ||
                    write.X is < 0 or >= EnemyBg2FrameLayout.TilemapWidth ||
                    write.Y is < 0 or >= EnemyBg2FrameLayout.TilemapHeight ||
                    write.Tiles is null || write.Tiles.Length is < 1 or >
                        EnemyBg2FrameLayout.TilemapWidth ||
                    write.X + write.Tiles.Length > EnemyBg2FrameLayout.TilemapWidth)
                    throw new InvalidDataException(
                        $"{family} BG2 frame {frame.Name} write {index} is outside its tilemap.");
                var tiles = new BackgroundTile[write.Tiles.Length];
                for (int cell = 0; cell < tiles.Length; cell++)
                    tiles[cell] = write.Tiles[cell]?.Placement() ?? throw new InvalidDataException(
                        $"{family} BG2 frame {frame.Name} write {index} cell {cell} has an invalid tile or palette.");
                writes[index] = new EnemyBg2TilemapWrite(
                    write.Y * EnemyBg2FrameLayout.TilemapWidth + write.X, tiles);
            }
            if (!frames.TryAdd(frame.Frame, writes))
                throw new InvalidDataException(
                    $"{family} BG2 frame {frame.Name} repeats a native identity.");
        }
        return new EnemyBg2FrameCatalog(frames);
    }
}
