using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Frontend.Title.Artwork;

namespace SuperMetroid.Core.Frontend.CeresCinematics.Artwork;

/// <summary>Editable OAM compositions unique to the station blast and Zebes reveal.</summary>
public sealed class CeresDestructionSpritePresentation : IIntroCinematicSpritePresentation
{
    /// <summary>Validated destruction and reveal compositions keyed by their bank-8C spritemap pointers.</summary>
    private readonly Dictionary<ushort, SpriteComposition> frames;

    /// <summary>Canonical identity of all selected decoded visual frames, preserving ordered OAM parts.</summary>
    public string ContentIdentity => SelectedPresentationHash.FromCompositions(nameof(CeresDestructionSpritePresentation), frames);

    /// <summary>Retains the fully validated compositions selected by the scene's native frame pointers.</summary>
    private CeresDestructionSpritePresentation(Dictionary<ushort, SpriteComposition> frames) =>
        this.frames = frames;

    /// <summary>Whether a bank-$8C spritemap identity belongs to this destruction/reveal catalog; the scene router uses this to distinguish unique frames from shared Ceres-flight artwork.</summary>
    public bool Contains(ushort pointer) => frames.ContainsKey(pointer);

    /// <summary>Appends the selected blast or backdrop composition in authored OAM order, applying palette inheritance and the requested native origin-clipping path.</summary>
    /// <param name="pointer">Installed bank-$8C spritemap identity; shared flight-frame identities must be routed to the flight catalog.</param>
    /// <param name="objects">Destination OAM buffer for this frame's ordered visual parts.</param>
    /// <param name="origin">Native wrapped sixteen-bit screen origin in pixels.</param>
    /// <param name="paletteBits">Packed OBJ palette bits used only by parts whose editable palette is null.</param>
    /// <param name="originIsOnScreen">True for ordinary origin clipping, false for the native off-screen Y-wrap path.</param>
    public void Draw(ushort pointer, ScreenSprites objects, Position16<OamPixels> origin,
        ushort paletteBits, bool originIsOnScreen) =>
        SpriteComposition.DrawInstalled(frames, "Ceres destruction sprite", pointer, objects, origin, paletteBits, originIsOnScreen);

    /// <summary>Loads version-1 <c>ceres-destruction-sprites.json</c>, requiring all 23 named compositions and validating each ordered part's offset, atlas region, size, palette, and priority.</summary>
    /// <param name="json">Caller-owned JSON stream consumed from its current position and left open; unknown and duplicate properties are rejected.</param>
    /// <returns>Selected compiled frames, sharing calculated stock compositions only when all supplied visual fields match.</returns>
    public static CeresDestructionSpritePresentation Load(Stream json)
    {
        Ensure.NotNull(json);
        CeresDestructionSpriteDocument document;
        try
        {
            using JsonDocument parsed = JsonDocument.Parse(json);
            EnemySpritemapCatalog.RejectDuplicateProperties(parsed.RootElement);
            document = parsed.RootElement.Deserialize<CeresDestructionSpriteDocument>(
                ArtworkJson.Options) ??
                throw new InvalidDataException("Ceres destruction sprite JSON is null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid Ceres destruction sprite JSON.", error);
        }
        IReadOnlyList<CeresDestructionSpriteFrameDefinition> definitions =
            CeresDestructionSpriteDefinitions.Frames;
        if (document.Version != CeresDestructionSpriteFormat.Version ||
            document.Frames is null || document.Frames.Count != definitions.Count)
            throw new InvalidDataException(
                $"Ceres destruction requires exactly {definitions.Count} named visual frames.");
        var frames = new Dictionary<ushort, SpriteComposition>();
        foreach (CeresDestructionSpriteFrameDefinition definition in definitions)
        {
            if (!document.Frames.TryGetValue(definition.Name, out SpriteVisualPart[]? visual) ||
                visual is null)
                throw new InvalidDataException(
                    $"Ceres destruction sprite {definition.Name} is missing.");
            var compiled = IntroCinematicSpriteCompiler.Compile(visual, definition.Name);
            compiled = CeresLargeAsteroidParts.CalculateIfMatching(definition.Pointer, compiled);
            compiled = IntroMotherBrainExplosionParts.CalculateIfMatching(definition.Pointer, compiled);
            compiled = CeresStationBlastParts.CalculateIfMatching(definition.Pointer, compiled);
            compiled = CeresLargeBlastParts.CalculateIfMatching(definition.Pointer, compiled);
            compiled = PlanetZebesTitleParts.CalculateIfMatching(definition.Backdrop, compiled);
            compiled = ZebesPlanetBandParts.CalculateIfMatching(definition.Backdrop, compiled);
            compiled = ZebesStarGridParts.CalculateIfMatching(definition.Backdrop, compiled);
            frames.Add(definition.Pointer, compiled);
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
