using System.Text.Json;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Intro.Artwork;

/// <summary>Editable explosion OAM art, independent of native placement and timing.</summary>
public sealed class IntroMotherBrainExplosionSpritePresentation
{
    /// <summary>Validated fourth-hit compositions keyed by the poses that show them.</summary>
    private readonly Dictionary<CinematicSpritePose, SpriteComposition> frames;

    /// <summary>Canonical identity of all selected decoded visual frames, preserving ordered OAM parts.</summary>
    public string ContentIdentity => CinematicSpriteArtwork.ContentIdentity(nameof(IntroMotherBrainExplosionSpritePresentation), frames);

    /// <summary>Retains the validated twelve-frame selection after JSON compilation.</summary>
    private IntroMotherBrainExplosionSpritePresentation(
        Dictionary<CinematicSpritePose, SpriteComposition> frames) => this.frames = frames;

    /// <summary>Draws one installed small- or large-explosion composition at its script-owned screen anchor.</summary>
    /// <param name="pose">The cinematic pose whose composition to draw.</param>
    /// <param name="objects">The OAM buffer receiving the ordered sprite parts.</param>
    /// <param name="origin">Native wrapped sixteen-bit screen origin in pixels.</param>
    /// <param name="paletteBits">OBJ palette attribute bits applied to inherited-palette parts.</param>
    public void Draw(CinematicSpritePose pose, ScreenSprites objects, Position16<OamPixels> origin, ushort paletteBits)
    {
        if (!frames.TryGetValue(pose, out SpriteComposition? frame))
            throw new InvalidDataException(
                $"Intro Mother Brain explosion frame for pose {pose.Index} is not installed.");
        frame.Draw(objects, origin, paletteBits);
    }

    /// <summary>Loads and validates all six small and six large fourth-hit explosion frames.</summary>
    /// <param name="json">The caller-owned stream containing the editable sprite document.</param>
    /// <returns>The compiled presentation keyed by native bank-$8C spritemap identities.</returns>
    /// <exception cref="InvalidDataException">The JSON, version, frame set, or sprite parts are invalid.</exception>
    public static IntroMotherBrainExplosionSpritePresentation Load(Stream json)
    {
        Ensure.NotNull(json);
        IntroMotherBrainExplosionSpriteDocument document;
        document = JsonAssetDocument.Read<IntroMotherBrainExplosionSpriteDocument>(json, ArtworkJson.Options, "intro Mother Brain explosion sprite");
        IReadOnlyList<CinematicSpriteFrameDefinition> definitions =
            IntroMotherBrainExplosionSpriteDefinitions.Frames;
        if (document.Version != IntroMotherBrainExplosionSpriteFormat.Version ||
            document.Frames is null || document.Frames.Count != definitions.Count)
            throw new InvalidDataException(
                "Intro Mother Brain explosions require twelve named visual frames.");
        var frames = new Dictionary<CinematicSpritePose, SpriteComposition>();
        foreach (CinematicSpriteFrameDefinition definition in definitions)
        {
            if (!document.Frames.TryGetValue(definition.Name, out SpriteVisualPart[]? visual) ||
                visual is null)
                throw new InvalidDataException(
                    $"Intro Mother Brain explosion frame {definition.Name} is missing.");
            frames.Add(definition.Pose,
                IntroMotherBrainExplosionParts.CalculateIfMatching(definition.Pose,
                    IntroCinematicSpriteCompiler.Compile(visual, definition.Name)));
        }
        return new IntroMotherBrainExplosionSpritePresentation(frames);
    }

    /// <summary>Validates and writes an editable Mother Brain explosion sprite document as JSON.</summary>
    /// <param name="json">The caller-owned destination stream.</param>
    /// <param name="document">The twelve-frame sprite document to validate and serialize.</param>
    public static void Write(Stream json, IntroMotherBrainExplosionSpriteDocument document)
    {
        Ensure.NotNull(json);
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document,
            ArtworkJson.Options);
        _ = Load(new MemoryStream(bytes, writable: false));
        json.Write(bytes);
    }
}
