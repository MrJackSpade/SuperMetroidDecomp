using System.Buffers.Binary;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SuperMetroid.Core.Assets;

/// <summary>Editable visual tilemap words for Ceres's five fixed warning overlays.</summary>
public sealed class CeresEscapeOverlayTilemapCatalog
{
    /// <summary>Canonical selected presentation data; no derived field is added to debugger states.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create("enemy-ceres-escape-overlay-v1", content =>
    {
        content.Append("shared", ReadOnlySpan<byte>.Empty);
        foreach (var page in CeresEscapeOverlayTilemapDefinitions.All)
        {
            content.Append("source", page.SourceAddress);
            content.Append("transfer", Transfer(page));
        }
    });

    /// <summary>Selected tile words differing from the calculated stock warning pages, keyed by source address.</summary>
    internal readonly Dictionary<int, ushort> edits;
    /// <summary>Retains only authored deviations from the stock Ceres warning tilemaps.</summary>
    private CeresEscapeOverlayTilemapCatalog(Dictionary<int, ushort> edits) => this.edits = edits;

    /// <summary>Expands one page's calculated and edited words into the contiguous little-endian DMA payload.</summary>
    private byte[] Transfer(CeresEscapeOverlayTilemapDefinition page)
    {
        // Ephemeral contiguous DMA/hash output, not a retained lookup definition.
        var bytes = new byte[page.WordCount * sizeof(ushort)];
        for (int index = 0; index < page.WordCount; index++)
        {
            ushort word = edits.TryGetValue(page.SourceAddress + index * 2, out ushort edit)
                ? edit : CeresEscapeOverlayTilemapDefinitions.StockWord(page, index);
            BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(index * 2), word);
        }
        return bytes;
    }
    /// <summary>Validates the five-page JSON document and compiles its edited source-address words.</summary>
    internal static CeresEscapeOverlayTilemapCatalog Load(Stream json)
    {
        Ensure.NotNull(json);
        CeresEscapeOverlayTilemapDocument document = JsonAssetDocument.Read<CeresEscapeOverlayTilemapDocument>(
            json, Options, "Ceres escape overlay tilemap");
        return new(Compile(document));
    }

    /// <summary>Validates and serializes an editable Ceres warning document as camel-case UTF-8 JSON.</summary>
    internal static byte[] Write(CeresEscapeOverlayTilemapDocument document)
    {
        _ = Compile(document);
        return JsonSerializer.SerializeToUtf8Bytes(document, Options);
    }

    /// <summary>Provides a page's DMA bytes only when both the native source address and exact transfer length match.</summary>
    internal bool TryResolve(int sourceAddress, int byteCount,
        out ReadOnlyMemory<byte> data)
    {
        foreach (var page in CeresEscapeOverlayTilemapDefinitions.All)
            if (page.SourceAddress == sourceAddress && page.WordCount * 2 == byteCount)
            {
                data = Transfer(page);
                return true;
            }
        data = default;
        return false;
    }

    /// <summary>Requires the exact five page names and dimensions while recording only words that differ from stock.</summary>
    private static Dictionary<int, ushort> Compile(
        CeresEscapeOverlayTilemapDocument document)
    {
        Ensure.NotNull(document);
        if (document.Version != CeresEscapeOverlayTilemapDefinitions.Version ||
            document.Pages is null ||
            document.Pages.Count != CeresEscapeOverlayTilemapDefinitions.All.Length)
            throw new InvalidDataException(
                "Ceres escape overlay requires version 1 and five named tilemaps.");
        var compiled = new Dictionary<int, ushort>();
        foreach (CeresEscapeOverlayTilemapDefinition definition in
                 CeresEscapeOverlayTilemapDefinitions.All)
        {
            if (!document.Pages.TryGetValue(definition.Name, out ushort[]? words) ||
                words is null || words.Length != definition.WordCount)
                throw new InvalidDataException(
                    $"Ceres escape tilemap {definition.Name} requires {definition.WordCount} words.");
            for (int index = 0; index < words.Length; index++)
                if (words[index] != CeresEscapeOverlayTilemapDefinitions.StockWord(definition, index))
                    compiled.Add(definition.SourceAddress + index * 2, words[index]);
        }
        if (document.Pages.Keys.Any(name =>
                !CeresEscapeOverlayTilemapDefinitions.All
                    .Any(definition => definition.Name == name)))
            throw new InvalidDataException(
                "Ceres escape overlay contains an unknown tilemap name.");
        return compiled;
    }

    /// <summary>Produces readable camel-case documents and rejects unknown JSON properties.</summary>
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
    };
}
