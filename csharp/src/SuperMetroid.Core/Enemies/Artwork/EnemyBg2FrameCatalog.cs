using System.Collections.Immutable;
using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>
/// Validated, named extended-enemy BG2 presentation shared by Phantoon and
/// Draygon. The native instruction selector still determines the frame identity.
/// </summary>
internal sealed class EnemyBg2FrameCatalog
{
    /// <summary>Canonical frames retain command order, destinations and each tile-run boundary.</summary>
    internal string ContentIdentity => SelectedPresentationHash.Create("enemy-bg2-frames-v2", content =>
        {
            foreach ((EnemyFrameId frame, EnemyBg2TilemapWrite[] writes) in frames.OrderBy(pair => pair.Key.ToString(), StringComparer.Ordinal))
            {
                content.AppendName("frame", frame.ToString());
                content.Append("writes", writes.Length);
                foreach (EnemyBg2TilemapWrite write in writes)
                {
                    content.Append("destination", write.DestinationWord);
                    content.AppendWords("tiles", write.Tiles.Span);
                }
            }
        });

    /// <summary>Validated ordered BG2 writes keyed by the extended frame that selects them.</summary>
    private readonly Dictionary<EnemyFrameId, EnemyBg2TilemapWrite[]> frames;

    /// <summary>Owns the validated writes for one enemy frame family.</summary>
    private EnemyBg2FrameCatalog(Dictionary<EnemyFrameId, EnemyBg2TilemapWrite[]> frames) =>
        this.frames = frames;

    /// <summary>Retrieves the ordered tilemap writes <paramref name="frame"/> selects, if present.</summary>
    internal bool TryGet(EnemyFrameId frame, out ReadOnlyMemory<EnemyBg2TilemapWrite> writes)
    {
        if (frames.TryGetValue(frame, out EnemyBg2TilemapWrite[]? found))
        {
            writes = found;
            return true;
        }
        writes = default;
        return false;
    }

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
                    write.X + write.Tiles.Length > EnemyBg2FrameLayout.TilemapWidth ||
                    write.Tiles.Any(tile => tile is < 0 or > ushort.MaxValue))
                    throw new InvalidDataException(
                        $"{family} BG2 frame {frame.Name} write {index} is outside its tilemap.");
                writes[index] = new EnemyBg2TilemapWrite(
                    write.Y * EnemyBg2FrameLayout.TilemapWidth + write.X,
                    write.Tiles.Select(tile => checked((ushort)tile)).ToArray());
            }
            if (!frames.TryAdd(frame.Frame, writes))
                throw new InvalidDataException(
                    $"{family} BG2 frame {frame.Name} repeats a native identity.");
        }
        return new EnemyBg2FrameCatalog(frames);
    }
}
