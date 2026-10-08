using System.Text.Json;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Editable explosion OAM art, independent of native placement and timing.</summary>
public sealed class IntroMotherBrainExplosionSpritePresentation
{
    private readonly Dictionary<ushort, SpriteComposition> frames;

    /// <summary>Canonical identity of all selected decoded visual frames, preserving ordered OAM parts.</summary>
    public string ContentIdentity => SelectedPresentationHash.FromCompositions(nameof(IntroMotherBrainExplosionSpritePresentation), frames);

    private IntroMotherBrainExplosionSpritePresentation(
        Dictionary<ushort, SpriteComposition> frames) => this.frames = frames;

    /// <summary>Draws one installed small- or large-explosion composition at its script-owned screen anchor.</summary>
    /// <param name="pointer">Native bank-$8C spritemap identity for one of the twelve frames.</param>
    /// <param name="oam">The OAM buffer receiving the ordered sprite parts.</param>
    /// <param name="x">Explosion anchor X coordinate in screen pixels.</param>
    /// <param name="y">Explosion anchor Y coordinate in screen pixels.</param>
    /// <param name="paletteBits">OBJ palette attribute bits applied to inherited-palette parts.</param>
    public void Draw(ushort pointer, OamBuffer oam, ushort x, ushort y, ushort paletteBits)
    {
        if (!frames.TryGetValue(pointer, out SpriteComposition? frame))
            throw new InvalidDataException(
                $"Intro Mother Brain explosion frame $8C:{pointer:X4} is not installed.");
        frame.DrawOnScreen(oam, x, y, paletteBits);
    }

    /// <summary>Loads and validates all six small and six large fourth-hit explosion frames.</summary>
    /// <param name="json">The caller-owned stream containing the editable sprite document.</param>
    /// <returns>The compiled presentation keyed by native bank-$8C spritemap identities.</returns>
    /// <exception cref="InvalidDataException">The JSON, version, frame set, or sprite parts are invalid.</exception>
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
        IReadOnlyList<IntroMotherBrainExplosionSpriteFrameDefinition> definitions =
            IntroMotherBrainExplosionSpriteDefinitions.Frames;
        if (document.Version != IntroMotherBrainExplosionSpriteFormat.Version ||
            document.Frames is null || document.Frames.Count != definitions.Count)
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
                IntroMotherBrainExplosionParts.CalculateIfMatching(definition.Pointer,
                    IntroCinematicSpriteCompiler.Compile(visual, definition.Name)));
        }
        return new IntroMotherBrainExplosionSpritePresentation(frames);
    }

    /// <summary>Validates and writes an editable Mother Brain explosion sprite document as JSON.</summary>
    /// <param name="json">The caller-owned destination stream.</param>
    /// <param name="document">The twelve-frame sprite document to validate and serialize.</param>
    public static void Write(Stream json, IntroMotherBrainExplosionSpriteDocument document)
    {
        ArgumentNullException.ThrowIfNull(json);
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document,
            MapPresentationFormat.JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        json.Write(bytes);
    }
}

/// <summary>Editable JSON schema for the opening flashback's fourth-hit explosion compositions.</summary>
public sealed record IntroMotherBrainExplosionSpriteDocument
{
    /// <summary>Gets the explosion-sprite schema version.</summary>
    public required int Version { get; init; }

    /// <summary>Gets the twelve named <c>small-explosion-0..5</c> and <c>big-explosion-0..5</c> compositions.</summary>
    public required Dictionary<string, SpriteVisualPart[]> Frames { get; init; }
}

/// <summary>Installed file identity for twelve fourth-hit explosion frames.</summary>
public static class IntroMotherBrainExplosionSpriteFormat
{
    /// <summary>The supported explosion-sprite JSON schema version.</summary>
    public const int Version = 1;

    /// <summary>The embedded editable explosion-sprite asset file name.</summary>
    public const string FileName = "intro-mother-brain-explosion-sprites.json";
}
