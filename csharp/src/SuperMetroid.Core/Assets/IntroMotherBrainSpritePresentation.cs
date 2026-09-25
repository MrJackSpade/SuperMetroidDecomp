using System.Text.Json;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Editable intro Mother Brain OAM art, separate from AI and instruction timing.</summary>
public sealed class IntroMotherBrainSpritePresentation
{
    private readonly Dictionary<ushort, SpriteComposition> frames;

    private IntroMotherBrainSpritePresentation(Dictionary<ushort, SpriteComposition> frames) =>
        this.frames = frames;

    public void Draw(ushort pointer, OamBuffer oam, ushort x, ushort y, ushort paletteBits)
    {
        if (!frames.TryGetValue(pointer, out SpriteComposition? frame))
            throw new InvalidDataException($"Intro Mother Brain frame $8C:{pointer:X4} is not installed.");
        frame.DrawOnScreen(oam, x, y, paletteBits);
    }

    public static IntroMotherBrainSpritePresentation Load(Stream json)
    {
        ArgumentNullException.ThrowIfNull(json);
        IntroMotherBrainSpriteDocument document;
        try
        {
            using JsonDocument parsed = JsonDocument.Parse(json);
            EnemySpritemapCatalog.RejectDuplicateProperties(parsed.RootElement);
            document = parsed.RootElement.Deserialize<IntroMotherBrainSpriteDocument>(
                MapPresentationFormat.JsonOptions) ??
                throw new InvalidDataException("Intro Mother Brain sprite JSON is null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid intro Mother Brain sprite JSON.", error);
        }
        ReadOnlySpan<IntroMotherBrainSpriteFrameDefinition> definitions =
            IntroMotherBrainSpriteDefinitions.Frames;
        if (document.Version != IntroMotherBrainSpriteFormat.Version ||
            document.Frames is null || document.Frames.Count != definitions.Length)
            throw new InvalidDataException("Intro Mother Brain requires three named visual frames.");
        var frames = new Dictionary<ushort, SpriteComposition>();
        foreach (IntroMotherBrainSpriteFrameDefinition definition in definitions)
        {
            if (!document.Frames.TryGetValue(definition.Name, out SpriteVisualPart[]? visual) ||
                visual is null)
                throw new InvalidDataException($"Intro Mother Brain frame {definition.Name} is missing.");
            frames.Add(definition.Pointer,
                IntroCinematicSpriteCompiler.Compile(visual, definition.Name));
        }
        return new IntroMotherBrainSpritePresentation(frames);
    }

    public static void Write(Stream json, IntroMotherBrainSpriteDocument document)
    {
        ArgumentNullException.ThrowIfNull(json);
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document,
            MapPresentationFormat.JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        json.Write(bytes);
    }
}

public sealed record IntroMotherBrainSpriteDocument
{
    public required int Version { get; init; }
    public required Dictionary<string, SpriteVisualPart[]> Frames { get; init; }
}

/// <summary>Installed file identity for the three intro Mother Brain visual frames.</summary>
public static class IntroMotherBrainSpriteFormat
{
    public const int Version = 1;
    public const string FileName = "intro-mother-brain-sprites.json";
}
