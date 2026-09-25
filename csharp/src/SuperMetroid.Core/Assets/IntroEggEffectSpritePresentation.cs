using System.Text.Json;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Editable egg-fragment and slime OAM art, without physical motion rules.</summary>
public sealed class IntroEggEffectSpritePresentation : IIntroCinematicSpritePresentation
{
    private readonly Dictionary<ushort, SpriteComposition> frames;

    private IntroEggEffectSpritePresentation(Dictionary<ushort, SpriteComposition> frames) =>
        this.frames = frames;

    public void Draw(ushort pointer, OamBuffer oam, ushort x, ushort y,
        ushort paletteBits, bool originIsOnScreen)
    {
        if (!frames.TryGetValue(pointer, out SpriteComposition? frame))
            throw new InvalidDataException(
                $"Intro egg effect frame $8C:{pointer:X4} is not installed.");
        if (originIsOnScreen)
            frame.DrawOnScreen(oam, x, y, paletteBits);
        else
            frame.DrawOffScreen(oam, x, y, paletteBits);
    }

    public static IntroEggEffectSpritePresentation Load(Stream json)
    {
        ArgumentNullException.ThrowIfNull(json);
        IntroEggEffectSpriteDocument document;
        try
        {
            using JsonDocument parsed = JsonDocument.Parse(json);
            EnemySpritemapCatalog.RejectDuplicateProperties(parsed.RootElement);
            document = parsed.RootElement.Deserialize<IntroEggEffectSpriteDocument>(
                MapPresentationFormat.JsonOptions) ??
                throw new InvalidDataException("Intro egg effect sprite JSON is null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid intro egg effect sprite JSON.", error);
        }
        ReadOnlySpan<IntroEggEffectSpriteFrameDefinition> definitions =
            IntroEggEffectSpriteDefinitions.Frames;
        if (document.Version != IntroEggEffectSpriteFormat.Version ||
            document.Frames is null || document.Frames.Count != definitions.Length)
            throw new InvalidDataException(
                "Intro egg effects require eleven named visual frames.");
        var frames = new Dictionary<ushort, SpriteComposition>();
        foreach (IntroEggEffectSpriteFrameDefinition definition in definitions)
        {
            if (!document.Frames.TryGetValue(definition.Name, out SpriteVisualPart[]? visual) ||
                visual is null)
                throw new InvalidDataException(
                    $"Intro egg effect frame {definition.Name} is missing.");
            frames.Add(definition.Pointer,
                IntroCinematicSpriteCompiler.Compile(visual, definition.Name));
        }
        return new IntroEggEffectSpritePresentation(frames);
    }

    public static void Write(Stream json, IntroEggEffectSpriteDocument document)
    {
        ArgumentNullException.ThrowIfNull(json);
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document,
            MapPresentationFormat.JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        json.Write(bytes);
    }
}

public sealed record IntroEggEffectSpriteDocument
{
    public required int Version { get; init; }
    public required Dictionary<string, SpriteVisualPart[]> Frames { get; init; }
}

/// <summary>Installed file identity for the eleven SR388 egg-effect frames.</summary>
public static class IntroEggEffectSpriteFormat
{
    public const int Version = 1;
    public const string FileName = "intro-egg-effect-sprites.json";
}
