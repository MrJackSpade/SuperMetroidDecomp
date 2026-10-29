using SuperMetroid.Core.Frontend.Intro;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core;
using SuperMetroid.AssetExtraction.Frontend.Intro;
using SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;

/// <summary>Editable OAM compositions for the 16 Zebes-explosion visual frames.</summary>
public sealed class EndingExplosionSpritePresentation : ICinematicSpriteFrames
{
    /// <summary>Validated OAM compositions keyed by the poses that show them.</summary>
    private readonly Dictionary<CinematicSpritePose, SpriteComposition> frames;

    /// <summary>Retains the decoded compositions after their stock shapes and calculated parts are checked.</summary>
    private EndingExplosionSpritePresentation(Dictionary<CinematicSpritePose, SpriteComposition> frames) =>
        this.frames = frames;

    /// <summary>Canonical identity of the selected decoded visual frames, not JSON formatting.</summary>
    public string ContentIdentity => CinematicSpriteArtwork.ContentIdentity(nameof(EndingExplosionSpritePresentation), frames);

    /// <inheritdoc/>
    CinematicSpriteFamily ICinematicSpriteFrames.Family => CinematicSpriteFamily.EndingExplosion;

    /// <inheritdoc/>
    IReadOnlyDictionary<CinematicSpritePose, SpriteComposition> ICinematicSpriteFrames.Frames => frames;

    /// <summary>Loads and validates the 16 named Zebes-explosion compositions from JSON.</summary>
    /// <param name="json">Stream containing an ending-explosion sprite document.</param>
    /// <returns>The compiled ending-explosion presentation.</returns>
    public static EndingExplosionSpritePresentation Load(Stream json)
    {
        Ensure.NotNull(json);
        EndingExplosionSpriteDocument document;
        document = JsonAssetDocument.Read<EndingExplosionSpriteDocument>(json, ArtworkJson.Options, "ending explosion sprite");
        IReadOnlyList<CinematicSpriteFrameDefinition> definitions =
            EndingExplosionSpriteDefinitions.Frames;
        if (document.Version != EndingExplosionSpriteFormat.Version ||
            document.Frames is null || document.Frames.Count != definitions.Count)
            throw new InvalidDataException(
                $"Ending explosion requires exactly {definitions.Count} named visual frames.");
        var frames = new Dictionary<CinematicSpritePose, SpriteComposition>();
        foreach (CinematicSpriteFrameDefinition definition in definitions)
        {
            if (!document.Frames.TryGetValue(definition.Name, out SpriteVisualPart[]? visual) ||
                visual is null)
                throw new InvalidDataException(
                    $"Ending explosion sprite {definition.Name} is missing.");
            SpriteComposition compiled = IntroCinematicSpriteCompiler.Compile(visual, definition.Name);
            compiled = EndingExplosionQuadrantParts.CalculateIfMatching(definition.Pose, compiled);
            compiled = EndingExplosionStarfieldParts.CalculateIfMatching(definition.Pose, compiled);
            compiled = EndingExplosionAfterglowParts.CalculateIfMatching(definition.Pose, compiled);
            compiled = EndingExplosionSilhouetteParts.CalculateIfMatching(definition.Pose, compiled);
            frames.Add(definition.Pose, EndingExplosionGridParts.CalculateIfMatching(definition.Pose, compiled));
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
