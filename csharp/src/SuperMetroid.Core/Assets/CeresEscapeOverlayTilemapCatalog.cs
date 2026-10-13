using SuperMetroid.Core.Game;
using System.Buffers.Binary;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SuperMetroid.Core.Assets;

/// <summary>One fixed visual tilemap source used during the Ceres escape warning.</summary>
internal readonly record struct CeresEscapeOverlayTilemapDefinition(
    string Name, int SourceAddress, int WordCount);

/// <summary>Native identity and queue geometry for the English and Japanese Ceres overlays.</summary>
internal static class CeresEscapeOverlayTilemapDefinitions
{
    /// <summary>Supported editable warning-overlay schema revision.</summary>
    internal const int Version = 1;
    /// <summary>Installed JSON file containing all five Ceres warning tilemap pages.</summary>
    internal const string FileName = "ceres-escape-overlay-tilemaps.json";

    /// <summary>English EMERGENCY title tilemap at $A6:C164.</summary>
    internal static readonly CeresEscapeOverlayTilemapDefinition Emergency =
        new("emergency", 0xa6c164, EmergencyText.Length);

    /// <summary>English EMERGENCY tilemap's BG1 destination $50CB.</summary>
    internal const ushort EmergencyDestination = 0x50cb;

    /// <summary>First Japanese self-destruct subtitle row at $A6:C3F4.</summary>
    internal static readonly CeresEscapeOverlayTilemapDefinition JapaneseFirst =
        new("japanese_0", 0xa6c3f4, JapaneseFirstLine.Length);

    /// <summary>Second Japanese self-destruct subtitle row at $A6:C40C.</summary>
    internal static readonly CeresEscapeOverlayTilemapDefinition JapaneseSecond =
        new("japanese_1", JapaneseFirst.SourceAddress + JapaneseFirst.WordCount * sizeof(ushort), JapaneseFirstLine.Length);

    /// <summary>Third Japanese self-destruct subtitle row at $A6:C424.</summary>
    internal static readonly CeresEscapeOverlayTilemapDefinition JapaneseThird =
        new("japanese_2", JapaneseSecond.SourceAddress + JapaneseSecond.WordCount * sizeof(ushort), JapaneseSecondLine.Length);

    /// <summary>Fourth Japanese self-destruct subtitle row at $A6:C43A.</summary>
    internal static readonly CeresEscapeOverlayTilemapDefinition JapaneseFourth =
        new("japanese_3", JapaneseThird.SourceAddress + JapaneseThird.WordCount * sizeof(ushort), JapaneseSecondLine.Length);

    // Authored warning wording and font ordering are categorical typography content.
    // The selected two-half packing, foreground priority and Japanese palette role
    // preserve that composition; glyph pixels and RGB paint remain separate obligations.
    /// <summary>English warning title rendered from the Latin tile alphabet.</summary>
    private const string EmergencyText = "EMERGENCY";
    /// <summary>First authored Japanese self-destruct subtitle row, shared by its upper and lower tile halves.</summary>
    private const string JapaneseFirstLine = "自爆装置が、作動しました";
    /// <summary>Second authored Japanese evacuation subtitle row, shared by its upper and lower tile halves.</summary>
    private const string JapaneseSecondLine = "ただちに脱出して下さい";
    /// <summary>Distinct Japanese characters in the native warning font order used to calculate tile selectors.</summary>
    private const string JapaneseGlyphs = "自爆装置が、作動まただちに脱出して下さい";
    /// <summary>$A6:C164 selects the Latin atlas beginning with A at BG tile $182.</summary>
    private const int LatinFirstTile = CeresEscapeVramTransferDefinitions.WarningBackgroundDestination / (8 * 8 * 4 / 16);
    /// <summary>$B7:DA00 source loaded at VRAM word $1820; Japanese upper/lower halves start at BG tiles $1A0/$1B0.</summary>
    private const int JapaneseUpperTile = (JapaneseTrailingUpperTile + MainGlyphCount - 1) / MainGlyphCount * MainGlyphCount,
        JapaneseLowerTile = JapaneseUpperTile + MainGlyphCount;
    /// <summary>$A6:C432..C438 selects the final four upper halves packed before the first sixteen at BG $19C..19F.</summary>
    private const int JapaneseTrailingUpperTile = LatinFirstTile + ('Z' - 'A' + 1);
    /// <summary>Number of Japanese upper glyph halves stored after the Latin alphabet.</summary>
    private const int MainGlyphCount = 16;
    /// <summary>BG tile attribute selecting the foreground warning layer.</summary>
    private const int Priority = 1 << 13;
    /// <summary>Foreground priority combined with the English alarm and Japanese subtitle palette rows.</summary>
    private const int EnglishStyle = Priority | (CeresRidleyPaletteRomData.AlarmCgramIndex / 16) << 10,
        JapaneseStyle = Priority | 7 << 10;

    /// <summary>Calculates a stock English or Japanese overlay word from its authored glyph order, tile half, priority, and palette.</summary>
    internal static ushort StockWord(CeresEscapeOverlayTilemapDefinition page, int index)
    {
        if ((uint)index >= page.WordCount) throw new ArgumentOutOfRangeException(nameof(index));
        if (page == Emergency) return (ushort)(EnglishStyle | LatinFirstTile + EmergencyText[index] - 'A');
        bool secondLine = page == JapaneseThird || page == JapaneseFourth;
        bool lowerHalf = page == JapaneseSecond || page == JapaneseFourth;
        if (page != JapaneseFirst && page != JapaneseSecond && page != JapaneseThird && page != JapaneseFourth)
            throw new ArgumentOutOfRangeException(nameof(page));
        char character = (secondLine ? JapaneseSecondLine : JapaneseFirstLine)[index];
        int glyph = JapaneseGlyphs.IndexOf(character, StringComparison.Ordinal);
        if (glyph < 0) throw new InvalidOperationException("Ceres subtitle character has no native glyph.");
        int tile = lowerHalf ? JapaneseLowerTile + glyph : glyph < MainGlyphCount
            ? JapaneseUpperTile + glyph : JapaneseTrailingUpperTile + glyph - MainGlyphCount;
        return (ushort)(JapaneseStyle | tile);
    }
    /// <summary>Five fixed overlays in English-title then Japanese row-half order.</summary>
    internal static PageSequence All => default;

    /// <summary>Enumerates the fixed warning pages without retaining a descriptor array.</summary>
    internal readonly struct PageSequence : IReadOnlyList<CeresEscapeOverlayTilemapDefinition>
    {
        /// <summary>One English title and four Japanese subtitle tilemaps.</summary>
        public int Count => 5;
        /// <summary>List length used by the editable document validator.</summary>
        public int Length => Count;
        /// <summary>Selects a named overlay and native source range by page ordinal.</summary>
        public CeresEscapeOverlayTilemapDefinition this[int index] => index switch
        {
            0 => Emergency,
            1 => JapaneseFirst,
            2 => JapaneseSecond,
            3 => JapaneseThird,
            4 => JapaneseFourth,
            _ => throw new IndexOutOfRangeException(),
        };
        /// <summary>Yields the five warning pages in transfer order.</summary>
        public IEnumerator<CeresEscapeOverlayTilemapDefinition> GetEnumerator()
        {
            for (int i = 0; i < Count; i++) yield return this[i];
        }
        /// <summary>Uses the same transfer-order traversal for non-generic callers.</summary>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
}

/// <summary>Human-readable visual tile words; DMA timing and destinations are not editable.</summary>
internal sealed record CeresEscapeOverlayTilemapDocument
{
    /// <summary>Schema revision checked before compiling warning-page edits.</summary>
    public int Version { get; init; }
    /// <summary>Five named arrays of complete BG tile words, one per fixed Ceres overlay.</summary>
    public required Dictionary<string, ushort[]> Pages { get; init; }
}

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
