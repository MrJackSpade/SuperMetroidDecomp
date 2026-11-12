using System.Text.Json;
using SuperMetroid.Core;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;

/// <summary>Editable OAM compositions of the Japanese text's next-page arrow (#1655), independent of its cadence and position.</summary>
public sealed class IntroNextPageArrowSpritePresentation : ICinematicSpriteFrames
{
    /// <summary>Validated arrow compositions keyed by the pose that shows them.</summary>
    private readonly Dictionary<CinematicSpritePose, SpriteComposition> frames;

    /// <inheritdoc/>
    CinematicSpriteFamily ICinematicSpriteFrames.Family => CinematicSpriteFamily.IntroNextPageArrow;

    /// <inheritdoc/>
    IReadOnlyDictionary<CinematicSpritePose, SpriteComposition> ICinematicSpriteFrames.Frames => frames;

    /// <summary>Canonical identity of all selected decoded visual frames, preserving ordered OAM parts.</summary>
    public string ContentIdentity => CinematicSpriteArtwork.ContentIdentity(nameof(IntroNextPageArrowSpritePresentation), frames);

    /// <summary>Retains the three validated frames after JSON compilation.</summary>
    private IntroNextPageArrowSpritePresentation(Dictionary<CinematicSpritePose, SpriteComposition> frames) =>
        this.frames = frames;

    /// <summary>Loads and validates the arrow's three visual frames.</summary>
    /// <param name="json">The caller-owned stream containing the editable sprite document.</param>
    /// <returns>The compiled presentation keyed by the poses the arrow shows.</returns>
    /// <exception cref="InvalidDataException">The JSON, version, frame set, or sprite parts are invalid.</exception>
    public static IntroNextPageArrowSpritePresentation Load(Stream json)
    {
        Ensure.NotNull(json);
        IntroNextPageArrowSpriteDocument document = JsonAssetDocument.Read<IntroNextPageArrowSpriteDocument>(
            json, ArtworkJson.Options, "intro next-page arrow sprite");
        IReadOnlyList<CinematicSpriteFrameDefinition> definitions = IntroNextPageArrowSpriteDefinitions.Frames;
        if (document.Version != IntroNextPageArrowSpriteFormat.Version ||
            document.Frames is null || document.Frames.Count != definitions.Count)
            throw new InvalidDataException("The intro next-page arrow requires its three named visual frames.");
        var frames = new Dictionary<CinematicSpritePose, SpriteComposition>();
        foreach (CinematicSpriteFrameDefinition definition in definitions)
        {
            if (!document.Frames.TryGetValue(definition.Name, out SpriteVisualPart[]? visual) || visual is null)
                throw new InvalidDataException($"Intro next-page arrow frame {definition.Name} is missing.");
            frames.Add(definition.Pose, IntroCinematicSpriteCompiler.Compile(visual, definition.Name));
        }
        return new IntroNextPageArrowSpritePresentation(frames);
    }

    /// <summary>Writes an editable next-page arrow sprite document as JSON.</summary>
    /// <param name="json">The caller-owned destination stream.</param>
    /// <param name="document">The three-frame sprite document to serialize.</param>
    public static void Write(Stream json, IntroNextPageArrowSpriteDocument document)
    {
        Ensure.NotNull(json);
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, ArtworkJson.Options);
        json.Write(bytes);
    }
}
