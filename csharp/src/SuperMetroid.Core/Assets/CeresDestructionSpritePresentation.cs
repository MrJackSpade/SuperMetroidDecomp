using System.Text.Json;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Editable OAM compositions unique to the station blast and Zebes reveal.</summary>
public sealed class CeresDestructionSpritePresentation : IIntroCinematicSpritePresentation
{
    private readonly Dictionary<ushort, SpriteComposition> frames;

    private CeresDestructionSpritePresentation(Dictionary<ushort, SpriteComposition> frames) =>
        this.frames = frames;

    public bool Contains(ushort pointer) => frames.ContainsKey(pointer);

    public void Draw(ushort pointer, OamBuffer oam, ushort x, ushort y,
        ushort paletteBits, bool originIsOnScreen)
    {
        if (!frames.TryGetValue(pointer, out SpriteComposition? frame))
            throw new InvalidDataException(
                $"Ceres destruction sprite frame $8C:{pointer:X4} is not installed.");
        if (originIsOnScreen)
            frame.DrawOnScreen(oam, x, y, paletteBits);
        else
            frame.DrawOffScreen(oam, x, y, paletteBits);
    }

    public static CeresDestructionSpritePresentation Load(Stream json)
    {
        ArgumentNullException.ThrowIfNull(json);
        CeresDestructionSpriteDocument document;
        try
        {
            using JsonDocument parsed = JsonDocument.Parse(json);
            EnemySpritemapCatalog.RejectDuplicateProperties(parsed.RootElement);
            document = parsed.RootElement.Deserialize<CeresDestructionSpriteDocument>(
                MapPresentationFormat.JsonOptions) ??
                throw new InvalidDataException("Ceres destruction sprite JSON is null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid Ceres destruction sprite JSON.", error);
        }
        ReadOnlySpan<CeresDestructionSpriteFrameDefinition> definitions =
            CeresDestructionSpriteDefinitions.Frames;
        if (document.Version != CeresDestructionSpriteFormat.Version ||
            document.Frames is null || document.Frames.Count != definitions.Length)
            throw new InvalidDataException(
                $"Ceres destruction requires exactly {definitions.Length} named visual frames.");
        var frames = new Dictionary<ushort, SpriteComposition>();
        foreach (CeresDestructionSpriteFrameDefinition definition in definitions)
        {
            if (!document.Frames.TryGetValue(definition.Name, out SpriteVisualPart[]? visual) ||
                visual is null)
                throw new InvalidDataException(
                    $"Ceres destruction sprite {definition.Name} is missing.");
            frames.Add(definition.Pointer,
                IntroCinematicSpriteCompiler.Compile(visual, definition.Name));
        }
        return new CeresDestructionSpritePresentation(frames);
    }

    public static void Write(Stream json, CeresDestructionSpriteDocument document)
    {
        ArgumentNullException.ThrowIfNull(json);
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document,
            MapPresentationFormat.JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        json.Write(bytes);
    }
}

/// <summary>Routes the scene's shared approach frames and unique destruction frames.</summary>
internal sealed class CeresSceneSpritePresentation(
    CeresFlightSpritePresentation flight,
    CeresDestructionSpritePresentation destruction) : IIntroCinematicSpritePresentation
{
    public void Draw(ushort pointer, OamBuffer oam, ushort x, ushort y,
        ushort paletteBits, bool originIsOnScreen)
    {
        IIntroCinematicSpritePresentation owner = destruction.Contains(pointer)
            ? destruction : flight;
        owner.Draw(pointer, oam, x, y, paletteBits, originIsOnScreen);
    }
}

public sealed record CeresDestructionSpriteDocument
{
    public required int Version { get; init; }
    public required Dictionary<string, SpriteVisualPart[]> Frames { get; init; }
}

/// <summary>Native bank-$8C frame identities consumed only by destruction/reveal actors.</summary>
public static class CeresDestructionSpriteDefinitions
{
    private static readonly CeresDestructionSpriteFrameDefinition[] StockFrames =
    [
        new("station-under-attack-large-asteroid", 0x909d, 19),
        new("planet-zebes", 0x9558, 50),
        new("planet-zebes-title", 0x9654, 11),
        new("zebes-stars-upper-left", 0x975e, 12),
        new("zebes-stars-upper-right", 0x979c, 6),
        new("zebes-stars-lower-left", 0x97bc, 4),
        new("zebes-stars-lower-right", 0x97d2, 7),
        new("small-blast-0", 0x97f7, 1),
        new("small-blast-1", 0x97fe, 1),
        new("small-blast-2", 0x9805, 4),
        new("small-blast-3", 0x981b, 4),
        new("small-blast-4", 0x9831, 4),
        new("small-blast-5", 0x9847, 4),
        new("large-blast-0", 0x98d2, 1),
        new("large-blast-1", 0x98d9, 1),
        new("large-blast-2", 0x98e0, 1),
        new("large-blast-3", 0x98e7, 1),
        new("station-blast-0", 0x98ee, 4),
        new("station-blast-1", 0x9904, 4),
        new("station-blast-2", 0x991a, 4),
        new("station-blast-3", 0x9930, 12),
        new("station-blast-4", 0x996e, 8),
        new("station-blast-5", 0x9998, 12),
    ];

    public static ReadOnlySpan<CeresDestructionSpriteFrameDefinition> Frames => StockFrames;
}

public readonly record struct CeresDestructionSpriteFrameDefinition(
    string Name, ushort Pointer, int StockPartCount);

public static class CeresDestructionSpriteFormat
{
    public const int Version = 1;
    public const string FileName = "ceres-destruction-sprites.json";
}
