using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Frontend.Intro.Artwork;

namespace SuperMetroid.Core.Frontend.Ending.Artwork;

/// <summary>Editable OAM compositions for the 16 Zebes-explosion visual frames.</summary>
public sealed class EndingExplosionSpritePresentation : IIntroCinematicSpritePresentation
{
    /// <summary>Validated OAM compositions keyed by the bank-$8C pointers used by the cinematic.</summary>
    private readonly Dictionary<ushort, SpriteComposition> frames;

    /// <summary>Retains the decoded compositions after their stock shapes and calculated parts are checked.</summary>
    private EndingExplosionSpritePresentation(Dictionary<ushort, SpriteComposition> frames) =>
        this.frames = frames;

    /// <summary>Canonical identity of the selected decoded visual frames, not JSON formatting.</summary>
    public string ContentIdentity => SelectedPresentationHash.FromCompositions(nameof(EndingExplosionSpritePresentation), frames);

    /// <summary>Draws the Zebes-explosion composition identified by its bank-$8C spritemap pointer.</summary>
    /// <param name="pointer">Bank-$8C pointer identifying one of the installed explosion frames.</param>
    /// <param name="objects">Object-attribute buffer that receives the composition.</param>
    /// <param name="origin">Native wrapped sixteen-bit screen origin in pixels.</param>
    /// <param name="paletteBits">SNES OAM attribute bits to combine with each part.</param>
    /// <param name="originIsOnScreen">Whether the origin is already in the visible coordinate range.</param>
    public void Draw(ushort pointer, ScreenSprites objects, Position16<OamPixels> origin,
        ushort paletteBits, bool originIsOnScreen)
    {
        if (!frames.TryGetValue(pointer, out SpriteComposition? frame))
            throw new InvalidDataException(
                $"Ending explosion sprite frame $8C:{pointer:X4} is not installed.");
        if (originIsOnScreen)
            frame.Draw(objects, origin, paletteBits);
        else
            frame.DrawOffScreen(objects, origin, paletteBits);
    }

    /// <summary>Loads and validates the 16 named Zebes-explosion compositions from JSON.</summary>
    /// <param name="json">Stream containing an ending-explosion sprite document.</param>
    /// <returns>The compiled ending-explosion presentation.</returns>
    public static EndingExplosionSpritePresentation Load(Stream json)
    {
        Ensure.NotNull(json);
        EndingExplosionSpriteDocument document;
        try
        {
            using JsonDocument parsed = JsonDocument.Parse(json);
            EnemySpritemapCatalog.RejectDuplicateProperties(parsed.RootElement);
            document = parsed.RootElement.Deserialize<EndingExplosionSpriteDocument>(
                ArtworkJson.Options) ??
                throw new InvalidDataException("Ending explosion sprite JSON is null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid ending explosion sprite JSON.", error);
        }
        IReadOnlyList<EndingExplosionSpriteFrameDefinition> definitions =
            EndingExplosionSpriteDefinitions.Frames;
        if (document.Version != EndingExplosionSpriteFormat.Version ||
            document.Frames is null || document.Frames.Count != definitions.Count)
            throw new InvalidDataException(
                $"Ending explosion requires exactly {definitions.Count} named visual frames.");
        var frames = new Dictionary<ushort, SpriteComposition>();
        foreach (EndingExplosionSpriteFrameDefinition definition in definitions)
        {
            if (!document.Frames.TryGetValue(definition.Name, out SpriteVisualPart[]? visual) ||
                visual is null)
                throw new InvalidDataException(
                    $"Ending explosion sprite {definition.Name} is missing.");
            SpriteComposition compiled = IntroCinematicSpriteCompiler.Compile(visual, definition.Name);
            compiled = EndingExplosionQuadrantParts.CalculateIfMatching(definition.Pointer, compiled);
            compiled = EndingExplosionStarfieldParts.CalculateIfMatching(definition.Pointer, compiled);
            compiled = EndingExplosionAfterglowParts.CalculateIfMatching(definition.Pointer, compiled);
            compiled = EndingExplosionSilhouetteParts.CalculateIfMatching(definition.Pointer, compiled);
            frames.Add(definition.Pointer, EndingExplosionGridParts.CalculateIfMatching(definition.Pointer, compiled));
        }
        return new EndingExplosionSpritePresentation(frames);
    }

    /// <summary>Validates and writes an ending-explosion sprite document as JSON.</summary>
    /// <param name="json">Destination stream.</param>
    /// <param name="document">Document to validate and serialize.</param>
    public static void Write(Stream json, EndingExplosionSpriteDocument document)
    {
        Ensure.NotNull(json);
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document,
            ArtworkJson.Options);
        _ = Load(new MemoryStream(bytes, writable: false));
        json.Write(bytes);
    }
}
