using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.AssetExtraction.Frontend.CeresCinematics.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;

/// <summary>Editable SR388 egg and baby OAM art; animation and motion remain compiled.</summary>
public sealed class IntroDiscoveryActorSpritePresentation : ICinematicSpriteFrames
{
    /// <summary>Validated egg, remnant, and baby compositions keyed by the poses that show them.</summary>
    private readonly Dictionary<CinematicSpritePose, SpriteComposition> frames;

    /// <inheritdoc/>
    CinematicSpriteFamily ICinematicSpriteFrames.Family => CinematicSpriteFamily.IntroDiscoveryActor;

    /// <inheritdoc/>
    IReadOnlyDictionary<CinematicSpritePose, SpriteComposition> ICinematicSpriteFrames.Frames => frames;

    /// <summary>Canonical identity of all selected decoded visual frames, preserving ordered OAM parts.</summary>
    public string ContentIdentity => CinematicSpriteArtwork.ContentIdentity(nameof(IntroDiscoveryActorSpritePresentation), frames);

    /// <summary>Retains the compiled twenty-frame selection after JSON validation.</summary>
    private IntroDiscoveryActorSpritePresentation(Dictionary<CinematicSpritePose, SpriteComposition> frames) =>
        this.frames = frames;

    /// <summary>Loads and validates all sixteen egg/remnant and four baby visual frames.</summary>
    /// <param name="json">The caller-owned stream containing the editable sprite document.</param>
    /// <returns>The compiled presentation keyed by native bank-$8C spritemap identities.</returns>
    /// <exception cref="InvalidDataException">The JSON, version, frame set, or sprite parts are invalid.</exception>
    public static IntroDiscoveryActorSpritePresentation Load(Stream json)
    {
        Ensure.NotNull(json);
        IntroDiscoveryActorSpriteDocument document;
        document = JsonAssetDocument.Read<IntroDiscoveryActorSpriteDocument>(json, ArtworkJson.Options, "intro discovery actor sprite");
        IReadOnlyList<CinematicSpriteFrameDefinition> definitions =
            IntroDiscoveryActorSpriteDefinitions.Frames;
        if (document.Version != IntroDiscoveryActorSpriteFormat.Version ||
            document.Frames is null || document.Frames.Count != definitions.Count)
            throw new InvalidDataException("Intro discovery actors require twenty named visual frames.");
        var frames = new Dictionary<CinematicSpritePose, SpriteComposition>();
        foreach (CinematicSpriteFrameDefinition definition in definitions)
        {
            if (!document.Frames.TryGetValue(definition.Name, out SpriteVisualPart[]? visual) ||
                visual is null)
                throw new InvalidDataException($"Intro discovery actor frame {definition.Name} is missing.");
            SpriteComposition compiled = IntroCinematicSpriteCompiler.Compile(visual, definition.Name);
            compiled = CeresLargeAsteroidParts.CalculateIfMatching(definition.Pose, compiled);
            compiled = IntroEggRockingParts.CalculateIfMatching(definition.Pose, compiled);
            compiled = IntroEggCrackingParts.CalculateIfMatching(definition.Pose, compiled);
            frames.Add(definition.Pose,
                IntroConfusedBabyParts.CalculateIfMatching(definition.Pose,
                    IntroEggRemnantParts.CalculateIfMatching(definition.Pose,
                        compiled)));
        }
        return new IntroDiscoveryActorSpritePresentation(frames);
    }

    /// <summary>Validates and writes an editable SR388 discovery-actor sprite document as JSON.</summary>
    /// <param name="json">The caller-owned destination stream.</param>
    /// <param name="document">The twenty-frame sprite document to validate and serialize.</param>
    public static void Write(Stream json, IntroDiscoveryActorSpriteDocument document)
    {
        Ensure.NotNull(json);
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document,
            ArtworkJson.Options);
        _ = Load(new MemoryStream(bytes, writable: false));
        json.Write(bytes);
    }
}
