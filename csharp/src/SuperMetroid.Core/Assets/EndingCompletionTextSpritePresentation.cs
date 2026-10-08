using System.Text.Json;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Editable OAM compositions for the 56 completion-message glyph frames.</summary>
public sealed class EndingCompletionTextSpritePresentation : IIntroCinematicSpritePresentation
{
    private readonly Dictionary<ushort, SpriteComposition> frames;

    private EndingCompletionTextSpritePresentation(Dictionary<ushort, SpriteComposition> frames) =>
        this.frames = frames;

    /// <summary>Canonical identity of the selected decoded visual frames, not JSON formatting.</summary>
    public string ContentIdentity => SelectedPresentationHash.FromCompositions(nameof(EndingCompletionTextSpritePresentation), frames);

    /// <summary>Appends the selected completion-text composition's parts in authored OAM order, preserving inherited palettes and the native on-screen/off-screen clipping path.</summary>
    /// <param name="pointer">Bank-$8C spritemap identity for a phrase prefix, digit, or colon; an uninstalled identity is rejected.</param>
    /// <param name="oam">Destination OAM buffer for this frame's text parts.</param>
    /// <param name="x">Native 16-bit screen-origin X coordinate in pixels, retaining wrapped negative values for off-screen drawing.</param>
    /// <param name="y">Native 16-bit screen-origin Y coordinate in pixels.</param>
    /// <param name="paletteBits">Packed OBJ palette bits used only by parts whose editable palette is null.</param>
    /// <param name="originIsOnScreen">Selects ordinary origin clipping when true or the native negative-origin Y-wrap path when false.</param>
    public void Draw(ushort pointer, OamBuffer oam, ushort x, ushort y,
        ushort paletteBits, bool originIsOnScreen)
    {
        if (!frames.TryGetValue(pointer, out SpriteComposition? frame))
            throw new InvalidDataException(
                $"Ending completion text frame $8C:{pointer:X4} is not installed.");
        if (originIsOnScreen)
            frame.DrawOnScreen(oam, x, y, paletteBits);
        else
            frame.DrawOffScreen(oam, x, y, paletteBits);
    }

    /// <summary>Loads version-1 <c>ending-completion-text-sprites.json</c>, requiring all 56 named compositions and validating each ordered part's offsets, tile region, size, palette, and priority.</summary>
    /// <param name="json">Caller-owned JSON stream consumed from its current position and left open; unknown and duplicate properties are rejected.</param>
    /// <returns>Compiled editable compositions, using calculated stock letter parts only when every supplied visual field matches.</returns>
    public static EndingCompletionTextSpritePresentation Load(Stream json)
    {
        ArgumentNullException.ThrowIfNull(json);
        EndingCompletionTextSpriteDocument document;
        try
        {
            using JsonDocument parsed = JsonDocument.Parse(json);
            EnemySpritemapCatalog.RejectDuplicateProperties(parsed.RootElement);
            document = parsed.RootElement.Deserialize<EndingCompletionTextSpriteDocument>(
                MapPresentationFormat.JsonOptions) ??
                throw new InvalidDataException("Ending completion text sprite JSON is null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid ending completion text sprite JSON.", error);
        }
        IReadOnlyList<EndingCompletionTextSpriteFrameDefinition> definitions =
            EndingCompletionTextSpriteDefinitions.Frames;
        if (document.Version != EndingCompletionTextSpriteFormat.Version ||
            document.Frames is null || document.Frames.Count != definitions.Count)
            throw new InvalidDataException(
                $"Ending completion text requires exactly {definitions.Count} named visual frames.");
        var frames = new Dictionary<ushort, SpriteComposition>();
        for (int index = 0; index < definitions.Count; index++)
        {
            EndingCompletionTextSpriteFrameDefinition definition = definitions[index];
            if (!document.Frames.TryGetValue(definition.Name, out SpriteVisualPart[]? visual) ||
                visual is null)
                throw new InvalidDataException(
                    $"Ending completion text frame {definition.Name} is missing.");
            frames.Add(definition.Pointer,
                IntroCinematicSpriteCompiler.Compile(visual, definition.Name)
                    .CalculateIfMatching(new EndingCompletionTextParts(index)));
        }
        return new EndingCompletionTextSpritePresentation(frames);
    }

    /// <summary>Serializes completion-text compositions as UTF-8 JSON and validates the complete named-frame set and OAM fields before writing any bytes.</summary>
    /// <param name="json">Destination written at its current position and left open.</param>
    /// <param name="document">Versioned phrase-prefix, digit, and colon compositions to serialize.</param>
    public static void Write(Stream json, EndingCompletionTextSpriteDocument document)
    {
        ArgumentNullException.ThrowIfNull(json);
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document,
            MapPresentationFormat.JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        json.Write(bytes);
    }
}

/// <summary>Editable completion-message OAM schema, independent of the fixed bank-$8B typewriter pacing, program transitions, and recorded clear-time values.</summary>
public sealed record EndingCompletionTextSpriteDocument
{
    /// <summary>Composition schema revision; loading currently requires version 1.</summary>
    public required int Version { get; init; }
    /// <summary>Exactly 56 named ordered part arrays: operation-00..14, completed-00..20, clear-time-00..08, digit-00..09, and colon-00; null part palettes inherit the drawing owner's palette.</summary>
    public required Dictionary<string, SpriteVisualPart[]> Frames { get; init; }
}

/// <summary>
/// The OAM record identities paired with the fourteen fixed bank-$8B ending
/// completion-text lists. Counts preserve the cartridge's progressively revealed
/// two-part-per-letter compositions without treating those visuals as timing data.
/// </summary>
public static class EndingCompletionTextSpriteDefinitions
{
    /// <summary>$8C:A69D, first THE OPERATION WAS prefix.</summary>
    internal const ushort OperationMap = 0xa69d;
    /// <summary>$8C:AB6B, first COMPLETED SUCCESSFULLY prefix.</summary>
    internal const ushort CompletedMap = 0xab6b;
    /// <summary>$8C:B49B, first CLEAR TIME prefix.</summary>
    internal const ushort ClearMap = 0xb49b;
    /// <summary>$8C:B67B, first of ten two-part digit frames.</summary>
    internal const ushort ZeroMap = 0xb67b;
    /// <summary>$8C:B66F, the two-part colon frame.</summary>
    internal const ushort ColonMap = 0xb66f;
    private const int OperationLetters = 15, CompletedLetters = 21, ClearLetters = 9;
    private const int PrefixCount = OperationLetters + CompletedLetters + ClearLetters;
    private const int FrameCount = PrefixCount + 11;

    /// <summary>Stable 56-frame catalog: 15 THE OPERATION WAS prefixes at $8C:A69D, 21 COMPLETED SUCCESSFULLY prefixes at $AB6B, nine CLEAR TIME prefixes at $B49B, then ten digits at $B67B and the colon at $B66F.</summary>
    public static IReadOnlyList<EndingCompletionTextSpriteFrameDefinition> Frames { get; } = new FrameView();

    /// <summary>Sum preceding records: two header bytes and two five-byte OAM parts
    /// per revealed letter. Shared with the generated typewriter instructions.</summary>
    internal static ushort PrefixMap(ushort first, int prefix) => (ushort)(first + 2 * prefix + 5 * prefix * (prefix + 1));
    internal static ushort DigitMap(int digit) => (ushort)(ZeroMap + digit * (2 + 2 * 5));

    private static EndingCompletionTextSpriteFrameDefinition Get(int index)
    {
        if ((uint)index >= FrameCount) throw new ArgumentOutOfRangeException(nameof(index));
        if (index < OperationLetters) return Prefix("operation", OperationMap, index);
        if (index < OperationLetters + CompletedLetters) return Prefix("completed", CompletedMap, index - OperationLetters);
        if (index < PrefixCount) return Prefix("clear-time", ClearMap, index - OperationLetters - CompletedLetters);
        int glyph = index - PrefixCount;
        return glyph == 10 ? new("colon-00", ColonMap, 2) : new(Key("digit", glyph), DigitMap(glyph), 2);
    }

    private static EndingCompletionTextSpriteFrameDefinition Prefix(string family, ushort first, int stage) =>
        new(Key(family, stage), PrefixMap(first, stage), 2 * (stage + 1));
    private static string Key(string family, int stage) => family + "-" + stage.ToString("D2", System.Globalization.CultureInfo.InvariantCulture);

    private sealed class FrameView : IReadOnlyList<EndingCompletionTextSpriteFrameDefinition>
    {
        public int Count => FrameCount;
        public EndingCompletionTextSpriteFrameDefinition this[int index] => Get(index);
        public IEnumerator<EndingCompletionTextSpriteFrameDefinition> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return Get(index);
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }

}

/// <summary>One named native completion-text composition identity and its original OAM part geometry; the part count describes stock artwork rather than imposing a timing or edit restriction.</summary>
/// <param name="Name">Stable JSON key identifying a phrase prefix, decimal digit, or colon composition.</param>
/// <param name="Pointer">Bank-relative $8C spritemap pointer selected by the fixed ending instruction lists.</param>
/// <param name="StockPartCount">Original ordered OAM part count: two per revealed letter, or two for a digit or colon.</param>
public readonly record struct EndingCompletionTextSpriteFrameDefinition(
    string Name, ushort Pointer, int StockPartCount);

/// <summary>Installed filename and supported schema revision for editable completion-message OAM compositions.</summary>
public static class EndingCompletionTextSpriteFormat
{
    /// <summary>Supported composition schema revision, requiring the complete 56-name frame set.</summary>
    public const int Version = 1;
    /// <summary>Installed editable JSON filename for completion phrase prefixes and clear-time digit/colon sprites.</summary>
    public const string FileName = "ending-completion-text-sprites.json";
}
