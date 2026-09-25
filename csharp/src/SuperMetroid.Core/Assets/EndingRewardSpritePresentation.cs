using System.Text.Json;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Editable OAM compositions for the 37 post-credits Samus reward frames.</summary>
public sealed class EndingRewardSpritePresentation : IIntroCinematicSpritePresentation
{
    private readonly Dictionary<ushort, SpriteComposition> frames;

    private EndingRewardSpritePresentation(Dictionary<ushort, SpriteComposition> frames) =>
        this.frames = frames;

    public void Draw(ushort pointer, OamBuffer oam, ushort x, ushort y,
        ushort paletteBits, bool originIsOnScreen)
    {
        if (!frames.TryGetValue(pointer, out SpriteComposition? frame))
            throw new InvalidDataException(
                $"Ending reward sprite frame $8C:{pointer:X4} is not installed.");
        if (originIsOnScreen)
            frame.DrawOnScreen(oam, x, y, paletteBits);
        else
            frame.DrawOffScreen(oam, x, y, paletteBits);
    }

    public static EndingRewardSpritePresentation Load(Stream json)
    {
        ArgumentNullException.ThrowIfNull(json);
        EndingRewardSpriteDocument document;
        try
        {
            using JsonDocument parsed = JsonDocument.Parse(json);
            EnemySpritemapCatalog.RejectDuplicateProperties(parsed.RootElement);
            document = parsed.RootElement.Deserialize<EndingRewardSpriteDocument>(
                MapPresentationFormat.JsonOptions) ??
                throw new InvalidDataException("Ending reward sprite JSON is null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid ending reward sprite JSON.", error);
        }
        ReadOnlySpan<EndingRewardSpriteFrameDefinition> definitions =
            EndingRewardSpriteDefinitions.Frames;
        if (document.Version != EndingRewardSpriteFormat.Version ||
            document.Frames is null || document.Frames.Count != definitions.Length)
            throw new InvalidDataException(
                $"Ending reward requires exactly {definitions.Length} named visual frames.");
        var frames = new Dictionary<ushort, SpriteComposition>();
        foreach (EndingRewardSpriteFrameDefinition definition in definitions)
        {
            if (!document.Frames.TryGetValue(definition.Name, out SpriteVisualPart[]? visual) ||
                visual is null)
                throw new InvalidDataException(
                    $"Ending reward frame {definition.Name} is missing.");
            frames.Add(definition.Pointer,
                IntroCinematicSpriteCompiler.Compile(visual, definition.Name));
        }
        return new EndingRewardSpritePresentation(frames);
    }

    public static void Write(Stream json, EndingRewardSpriteDocument document)
    {
        ArgumentNullException.ThrowIfNull(json);
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document,
            MapPresentationFormat.JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        json.Write(bytes);
    }
}

public sealed record EndingRewardSpriteDocument
{
    public required int Version { get; init; }
    public required Dictionary<string, SpriteVisualPart[]> Frames { get; init; }
}

/// <summary>
/// Cartridge OAM record identities selected by the suitless and suited reward
/// lists at $8B:ED1D..EE5C. Counts describe visuals, not actor timing.
/// </summary>
public static class EndingRewardSpriteDefinitions
{
    private static readonly EndingRewardSpriteFrameDefinition[] StockFrames =
    [
        new("suitless-idle-upper", 0x9ff7, 28),
        new("suitless-lower", 0xa243, 14),
        new("suitless-hair-1", 0xa085, 9),
        new("suitless-hair-2", 0xa0b4, 10),
        new("suitless-hair-3", 0xa0e8, 10),
        new("suitless-hair-4", 0xa11c, 10),
        new("suitless-hair-5", 0xa150, 9),
        new("suitless-hair-6", 0xa17f, 10),
        new("suitless-hair-7", 0xa1b3, 15),
        new("suitless-hair-8", 0xa200, 13),
        new("suitless-standing", 0x9ea2, 28),
        new("suitless-prepare-jump", 0x9f30, 20),
        new("suitless-jumping", 0x9f96, 19),
        new("samus-falling", 0x9d5a, 15),
        new("samus-landing", 0x9da7, 13),
        new("samus-landed", 0x9dea, 21),
        new("samus-shooting", 0x9e55, 15),
        new("suited-idle-body", 0x99d6, 34),
        new("suited-helmet-head", 0x9cac, 4),
        new("helmetless-head-1", 0x9c7c, 2),
        new("suited-headless-body", 0x9cc2, 30),
        new("suited-arm-1", 0x9b9f, 5),
        new("suited-arm-2", 0x9bba, 6),
        new("suited-arm-3", 0x9bda, 5),
        new("suited-arm-4", 0x9bf5, 5),
        new("suited-arm-5", 0x9c10, 5),
        new("suited-arm-6", 0x9c2b, 5),
        new("suited-arm-7", 0x9c46, 5),
        new("suited-arm-8", 0x9c61, 5),
        new("helmetless-head-2", 0x9c88, 2),
        new("helmetless-head-3", 0x9c94, 2),
        new("helmetless-head-4", 0x9ca0, 2),
        new("suited-prepare-jump", 0x9a82, 22),
        new("suited-jumping", 0x9af2, 20),
        new("suited-helmet-jump-head-1", 0x9b58, 5),
        new("suited-helmet-jump-head-2", 0x9b73, 5),
        new("helmetless-jump-head", 0x9b8e, 3),
    ];

    public static ReadOnlySpan<EndingRewardSpriteFrameDefinition> Frames => StockFrames;
}

public readonly record struct EndingRewardSpriteFrameDefinition(
    string Name, ushort Pointer, int StockPartCount);

public static class EndingRewardSpriteFormat
{
    public const int Version = 1;
    public const string FileName = "ending-reward-sprites.json";
}
