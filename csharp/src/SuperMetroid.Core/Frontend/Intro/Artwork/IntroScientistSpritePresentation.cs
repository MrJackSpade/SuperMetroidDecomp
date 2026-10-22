using System.Text.Json;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Intro.Artwork;

/// <summary>Editable baby-Metroid OAM art for the two scientist scenes only.</summary>
public sealed class IntroScientistSpritePresentation : ICinematicSpriteFrames
{
    /// <summary>Validated baby-Metroid compositions keyed by bank-$8C spritemap identity.</summary>
    private readonly Dictionary<CinematicSpritePose, SpriteComposition> frames;

    /// <inheritdoc/>
    CinematicSpriteFamily ICinematicSpriteFrames.Family => CinematicSpriteFamily.IntroScientist;

    /// <inheritdoc/>
    IReadOnlyDictionary<CinematicSpritePose, SpriteComposition> ICinematicSpriteFrames.Frames => frames;

    /// <summary>Canonical identity of all selected decoded visual frames, preserving ordered OAM parts.</summary>
    public string ContentIdentity => CinematicSpriteArtwork.ContentIdentity(nameof(IntroScientistSpritePresentation), frames);

    /// <summary>Retains the ten validated frames after JSON compilation.</summary>
    private IntroScientistSpritePresentation(Dictionary<CinematicSpritePose, SpriteComposition> frames) =>
        this.frames = frames;

    /// <summary>Loads and validates all ten baby-Metroid visual frames for the scientist scenes.</summary>
    /// <param name="json">The caller-owned stream containing the editable sprite document.</param>
    /// <returns>The compiled presentation keyed by native bank-$8C spritemap identities.</returns>
    /// <exception cref="InvalidDataException">The JSON, version, frame set, or sprite parts are invalid.</exception>
    public static IntroScientistSpritePresentation Load(Stream json)
    {
        Ensure.NotNull(json);
        IntroScientistSpriteDocument document;
        document = JsonAssetDocument.Read<IntroScientistSpriteDocument>(json, ArtworkJson.Options, "intro scientist sprite");
        IReadOnlyList<CinematicSpriteFrameDefinition> definitions =
            IntroScientistSpriteDefinitions.Frames;
        if (document.Version != IntroScientistSpriteFormat.Version ||
            document.Frames is null || document.Frames.Count != definitions.Count)
            throw new InvalidDataException("Intro scientist scenes require ten named visual frames.");
        var frames = new Dictionary<CinematicSpritePose, SpriteComposition>();
        foreach (CinematicSpriteFrameDefinition definition in definitions)
        {
            if (!document.Frames.TryGetValue(definition.Name, out SpriteVisualPart[]? visual) ||
                visual is null)
                throw new InvalidDataException(
                    $"Intro scientist sprite frame {definition.Name} is missing.");
            frames.Add(definition.Pose,
                IntroScientistParts.CalculateIfMatching(definition.Pose,
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
            ArtworkJson.Options);
        _ = Load(new MemoryStream(bytes, writable: false));
        json.Write(bytes);
    }
}
