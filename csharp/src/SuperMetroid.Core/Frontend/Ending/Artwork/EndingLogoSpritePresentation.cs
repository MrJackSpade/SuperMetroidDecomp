using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Frontend.Intro.Artwork;

namespace SuperMetroid.Core.Frontend.Ending.Artwork;

/// <summary>Editable OAM compositions for the eight final assembling-logo frames.</summary>
public sealed class EndingLogoSpritePresentation : IIntroCinematicSpritePresentation
{
    /// <summary>Compiled compositions indexed by the spritemap pointers used by the ending script.</summary>
    private readonly Dictionary<ushort, SpriteComposition> frames;

    /// <summary>Retains the validated frame set after related logo parts have been calculated.</summary>
    private EndingLogoSpritePresentation(Dictionary<ushort, SpriteComposition> frames) =>
        this.frames = frames;

    /// <summary>Canonical identity of the selected decoded visual frames, not JSON formatting.</summary>
    public string ContentIdentity => SelectedPresentationHash.FromCompositions(nameof(EndingLogoSpritePresentation), frames);

    /// <summary>Draws the ending-logo composition identified by its bank-$8C spritemap pointer.</summary>
    /// <param name="pointer">Bank-$8C pointer identifying one of the eight installed logo frames.</param>
    /// <param name="objects">Object-attribute buffer that receives the composition.</param>
    /// <param name="origin">Native wrapped sixteen-bit screen origin in pixels.</param>
    /// <param name="paletteBits">SNES OAM attribute bits to combine with each part.</param>
    /// <param name="originIsOnScreen">Whether the origin is already in the visible coordinate range.</param>
    public void Draw(ushort pointer, ScreenSprites objects, Position16<OamPixels> origin,
        ushort paletteBits, bool originIsOnScreen) =>
        SpriteComposition.DrawInstalled(frames, "Ending logo sprite", pointer, objects, origin, paletteBits, originIsOnScreen);

    /// <summary>Loads and validates the eight named final-logo compositions from JSON.</summary>
    /// <param name="json">Stream containing an ending-logo sprite document.</param>
    /// <returns>The compiled ending-logo presentation.</returns>
    public static EndingLogoSpritePresentation Load(Stream json)
    {
        Ensure.NotNull(json);
        EndingLogoSpriteDocument document;
        try
        {
            using JsonDocument parsed = JsonDocument.Parse(json);
            EnemySpritemapCatalog.RejectDuplicateProperties(parsed.RootElement);
            document = parsed.RootElement.Deserialize<EndingLogoSpriteDocument>(
                ArtworkJson.Options) ??
                throw new InvalidDataException("Ending logo sprite JSON is null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid ending logo sprite JSON.", error);
        }
        IReadOnlyList<SpriteFrameDefinition> definitions =
            EndingLogoSpriteDefinitions.Frames;
        if (document.Version != EndingLogoSpriteFormat.Version ||
            document.Frames is null || document.Frames.Count != definitions.Count)
            throw new InvalidDataException(
                $"Ending logo requires exactly {definitions.Count} named visual frames.");
        var frames = new Dictionary<ushort, SpriteComposition>();
        foreach (SpriteFrameDefinition definition in definitions)
        {
            if (!document.Frames.TryGetValue(definition.Name, out SpriteVisualPart[]? visual) ||
                visual is null)
                throw new InvalidDataException(
                    $"Ending logo frame {definition.Name} is missing.");
            frames.Add(definition.Pointer,
                IntroCinematicSpriteCompiler.Compile(visual, definition.Name));
        }
        SpriteComposition upper = frames[definitions[0].Pointer].CalculateIfMatching(new EndingLogoUpperParts());
        frames[definitions[0].Pointer] = upper;
        SpriteComposition completeRight = EndingLogoWrapParts.CalculateIfMatching(frames[definitions[4].Pointer]);
        frames[definitions[4].Pointer] = completeRight;
        for (int index = 0; index < definitions.Count; index++)
        {
            ushort pointer = definitions[index].Pointer;
            frames[pointer] = EndingLogoRelatedParts.CalculateIfMatching(index, frames[pointer], upper, completeRight);
        }
        return new EndingLogoSpritePresentation(frames);
    }

    /// <summary>Validates and writes an ending-logo sprite document as JSON.</summary>
    /// <param name="json">Destination stream.</param>
    /// <param name="document">Document to validate and serialize.</param>
    public static void Write(Stream json, EndingLogoSpriteDocument document)
    {
        Ensure.NotNull(json);
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document,
            ArtworkJson.Options);
        _ = Load(new MemoryStream(bytes, writable: false));
        json.Write(bytes);
    }
}
