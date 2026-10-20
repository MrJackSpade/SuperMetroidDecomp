using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Frontend.Intro.Artwork;

/// <summary>Editable explosion OAM art, independent of native placement and timing.</summary>
public sealed class IntroMotherBrainExplosionSpritePresentation
{
    /// <summary>Validated fourth-hit compositions keyed by their native bank-$8C spritemap pointers.</summary>
    private readonly Dictionary<ushort, SpriteComposition> frames;

    /// <summary>Canonical identity of all selected decoded visual frames, preserving ordered OAM parts.</summary>
    public string ContentIdentity => SelectedPresentationHash.FromCompositions(nameof(IntroMotherBrainExplosionSpritePresentation), frames);

    /// <summary>Retains the validated twelve-frame selection after JSON compilation.</summary>
    private IntroMotherBrainExplosionSpritePresentation(
        Dictionary<ushort, SpriteComposition> frames) => this.frames = frames;

    /// <summary>Draws one installed small- or large-explosion composition at its script-owned screen anchor.</summary>
    /// <param name="pointer">Native bank-$8C spritemap identity for one of the twelve frames.</param>
    /// <param name="objects">The OAM buffer receiving the ordered sprite parts.</param>
    /// <param name="origin">Native wrapped sixteen-bit screen origin in pixels.</param>
    /// <param name="paletteBits">OBJ palette attribute bits applied to inherited-palette parts.</param>
    public void Draw(ushort pointer, ScreenSprites objects, Position16<OamPixels> origin, ushort paletteBits)
    {
        if (!frames.TryGetValue(pointer, out SpriteComposition? frame))
            throw new InvalidDataException(
                $"Intro Mother Brain explosion frame $8C:{pointer:X4} is not installed.");
        frame.Draw(objects, origin, paletteBits);
    }

    /// <summary>Loads and validates all six small and six large fourth-hit explosion frames.</summary>
    /// <param name="json">The caller-owned stream containing the editable sprite document.</param>
    /// <returns>The compiled presentation keyed by native bank-$8C spritemap identities.</returns>
    /// <exception cref="InvalidDataException">The JSON, version, frame set, or sprite parts are invalid.</exception>
    public static IntroMotherBrainExplosionSpritePresentation Load(Stream json)
    {
        Ensure.NotNull(json);
        IntroMotherBrainExplosionSpriteDocument document;
        try
        {
            using JsonDocument parsed = JsonDocument.Parse(json);
            EnemySpritemapCatalog.RejectDuplicateProperties(parsed.RootElement);
            document = parsed.RootElement.Deserialize<IntroMotherBrainExplosionSpriteDocument>(
                ArtworkJson.Options) ??
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
        Ensure.NotNull(json);
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document,
            ArtworkJson.Options);
        _ = Load(new MemoryStream(bytes, writable: false));
        json.Write(bytes);
    }
}
