using System.Text.Json;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Editable OAM compositions unique to the station blast and Zebes reveal.</summary>
public sealed class CeresDestructionSpritePresentation : IIntroCinematicSpritePresentation
{
    /// <summary>Decoded sprite compositions indexed by their bank-relative scene pointers.</summary>
    private readonly Dictionary<ushort, SpriteComposition> frames;

    /// <summary>Canonical identity of all selected decoded visual frames, preserving ordered OAM parts.</summary>
    public string ContentIdentity => SelectedPresentationHash.FromCompositions(nameof(CeresDestructionSpritePresentation), frames);

    /// <summary>Stores the already validated frame map used by scene drawing.</summary>
    /// <param name="frames">Compiled compositions keyed by the pointers referenced by cinematic sprite instructions.</param>
    private CeresDestructionSpritePresentation(Dictionary<ushort, SpriteComposition> frames) =>
        this.frames = frames;

    /// <summary>Whether a bank-$8C spritemap identity belongs to this destruction/reveal catalog; the scene router uses this to distinguish unique frames from shared Ceres-flight artwork.</summary>
    public bool Contains(ushort pointer) => frames.ContainsKey(pointer);

    /// <summary>Appends the selected blast or backdrop composition in authored OAM order, applying palette inheritance and the requested native origin-clipping path.</summary>
    /// <param name="pointer">Installed bank-$8C spritemap identity; shared flight-frame identities must be routed to the flight catalog.</param>
    /// <param name="oam">Destination OAM buffer for this frame's ordered visual parts.</param>
    /// <param name="x">Native 16-bit screen-origin X coordinate in pixels, retaining wrapped negative values.</param>
    /// <param name="y">Native 16-bit screen-origin Y coordinate in pixels.</param>
    /// <param name="paletteBits">Packed OBJ palette bits used only by parts whose editable palette is null.</param>
    /// <param name="originIsOnScreen">True for ordinary origin clipping, false for the native off-screen Y-wrap path.</param>
    public void Draw(ushort pointer, OamBuffer oam, ushort x, ushort y,
        ushort paletteBits, bool originIsOnScreen)
    {
        if (!frames.TryGetValue(pointer, out SpriteComposition? frame))
            throw new InvalidDataException(
                $"Ceres destruction sprite frame $8C:{pointer:X4} is not installed.");
        if (originIsOnScreen)
            frame.DrawOnScreen(oam, x, y, paletteBits);
        else
            frame.DrawOffScreen(oam, x, y, paletteBits);
    }

    /// <summary>Loads version-1 <c>ceres-destruction-sprites.json</c>, requiring all 23 named compositions and validating each ordered part's offset, atlas region, size, palette, and priority.</summary>
    /// <param name="json">Caller-owned JSON stream consumed from its current position and left open; unknown and duplicate properties are rejected.</param>
    /// <returns>Selected compiled frames, sharing calculated stock compositions only when all supplied visual fields match.</returns>
    public static CeresDestructionSpritePresentation Load(Stream json)
    {
        ArgumentNullException.ThrowIfNull(json);
        CeresDestructionSpriteDocument document;
        try
        {
            using JsonDocument parsed = JsonDocument.Parse(json);
            EnemySpritemapCatalog.RejectDuplicateProperties(parsed.RootElement);
            document = parsed.RootElement.Deserialize<CeresDestructionSpriteDocument>(
                MapPresentationFormat.JsonOptions) ??
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
            compiled = PlanetZebesTitleParts.CalculateIfMatching(definition.Pointer, compiled);
            compiled = ZebesPlanetBandParts.CalculateIfMatching(definition.Pointer, compiled);
            compiled = ZebesStarGridParts.CalculateIfMatching(definition.Pointer, compiled);
            frames.Add(definition.Pointer, compiled);
        }
        return new CeresDestructionSpritePresentation(frames);
    }

    /// <summary>Serializes the destruction/reveal compositions as UTF-8 JSON and validates the complete named-frame set and OAM fields before writing any bytes.</summary>
    /// <param name="json">Destination written at its current position and left open.</param>
    /// <param name="document">Versioned selected backdrop and blast compositions to serialize.</param>
    public static void Write(Stream json, CeresDestructionSpriteDocument document)
    {
        ArgumentNullException.ThrowIfNull(json);
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document,
            MapPresentationFormat.JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        json.Write(bytes);
    }
}

/// <summary>Routes the scene's shared approach frames and unique destruction frames.</summary>
internal sealed class CeresSceneSpritePresentation(
    CeresFlightSpritePresentation flight,
    CeresDestructionSpritePresentation destruction) : IIntroCinematicSpritePresentation
{
    public void Draw(ushort pointer, OamBuffer oam, ushort x, ushort y,
        ushort paletteBits, bool originIsOnScreen)
    {
        IIntroCinematicSpritePresentation owner = destruction.Contains(pointer)
            ? destruction : flight;
        owner.Draw(pointer, oam, x, y, paletteBits, originIsOnScreen);
    }
}

/// <summary>Editable OAM composition schema for the Ceres explosion and Zebes reveal, separate from cinematic actors, motion, timing, and the shared approach artwork.</summary>
public sealed record CeresDestructionSpriteDocument
{
    /// <summary>Composition schema revision; loading currently requires version 1.</summary>
    public required int Version { get; init; }
    /// <summary>Exactly 23 named ordered part arrays: seven asteroid/planet/title/star backdrops, six small blasts, four large blasts, and six station blasts; null part palettes inherit the actor's palette.</summary>
    public required Dictionary<string, SpriteVisualPart[]> Frames { get; init; }
}

/// <summary>One stable destruction/reveal composition identity with its original native OAM geometry; stock part count is descriptive and does not restrict independently edited compositions.</summary>
/// <param name="Name">Stable JSON key for a backdrop or numbered blast frame.</param>
/// <param name="Pointer">Bank-relative $8C spritemap pointer selected by the scene's compiled sprite instruction lists.</param>
/// <param name="StockPartCount">Number of ordered five-byte OAM parts in the original record, excluding its two-byte count header.</param>
public readonly record struct CeresDestructionSpriteFrameDefinition(
    string Name, ushort Pointer, int StockPartCount);

/// <summary>Installed filename and supported schema revision for the selected Ceres-destruction and Zebes-reveal OAM compositions.</summary>
public static class CeresDestructionSpriteFormat
{
    /// <summary>Supported composition schema revision, requiring the complete 23-name frame set.</summary>
    public const int Version = 1;
    /// <summary>Installed editable JSON filename for unique destruction/reveal backdrops and blast frames.</summary>
    public const string FileName = "ceres-destruction-sprites.json";
}

/// <summary>
/// Shared single-cell star rendering for8C:975E/979C/97BC/97D2. Ordered grid
/// positions and glyph choices are retained decorative star composition under #1165.
/// Native 8B:C8B9-C991 translates whole sheets; individual stars have no functional roles.
/// small size, zero priority and no flips are common to all29 original parts.
/// </summary>
internal sealed class ZebesStarGridParts : IReadOnlyList<CompiledSpritePart>
{
    /// <summary>Grid coordinates and tile identities for the supplied star composition.</summary>
    private readonly (sbyte X, sbyte Y, ushort Tile)[] placements;

    /// <summary>Captures each supplied star's cell position and tile for grid-based composition matching.</summary>
    /// <param name="supplied">Compiled star composition whose original placements must be preserved.</param>
    private ZebesStarGridParts(SpriteComposition supplied)
    {
        placements = new (sbyte, sbyte, ushort)[supplied.PartCount];
        for (int index = 0; index < placements.Length; index++)
        {
            CompiledSpritePart part = supplied.Part(index);
            placements[index] = ((sbyte)(part.X.SignedOffset / 8),
                (sbyte)(unchecked((sbyte)part.Y) / 8), (ushort)part.Attributes.TileNumber);
        }
    }
    /// <summary>
    /// Reuses the regular star-grid composition for the four star-sheet pointers only when
    /// its complete visual fields match the supplied editable composition.
    /// </summary>
    /// <param name="pointer">Spritemap identity being compiled.</param>
    /// <param name="supplied">Composition produced from the editable frame definition.</param>
    /// <returns>The matching calculated composition, or <paramref name="supplied"/> unchanged.</returns>
    internal static SpriteComposition CalculateIfMatching(ushort pointer, SpriteComposition supplied)
    {
        if (pointer is not (CeresDestructionSpriteDefinitions.UpperLeftStars or
            CeresDestructionSpriteDefinitions.UpperRightStars or
            CeresDestructionSpriteDefinitions.LowerLeftStars or
            CeresDestructionSpriteDefinitions.LowerRightStars)) return supplied;
        // Full-field matching preserves off-grid and independently edited appearance.
        return supplied.CalculateIfMatching(new ZebesStarGridParts(supplied));
    }
    /// <summary>Gets the number of star cells captured from the supplied composition.</summary>
    public int Count => placements.Length;

    /// <summary>Gets the compiled OAM part reconstructed for a star-grid cell.</summary>
    /// <param name="index">Zero-based cell index in the original composition order.</param>
    /// <returns>A small, unflipped priority-zero sprite part at the cell's native pixel position.</returns>
    public CompiledSpritePart this[int index]
    {
        get
        {
            var point = placements[index];
            return new(SnesSpritemapXWord.Create(point.X * 8, false), unchecked((byte)(point.Y * 8)),
                SnesObjAttributeWord.Create(point.Tile, 0, 0), true);
        }
    }
    /// <summary>Enumerates reconstructed star parts in their original composition order.</summary>
    /// <returns>An enumerator over the captured star cells.</returns>
    public IEnumerator<CompiledSpritePart> GetEnumerator()
    {
        for (int index = 0; index < Count; index++) yield return this[index];
    }
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
}
