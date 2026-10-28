using SuperMetroid.Core.Frontend.Intro;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core;
using SuperMetroid.AssetExtraction.Frontend.Intro;
using SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.CeresCinematics.Artwork;

/// <summary>
/// Editable visual compositions for the six star, station, asteroid, and vortex
/// frames shared by the Ceres approach and destruction scenes. Animation timing
/// and actor motion remain in their compiled owners.
/// </summary>
public sealed class CeresFlightSpritePresentation : ICinematicSpriteFrames
{
    /// <summary>Installed Ceres compositions keyed by the poses that show them.</summary>
    private readonly Dictionary<CinematicSpritePose, SpriteComposition> frames;

    /// <inheritdoc/>
    CinematicSpriteFamily ICinematicSpriteFrames.Family => CinematicSpriteFamily.CeresFlight;

    /// <inheritdoc/>
    IReadOnlyDictionary<CinematicSpritePose, SpriteComposition> ICinematicSpriteFrames.Frames => frames;

    /// <summary>Canonical identity of all selected decoded visual frames, preserving ordered OAM parts.</summary>
    public string ContentIdentity => CinematicSpriteArtwork.ContentIdentity(nameof(CeresFlightSpritePresentation), frames);

    /// <summary>Retains the validated composition map shared by approach and destruction scenes.</summary>
    private CeresFlightSpritePresentation(Dictionary<CinematicSpritePose, SpriteComposition> frames) =>
        this.frames = frames;

    /// <summary>Loads exactly the six named visual compositions, rejecting duplicate properties, unsupported versions, missing frames, and invalid OAM parts; compiles selected artwork without importing actor timing or motion.</summary>
    /// <param name="json">Caller-owned UTF-8 JSON stream containing the versioned Ceres flight sprite document.</param>
    /// <returns>The immutable bank-$8C-keyed presentation shared by approach and destruction scenes.</returns>
    public static CeresFlightSpritePresentation Load(Stream json)
    {
        Ensure.NotNull(json);
        CeresFlightSpriteDocument document;
        document = JsonAssetDocument.Read<CeresFlightSpriteDocument>(json, ArtworkJson.Options, "Ceres flight sprite");
        IReadOnlyList<CinematicSpriteFrameDefinition> definitions =
            CeresFlightSpriteDefinitions.Frames;
        if (document.Version != CeresFlightSpriteFormat.Version ||
            document.Frames is null || document.Frames.Count != definitions.Count)
            throw new InvalidDataException("Ceres flight requires exactly six named visual frames.");
        var frames = new Dictionary<CinematicSpritePose, SpriteComposition>();
        foreach (CinematicSpriteFrameDefinition definition in definitions)
        {
            if (!document.Frames.TryGetValue(definition.Name, out SpriteVisualPart[]? visual) ||
                visual is null)
                throw new InvalidDataException($"Ceres flight sprite {definition.Name} is missing.");
            var compiled = IntroCinematicSpriteCompiler.Compile(visual, definition.Name);
            compiled = CeresStarPointParts.CalculateIfMatching(definition.Pose, compiled);
            compiled = CeresLargeAsteroidParts.CalculateIfMatching(definition.Pose, compiled);
            compiled = CeresStationParts.CalculateIfMatching(definition.Pose, compiled);
            compiled = CeresSmallAsteroidParts.CalculateIfMatching(definition.Pose, compiled);
            compiled = CeresVortexParts.CalculateIfMatching(definition.Pose, compiled,
                frames.GetValueOrDefault(CeresFlightSpriteDefinitions.Stars));
            frames.Add(definition.Pose, compiled);
        }
        return new CeresFlightSpritePresentation(frames);
    }

    /// <summary>Serializes a sprite document using the presentation JSON options and validates it through <see cref="Load"/> before writing any bytes to the destination.</summary>
    /// <param name="json">Caller-owned destination stream for the validated UTF-8 JSON.</param>
    /// <param name="document">Document containing all six required named visual frames.</param>
    public static void Write(Stream json, CeresFlightSpriteDocument document)
    {
        Ensure.NotNull(json);
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document,
            ArtworkJson.Options);
        _ = Load(new MemoryStream(bytes, writable: false));
        json.Write(bytes);
    }
}
