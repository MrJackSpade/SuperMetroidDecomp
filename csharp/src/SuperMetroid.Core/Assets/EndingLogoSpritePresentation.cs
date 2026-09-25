using System.Text.Json;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Editable OAM compositions for the eight final assembling-logo frames.</summary>
public sealed class EndingLogoSpritePresentation : IIntroCinematicSpritePresentation
{
    private readonly Dictionary<ushort, SpriteComposition> frames;

    private EndingLogoSpritePresentation(Dictionary<ushort, SpriteComposition> frames) =>
        this.frames = frames;

    public void Draw(ushort pointer, OamBuffer oam, ushort x, ushort y,
        ushort paletteBits, bool originIsOnScreen)
    {
        if (!frames.TryGetValue(pointer, out SpriteComposition? frame))
            throw new InvalidDataException(
                $"Ending logo sprite frame $8C:{pointer:X4} is not installed.");
        if (originIsOnScreen)
            frame.DrawOnScreen(oam, x, y, paletteBits);
        else
            frame.DrawOffScreen(oam, x, y, paletteBits);
    }

    public static EndingLogoSpritePresentation Load(Stream json)
    {
        ArgumentNullException.ThrowIfNull(json);
        EndingLogoSpriteDocument document;
        try
        {
            using JsonDocument parsed = JsonDocument.Parse(json);
            EnemySpritemapCatalog.RejectDuplicateProperties(parsed.RootElement);
            document = parsed.RootElement.Deserialize<EndingLogoSpriteDocument>(
                MapPresentationFormat.JsonOptions) ??
                throw new InvalidDataException("Ending logo sprite JSON is null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid ending logo sprite JSON.", error);
        }
        ReadOnlySpan<EndingLogoSpriteFrameDefinition> definitions =
            EndingLogoSpriteDefinitions.Frames;
        if (document.Version != EndingLogoSpriteFormat.Version ||
            document.Frames is null || document.Frames.Count != definitions.Length)
            throw new InvalidDataException(
                $"Ending logo requires exactly {definitions.Length} named visual frames.");
        var frames = new Dictionary<ushort, SpriteComposition>();
        foreach (EndingLogoSpriteFrameDefinition definition in definitions)
        {
            if (!document.Frames.TryGetValue(definition.Name, out SpriteVisualPart[]? visual) ||
                visual is null)
                throw new InvalidDataException(
                    $"Ending logo frame {definition.Name} is missing.");
            frames.Add(definition.Pointer,
                IntroCinematicSpriteCompiler.Compile(visual, definition.Name));
        }
        return new EndingLogoSpritePresentation(frames);
    }

    public static void Write(Stream json, EndingLogoSpriteDocument document)
    {
        ArgumentNullException.ThrowIfNull(json);
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document,
            MapPresentationFormat.JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        json.Write(bytes);
    }
}

public sealed record EndingLogoSpriteDocument
{
    public required int Version { get; init; }
    public required Dictionary<string, SpriteVisualPart[]> Frames { get; init; }
}

/// <summary>Cartridge OAM identities paired with the four $8B:EE5D..EE9A logo lists.</summary>
public static class EndingLogoSpriteDefinitions
{
    private static readonly EndingLogoSpriteFrameDefinition[] StockFrames =
    [
        new("s-upper", 0xb97f, 14),
        new("s-lower", 0xb9c7, 14),
        new("circle-right-1", 0xba0f, 12),
        new("circle-right-2", 0xba4d, 18),
        new("circle-right-3", 0xbaa9, 25),
        new("circle-left-1", 0xbb28, 12),
        new("circle-left-2", 0xbb66, 18),
        new("circle-left-3", 0xbbc2, 25),
    ];

    public static ReadOnlySpan<EndingLogoSpriteFrameDefinition> Frames => StockFrames;
}

public readonly record struct EndingLogoSpriteFrameDefinition(
    string Name, ushort Pointer, int StockPartCount);

public static class EndingLogoSpriteFormat
{
    public const int Version = 1;
    public const string FileName = "ending-logo-sprites.json";
}
