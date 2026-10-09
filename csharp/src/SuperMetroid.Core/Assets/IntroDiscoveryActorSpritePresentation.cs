using System.Text.Json;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Editable SR388 egg and baby OAM art; animation and motion remain compiled.</summary>
public sealed class IntroDiscoveryActorSpritePresentation : IIntroCinematicSpritePresentation
{
    /// <summary>Compiled discovery-actor compositions keyed by their native bank-$8C spritemap pointers.</summary>
    private readonly Dictionary<ushort, SpriteComposition> frames;

    /// <summary>Canonical identity of all selected decoded visual frames, preserving ordered OAM parts.</summary>
    public string ContentIdentity => SelectedPresentationHash.FromCompositions(nameof(IntroDiscoveryActorSpritePresentation), frames);

    /// <summary>Creates a presentation from the frame compositions validated and compiled by <see cref="Load"/>.</summary>
    /// <param name="frames">Compiled compositions indexed by each actor frame's native bank-$8C pointer.</param>
    private IntroDiscoveryActorSpritePresentation(Dictionary<ushort, SpriteComposition> frames) =>
        this.frames = frames;

    /// <summary>Draws one installed egg, remnant, confused-baby, or hatched-baby composition.</summary>
    /// <param name="pointer">Native bank-$8C spritemap identity for one of the twenty actor frames.</param>
    /// <param name="oam">The OAM buffer receiving the ordered sprite parts.</param>
    /// <param name="x">Actor anchor X coordinate in the selected coordinate space.</param>
    /// <param name="y">Actor anchor Y coordinate in the selected coordinate space.</param>
    /// <param name="paletteBits">OBJ palette attribute bits applied to inherited-palette parts.</param>
    /// <param name="originIsOnScreen">Whether the anchor is already in screen space rather than the off-screen cinematic space.</param>
    public void Draw(ushort pointer, OamBuffer oam, ushort x, ushort y,
        ushort paletteBits, bool originIsOnScreen)
    {
        if (!frames.TryGetValue(pointer, out SpriteComposition? frame))
            throw new InvalidDataException($"Intro discovery actor frame $8C:{pointer:X4} is not installed.");
        if (originIsOnScreen)
            frame.DrawOnScreen(oam, x, y, paletteBits);
        else
            frame.DrawOffScreen(oam, x, y, paletteBits);
    }

    /// <summary>Loads and validates all sixteen egg/remnant and four baby visual frames.</summary>
    /// <param name="json">The caller-owned stream containing the editable sprite document.</param>
    /// <returns>The compiled presentation keyed by native bank-$8C spritemap identities.</returns>
    /// <exception cref="InvalidDataException">The JSON, version, frame set, or sprite parts are invalid.</exception>
    public static IntroDiscoveryActorSpritePresentation Load(Stream json)
    {
        ArgumentNullException.ThrowIfNull(json);
        IntroDiscoveryActorSpriteDocument document;
        try
        {
            using JsonDocument parsed = JsonDocument.Parse(json);
            EnemySpritemapCatalog.RejectDuplicateProperties(parsed.RootElement);
            document = parsed.RootElement.Deserialize<IntroDiscoveryActorSpriteDocument>(
                MapPresentationFormat.JsonOptions) ??
                throw new InvalidDataException("Intro discovery actor sprite JSON is null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid intro discovery actor sprite JSON.", error);
        }
        IReadOnlyList<IntroDiscoveryActorSpriteFrameDefinition> definitions =
            IntroDiscoveryActorSpriteDefinitions.Frames;
        if (document.Version != IntroDiscoveryActorSpriteFormat.Version ||
            document.Frames is null || document.Frames.Count != definitions.Count)
            throw new InvalidDataException("Intro discovery actors require twenty named visual frames.");
        var frames = new Dictionary<ushort, SpriteComposition>();
        foreach (IntroDiscoveryActorSpriteFrameDefinition definition in definitions)
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
        ArgumentNullException.ThrowIfNull(json);
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document,
            MapPresentationFormat.JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        json.Write(bytes);
    }
}

/// <summary>Editable JSON schema for the SR388 egg, remnants, and baby Metroid compositions.</summary>
public sealed record IntroDiscoveryActorSpriteDocument
{
    /// <summary>Gets the discovery-actor sprite schema version.</summary>
    public required int Version { get; init; }

    /// <summary>Gets all twenty required named visual compositions in the compiled actor set.</summary>
    public required Dictionary<string, SpriteVisualPart[]> Frames { get; init; }
}

/// <summary>Installed file identity for the twenty SR388 egg and baby actor frames.</summary>
public static class IntroDiscoveryActorSpriteFormat
{
    /// <summary>The supported discovery-actor sprite JSON schema version.</summary>
    public const int Version = 1;

    /// <summary>The embedded editable discovery-actor sprite asset file name.</summary>
    public const string FileName = "intro-discovery-actor-sprites.json";
}
