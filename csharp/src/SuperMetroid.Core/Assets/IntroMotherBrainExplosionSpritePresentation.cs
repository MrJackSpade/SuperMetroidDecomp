using System.Text.Json;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Editable explosion OAM art, independent of native placement and timing.</summary>
public sealed class IntroMotherBrainExplosionSpritePresentation
{
    private readonly Dictionary<ushort, SpriteComposition> frames;

    private IntroMotherBrainExplosionSpritePresentation(
        Dictionary<ushort, SpriteComposition> frames) => this.frames = frames;

    public void Draw(ushort pointer, OamBuffer oam, ushort x, ushort y, ushort paletteBits)
    {
        if (!frames.TryGetValue(pointer, out SpriteComposition? frame))
            throw new InvalidDataException(
                $"Intro Mother Brain explosion frame $8C:{pointer:X4} is not installed.");
        frame.DrawOnScreen(oam, x, y, paletteBits);
    }

    public static IntroMotherBrainExplosionSpritePresentation Load(Stream json)
    {
        ArgumentNullException.ThrowIfNull(json);
        IntroMotherBrainExplosionSpriteDocument document;
        try
        {
            using JsonDocument parsed = JsonDocument.Parse(json);
            EnemySpritemapCatalog.RejectDuplicateProperties(parsed.RootElement);
            document = parsed.RootElement.Deserialize<IntroMotherBrainExplosionSpriteDocument>(
                MapPresentationFormat.JsonOptions) ??
                throw new InvalidDataException("Intro Mother Brain explosion sprite JSON is null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid intro Mother Brain explosion sprite JSON.", error);
        }
        ReadOnlySpan<IntroMotherBrainExplosionSpriteFrameDefinition> definitions =
            IntroMotherBrainExplosionSpriteDefinitions.Frames;
        if (document.Version != IntroMotherBrainExplosionSpriteFormat.Version ||
            document.Frames is null || document.Frames.Count != definitions.Length)
            throw new InvalidDataException(
                "Intro Mother Brain explosions require twelve named visual frames.");
        var frames = new Dictionary<ushort, SpriteComposition>();
        foreach (IntroMotherBrainExplosionSpriteFrameDefinition definition in definitions)
        {
            if (!document.Frames.TryGetValue(definition.Name, out SpriteVisualPart[]? visual) ||
                visual is null)
                throw new InvalidDataException(
                    $"Intro Mother Brain explosion frame {definition.Name} is missing.");
            frames.Add(definition.Pointer,
                IntroCinematicSpriteCompiler.Compile(visual, definition.Name));
        }
        return new IntroMotherBrainExplosionSpritePresentation(frames);
    }

    public static void Write(Stream json, IntroMotherBrainExplosionSpriteDocument document)
    {
        ArgumentNullException.ThrowIfNull(json);
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document,
            MapPresentationFormat.JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        json.Write(bytes);
    }
}

public sealed record IntroMotherBrainExplosionSpriteDocument
{
    public required int Version { get; init; }
    public required Dictionary<string, SpriteVisualPart[]> Frames { get; init; }
}

/// <summary>Installed file identity for twelve fourth-hit explosion frames.</summary>
public static class IntroMotherBrainExplosionSpriteFormat
{
    public const int Version = 1;
    public const string FileName = "intro-mother-brain-explosion-sprites.json";
}
