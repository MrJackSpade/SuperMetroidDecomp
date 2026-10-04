using System.Text.Json;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Editable OAM compositions for the six atmospheric ending clouds.</summary>
public sealed class EndingCloudSpritePresentation : IIntroCinematicSpritePresentation
{
    private readonly Dictionary<ushort, SpriteComposition> frames;

    private EndingCloudSpritePresentation(Dictionary<ushort, SpriteComposition> frames) =>
        this.frames = frames;

    /// <summary>Canonical identity of the selected decoded visual frames, not JSON formatting.</summary>
    public string ContentIdentity => SelectedPresentationHash.FromCompositions(nameof(EndingCloudSpritePresentation), frames);

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
        IReadOnlyList<EndingCloudSpriteFrameDefinition> definitions =
            EndingCloudSpriteDefinitions.Frames;
        if (document.Version != EndingCloudSpriteFormat.Version ||
            document.Frames is null || document.Frames.Count != definitions.Count)
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
    /// <summary>$8C:B6F3, EndingCutsceneBottomCloudsPattern; four16-part records
    /// precede the right and left32-part records.</summary>
    private const ushort FirstRecord = 0xb6f3;
    private const int FrameCount = 6;
    // Native storage order is distinct from the actor/list order.
    private enum Record { BottomPattern, TopPattern, BottomEdge, TopEdge, Right, Left }
    private enum Role { UpperPattern, UpperEdge, LowerEdge, LowerPattern, Right, Left }

    public static IReadOnlyList<EndingCloudSpriteFrameDefinition> Frames { get; } = new FrameView();

    private static EndingCloudSpriteFrameDefinition Get(int index) => (Role)index switch
    {
        Role.UpperPattern => Define("scene-b-upper-a", Record.TopPattern),
        Role.UpperEdge => Define("scene-b-upper-b", Record.TopEdge),
        Role.LowerEdge => Define("scene-b-lower-a", Record.BottomEdge),
        Role.LowerPattern => Define("scene-b-lower-b", Record.BottomPattern),
        Role.Right => Define("scene-a-right", Record.Right),
        Role.Left => Define("scene-a-left", Record.Left),
        _ => throw new ArgumentOutOfRangeException(nameof(index)),
    };

    private static EndingCloudSpriteFrameDefinition Define(string name, Record record)
    {
        int index = (int)record;
        int horizontalRecords = Math.Min(index, 4);
        int sideRecords = Math.Max(index - 4, 0);
        int offset = horizontalRecords * (2 + 16 * 5) + sideRecords * (2 + 32 * 5);
        return new(name, (ushort)(FirstRecord + offset), index < 4 ? 16 : 32);
    }

    private sealed class FrameView : IReadOnlyList<EndingCloudSpriteFrameDefinition>
    {
        public int Count => FrameCount;
        public EndingCloudSpriteFrameDefinition this[int index] => Get(index);
        public IEnumerator<EndingCloudSpriteFrameDefinition> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return Get(index);
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }

}

public readonly record struct EndingCloudSpriteFrameDefinition(
    string Name, ushort Pointer, int StockPartCount);

public static class EndingCloudSpriteFormat
{
    public const int Version = 1;
    public const string FileName = "ending-cloud-sprites.json";
}
