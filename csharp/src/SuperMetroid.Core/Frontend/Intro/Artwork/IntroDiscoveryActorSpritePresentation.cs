using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.CeresCinematics.Artwork;

namespace SuperMetroid.Core.Frontend.Intro.Artwork;

/// <summary>Editable SR388 egg and baby OAM art; animation and motion remain compiled.</summary>
public sealed class IntroDiscoveryActorSpritePresentation : IIntroCinematicSpritePresentation
{
    /// <summary>Validated egg, remnant, and baby compositions keyed by native bank-$8C pointers.</summary>
    private readonly Dictionary<ushort, SpriteComposition> frames;

    /// <summary>Canonical identity of all selected decoded visual frames, preserving ordered OAM parts.</summary>
    public string ContentIdentity => SelectedPresentationHash.FromCompositions(nameof(IntroDiscoveryActorSpritePresentation), frames);

    /// <summary>Retains the compiled twenty-frame selection after JSON validation.</summary>
    private IntroDiscoveryActorSpritePresentation(Dictionary<ushort, SpriteComposition> frames) =>
        this.frames = frames;

    /// <summary>Draws one installed egg, remnant, confused-baby, or hatched-baby composition.</summary>
    /// <param name="pointer">Native bank-$8C spritemap identity for one of the twenty actor frames.</param>
    /// <param name="objects">The OAM buffer receiving the ordered sprite parts.</param>
    /// <param name="origin">Native wrapped sixteen-bit screen origin in pixels.</param>
    /// <param name="paletteBits">OBJ palette attribute bits applied to inherited-palette parts.</param>
    /// <param name="originIsOnScreen">Whether the anchor is already in screen space rather than the off-screen cinematic space.</param>
    public void Draw(ushort pointer, ScreenSprites objects, Position16<OamPixels> origin,
        ushort paletteBits, bool originIsOnScreen) =>
        SpriteComposition.DrawInstalled(frames, "Intro discovery actor", pointer, objects, origin, paletteBits, originIsOnScreen);

    /// <summary>Loads and validates all sixteen egg/remnant and four baby visual frames.</summary>
    /// <param name="json">The caller-owned stream containing the editable sprite document.</param>
    /// <returns>The compiled presentation keyed by native bank-$8C spritemap identities.</returns>
    /// <exception cref="InvalidDataException">The JSON, version, frame set, or sprite parts are invalid.</exception>
    public static IntroDiscoveryActorSpritePresentation Load(Stream json)
    {
        Ensure.NotNull(json);
        IntroDiscoveryActorSpriteDocument document;
        document = JsonAssetDocument.Read<IntroDiscoveryActorSpriteDocument>(json, ArtworkJson.Options, "intro discovery actor sprite");
        IReadOnlyList<IntroSpriteFrameDefinition> definitions =
            IntroDiscoveryActorSpriteDefinitions.Frames;
        if (document.Version != IntroDiscoveryActorSpriteFormat.Version ||
            document.Frames is null || document.Frames.Count != definitions.Count)
            throw new InvalidDataException("Intro discovery actors require twenty named visual frames.");
        var frames = new Dictionary<ushort, SpriteComposition>();
        foreach (IntroSpriteFrameDefinition definition in definitions)
        {
            if (!document.Frames.TryGetValue(definition.Name, out SpriteVisualPart[]? visual) ||
                visual is null)
                throw new InvalidDataException($"Intro discovery actor frame {definition.Name} is missing.");
            SpriteComposition compiled = IntroCinematicSpriteCompiler.Compile(visual, definition.Name);
            compiled = CeresLargeAsteroidParts.CalculateIfMatching(definition.Pointer, compiled);
            compiled = IntroEggRockingParts.CalculateIfMatching(definition.Pointer, compiled);
            compiled = IntroEggCrackingParts.CalculateIfMatching(definition.Pointer, compiled);
            frames.Add(definition.Pointer,
                IntroConfusedBabyParts.CalculateIfMatching(definition.Pointer,
                    IntroEggRemnantParts.CalculateIfMatching(definition.Pointer,
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
