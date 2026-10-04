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

    public static void Write(Stream json, EndingCompletionTextSpriteDocument document)
    {
        ArgumentNullException.ThrowIfNull(json);
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document,
            MapPresentationFormat.JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        json.Write(bytes);
    }
}

public sealed record EndingCompletionTextSpriteDocument
{
    public required int Version { get; init; }
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

public readonly record struct EndingCompletionTextSpriteFrameDefinition(
    string Name, ushort Pointer, int StockPartCount);

public static class EndingCompletionTextSpriteFormat
{
    public const int Version = 1;
    public const string FileName = "ending-completion-text-sprites.json";
}
