using SuperMetroid.Core.Frontend.Intro;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core;
using SuperMetroid.AssetExtraction.Frontend.Intro;
using SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;

/// <summary>Editable OAM compositions for the 56 completion-message glyph frames.</summary>
public sealed class EndingCompletionTextSpritePresentation : ICinematicSpriteFrames
{
    /// <summary>Compiled phrase prefixes, digits, and colon keyed by the poses that show them.</summary>
    private readonly Dictionary<CinematicSpritePose, SpriteComposition> frames;

    /// <summary>Retains the complete validated composition set independently of authoring arrays.</summary>
    private EndingCompletionTextSpritePresentation(Dictionary<CinematicSpritePose, SpriteComposition> frames) =>
        this.frames = frames;

    /// <summary>Canonical identity of the selected decoded visual frames, not JSON formatting.</summary>
    public string ContentIdentity => CinematicSpriteArtwork.ContentIdentity(nameof(EndingCompletionTextSpritePresentation), frames);

    /// <inheritdoc/>
    CinematicSpriteFamily ICinematicSpriteFrames.Family => CinematicSpriteFamily.EndingCompletionText;

    /// <inheritdoc/>
    IReadOnlyDictionary<CinematicSpritePose, SpriteComposition> ICinematicSpriteFrames.Frames => frames;

    /// <summary>Loads version-1 <c>ending-completion-text-sprites.json</c>, requiring all 56 named compositions and validating each ordered part's offsets, tile region, size, palette, and priority.</summary>
    /// <param name="json">Caller-owned JSON stream consumed from its current position and left open; unknown and duplicate properties are rejected.</param>
    /// <returns>Compiled editable compositions, using calculated stock letter parts only when every supplied visual field matches.</returns>
    public static EndingCompletionTextSpritePresentation Load(Stream json)
    {
        Ensure.NotNull(json);
        EndingCompletionTextSpriteDocument document;
        document = JsonAssetDocument.Read<EndingCompletionTextSpriteDocument>(json, ArtworkJson.Options, "ending completion text sprite");
        IReadOnlyList<CinematicSpriteFrameDefinition> definitions =
            EndingCompletionTextSpriteDefinitions.Frames;
        if (document.Version != EndingCompletionTextSpriteFormat.Version ||
            document.Frames is null || document.Frames.Count != definitions.Count)
            throw new InvalidDataException(
                $"Ending completion text requires exactly {definitions.Count} named visual frames.");
        var frames = new Dictionary<CinematicSpritePose, SpriteComposition>();
        for (int index = 0; index < definitions.Count; index++)
        {
            CinematicSpriteFrameDefinition definition = definitions[index];
            if (!document.Frames.TryGetValue(definition.Name, out SpriteVisualPart[]? visual) ||
                visual is null)
                throw new InvalidDataException(
                    $"Ending completion text frame {definition.Name} is missing.");
            frames.Add(definition.Pose,
                IntroCinematicSpriteCompiler.Compile(visual, definition.Name)
                    .CalculateIfMatching(new EndingCompletionTextParts(index)));
        }
        return new EndingCompletionTextSpritePresentation(frames);
    }

    /// <summary>Serializes completion-text compositions as UTF-8 JSON and validates the complete named-frame set and OAM fields before writing any bytes.</summary>
    /// <param name="json">Destination written at its current position and left open.</param>
    /// <param name="document">Versioned phrase-prefix, digit, and colon compositions to serialize.</param>
    public static void Write(Stream json, EndingCompletionTextSpriteDocument document)
    {
        Ensure.NotNull(json);
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document,
            ArtworkJson.Options);
        _ = Load(new MemoryStream(bytes, writable: false));
        json.Write(bytes);
    }
}
