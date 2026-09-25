using System.Text.Json;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Editable OAM compositions for the 56 completion-message glyph frames.</summary>
public sealed class EndingCompletionTextSpritePresentation : IIntroCinematicSpritePresentation
{
    private readonly Dictionary<ushort, SpriteComposition> frames;

    private EndingCompletionTextSpritePresentation(Dictionary<ushort, SpriteComposition> frames) =>
        this.frames = frames;

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
        ReadOnlySpan<EndingCompletionTextSpriteFrameDefinition> definitions =
            EndingCompletionTextSpriteDefinitions.Frames;
        if (document.Version != EndingCompletionTextSpriteFormat.Version ||
            document.Frames is null || document.Frames.Count != definitions.Length)
            throw new InvalidDataException(
                $"Ending completion text requires exactly {definitions.Length} named visual frames.");
        var frames = new Dictionary<ushort, SpriteComposition>();
        foreach (EndingCompletionTextSpriteFrameDefinition definition in definitions)
        {
            if (!document.Frames.TryGetValue(definition.Name, out SpriteVisualPart[]? visual) ||
                visual is null)
                throw new InvalidDataException(
                    $"Ending completion text frame {definition.Name} is missing.");
            frames.Add(definition.Pointer,
                IntroCinematicSpriteCompiler.Compile(visual, definition.Name));
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
    private static readonly EndingCompletionTextSpriteFrameDefinition[] StockFrames =
    [
        new("operation-00", 0xa69d, 2),
        new("operation-01", 0xa6a9, 4),
        new("operation-02", 0xa6bf, 6),
        new("operation-03", 0xa6df, 8),
        new("operation-04", 0xa709, 10),
        new("operation-05", 0xa73d, 12),
        new("operation-06", 0xa77b, 14),
        new("operation-07", 0xa7c3, 16),
        new("operation-08", 0xa815, 18),
        new("operation-09", 0xa871, 20),
        new("operation-10", 0xa8d7, 22),
        new("operation-11", 0xa947, 24),
        new("operation-12", 0xa9c1, 26),
        new("operation-13", 0xaa45, 28),
        new("operation-14", 0xaad3, 30),
        new("completed-00", 0xab6b, 2),
        new("completed-01", 0xab77, 4),
        new("completed-02", 0xab8d, 6),
        new("completed-03", 0xabad, 8),
        new("completed-04", 0xabd7, 10),
        new("completed-05", 0xac0b, 12),
        new("completed-06", 0xac49, 14),
        new("completed-07", 0xac91, 16),
        new("completed-08", 0xace3, 18),
        new("completed-09", 0xad3f, 20),
        new("completed-10", 0xada5, 22),
        new("completed-11", 0xae15, 24),
        new("completed-12", 0xae8f, 26),
        new("completed-13", 0xaf13, 28),
        new("completed-14", 0xafa1, 30),
        new("completed-15", 0xb039, 32),
        new("completed-16", 0xb0db, 34),
        new("completed-17", 0xb187, 36),
        new("completed-18", 0xb23d, 38),
        new("completed-19", 0xb2fd, 40),
        new("completed-20", 0xb3c7, 42),
        new("clear-time-00", 0xb49b, 2),
        new("clear-time-01", 0xb4a7, 4),
        new("clear-time-02", 0xb4bd, 6),
        new("clear-time-03", 0xb4dd, 8),
        new("clear-time-04", 0xb507, 10),
        new("clear-time-05", 0xb53b, 12),
        new("clear-time-06", 0xb579, 14),
        new("clear-time-07", 0xb5c1, 16),
        new("clear-time-08", 0xb613, 18),
        new("digit-00", 0xb67b, 2),
        new("digit-01", 0xb687, 2),
        new("digit-02", 0xb693, 2),
        new("digit-03", 0xb69f, 2),
        new("digit-04", 0xb6ab, 2),
        new("digit-05", 0xb6b7, 2),
        new("digit-06", 0xb6c3, 2),
        new("digit-07", 0xb6cf, 2),
        new("digit-08", 0xb6db, 2),
        new("digit-09", 0xb6e7, 2),
        new("colon-00", 0xb66f, 2),
    ];

    public static ReadOnlySpan<EndingCompletionTextSpriteFrameDefinition> Frames => StockFrames;
}

public readonly record struct EndingCompletionTextSpriteFrameDefinition(
    string Name, ushort Pointer, int StockPartCount);

public static class EndingCompletionTextSpriteFormat
{
    public const int Version = 1;
    public const string FileName = "ending-completion-text-sprites.json";
}
