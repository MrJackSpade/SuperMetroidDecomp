using SuperMetroid.Core.Frontend.Intro;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Intro.Artwork;

namespace SuperMetroid.Core.Frontend.Ending.Artwork;

/// <summary>Editable OAM compositions for the six atmospheric ending clouds.</summary>
public sealed class EndingCloudSpritePresentation : IIntroCinematicSpritePresentation
{
    /// <summary>Compiled cloud compositions keyed by the poses that show them.</summary>
    private readonly Dictionary<CinematicSpritePose, SpriteComposition> frames;

    /// <summary>Accepts the complete validated six-frame cloud set.</summary>
    private EndingCloudSpritePresentation(Dictionary<CinematicSpritePose, SpriteComposition> frames) =>
        this.frames = frames;

    /// <summary>Canonical identity of the selected decoded visual frames, not JSON formatting.</summary>
    public string ContentIdentity => CinematicSpriteArtwork.ContentIdentity(nameof(EndingCloudSpritePresentation), frames);

    /// <summary>Draws the ending-cloud composition identified by its bank-$8C spritemap pose.</summary>
    /// <param name="pose">The cinematic pose whose composition to draw.</param>
    /// <param name="objects">Object-attribute buffer that receives the composition.</param>
    /// <param name="origin">Native wrapped sixteen-bit screen origin in pixels.</param>
    /// <param name="paletteBits">SNES OAM attribute bits to combine with each part.</param>
    /// <param name="originIsOnScreen">Whether the origin is already in the visible coordinate range.</param>
    public void Draw(CinematicSpritePose pose, ScreenSprites objects, Position16<OamPixels> origin,
        ushort paletteBits, bool originIsOnScreen) =>
        CinematicSpriteArtwork.DrawInstalled(frames, "Ending cloud sprite", pose, objects, origin, paletteBits, originIsOnScreen);

    /// <summary>Loads and validates the six named atmospheric cloud compositions from JSON.</summary>
    /// <param name="json">Stream containing an ending-cloud sprite document.</param>
    /// <returns>The compiled ending-cloud presentation.</returns>
    public static EndingCloudSpritePresentation Load(Stream json)
    {
        Ensure.NotNull(json);
        EndingCloudSpriteDocument document;
        document = JsonAssetDocument.Read<EndingCloudSpriteDocument>(json, ArtworkJson.Options, "ending cloud sprite");
        IReadOnlyList<CinematicSpriteFrameDefinition> definitions =
            EndingCloudSpriteDefinitions.Frames;
        if (document.Version != EndingCloudSpriteFormat.Version ||
            document.Frames is null || document.Frames.Count != definitions.Count)
            throw new InvalidDataException("Ending clouds require exactly six named visual frames.");
        var frames = new Dictionary<CinematicSpritePose, SpriteComposition>();
        for (int index = 0; index < definitions.Count; index++)
        {
            CinematicSpriteFrameDefinition definition = definitions[index];
            if (!document.Frames.TryGetValue(definition.Name, out SpriteVisualPart[]? visual) ||
                visual is null)
                throw new InvalidDataException($"Ending cloud sprite {definition.Name} is missing.");
            frames.Add(definition.Pose,
                IntroCinematicSpriteCompiler.Compile(visual, definition.Name)
                    .CalculateIfMatching(new EndingCloudGridParts((EndingCloudSpriteDefinitions.Role)index)));
        }
        return new EndingCloudSpritePresentation(frames);
    }

    /// <summary>Validates and writes an ending-cloud sprite document as JSON.</summary>
    /// <param name="json">Destination stream.</param>
    /// <param name="document">Document to validate and serialize.</param>
    public static void Write(Stream json, EndingCloudSpriteDocument document)
    {
        Ensure.NotNull(json);
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document,
            ArtworkJson.Options);
        _ = Load(new MemoryStream(bytes, writable: false));
        json.Write(bytes);
    }
}
