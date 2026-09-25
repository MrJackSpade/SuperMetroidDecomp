using System.Text.Json;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Editable OAM compositions for the 16 Zebes-explosion visual frames.</summary>
public sealed class EndingExplosionSpritePresentation : IIntroCinematicSpritePresentation
{
    private readonly Dictionary<ushort, SpriteComposition> frames;

    private EndingExplosionSpritePresentation(Dictionary<ushort, SpriteComposition> frames) =>
        this.frames = frames;

    public void Draw(ushort pointer, OamBuffer oam, ushort x, ushort y,
        ushort paletteBits, bool originIsOnScreen)
    {
        if (!frames.TryGetValue(pointer, out SpriteComposition? frame))
            throw new InvalidDataException(
                $"Ending explosion sprite frame $8C:{pointer:X4} is not installed.");
        if (originIsOnScreen)
            frame.DrawOnScreen(oam, x, y, paletteBits);
        else
            frame.DrawOffScreen(oam, x, y, paletteBits);
    }

    public static EndingExplosionSpritePresentation Load(Stream json)
    {
        ArgumentNullException.ThrowIfNull(json);
        EndingExplosionSpriteDocument document;
        try
        {
            using JsonDocument parsed = JsonDocument.Parse(json);
            EnemySpritemapCatalog.RejectDuplicateProperties(parsed.RootElement);
            document = parsed.RootElement.Deserialize<EndingExplosionSpriteDocument>(
                MapPresentationFormat.JsonOptions) ??
                throw new InvalidDataException("Ending explosion sprite JSON is null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid ending explosion sprite JSON.", error);
        }
        ReadOnlySpan<EndingExplosionSpriteFrameDefinition> definitions =
            EndingExplosionSpriteDefinitions.Frames;
        if (document.Version != EndingExplosionSpriteFormat.Version ||
            document.Frames is null || document.Frames.Count != definitions.Length)
            throw new InvalidDataException(
                $"Ending explosion requires exactly {definitions.Length} named visual frames.");
        var frames = new Dictionary<ushort, SpriteComposition>();
        foreach (EndingExplosionSpriteFrameDefinition definition in definitions)
        {
            if (!document.Frames.TryGetValue(definition.Name, out SpriteVisualPart[]? visual) ||
                visual is null)
                throw new InvalidDataException(
                    $"Ending explosion sprite {definition.Name} is missing.");
            frames.Add(definition.Pointer,
                IntroCinematicSpriteCompiler.Compile(visual, definition.Name));
        }
        return new EndingExplosionSpritePresentation(frames);
    }

    public static void Write(Stream json, EndingExplosionSpriteDocument document)
    {
        ArgumentNullException.ThrowIfNull(json);
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document,
            MapPresentationFormat.JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        json.Write(bytes);
    }
}

public sealed record EndingExplosionSpriteDocument
{
    public required int Version { get; init; }
    public required Dictionary<string, SpriteVisualPart[]> Frames { get; init; }
}

/// <summary>Distinct bank-$8C frame identities consumed by the eight explosion actors.</summary>
public static class EndingExplosionSpriteDefinitions
{
    private static readonly EndingExplosionSpriteFrameDefinition[] StockFrames =
    [
        new("planet-damage-0", 0xa396, 4),
        new("planet-damage-1", 0xa3ac, 4),
        new("planet-damage-2", 0xa3c2, 4),
        new("planet-damage-3", 0xa3d8, 4),
        new("planet-flash-0", 0xa3ee, 4),
        new("planet-flash-1", 0xa404, 4),
        new("planet-flash-2", 0xa41a, 4),
        new("planet-flash-3", 0xa430, 4),
        new("lava-0", 0xa446, 4),
        new("lava-1", 0xa45c, 4),
        new("glow-0", 0xa472, 12),
        new("glow-1", 0xa4b0, 20),
        new("glow-2", 0xa516, 20),
        new("starfield", 0xa28b, 53),
        new("silhouette", 0xa57c, 20),
        new("afterglow", 0xa5e2, 37),
    ];

    public static ReadOnlySpan<EndingExplosionSpriteFrameDefinition> Frames => StockFrames;
}

public readonly record struct EndingExplosionSpriteFrameDefinition(
    string Name, ushort Pointer, int StockPartCount);

public static class EndingExplosionSpriteFormat
{
    public const int Version = 1;
    public const string FileName = "ending-explosion-sprites.json";
}
