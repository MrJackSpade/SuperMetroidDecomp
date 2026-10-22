using SuperMetroid.Core.Frontend.Intro;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Frontend.Title.Artwork;

namespace SuperMetroid.Core.Frontend.CeresCinematics.Artwork;

/// <summary>Editable OAM compositions unique to the station blast and Zebes reveal.</summary>
public sealed class CeresDestructionSpritePresentation : ICinematicSpriteFrames
{
    /// <summary>Validated destruction and reveal compositions keyed by the poses that show them.</summary>
    private readonly Dictionary<CinematicSpritePose, SpriteComposition> frames;

    /// <inheritdoc/>
    CinematicSpriteFamily ICinematicSpriteFrames.Family => CinematicSpriteFamily.CeresDestruction;

    /// <inheritdoc/>
    IReadOnlyDictionary<CinematicSpritePose, SpriteComposition> ICinematicSpriteFrames.Frames => frames;

    /// <summary>Canonical identity of all selected decoded visual frames, preserving ordered OAM parts.</summary>
    public string ContentIdentity => CinematicSpriteArtwork.ContentIdentity(nameof(CeresDestructionSpritePresentation), frames);

    /// <summary>Retains the fully validated compositions the scene's poses show.</summary>
    private CeresDestructionSpritePresentation(Dictionary<CinematicSpritePose, SpriteComposition> frames) =>
        this.frames = frames;

    /// <summary>Loads version-1 <c>ceres-destruction-sprites.json</c>, requiring all 23 named compositions and validating each ordered part's offset, atlas region, size, palette, and priority.</summary>
    /// <param name="json">Caller-owned JSON stream consumed from its current position and left open; unknown and duplicate properties are rejected.</param>
    /// <returns>Selected compiled frames, sharing calculated stock compositions only when all supplied visual fields match.</returns>
    public static CeresDestructionSpritePresentation Load(Stream json)
    {
        Ensure.NotNull(json);
        CeresDestructionSpriteDocument document;
        document = JsonAssetDocument.Read<CeresDestructionSpriteDocument>(json, ArtworkJson.Options, "Ceres destruction sprite");
        IReadOnlyList<CeresDestructionSpriteFrameDefinition> definitions =
            CeresDestructionSpriteDefinitions.Frames;
        if (document.Version != CeresDestructionSpriteFormat.Version ||
            document.Frames is null || document.Frames.Count != definitions.Count)
            throw new InvalidDataException(
                $"Ceres destruction requires exactly {definitions.Count} named visual frames.");
        var frames = new Dictionary<CinematicSpritePose, SpriteComposition>();
        foreach (CeresDestructionSpriteFrameDefinition definition in definitions)
        {
            if (!document.Frames.TryGetValue(definition.Name, out SpriteVisualPart[]? visual) ||
                visual is null)
                throw new InvalidDataException(
                    $"Ceres destruction sprite {definition.Name} is missing.");
            var compiled = IntroCinematicSpriteCompiler.Compile(visual, definition.Name);
            compiled = CeresLargeAsteroidParts.CalculateIfMatching(definition.Pose, compiled);
            compiled = IntroMotherBrainExplosionParts.CalculateIfMatching(definition.Pose, compiled);
            compiled = CeresStationBlastParts.CalculateIfMatching(definition.Pose, compiled);
            compiled = CeresLargeBlastParts.CalculateIfMatching(definition.Pose, compiled);
            compiled = PlanetZebesTitleParts.CalculateIfMatching(definition.Backdrop, compiled);
            compiled = ZebesPlanetBandParts.CalculateIfMatching(definition.Backdrop, compiled);
            compiled = ZebesStarGridParts.CalculateIfMatching(definition.Backdrop, compiled);
            frames.Add(definition.Pose, compiled);
        }
        return new CeresDestructionSpritePresentation(frames);
    }

    /// <summary>Serializes the destruction/reveal compositions as UTF-8 JSON and validates the complete named-frame set and OAM fields before writing any bytes.</summary>
    /// <param name="json">Destination written at its current position and left open.</param>
    /// <param name="document">Versioned selected backdrop and blast compositions to serialize.</param>
    public static void Write(Stream json, CeresDestructionSpriteDocument document)
    {
        Ensure.NotNull(json);
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document,
            ArtworkJson.Options);
        _ = Load(new MemoryStream(bytes, writable: false));
        json.Write(bytes);
    }
}
