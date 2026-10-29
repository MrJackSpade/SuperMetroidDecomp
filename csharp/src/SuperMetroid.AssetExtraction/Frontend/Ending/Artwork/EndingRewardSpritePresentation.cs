using SuperMetroid.Core.Frontend.Intro;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core;
using SuperMetroid.AssetExtraction.Frontend.Intro;
using SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;

/// <summary>Editable OAM compositions for the 37 post-credits Samus reward frames.</summary>
public sealed class EndingRewardSpritePresentation : ICinematicSpriteFrames
{
    /// <summary>Validated editable OAM compositions keyed by the poses that show them.</summary>
    private readonly Dictionary<CinematicSpritePose, SpriteComposition> frames;

    /// <summary>Retains compiled compositions after all published reward frames are checked.</summary>
    private EndingRewardSpritePresentation(Dictionary<CinematicSpritePose, SpriteComposition> frames) =>
        this.frames = frames;

    /// <summary>Canonical identity of the selected decoded visual frames, not JSON formatting.</summary>
    public string ContentIdentity => CinematicSpriteArtwork.ContentIdentity(nameof(EndingRewardSpritePresentation), frames);

    /// <inheritdoc/>
    CinematicSpriteFamily ICinematicSpriteFrames.Family => CinematicSpriteFamily.EndingReward;

    /// <inheritdoc/>
    IReadOnlyDictionary<CinematicSpritePose, SpriteComposition> ICinematicSpriteFrames.Frames => frames;

    /// <summary>Loads and validates all 37 named post-credits Samus visual frames.</summary>
    public static EndingRewardSpritePresentation Load(Stream json)
    {
        Ensure.NotNull(json);
        EndingRewardSpriteDocument document;
        document = JsonAssetDocument.Read<EndingRewardSpriteDocument>(json, ArtworkJson.Options, "ending reward sprite");
        IReadOnlyList<CinematicSpriteFrameDefinition> definitions =
            EndingRewardSpriteDefinitions.Frames;
        if (document.Version != EndingRewardSpriteFormat.Version ||
            document.Frames is null || document.Frames.Count != definitions.Count)
            throw new InvalidDataException(
                $"Ending reward requires exactly {definitions.Count} named visual frames.");
        var frames = new Dictionary<CinematicSpritePose, SpriteComposition>();
        foreach (CinematicSpriteFrameDefinition definition in definitions)
        {
            if (!document.Frames.TryGetValue(definition.Name, out SpriteVisualPart[]? visual) ||
                visual is null)
                throw new InvalidDataException(
                    $"Ending reward frame {definition.Name} is missing.");
            SpriteComposition composition = IntroCinematicSpriteCompiler.Compile(visual, definition.Name);
            composition = EndingRewardHeadParts.CalculateIfMatching(definition.Pose, composition);
            composition = EndingRewardStandingParts.CalculateIfMatching(definition.Pose, composition);
            composition = EndingRewardPrepareJumpParts.CalculateIfMatching(definition.Pose, composition);
            composition = EndingRewardJumpParts.CalculateIfMatching(definition.Pose, composition);
            composition = EndingRewardSuitlessGridParts.CalculateIfMatching(definition.Pose, composition);
            composition = EndingRewardSuitlessStandingParts.CalculateIfMatching(definition.Pose, composition);
            composition = EndingRewardShootingSceneParts.CalculateIfMatching(definition.Pose, composition);
            composition = EndingRewardArmParts.CalculateIfMatching(definition.Pose, composition);
            composition = EndingRewardHairParts.CalculateIfMatching(definition.Pose, composition);
            frames.Add(definition.Pose, composition);
        }
        return new EndingRewardSpritePresentation(frames);
    }

    /// <summary>Validates and writes an ending-reward sprite document as JSON.</summary>
    public static void Write(Stream json, EndingRewardSpriteDocument document)
    {
        Ensure.NotNull(json);
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document,
            ArtworkJson.Options);
        _ = Load(new MemoryStream(bytes, writable: false));
        json.Write(bytes);
    }
}
