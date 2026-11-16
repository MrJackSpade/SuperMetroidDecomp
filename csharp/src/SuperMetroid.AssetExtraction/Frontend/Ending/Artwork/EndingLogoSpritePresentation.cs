using SuperMetroid.Core.Frontend.Intro;
using System.Text.Json;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core;
using SuperMetroid.AssetExtraction.Frontend.Intro;
using SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;

/// <summary>Editable OAM compositions for the eight final assembling-logo frames.</summary>
public sealed class EndingLogoSpritePresentation : ICinematicSpriteFrames
{
    /// <summary>Compiled compositions indexed by the poses that show them.</summary>
    private readonly Dictionary<CinematicSpritePose, SpriteComposition> frames;

    /// <summary>Retains every installed composition, compiled as installed.</summary>
    private EndingLogoSpritePresentation(Dictionary<CinematicSpritePose, SpriteComposition> frames) =>
        this.frames = frames;

    /// <summary>Canonical identity of the selected decoded visual frames, not JSON formatting.</summary>
    public string ContentIdentity => CinematicSpriteArtwork.ContentIdentity(nameof(EndingLogoSpritePresentation), frames);

    /// <inheritdoc/>
    CinematicSpriteFamily ICinematicSpriteFrames.Family => CinematicSpriteFamily.EndingLogo;

    /// <inheritdoc/>
    IReadOnlyDictionary<CinematicSpritePose, SpriteComposition> ICinematicSpriteFrames.Frames => frames;

    /// <summary>Loads and validates the eight named final-logo compositions from JSON.</summary>
    /// <param name="json">Stream containing an ending-logo sprite document.</param>
    /// <returns>The compiled ending-logo presentation.</returns>
    public static EndingLogoSpritePresentation Load(Stream json)
    {
        Ensure.NotNull(json);
        EndingLogoSpriteDocument document;
        document = JsonAssetDocument.Read<EndingLogoSpriteDocument>(json, ArtworkJson.Options, "ending logo sprite");
        IReadOnlyList<CinematicSpriteFrameDefinition> definitions =
            EndingLogoSpriteDefinitions.Frames;
        if (document.Version != EndingLogoSpriteFormat.Version ||
            document.Frames is null || document.Frames.Count != definitions.Count)
            throw new InvalidDataException(
                $"Ending logo requires exactly {definitions.Count} named visual frames.");
        var frames = new Dictionary<CinematicSpritePose, SpriteComposition>();
        foreach (CinematicSpriteFrameDefinition definition in definitions)
        {
            if (!document.Frames.TryGetValue(definition.Name, out SpriteVisualPart[]? visual) ||
                visual is null)
                throw new InvalidDataException(
                    $"Ending logo frame {definition.Name} is missing.");
            frames.Add(definition.Pose,
                IntroCinematicSpriteCompiler.Compile(visual, definition.Name));
        }
        return new EndingLogoSpritePresentation(frames);
    }

    /// <summary>Writes an ending-logo sprite document as JSON.</summary>
    /// <param name="json">Destination stream.</param>
    /// <param name="document">Document to serialize.</param>
    public static void Write(Stream json, EndingLogoSpriteDocument document)
    {
        Ensure.NotNull(json);
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document,
            ArtworkJson.Options);
        json.Write(bytes);
    }
}
