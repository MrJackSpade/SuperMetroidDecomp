using System.Text.Json;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Editable OAM compositions for the 16 Zebes-explosion visual frames.</summary>
public sealed class EndingExplosionSpritePresentation : IIntroCinematicSpritePresentation
{
    private readonly Dictionary<ushort, SpriteComposition> frames;

    private EndingExplosionSpritePresentation(Dictionary<ushort, SpriteComposition> frames) =>
        this.frames = frames;

    /// <summary>Canonical identity of the selected decoded visual frames, not JSON formatting.</summary>
    public string ContentIdentity => SelectedPresentationHash.FromCompositions(nameof(EndingExplosionSpritePresentation), frames);

    /// <summary>Draws the Zebes-explosion composition identified by its bank-$8C spritemap pointer.</summary>
    /// <param name="pointer">Bank-$8C pointer identifying one of the installed explosion frames.</param>
    /// <param name="oam">Object-attribute buffer that receives the composition.</param>
    /// <param name="x">Horizontal origin in screen-space coordinates.</param>
    /// <param name="y">Vertical origin in screen-space coordinates.</param>
    /// <param name="paletteBits">SNES OAM attribute bits to combine with each part.</param>
    /// <param name="originIsOnScreen">Whether the origin is already in the visible coordinate range.</param>
    public void Draw(ushort pointer, OamBuffer oam, ushort x, ushort y,
        ushort paletteBits, bool originIsOnScreen)
    {
        if (!frames.TryGetValue(pointer, out SpriteComposition? frame))
            throw new InvalidDataException(
                $"Ending explosion sprite frame $8C:{pointer:X4} is not installed.");
        if (originIsOnScreen)
            frame.DrawOnScreen(oam, x, y, paletteBits);
        else
            frame.DrawOffScreen(oam, x, y, paletteBits);
    }

    /// <summary>Loads and validates the 16 named Zebes-explosion compositions from JSON.</summary>
    /// <param name="json">Stream containing an ending-explosion sprite document.</param>
    /// <returns>The compiled ending-explosion presentation.</returns>
    public static EndingExplosionSpritePresentation Load(Stream json)
    {
        ArgumentNullException.ThrowIfNull(json);
        EndingExplosionSpriteDocument document;
        try
        {
            using JsonDocument parsed = JsonDocument.Parse(json);
            EnemySpritemapCatalog.RejectDuplicateProperties(parsed.RootElement);
            document = parsed.RootElement.Deserialize<EndingExplosionSpriteDocument>(
                MapPresentationFormat.JsonOptions) ??
                throw new InvalidDataException("Ending explosion sprite JSON is null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid ending explosion sprite JSON.", error);
        }
        IReadOnlyList<EndingExplosionSpriteFrameDefinition> definitions =
            EndingExplosionSpriteDefinitions.Frames;
        if (document.Version != EndingExplosionSpriteFormat.Version ||
            document.Frames is null || document.Frames.Count != definitions.Count)
            throw new InvalidDataException(
                $"Ending explosion requires exactly {definitions.Count} named visual frames.");
        var frames = new Dictionary<ushort, SpriteComposition>();
        foreach (EndingExplosionSpriteFrameDefinition definition in definitions)
        {
            if (!document.Frames.TryGetValue(definition.Name, out SpriteVisualPart[]? visual) ||
                visual is null)
                throw new InvalidDataException(
                    $"Ending explosion sprite {definition.Name} is missing.");
            SpriteComposition compiled = IntroCinematicSpriteCompiler.Compile(visual, definition.Name);
            compiled = EndingExplosionQuadrantParts.CalculateIfMatching(definition.Pointer, compiled);
            compiled = EndingExplosionStarfieldParts.CalculateIfMatching(definition.Pointer, compiled);
            compiled = EndingExplosionAfterglowParts.CalculateIfMatching(definition.Pointer, compiled);
            compiled = EndingExplosionSilhouetteParts.CalculateIfMatching(definition.Pointer, compiled);
            frames.Add(definition.Pointer, EndingExplosionGridParts.CalculateIfMatching(definition.Pointer, compiled));
        }
        return new EndingExplosionSpritePresentation(frames);
    }

    /// <summary>Validates and writes an ending-explosion sprite document as JSON.</summary>
    /// <param name="json">Destination stream.</param>
    /// <param name="document">Document to validate and serialize.</param>
    public static void Write(Stream json, EndingExplosionSpriteDocument document)
    {
        ArgumentNullException.ThrowIfNull(json);
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document,
            MapPresentationFormat.JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        json.Write(bytes);
    }
}

/// <summary>JSON schema for the editable Zebes-explosion compositions.</summary>
public sealed record EndingExplosionSpriteDocument
{
    /// <summary>Gets the schema version, which must equal <see cref="EndingExplosionSpriteFormat.Version"/>.</summary>
    public required int Version { get; init; }

    /// <summary>Gets the compositions keyed by the published explosion-frame names.</summary>
    public required Dictionary<string, SpriteVisualPart[]> Frames { get; init; }
}

/// <summary>Distinct bank-$8C frame identities consumed by the eight explosion actors.</summary>
public static class EndingExplosionSpriteDefinitions
{
    /// <summary>Mutually exclusive poses in the published explosion frame order.</summary>
    internal enum Pose
    {
        DamageFirst, Glow = 10, SupernovaFirst = 11, SupernovaSecond = 12, Stars = 13, Silhouette = 14, Afterglow = 15,
    }
    /// <summary>$8C:A396, ExplodingPlanetZebesFrame1; ten four-part records
    /// contain four damage poses, four flash poses and two lava poses.</summary>
    private const ushort PlanetFirst = 0xa396;
    /// <summary>$8C:A28B, ZebesBoomStarryBackground, independent of the planet record chain.</summary>
    private const ushort Starfield = 0xa28b;
    private const int SmallParts = 4, GlowParts = 12, SupernovaParts = 20;
    private const int FrameCount = 16;

    /// <summary>Ordered asset identities, calculated on demand without a stored frame array.</summary>
    public static IReadOnlyList<EndingExplosionSpriteFrameDefinition> Frames { get; } = new FrameView();



    /// <summary>$8C:A396 consecutive counted OAM records, except the independent
    /// $8C:A28B starfield. Shared by asset identity and executable display operands.</summary>
    internal static ushort Pointer(Pose pose)
    {
        int index = (int)pose;
        if ((uint)index >= FrameCount) throw new ArgumentOutOfRangeException(nameof(pose));
        if (index < 10) return (ushort)(PlanetFirst + index * RecordBytes(SmallParts));
        int glow = PlanetFirst + 10 * RecordBytes(SmallParts);
        int supernova = glow + RecordBytes(GlowParts);
        return pose switch
        {
            Pose.Glow => (ushort)glow,
            Pose.SupernovaFirst => (ushort)supernova,
            Pose.SupernovaSecond => (ushort)(supernova + RecordBytes(SupernovaParts)),
            Pose.Stars => Starfield,
            Pose.Silhouette => (ushort)(supernova + 2 * RecordBytes(SupernovaParts)),
            Pose.Afterglow => (ushort)(supernova + 3 * RecordBytes(SupernovaParts)),
            _ => throw new ArgumentOutOfRangeException(nameof(pose)),
        };
    }

    private static EndingExplosionSpriteFrameDefinition Get(int index)
    {
        if ((uint)index >= FrameCount) throw new ArgumentOutOfRangeException(nameof(index));
        ushort pointer = Pointer((Pose)index);
        if (index < 10)
        {
            string family = index < 4 ? "planet-damage" : index < 8 ? "planet-flash" : "lava";
            int stage = index < 4 ? index : index < 8 ? index - 4 : index - 8;
            return new(family + "-" + stage.ToString(System.Globalization.CultureInfo.InvariantCulture),
                pointer, SmallParts);
        }
        // The glow and supernova compositions follow the small planet/core records.
        return (Pose)index switch
        {
            Pose.Glow => new("glow-0", pointer, GlowParts),
            Pose.SupernovaFirst => new("glow-1", pointer, SupernovaParts),
            Pose.SupernovaSecond => new("glow-2", pointer, SupernovaParts),
            Pose.Stars => new("starfield", pointer, 53),
            Pose.Silhouette => new("silhouette", pointer, SupernovaParts),
            Pose.Afterglow => new("afterglow", pointer, 37),
            _ => throw new ArgumentOutOfRangeException(nameof(index)),
        };
    }

    private static int RecordBytes(int parts) => sizeof(ushort) + 5 * parts;

    private sealed class FrameView : IReadOnlyList<EndingExplosionSpriteFrameDefinition>
    {
        public int Count => FrameCount;
        public EndingExplosionSpriteFrameDefinition this[int index] => Get(index);
        public IEnumerator<EndingExplosionSpriteFrameDefinition> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return Get(index);
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }

}

/// <summary>Identifies one editable ending-explosion composition and its stock cartridge shape.</summary>
/// <param name="Name">Stable JSON key for the frame.</param>
/// <param name="Pointer">Bank-$8C pointer used by the cinematic instruction lists.</param>
/// <param name="StockPartCount">Number of OAM parts in the cartridge composition.</param>
public readonly record struct EndingExplosionSpriteFrameDefinition(
    string Name, ushort Pointer, int StockPartCount);

/// <summary>Defines the ending-explosion sprite document contract.</summary>
public static class EndingExplosionSpriteFormat
{
    /// <summary>Current ending-explosion sprite schema version.</summary>
    public const int Version = 1;

    /// <summary>Canonical ending-explosion sprite asset file name.</summary>
    public const string FileName = "ending-explosion-sprites.json";
}
