using System.Text.Json;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Intro.Artwork;

/// <summary>Editable baby-Metroid OAM art for the two scientist scenes only.</summary>
public sealed class IntroScientistSpritePresentation : IIntroCinematicSpritePresentation
{
    /// <summary>Validated baby-Metroid compositions keyed by bank-$8C spritemap identity.</summary>
    private readonly Dictionary<ushort, SpriteComposition> frames;

    /// <summary>Canonical identity of all selected decoded visual frames, preserving ordered OAM parts.</summary>
    public string ContentIdentity => SelectedPresentationHash.FromCompositions(nameof(IntroScientistSpritePresentation), frames);

    /// <summary>Retains the ten validated frames after JSON compilation.</summary>
    private IntroScientistSpritePresentation(Dictionary<ushort, SpriteComposition> frames) =>
        this.frames = frames;

    /// <summary>Draws one installed baby-Metroid composition used by the two scientist scenes.</summary>
    /// <param name="pointer">Native bank-$8C spritemap identity for one of the ten frames.</param>
    /// <param name="objects">The OAM buffer receiving the ordered sprite parts.</param>
    /// <param name="origin">Native wrapped sixteen-bit screen origin in pixels.</param>
    /// <param name="paletteBits">OBJ palette attribute bits applied to inherited-palette parts.</param>
    /// <param name="originIsOnScreen">Whether the anchor is already in screen space rather than the off-screen cinematic space.</param>
    public void Draw(ushort pointer, ScreenSprites objects, Position16<OamPixels> origin,
        ushort paletteBits, bool originIsOnScreen) =>
        SpriteComposition.DrawInstalled(frames, "Intro scientist baby", pointer, objects, origin, paletteBits, originIsOnScreen);

    /// <summary>Loads and validates all ten baby-Metroid visual frames for the scientist scenes.</summary>
    /// <param name="json">The caller-owned stream containing the editable sprite document.</param>
    /// <returns>The compiled presentation keyed by native bank-$8C spritemap identities.</returns>
    /// <exception cref="InvalidDataException">The JSON, version, frame set, or sprite parts are invalid.</exception>
    public static IntroScientistSpritePresentation Load(Stream json)
    {
        Ensure.NotNull(json);
        IntroScientistSpriteDocument document;
        document = JsonAssetDocument.Read<IntroScientistSpriteDocument>(json, ArtworkJson.Options, "intro scientist sprite");
        IReadOnlyList<IntroSpriteFrameDefinition> definitions =
            IntroScientistSpriteDefinitions.Frames;
        if (document.Version != IntroScientistSpriteFormat.Version ||
            document.Frames is null || document.Frames.Count != definitions.Count)
            throw new InvalidDataException("Intro scientist scenes require ten named visual frames.");
        var frames = new Dictionary<ushort, SpriteComposition>();
        foreach (IntroSpriteFrameDefinition definition in definitions)
        {
            if (!document.Frames.TryGetValue(definition.Name, out SpriteVisualPart[]? visual) ||
                visual is null)
                throw new InvalidDataException(
                    $"Intro scientist sprite frame {definition.Name} is missing.");
            frames.Add(definition.Pointer,
                IntroScientistParts.CalculateIfMatching(definition.Pointer,
                    IntroCinematicSpriteCompiler.Compile(visual, definition.Name)));
        }
        return new IntroScientistSpritePresentation(frames);
    }

    /// <summary>Validates and writes an editable scientist-scene baby sprite document as JSON.</summary>
    /// <param name="json">The caller-owned destination stream.</param>
    /// <param name="document">The ten-frame sprite document to validate and serialize.</param>
    public static void Write(Stream json, IntroScientistSpriteDocument document)
    {
        Ensure.NotNull(json);
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document,
            ArtworkJson.Options);
        _ = Load(new MemoryStream(bytes, writable: false));
        json.Write(bytes);
    }
}
