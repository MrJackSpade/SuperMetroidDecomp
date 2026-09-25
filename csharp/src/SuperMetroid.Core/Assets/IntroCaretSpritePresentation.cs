using System.Text.Json;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Editable visible OAM composition of the intro caret, independent of its blink script.</summary>
public sealed class IntroCaretSpritePresentation
{
    private readonly Dictionary<ushort, SpriteComposition> frames;

    private IntroCaretSpritePresentation(Dictionary<ushort, SpriteComposition> frames) =>
        this.frames = frames;

    public void Draw(ushort pointer, OamBuffer oam, ushort x, ushort y, ushort paletteBits)
    {
        if (!frames.TryGetValue(pointer, out SpriteComposition? frame))
            throw new InvalidDataException($"Opening caret frame $8C:{pointer:X4} is not installed.");
        frame.DrawOnScreen(oam, x, y, paletteBits);
    }

    public static IntroCaretSpritePresentation Load(Stream json)
    {
        ArgumentNullException.ThrowIfNull(json);
        IntroCaretSpriteDocument document;
        try
        {
            using JsonDocument parsed = JsonDocument.Parse(json);
            EnemySpritemapCatalog.RejectDuplicateProperties(parsed.RootElement);
            document = parsed.RootElement.Deserialize<IntroCaretSpriteDocument>(
                MapPresentationFormat.JsonOptions) ??
                throw new InvalidDataException("Opening caret composition JSON is null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid opening caret composition JSON.", error);
        }
        ReadOnlySpan<IntroCaretFrameDefinition> definitions = IntroCaretSpriteDefinitions.Frames;
        bool previousVersion = document.Version == IntroCaretSpriteFormat.PreviousVersion;
        if (document.Frames is null ||
            (previousVersion
                ? document.Frames.Count != IntroCaretSpriteDefinitions.PreviousFrameNames.Length ||
                    IntroCaretSpriteDefinitions.PreviousFrameNames.Any(name =>
                        !document.Frames.TryGetValue(name, out SpriteVisualPart[]? parts) ||
                        parts is null || parts.Length > IntroCaretSpriteDefinitions.MaximumParts)
                : document.Version != IntroCaretSpriteFormat.Version ||
                    document.Frames.Count != definitions.Length))
            throw new InvalidDataException("Opening caret requires its one visible sprite frame.");

        var frames = new Dictionary<ushort, SpriteComposition>();
        foreach (IntroCaretFrameDefinition definition in definitions)
        {
            string sourceName = previousVersion
                ? IntroCaretSpriteDefinitions.PreviousFrameNames[0] : definition.Name;
            if (!document.Frames.TryGetValue(sourceName, out SpriteVisualPart[]? visual) ||
                visual is null || visual.Length > IntroCaretSpriteDefinitions.MaximumParts)
                throw new InvalidDataException($"Opening caret frame {definition.Name} is missing or too large.");
            frames.Add(definition.Pointer,
                IntroCinematicSpriteCompiler.Compile(visual, definition.Name));
        }
        return new IntroCaretSpritePresentation(frames);
    }

    public static void Write(Stream json, IntroCaretSpriteDocument document)
    {
        ArgumentNullException.ThrowIfNull(json);
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document,
            MapPresentationFormat.JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        json.Write(bytes);
    }
}

public sealed record IntroCaretSpriteDocument
{
    public required int Version { get; init; }
    public required Dictionary<string, SpriteVisualPart[]> Frames { get; init; }
}

/// <summary>Installed file identity and schema version for opening caret compositions.</summary>
public static class IntroCaretSpriteFormat
{
    public const int Version = 2;
    public const int PreviousVersion = 1;
    public const string FileName = "intro-caret-sprites.json";
}
