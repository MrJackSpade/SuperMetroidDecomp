using SuperMetroid.Core.Frontend.Ending;
using SuperMetroid.Core.Frontend.Intro;
using System.Text.Json;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core;
using SuperMetroid.AssetExtraction.Frontend.Intro;
using SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;

/// <summary>Editable OAM compositions for the 56 completion-message glyph frames.</summary>
public sealed class EndingCompletionTextSpritePresentation : ICinematicSpriteFrames
{
    /// <summary>Compiled digits and colon keyed by the poses that show them.</summary>
    internal readonly Dictionary<CinematicSpritePose, SpriteComposition> frames;

    /// <summary>Compiled typed-text steps keyed by the text and the letters each shows typed (#2195).</summary>
    private readonly Dictionary<EndingTextTyping, SpriteComposition> typings;

    /// <summary>Retains the complete validated composition set independently of authoring arrays.</summary>
    private EndingCompletionTextSpritePresentation(Dictionary<CinematicSpritePose, SpriteComposition> frames,
        Dictionary<EndingTextTyping, SpriteComposition> typings)
    {
        this.frames = frames;
        this.typings = typings;
    }

    /// <summary>Each typed text's steps, each the whole of the text typed as far as one more letter (#2195).</summary>
    internal IReadOnlyDictionary<EndingTextTyping, SpriteComposition> Typings => typings;

    /// <inheritdoc/>
    CinematicSpriteFamily ICinematicSpriteFrames.Family => CinematicSpriteFamily.EndingCompletionText;

    /// <inheritdoc/>
    IReadOnlyDictionary<CinematicSpritePose, SpriteComposition> ICinematicSpriteFrames.Frames => frames;

    /// <summary>Loads version-1 <c>ending-completion-text-sprites.json</c>, requiring all 56 named compositions and validating each ordered part's offsets, tile region, size, palette, and priority.</summary>
    /// <param name="json">Caller-owned JSON stream consumed from its current position and left open; unknown and duplicate properties are rejected.</param>
    /// <returns>Every installed composition, compiled as installed.</returns>
    public static EndingCompletionTextSpritePresentation Load(Stream json)
    {
        Ensure.NotNull(json);
        EndingCompletionTextSpriteDocument document;
        document = JsonAssetDocument.Read<EndingCompletionTextSpriteDocument>(json, ArtworkJson.Options, "ending completion text sprite");
        IReadOnlyList<CinematicSpriteFrameDefinition> definitions =
            EndingCompletionTextSpriteDefinitions.Frames;
        IReadOnlyList<EndingTypedStepDefinition> steps = EndingCompletionTextSpriteDefinitions.Typings;
        if (document.Version != EndingCompletionTextSpriteFormat.Version ||
            document.Frames is null || document.Frames.Count != definitions.Count + steps.Count)
            throw new InvalidDataException(
                $"Ending completion text requires exactly {definitions.Count + steps.Count} named visual frames.");
        var frames = new Dictionary<CinematicSpritePose, SpriteComposition>();
        foreach (CinematicSpriteFrameDefinition definition in definitions)
        {
            if (!document.Frames.TryGetValue(definition.Name, out SpriteVisualPart[]? visual) ||
                visual is null)
                throw new InvalidDataException(
                    $"Ending completion text frame {definition.Name} is missing.");
            frames.Add(definition.Pose, IntroCinematicSpriteCompiler.Compile(visual, definition.Name));
        }
        var typings = new Dictionary<EndingTextTyping, SpriteComposition>();
        foreach (EndingTypedStepDefinition step in steps)
        {
            if (!document.Frames.TryGetValue(step.Name, out SpriteVisualPart[]? visual) ||
                visual is null)
                throw new InvalidDataException(
                    $"Ending completion text frame {step.Name} is missing.");
            typings.Add(step.Typed, IntroCinematicSpriteCompiler.Compile(visual, step.Name));
        }
        return new EndingCompletionTextSpritePresentation(frames, typings);
    }

    /// <summary>Serializes completion-text compositions as UTF-8 JSON.</summary>
    /// <param name="json">Destination written at its current position and left open.</param>
    /// <param name="document">Versioned phrase-prefix, digit, and colon compositions to serialize.</param>
    public static void Write(Stream json, EndingCompletionTextSpriteDocument document)
    {
        Ensure.NotNull(json);
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document,
            ArtworkJson.Options);
        json.Write(bytes);
    }
}
