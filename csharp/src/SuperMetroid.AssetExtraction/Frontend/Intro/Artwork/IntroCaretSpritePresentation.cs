using System.Text.Json;
using SuperMetroid.Core;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;

/// <summary>Editable visible OAM composition of the intro caret, independent of its blink script.</summary>
public sealed class IntroCaretSpritePresentation : ICinematicSpriteFrames
{
    /// <summary>Compiled visible composition keyed by its native bank-$8C frame pose.</summary>
    private readonly Dictionary<CinematicSpritePose, SpriteComposition> frames;

    /// <inheritdoc/>
    CinematicSpriteFamily ICinematicSpriteFrames.Family => CinematicSpriteFamily.IntroCaret;

    /// <inheritdoc/>
    IReadOnlyDictionary<CinematicSpritePose, SpriteComposition> ICinematicSpriteFrames.Frames => frames;

    /// <summary>Canonical identity of all selected decoded visual frames, preserving ordered OAM parts.</summary>
    public string ContentIdentity => CinematicSpriteArtwork.ContentIdentity(nameof(IntroCaretSpritePresentation), frames);

    /// <summary>Retains the validated visible-frame lookup.</summary>
    private IntroCaretSpritePresentation(Dictionary<CinematicSpritePose, SpriteComposition> frames) =>
        this.frames = frames;

    /// <summary>Loads and validates the visible caret's OAM composition, rejecting duplicate properties, unsupported schemas, missing frame names, and invalid sprite parts.</summary>
    /// <param name="json">UTF-8 JSON stream containing the one-frame schema.</param>
    /// <returns>The validated presentation installed under native visible-frame identity <c>$8C:8D68</c>.</returns>
    public static IntroCaretSpritePresentation Load(Stream json)
    {
        Ensure.NotNull(json);
        IntroCaretSpriteDocument document;
        document = JsonAssetDocument.Read<IntroCaretSpriteDocument>(json, ArtworkJson.Options, "opening caret composition");
        var definitions = IntroCaretSpriteDefinitions.Frames;
        if (document.Frames is null ||
            document.Version != IntroCaretSpriteVersion.Current.Revision ||
            document.Frames.Count != definitions.Count)
            throw new InvalidDataException("Opening caret requires its one visible sprite frame.");

        var frames = new Dictionary<CinematicSpritePose, SpriteComposition>();
        foreach (CinematicSpriteFrameDefinition definition in definitions)
        {
            if (!document.Frames.TryGetValue(definition.Name, out SpriteVisualPart[]? visual) ||
                visual is null || visual.Length > IntroCaretSpriteDefinitions.MaximumParts)
                throw new InvalidDataException($"Opening caret frame {definition.Name} is missing or too large.");
            frames.Add(definition.Pose,
                IntroScientistParts.CalculateIfMatching(definition.Pose,
                    IntroCinematicSpriteCompiler.Compile(visual, definition.Name)));
        }
        return new IntroCaretSpritePresentation(frames);
    }

    /// <summary>Serializes a caret document using the presentation JSON options and writes it to the destination stream.</summary>
    /// <param name="json">Destination stream for the UTF-8 JSON.</param>
    /// <param name="document">Document to serialize.</param>
    public static void Write(Stream json, IntroCaretSpriteDocument document)
    {
        Ensure.NotNull(json);
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document,
            ArtworkJson.Options);
        json.Write(bytes);
    }
}
