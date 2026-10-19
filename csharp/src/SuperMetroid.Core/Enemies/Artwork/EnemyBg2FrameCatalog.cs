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
    internal string ContentIdentity => SelectedPresentationHash.Create("enemy-bg2-frames-v1", content =>
        {
            foreach ((ushort frame, EnemyBg2TilemapWrite[] writes) in frames.OrderBy(pair => pair.Key))
            {
                content.Append("frame", frame);
                content.Append("writes", writes.Length);
                foreach (EnemyBg2TilemapWrite write in writes)
                {
                    content.Append("destination", write.DestinationWord);
                    content.AppendWords("tiles", write.Tiles.Span);
                }
            }
        });

    /// <summary>Validated ordered BG2 writes keyed by each native extended-frame pointer.</summary>
    private readonly Dictionary<ushort, EnemyBg2TilemapWrite[]> frames;

    /// <summary>Owns the validated writes for one enemy frame family.</summary>
    private EnemyBg2FrameCatalog(Dictionary<ushort, EnemyBg2TilemapWrite[]> frames) =>
        this.frames = frames;

    /// <summary>Retrieves the ordered tilemap writes selected by a native frame pointer, if present.</summary>
    internal bool TryGet(ushort pointer, out ReadOnlyMemory<EnemyBg2TilemapWrite> writes)
    {
        if (frames.TryGetValue(pointer, out EnemyBg2TilemapWrite[]? found))
        {
            writes = found;
            return true;
        }
        writes = default;
        return false;
    }

    /// <summary>Validates a complete named frame document and compiles its horizontal tile runs to BG2 destinations.</summary>
    internal static EnemyBg2FrameCatalog Load(Stream json,
        EnemyBg2FrameDefinitionSequence definitions, int version, string family)
    {
        Ensure.NotNull(json);
        EnemyBg2FrameDocument document;
        try
        {
            using JsonDocument parsed = JsonDocument.Parse(json);
            EnemySpritemapCatalog.RejectDuplicateProperties(parsed.RootElement);
            document = parsed.RootElement.Deserialize<EnemyBg2FrameDocument>(
                new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
                }) ?? throw new InvalidDataException($"{family} BG2 frames are null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException($"Invalid {family} BG2 frame JSON.", error);
        }
        if (document.Version != version || document.Frames is null ||
            document.Frames.Count != definitions.Length)
            throw new InvalidDataException(
                $"{family} BG2 frames require the current version and every named frame.");

        var frames = new Dictionary<ushort, EnemyBg2TilemapWrite[]>();
        foreach (EnemyBg2FrameDefinition frame in definitions)
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
            if (!frames.TryAdd(frame.Pointer, writes))
                throw new InvalidDataException(
                    $"{family} BG2 frame {frame.Name} repeats a native identity.");
        }
        return new EnemyBg2FrameCatalog(frames);
    }
}
