using System.Text.Json;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Editable OAM compositions for the eight final assembling-logo frames.</summary>
public sealed class EndingLogoSpritePresentation : IIntroCinematicSpritePresentation
{
    private readonly Dictionary<ushort, SpriteComposition> frames;

    private EndingLogoSpritePresentation(Dictionary<ushort, SpriteComposition> frames) =>
        this.frames = frames;

    /// <summary>Canonical identity of the selected decoded visual frames, not JSON formatting.</summary>
    public string ContentIdentity => SelectedPresentationHash.FromCompositions(nameof(EndingLogoSpritePresentation), frames);

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
        IReadOnlyList<EndingLogoSpriteFrameDefinition> definitions =
            EndingLogoSpriteDefinitions.Frames;
        if (document.Version != EndingLogoSpriteFormat.Version ||
            document.Frames is null || document.Frames.Count != definitions.Count)
            throw new InvalidDataException(
                $"Ending logo requires exactly {definitions.Count} named visual frames.");
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
    /// <summary>$8C:B97F, ScrewAttackSymbolUpperPart, followed by the lower S,
    /// three right-wrap and three left-wrap compositions.</summary>
    private const ushort FirstFrame = 0xb97f;
    private const int FrameCount = 8, SParts = 14;
    private enum CircleStage { First, Second, Complete }

    public static IReadOnlyList<EndingLogoSpriteFrameDefinition> Frames { get; } = new FrameView();

    internal static ushort FramePointer(int index)
    {
        if ((uint)index >= FrameCount) throw new ArgumentOutOfRangeException(nameof(index));
        if (index < 2) return (ushort)(FirstFrame + index * RecordBytes(SParts));
        int circle = (index - 2) / 3;
        int stage = (index - 2) % 3;
        int offset = 2 * RecordBytes(SParts);
        for (int preceding = 0; preceding < 3; preceding++)
            offset += circle * RecordBytes(CircleParts((CircleStage)preceding));
        for (int preceding = 0; preceding < stage; preceding++)
            offset += RecordBytes(CircleParts((CircleStage)preceding));
        return (ushort)(FirstFrame + offset);
    }

    private static int CircleParts(CircleStage stage) => stage switch
    {
        CircleStage.First => 12,
        CircleStage.Second => 18,
        CircleStage.Complete => 25,
        _ => throw new ArgumentOutOfRangeException(nameof(stage)),
    };
    private static int RecordBytes(int parts) => sizeof(ushort) + parts * 5;

    private static EndingLogoSpriteFrameDefinition Get(int index)
    {
        ushort pointer = FramePointer(index);
        if (index < 2) return new(index == 0 ? "s-upper" : "s-lower", pointer, SParts);
        int stage = (index - 2) % 3;
        string side = index < 5 ? "right" : "left";
        string name = "circle-" + side + "-" + (stage + 1).ToString(System.Globalization.CultureInfo.InvariantCulture);
        return new(name, pointer, CircleParts((CircleStage)stage));
    }

    private sealed class FrameView : IReadOnlyList<EndingLogoSpriteFrameDefinition>
    {
        public int Count => FrameCount;
        public EndingLogoSpriteFrameDefinition this[int index] => Get(index);
        public IEnumerator<EndingLogoSpriteFrameDefinition> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return Get(index);
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }

}

public readonly record struct EndingLogoSpriteFrameDefinition(
    string Name, ushort Pointer, int StockPartCount);

public static class EndingLogoSpriteFormat
{
    public const int Version = 1;
    public const string FileName = "ending-logo-sprites.json";
}
