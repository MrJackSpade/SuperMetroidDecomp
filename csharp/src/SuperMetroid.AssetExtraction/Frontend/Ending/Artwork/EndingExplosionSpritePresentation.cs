using SuperMetroid.Core.Frontend.Intro;
using System.Text.Json;
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

    /// <summary>Retains every installed composition, compiled as installed.</summary>
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
            frames.Add(definition.Pose, IntroCinematicSpriteCompiler.Compile(visual, definition.Name));
        }
        return new EndingExplosionSpritePresentation(frames);
    }

    /// <summary>Writes an ending-explosion sprite document as JSON.</summary>
    /// <param name="json">Destination stream.</param>
    /// <param name="document">Document to serialize.</param>
    public static void Write(Stream json, EndingExplosionSpriteDocument document)
    {
        Ensure.NotNull(json);
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document,
            ArtworkJson.Options);
        json.Write(bytes);
    }
}
