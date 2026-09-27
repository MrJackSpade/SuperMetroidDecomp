using System.Text.Json;
using System.Text.Json.Serialization;

namespace SuperMetroid.Core.Assets;

/// <summary>One ordered visual BG2 write; destination and tile references are editable.</summary>
internal readonly record struct PhantoonBg2TilemapWrite(
    ushort DestinationWord, ReadOnlyMemory<ushort> Tiles);

/// <summary>
/// Installed Phantoon BG2 presentation. The cartridge's frame selectors remain
/// compiled; this file contains only tilemap placement and tile references.
/// </summary>
public sealed class PhantoonBg2FrameCatalog
{
    private readonly Dictionary<ushort, PhantoonBg2TilemapWrite[]> frames;

    private PhantoonBg2FrameCatalog(Dictionary<ushort, PhantoonBg2TilemapWrite[]> frames) =>
        this.frames = frames;

    internal bool TryGet(ushort pointer, out ReadOnlyMemory<PhantoonBg2TilemapWrite> writes)
    {
        if (frames.TryGetValue(pointer, out PhantoonBg2TilemapWrite[]? found))
        {
            writes = found;
            return true;
        }
        writes = default;
        return false;
    }

    public static PhantoonBg2FrameCatalog Load(Stream json)
    {
        ArgumentNullException.ThrowIfNull(json);
        PhantoonBg2FrameDocument document;
        try
        {
            using JsonDocument parsed = JsonDocument.Parse(json);
            EnemySpritemapCatalog.RejectDuplicateProperties(parsed.RootElement);
            document = parsed.RootElement.Deserialize<PhantoonBg2FrameDocument>(
                new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
                }) ?? throw new InvalidDataException("Phantoon BG2 frames are null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid Phantoon BG2 frame JSON.", error);
        }
        if (document.Version != PhantoonBg2FrameDefinitions.Version ||
            document.Frames is null ||
            document.Frames.Count != PhantoonBg2FrameDefinitions.Frames.Length)
            throw new InvalidDataException(
                "Phantoon BG2 frames require the current version and every named frame.");

        var frames = new Dictionary<ushort, PhantoonBg2TilemapWrite[]>();
        foreach (PhantoonBg2FrameDefinition frame in PhantoonBg2FrameDefinitions.Frames)
        {
            if (!document.Frames.TryGetValue(frame.Name, out PhantoonBg2WriteDocument[]? source) ||
                source is null || source.Length is < 1 or > 128)
                throw new InvalidDataException($"Phantoon BG2 frame {frame.Name} is missing or empty.");
            var writes = new PhantoonBg2TilemapWrite[source.Length];
            for (int index = 0; index < writes.Length; index++)
            {
                PhantoonBg2WriteDocument? write = source[index];
                if (write is null || write.X is < 0 or >= PhantoonBg2FrameDefinitions.TilemapWidth ||
                    write.Y is < 0 or >= PhantoonBg2FrameDefinitions.TilemapHeight ||
                    write.Tiles is null || write.Tiles.Length is < 1 or > 32 ||
                    write.X + write.Tiles.Length > PhantoonBg2FrameDefinitions.TilemapWidth ||
                    write.Tiles.Any(tile => tile is < 0 or > ushort.MaxValue))
                    throw new InvalidDataException(
                        $"Phantoon BG2 frame {frame.Name} write {index} is outside its tilemap.");
                writes[index] = new PhantoonBg2TilemapWrite(
                    checked((ushort)(write.Y * PhantoonBg2FrameDefinitions.TilemapWidth + write.X)),
                    write.Tiles.Select(tile => checked((ushort)tile)).ToArray());
            }
            frames.Add(frame.Pointer, writes);
        }
        return new PhantoonBg2FrameCatalog(frames);
    }
}

/// <summary>Named visual frames; no collision or instruction-program data.</summary>
public sealed record PhantoonBg2FrameDocument
{
    public required int Version { get; init; }
    public required Dictionary<string, PhantoonBg2WriteDocument[]> Frames { get; init; }
}

/// <summary>A horizontal BG2 tile run in 32-by-64 tilemap coordinates.</summary>
public sealed record PhantoonBg2WriteDocument
{
    public required int X { get; init; }
    public required int Y { get; init; }
    public required int[] Tiles { get; init; }
}
