using System.Text.Json;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.Core.Assets;

/// <summary>Editable baby-Metroid OAM art for the two scientist scenes only.</summary>
public sealed class IntroScientistSpritePresentation : IIntroCinematicSpritePresentation
{
    /// <summary>Validated baby-Metroid compositions keyed by bank-$8C spritemap identity.</summary>
    private readonly Dictionary<ushort, SpriteComposition> frames;

    /// <summary>Canonical identity of all selected decoded visual frames, preserving ordered OAM parts.</summary>
    public string ContentIdentity => SelectedPresentationHash.FromCompositions(nameof(IntroScientistSpritePresentation), frames);

    /// <summary>Retains the ten validated frames after JSON compilation.</summary>
    private IntroScientistSpritePresentation(Dictionary<ushort, SpriteComposition> frames) =>
        this.frames = frames;

    /// <summary>Draws one installed baby-Metroid composition used by the two scientist scenes.</summary>
    /// <param name="pointer">Native bank-$8C spritemap identity for one of the ten frames.</param>
    /// <param name="oam">The OAM buffer receiving the ordered sprite parts.</param>
    /// <param name="origin">Native wrapped sixteen-bit screen origin in pixels.</param>
    /// <param name="paletteBits">OBJ palette attribute bits applied to inherited-palette parts.</param>
    /// <param name="originIsOnScreen">Whether the anchor is already in screen space rather than the off-screen cinematic space.</param>
    public void Draw(ushort pointer, OamBuffer oam, Position16<OamPixels> origin,
        ushort paletteBits, bool originIsOnScreen)
    {
        if (!frames.TryGetValue(pointer, out SpriteComposition? frame))
            throw new InvalidDataException(
                $"Intro scientist baby frame $8C:{pointer:X4} is not installed.");
        if (originIsOnScreen)
            frame.DrawOnScreen(oam, origin, paletteBits);
        else
            frame.DrawOffScreen(oam, origin, paletteBits);
    }

    /// <summary>Loads and validates all ten baby-Metroid visual frames for the scientist scenes.</summary>
    /// <param name="json">The caller-owned stream containing the editable sprite document.</param>
    /// <returns>The compiled presentation keyed by native bank-$8C spritemap identities.</returns>
    /// <exception cref="InvalidDataException">The JSON, version, frame set, or sprite parts are invalid.</exception>
    public static IntroScientistSpritePresentation Load(Stream json)
    {
        Ensure.NotNull(json);
        IntroScientistSpriteDocument document;
        try
        {
            using JsonDocument parsed = JsonDocument.Parse(json);
            EnemySpritemapCatalog.RejectDuplicateProperties(parsed.RootElement);
            document = parsed.RootElement.Deserialize<IntroScientistSpriteDocument>(
                MapPresentationFormat.JsonOptions) ??
                throw new InvalidDataException("Intro scientist sprite JSON is null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid intro scientist sprite JSON.", error);
        }
        IReadOnlyList<IntroScientistSpriteFrameDefinition> definitions =
            IntroScientistSpriteDefinitions.Frames;
        if (document.Version != IntroScientistSpriteFormat.Version ||
            document.Frames is null || document.Frames.Count != definitions.Count)
            throw new InvalidDataException("Intro scientist scenes require ten named visual frames.");
        var frames = new Dictionary<ushort, SpriteComposition>();
        foreach (IntroScientistSpriteFrameDefinition definition in definitions)
        {
            if (!document.Frames.TryGetValue(definition.Name, out SpriteVisualPart[]? visual) ||
                visual is null)
                throw new InvalidDataException(
                    $"Intro scientist sprite frame {definition.Name} is missing.");
            frames.Add(definition.Pointer,
                IntroScientistParts.CalculateIfMatching(definition.Pointer,
                    IntroCinematicSpriteCompiler.Compile(visual, definition.Name)));
        }
        return new IntroScientistSpritePresentation(frames);
    }

    /// <summary>Validates and writes an editable scientist-scene baby sprite document as JSON.</summary>
    /// <param name="json">The caller-owned destination stream.</param>
    /// <param name="document">The ten-frame sprite document to validate and serialize.</param>
    public static void Write(Stream json, IntroScientistSpriteDocument document)
    {
        Ensure.NotNull(json);
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document,
            MapPresentationFormat.JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        json.Write(bytes);
    }
}

/// <summary>Editable JSON schema for the scientist scenes' baby-Metroid compositions.</summary>
public sealed record IntroScientistSpriteDocument
{
    /// <summary>Must match <see cref="IntroScientistSpriteFormat.Version"/> before any visual frame is accepted.</summary>
    public required int Version { get; init; }

    /// <summary>Gets all ten required named baby-Metroid visual compositions.</summary>
    public required Dictionary<string, SpriteVisualPart[]> Frames { get; init; }
}

/// <summary>Installed file identity for ten scientist-scene baby sprite frames.</summary>
public static class IntroScientistSpriteFormat
{
    /// <summary>The supported scientist-scene sprite JSON schema version.</summary>
    public const int Version = 1;

    /// <summary>The embedded editable scientist-scene sprite asset file name.</summary>
    public const string FileName = "intro-scientist-baby-sprites.json";
}
