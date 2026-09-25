using System.Text.Json;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Editable OAM compositions for the six atmospheric ending clouds.</summary>
public sealed class EndingCloudSpritePresentation : IIntroCinematicSpritePresentation
{
    private readonly Dictionary<ushort, SpriteComposition> frames;

    private EndingCloudSpritePresentation(Dictionary<ushort, SpriteComposition> frames) =>
        this.frames = frames;

    public void Draw(ushort pointer, OamBuffer oam, ushort x, ushort y,
        ushort paletteBits, bool originIsOnScreen)
    {
        if (!frames.TryGetValue(pointer, out SpriteComposition? frame))
            throw new InvalidDataException(
                $"Ending cloud sprite frame $8C:{pointer:X4} is not installed.");
        if (originIsOnScreen)
            frame.DrawOnScreen(oam, x, y, paletteBits);
        else
            frame.DrawOffScreen(oam, x, y, paletteBits);
    }

    public static EndingCloudSpritePresentation Load(Stream json)
    {
        ArgumentNullException.ThrowIfNull(json);
        EndingCloudSpriteDocument document;
        try
        {
            using JsonDocument parsed = JsonDocument.Parse(json);
            EnemySpritemapCatalog.RejectDuplicateProperties(parsed.RootElement);
            document = parsed.RootElement.Deserialize<EndingCloudSpriteDocument>(
                MapPresentationFormat.JsonOptions) ??
                throw new InvalidDataException("Ending cloud sprite JSON is null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid ending cloud sprite JSON.", error);
        }
        ReadOnlySpan<EndingCloudSpriteFrameDefinition> definitions =
            EndingCloudSpriteDefinitions.Frames;
        if (document.Version != EndingCloudSpriteFormat.Version ||
            document.Frames is null || document.Frames.Count != definitions.Length)
            throw new InvalidDataException("Ending clouds require exactly six named visual frames.");
        var frames = new Dictionary<ushort, SpriteComposition>();
        foreach (EndingCloudSpriteFrameDefinition definition in definitions)
        {
            if (!document.Frames.TryGetValue(definition.Name, out SpriteVisualPart[]? visual) ||
                visual is null)
                throw new InvalidDataException($"Ending cloud sprite {definition.Name} is missing.");
            frames.Add(definition.Pointer,
                IntroCinematicSpriteCompiler.Compile(visual, definition.Name));
        }
        return new EndingCloudSpritePresentation(frames);
    }

    public static void Write(Stream json, EndingCloudSpriteDocument document)
    {
        ArgumentNullException.ThrowIfNull(json);
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document,
            MapPresentationFormat.JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        json.Write(bytes);
    }
}

public sealed record EndingCloudSpriteDocument
{
    public required int Version { get; init; }
    public required Dictionary<string, SpriteVisualPart[]> Frames { get; init; }
}

/// <summary>
/// Cartridge identities are ordered by the six consecutive bank-$8B instruction
/// lists at $ECED, $ECF5, $ECFD, $ED05, $ED0D, and $ED15.
/// </summary>
public static class EndingCloudSpriteDefinitions
{
    private static readonly EndingCloudSpriteFrameDefinition[] StockFrames =
    [
        new("scene-b-upper-a", 0xb745, 16),
        new("scene-b-upper-b", 0xb7e9, 16),
        new("scene-b-lower-a", 0xb797, 16),
        new("scene-b-lower-b", 0xb6f3, 16),
        new("scene-a-right", 0xb83b, 32),
        new("scene-a-left", 0xb8dd, 32),
    ];

    public static ReadOnlySpan<EndingCloudSpriteFrameDefinition> Frames => StockFrames;
}

public readonly record struct EndingCloudSpriteFrameDefinition(
    string Name, ushort Pointer, int StockPartCount);

public static class EndingCloudSpriteFormat
{
    public const int Version = 1;
    public const string FileName = "ending-cloud-sprites.json";
}
