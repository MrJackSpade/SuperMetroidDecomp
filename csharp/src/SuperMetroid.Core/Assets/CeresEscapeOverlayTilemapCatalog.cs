using System.Buffers.Binary;
using System.Text.Json;

namespace SuperMetroid.Core.Assets;

/// <summary>One fixed visual tilemap source used during the Ceres escape warning.</summary>
internal readonly record struct CeresEscapeOverlayTilemapDefinition(
    string Name, int SourceAddress, int WordCount);

/// <summary>Native identity and queue geometry for the English and Japanese Ceres overlays.</summary>
internal static class CeresEscapeOverlayTilemapDefinitions
{
    internal const int Version = 1;
    internal const string FileName = "ceres-escape-overlay-tilemaps.json";

    /// <summary>English EMERGENCY title tilemap at $A6:C164.</summary>
    internal static readonly CeresEscapeOverlayTilemapDefinition Emergency =
        new("emergency", 0xa6c164, 9);

    /// <summary>English EMERGENCY tilemap's BG1 destination $50CB.</summary>
    internal const ushort EmergencyDestination = 0x50cb;

    /// <summary>First Japanese self-destruct subtitle row at $A6:C3F4.</summary>
    internal static readonly CeresEscapeOverlayTilemapDefinition JapaneseFirst =
        new("japanese_0", 0xa6c3f4, 12);

    /// <summary>Second Japanese self-destruct subtitle row at $A6:C40C.</summary>
    internal static readonly CeresEscapeOverlayTilemapDefinition JapaneseSecond =
        new("japanese_1", 0xa6c40c, 12);

    /// <summary>Third Japanese self-destruct subtitle row at $A6:C424.</summary>
    internal static readonly CeresEscapeOverlayTilemapDefinition JapaneseThird =
        new("japanese_2", 0xa6c424, 11);

    /// <summary>Fourth Japanese self-destruct subtitle row at $A6:C43A.</summary>
    internal static readonly CeresEscapeOverlayTilemapDefinition JapaneseFourth =
        new("japanese_3", 0xa6c43a, 11);

    private static readonly CeresEscapeOverlayTilemapDefinition[] Definitions =
        [Emergency, JapaneseFirst, JapaneseSecond, JapaneseThird, JapaneseFourth];

    internal static ReadOnlySpan<CeresEscapeOverlayTilemapDefinition> All => Definitions;

    internal static bool IsSource(int sourceAddress, int byteCount)
    {
        foreach (CeresEscapeOverlayTilemapDefinition page in Definitions)
            if (page.SourceAddress == sourceAddress &&
                page.WordCount * sizeof(ushort) == byteCount)
                return true;
        return false;
    }

    internal static bool ContainsByteAddress(int address)
    {
        foreach (CeresEscapeOverlayTilemapDefinition page in Definitions)
            if (address >= page.SourceAddress &&
                address < page.SourceAddress + page.WordCount * sizeof(ushort))
                return true;
        return false;
    }
}

/// <summary>Human-readable visual tile words; DMA timing and destinations are not editable.</summary>
internal sealed record CeresEscapeOverlayTilemapDocument
{
    public int Version { get; init; }
    public required Dictionary<string, ushort[]> Pages { get; init; }
}

/// <summary>Editable visual tilemap words for Ceres's five fixed warning overlays.</summary>
public sealed class CeresEscapeOverlayTilemapCatalog
{
    private readonly Dictionary<int, byte[]> pages;

    private CeresEscapeOverlayTilemapCatalog(Dictionary<int, byte[]> pages) =>
        this.pages = pages;

    internal static CeresEscapeOverlayTilemapCatalog Load(Stream json)
    {
        ArgumentNullException.ThrowIfNull(json);
        CeresEscapeOverlayTilemapDocument document;
        try
        {
            document = JsonSerializer.Deserialize<CeresEscapeOverlayTilemapDocument>(
                json, Options) ?? throw new InvalidDataException(
                    "Ceres escape overlay tilemap JSON is null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException(
                "Invalid Ceres escape overlay tilemap JSON.", error);
        }
        return new(Compile(document));
    }

    internal static byte[] Write(CeresEscapeOverlayTilemapDocument document)
    {
        _ = Compile(document);
        return JsonSerializer.SerializeToUtf8Bytes(document, Options);
    }

    internal bool TryResolve(int sourceAddress, int byteCount,
        out ReadOnlyMemory<byte> data)
    {
        if (pages.TryGetValue(sourceAddress, out byte[]? page) &&
            page.Length == byteCount)
        {
            data = page;
            return true;
        }
        data = default;
        return false;
    }

    private static Dictionary<int, byte[]> Compile(
        CeresEscapeOverlayTilemapDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);
        if (document.Version != CeresEscapeOverlayTilemapDefinitions.Version ||
            document.Pages is null ||
            document.Pages.Count != CeresEscapeOverlayTilemapDefinitions.All.Length)
            throw new InvalidDataException(
                "Ceres escape overlay requires version 1 and five named tilemaps.");
        var compiled = new Dictionary<int, byte[]>();
        foreach (CeresEscapeOverlayTilemapDefinition definition in
                 CeresEscapeOverlayTilemapDefinitions.All)
        {
            if (!document.Pages.TryGetValue(definition.Name, out ushort[]? words) ||
                words is null || words.Length != definition.WordCount)
                throw new InvalidDataException(
                    $"Ceres escape tilemap {definition.Name} requires {definition.WordCount} words.");
            byte[] bytes = new byte[words.Length * sizeof(ushort)];
            for (int index = 0; index < words.Length; index++)
                BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(index * 2),
                    words[index]);
            compiled.Add(definition.SourceAddress, bytes);
        }
        if (document.Pages.Keys.Any(name =>
                !CeresEscapeOverlayTilemapDefinitions.All.ToArray()
                    .Any(definition => definition.Name == name)))
            throw new InvalidDataException(
                "Ceres escape overlay contains an unknown tilemap name.");
        return compiled;
    }

    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
    };
}
