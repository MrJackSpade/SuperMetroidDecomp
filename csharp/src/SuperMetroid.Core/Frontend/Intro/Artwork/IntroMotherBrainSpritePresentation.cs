using System.Text.Json;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Intro.Artwork;

/// <summary>Editable intro Mother Brain OAM art, separate from AI and instruction timing.</summary>
public sealed class IntroMotherBrainSpritePresentation
{
    /// <summary>Compiled OAM compositions keyed by the poses that show them.</summary>
    private readonly Dictionary<CinematicSpritePose, SpriteComposition> frames;

    /// <summary>Canonical identity of all selected decoded visual frames, preserving ordered OAM parts.</summary>
    public string ContentIdentity => CinematicSpriteArtwork.ContentIdentity(nameof(IntroMotherBrainSpritePresentation), frames);

    /// <summary>Retains the validated frame set after source arrays have been copied into compositions.</summary>
    private IntroMotherBrainSpritePresentation(Dictionary<CinematicSpritePose, SpriteComposition> frames) =>
        this.frames = frames;

    /// <summary>Appends one installed Mother Brain composition through the on-screen OAM path used by the opening flashback, without advancing animation.</summary>
    /// <param name="pose">The cinematic pose whose composition to draw.</param>
    /// <param name="objects">Destination object-attribute buffer; authored part order is preserved for sprite overlap.</param>
    /// <param name="origin">Native wrapped sixteen-bit screen origin in pixels.</param>
    /// <param name="paletteBits">Actor's OBJ palette bits, applied only to parts configured to inherit the palette.</param>
    /// <exception cref="InvalidDataException">The requested frame identity is not installed.</exception>
    public void Draw(CinematicSpritePose pose, ScreenSprites objects, Position16<OamPixels> origin, ushort paletteBits)
    {
        if (!frames.TryGetValue(pose, out SpriteComposition? frame))
            throw new InvalidDataException($"Intro Mother Brain frame for pose {pose.Index} is not installed.");
        frame.Draw(objects, origin, paletteBits);
    }

    /// <summary>Validates and compiles the three named intro Mother Brain frames into independently owned visual compositions.</summary>
    /// <param name="json">Readable JSON stream at its current position; it remains open and caller-owned.</param>
    /// <returns>Artwork keyed by the original bank-$8C identities, independent of subsequent edits to authoring arrays.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is null.</exception>
    /// <exception cref="InvalidDataException">JSON is malformed, contains duplicate properties, has an unsupported version or frame set, or contains invalid OAM visual fields.</exception>
    public static IntroMotherBrainSpritePresentation Load(Stream json)
    {
        Ensure.NotNull(json);
        IntroMotherBrainSpriteDocument document;
        document = JsonAssetDocument.Read<IntroMotherBrainSpriteDocument>(json, ArtworkJson.Options, "intro Mother Brain sprite");
        IReadOnlyList<CinematicSpriteFrameDefinition> definitions =
            IntroMotherBrainSpriteDefinitions.Frames;
        if (document.Version != IntroMotherBrainSpriteFormat.Version ||
            document.Frames is null || document.Frames.Count != definitions.Count)
            throw new InvalidDataException("Intro Mother Brain requires three named visual frames.");
        var frames = new Dictionary<CinematicSpritePose, SpriteComposition>();
        foreach (CinematicSpriteFrameDefinition definition in definitions)
        {
            if (!document.Frames.TryGetValue(definition.Name, out SpriteVisualPart[]? visual) ||
                visual is null)
                throw new InvalidDataException($"Intro Mother Brain frame {definition.Name} is missing.");
            frames.Add(definition.Pose,
                IntroMotherBrainParts.CalculateIfMatching(definition.Pose,
                    IntroCinematicSpriteCompiler.Compile(visual, definition.Name)));
        }
        return new IntroMotherBrainSpritePresentation(frames);
    }

    /// <summary>Serializes and validates the complete Mother Brain visual document before writing any bytes to the destination.</summary>
    /// <param name="json">Writable destination at its current position; it remains open and caller-owned.</param>
    /// <param name="document">Editable artwork containing the supported version and all three canonical frame names.</param>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is null.</exception>
    /// <exception cref="InvalidDataException">The serialized document does not satisfy the loader's schema and visual-field constraints.</exception>
    public static void Write(Stream json, IntroMotherBrainSpriteDocument document)
    {
        Ensure.NotNull(json);
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document,
            ArtworkJson.Options);
        _ = Load(new MemoryStream(bytes, writable: false));
        json.Write(bytes);
    }
}
