using SuperMetroid.Core.Game;
using System.Buffers.Binary;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SuperMetroid.Core.Assets;

/// <summary>One fixed visual tilemap source used during the Ceres escape warning.</summary>
/// <param name="Name">Stable JSON key used to identify this overlay's editable tile words.</param>
/// <param name="SourceAddress">ROM address whose native words are represented by the overlay.</param>
/// <param name="WordCount">Number of 16-bit tile words transferred for the overlay.</param>
internal readonly record struct CeresEscapeOverlayTilemapDefinition(
    string Name, int SourceAddress, int WordCount);

/// <summary>Native identity and queue geometry for the English and Japanese Ceres overlays.</summary>
internal static class CeresEscapeOverlayTilemapDefinitions
{
    /// <summary>Schema version accepted by the Ceres overlay document reader.</summary>
    internal const int Version = 1;
    /// <summary>Repository asset filename containing editable Ceres overlay words.</summary>
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
    /// <summary>Ordered Latin text used to select the native tiles for the English warning title.</summary>
    private const string EmergencyText = "EMERGENCY";
    /// <summary>First authored Japanese warning row, whose characters map to the upper-half glyph tiles.</summary>
    private const string JapaneseFirstLine = "自爆装置が、作動しました";
    /// <summary>Second authored Japanese warning row, whose characters map to the lower-half glyph tiles.</summary>
    private const string JapaneseSecondLine = "ただちに脱出して下さい";
    /// <summary>Distinct Japanese characters packed into the glyph atlas and searched when building subtitle words.</summary>
    private const string JapaneseGlyphs = "自爆装置が、作動まただちに脱出して下さい";
    /// <summary>$A6:C164 selects the Latin atlas beginning with A at BG tile $182.</summary>
    private const int LatinFirstTile = CeresEscapeVramTransferDefinitions.WarningBackgroundDestination / (8 * 8 * 4 / 16);
    /// <summary>$B7:DA00 source loaded at VRAM word $1820; Japanese upper/lower halves start at BG tiles $1A0/$1B0.</summary>
    private const int JapaneseUpperTile = (JapaneseTrailingUpperTile + MainGlyphCount - 1) / MainGlyphCount * MainGlyphCount,
        JapaneseLowerTile = JapaneseUpperTile + MainGlyphCount;
    /// <summary>$A6:C432..C438 selects the final four upper halves packed before the first sixteen at BG $19C..19F.</summary>
    private const int JapaneseTrailingUpperTile = LatinFirstTile + ('Z' - 'A' + 1);
    /// <summary>Glyphs packed in each complete upper- or lower-half block of the Japanese atlas.</summary>
    private const int MainGlyphCount = 16;
    /// <summary>$A6:C164 and C3F4 use priority plus BG palettes six and seven respectively.</summary>
    private const int Priority = 1 << 13;
    /// <summary>Tile attributes for the English and Japanese overlays, including priority and palette selection.</summary>
    private const int EnglishStyle = Priority | (CeresRidleyPaletteRomData.AlarmCgramIndex / 16) << 10,
        JapaneseStyle = Priority | 7 << 10;

    /// <summary>Returns the native tile attribute word for one position in a fixed overlay page.</summary>
    /// <param name="page">Overlay page whose authored text and tile layout determine the result.</param>
    /// <param name="index">Zero-based word position within that page.</param>
    /// <returns>The tile index combined with the page's priority and palette attributes.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The position is outside the page or the page is not a fixed overlay.</exception>
    /// <exception cref="InvalidOperationException">A Japanese subtitle character has no corresponding native glyph.</exception>
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
    /// <summary>Enumerates the English title followed by the four Japanese subtitle rows.</summary>
    internal static PageSequence All => default;

    /// <summary>Value sequence describing the five fixed Ceres warning pages.</summary>
    internal readonly struct PageSequence : IReadOnlyList<CeresEscapeOverlayTilemapDefinition>
    {
        /// <summary>Gets the fixed number of overlay pages.</summary>
        public int Count => 5;
        /// <summary>Gets the number of overlay pages for callers using sequence terminology.</summary>
        public int Length => Count;
        /// <summary>Gets the page at its stable native transfer order.</summary>
        /// <param name="index">Zero-based position from the English title through the Japanese rows.</param>
        /// <exception cref="IndexOutOfRangeException">The index is not in the five-page sequence.</exception>
        public CeresEscapeOverlayTilemapDefinition this[int index] => index switch
        {
            0 => Emergency,
            1 => JapaneseFirst,
            2 => JapaneseSecond,
            3 => JapaneseThird,
            4 => JapaneseFourth,
            _ => throw new IndexOutOfRangeException(),
        };
        /// <summary>Enumerates the fixed pages in native transfer order.</summary>
        /// <returns>An enumerator over the English page followed by the Japanese rows.</returns>
        public IEnumerator<CeresEscapeOverlayTilemapDefinition> GetEnumerator()
        {
            for (int i = 0; i < Count; i++) yield return this[i];
        }
        /// <summary>Returns a non-generic enumerator over this sequence.</summary>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
}

/// <summary>Human-readable visual tile words; DMA timing and destinations are not editable.</summary>
internal sealed record CeresEscapeOverlayTilemapDocument
{
    /// <summary>Gets the schema revision that must match the supported overlay format.</summary>
    public int Version { get; init; }
    /// <summary>Gets the named 16-bit tile words for the English title and Japanese subtitle rows.</summary>
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

    /// <summary>Sparse overrides keyed by ROM byte address; omitted words retain their native artwork value.</summary>
    private readonly Dictionary<int, ushort> edits;

    /// <summary>Creates a catalog from the validated sparse set of tile-word overrides.</summary>
    /// <param name="edits">Changed tile words keyed by their source ROM byte address.</param>
    private CeresEscapeOverlayTilemapCatalog(Dictionary<int, ushort> edits) => this.edits = edits;

    /// <summary>Materializes one page as little-endian tile words, applying overrides over native defaults.</summary>
    /// <param name="page">Fixed page whose transfer bytes are requested.</param>
    /// <returns>A newly allocated contiguous byte payload for the page.</returns>
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
    /// <summary>Reads and validates a JSON document, retaining only words that differ from native artwork.</summary>
    /// <param name="json">Readable stream containing the versioned Ceres overlay document.</param>
    /// <returns>A catalog that resolves the document's page transfers.</returns>
    /// <exception cref="ArgumentNullException">The stream is <see langword="null"/>.</exception>
    /// <exception cref="InvalidDataException">The document has an unsupported version, incomplete pages, or unknown page names.</exception>
    internal static CeresEscapeOverlayTilemapCatalog Load(Stream json)
    {
        ArgumentNullException.ThrowIfNull(json);
        CeresEscapeOverlayTilemapDocument document = JsonAssetDocument.Read<CeresEscapeOverlayTilemapDocument>(
            json, Options, "Ceres escape overlay tilemap");
        return new(Compile(document));
    }

    /// <summary>Validates and serializes a document using the catalog's stable JSON settings.</summary>
    /// <param name="document">Complete versioned tilemap data to validate and encode.</param>
    /// <returns>UTF-8 JSON bytes for the document.</returns>
    /// <exception cref="ArgumentNullException">The document is <see langword="null"/>.</exception>
    /// <exception cref="InvalidDataException">The document does not contain exactly the supported pages and word counts.</exception>
    internal static byte[] Write(CeresEscapeOverlayTilemapDocument document)
    {
        _ = Compile(document);
        return JsonSerializer.SerializeToUtf8Bytes(document, Options);
    }

    /// <summary>Resolves an exact fixed-page address and byte length to its edited transfer payload.</summary>
    /// <param name="sourceAddress">ROM source address requested by the asset transfer.</param>
    /// <param name="byteCount">Requested payload length in bytes.</param>
    /// <param name="data">Receives the page bytes on success, or an empty value when no page matches.</param>
    /// <returns><see langword="true"/> when both address and byte length identify a known page.</returns>
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

    /// <summary>Checks document shape and converts its page arrays into sparse source-addressed overrides.</summary>
    /// <param name="document">Versioned data checked against the fixed native page definitions.</param>
    /// <returns>Only tile words that differ from the corresponding native defaults.</returns>
    /// <exception cref="ArgumentNullException">The document is <see langword="null"/>.</exception>
    /// <exception cref="InvalidDataException">A version, page name, page count, or word count is invalid.</exception>
    private static Dictionary<int, ushort> Compile(
        CeresEscapeOverlayTilemapDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);
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
                !CeresEscapeOverlayTilemapDefinitions.All.ToArray()
                    .Any(definition => definition.Name == name)))
            throw new InvalidDataException(
                "Ceres escape overlay contains an unknown tilemap name.");
        return compiled;
    }

    /// <summary>Uses camel-case names, rejects unknown JSON fields, and keeps asset output readable.</summary>
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
    };
}
