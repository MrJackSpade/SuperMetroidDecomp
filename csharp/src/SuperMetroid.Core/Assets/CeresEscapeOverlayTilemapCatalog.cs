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
    internal const int Version = 1;
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
    private const string EmergencyText = "EMERGENCY";
    private const string JapaneseFirstLine = "自爆装置が、作動しました";
    private const string JapaneseSecondLine = "ただちに脱出して下さい";
    private const string JapaneseGlyphs = "自爆装置が、作動まただちに脱出して下さい";
    /// <summary>$A6:C164 selects the Latin atlas beginning with A at BG tile $182.</summary>
    private const int LatinFirstTile = CeresEscapeVramTransferDefinitions.WarningBackgroundDestination / (8 * 8 * 4 / 16);
    /// <summary>$B7:DA00 source loaded at VRAM word $1820; Japanese upper/lower halves start at BG tiles $1A0/$1B0.</summary>
    private const int JapaneseUpperTile = (JapaneseTrailingUpperTile + MainGlyphCount - 1) / MainGlyphCount * MainGlyphCount,
        JapaneseLowerTile = JapaneseUpperTile + MainGlyphCount;
    /// <summary>$A6:C432..C438 selects the final four upper halves packed before the first sixteen at BG $19C..19F.</summary>
    private const int JapaneseTrailingUpperTile = LatinFirstTile + ('Z' - 'A' + 1);
    private const int MainGlyphCount = 16;
    /// <summary>$A6:C164 and C3F4 use priority plus BG palettes six and seven respectively.</summary>
    private const int Priority = 1 << 13;
    private const int EnglishStyle = Priority | (CeresRidleyPaletteRomData.AlarmCgramIndex / 16) << 10,
        JapaneseStyle = Priority | 7 << 10;

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
    internal static PageSequence All => default;

    internal readonly struct PageSequence : IReadOnlyList<CeresEscapeOverlayTilemapDefinition>
    {
        public int Count => 5;
        public int Length => Count;
        public CeresEscapeOverlayTilemapDefinition this[int index] => index switch
        {
            0 => Emergency,
            1 => JapaneseFirst,
            2 => JapaneseSecond,
            3 => JapaneseThird,
            4 => JapaneseFourth,
            _ => throw new IndexOutOfRangeException(),
        };
        public IEnumerator<CeresEscapeOverlayTilemapDefinition> GetEnumerator()
        {
            for (int i = 0; i < Count; i++) yield return this[i];
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }

    internal static bool IsSource(int sourceAddress, int byteCount)
    {
        foreach (CeresEscapeOverlayTilemapDefinition page in All)
            if (page.SourceAddress == sourceAddress &&
                page.WordCount * sizeof(ushort) == byteCount)
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

    private readonly Dictionary<int, ushort> edits;
    private CeresEscapeOverlayTilemapCatalog(Dictionary<int, ushort> edits) => this.edits = edits;

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
    internal static CeresEscapeOverlayTilemapCatalog Load(Stream json)
    {
        ArgumentNullException.ThrowIfNull(json);
        CeresEscapeOverlayTilemapDocument document = JsonAssetDocument.Read<CeresEscapeOverlayTilemapDocument>(
            json, Options, "Ceres escape overlay tilemap");
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
        foreach (var page in CeresEscapeOverlayTilemapDefinitions.All)
            if (page.SourceAddress == sourceAddress && page.WordCount * 2 == byteCount)
            {
                data = Transfer(page);
                return true;
            }
        data = default;
        return false;
    }

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

    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
    };
}
