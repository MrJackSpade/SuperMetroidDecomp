using System.Text.Json;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Editable OAM compositions for the 16 Zebes-explosion visual frames.</summary>
public sealed class EndingExplosionSpritePresentation : IIntroCinematicSpritePresentation
{
    /// <summary>Compiled sprite compositions indexed by their bank-$8C spritemap pointer.</summary>
    private readonly Dictionary<ushort, SpriteComposition> frames;

    /// <summary>Creates a presentation from the validated pointer-to-composition map.</summary>
    /// <param name="frames">All installed explosion frames, keyed by the pointer consumed by cinematic actors.</param>
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
        /// <summary>First of the ten small planet-damage, flash, and lava records.</summary>
        DamageFirst,
        /// <summary>First glow composition following the small planet records.</summary>
        Glow = 10,
        /// <summary>First expanded supernova composition.</summary>
        SupernovaFirst = 11,
        /// <summary>Second expanded supernova composition.</summary>
        SupernovaSecond = 12,
        /// <summary>Independent starfield spritemap outside the planet record chain.</summary>
        Stars = 13,
        /// <summary>Planet silhouette composition after the supernova records.</summary>
        Silhouette = 14,
        /// <summary>Final explosion afterglow composition.</summary>
        Afterglow = 15,
    }
    /// <summary>$8C:A396, ExplodingPlanetZebesFrame1; ten four-part records
    /// contain four damage poses, four flash poses and two lava poses.</summary>
    private const ushort PlanetFirst = 0xa396;
    /// <summary>$8C:A28B, ZebesBoomStarryBackground, independent of the planet record chain.</summary>
    private const ushort Starfield = 0xa28b;
    /// <summary>OAM-part counts for the initial planet/lava, first glow, and expanded supernova/silhouette spritemap records, respectively.</summary>
    private const int SmallParts = 4, GlowParts = 12, SupernovaParts = 20;
    /// <summary>Total number of named visual frames in the ending sequence.</summary>
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

    /// <summary>Calculates the stable name, native pointer, and stock part count for one ordered frame index.</summary>
    /// <param name="index">Zero-based position in the published sixteen-frame sequence.</param>
    /// <returns>The identity and expected cartridge shape for that frame.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The index is outside the frame sequence.</exception>
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

    /// <summary>Computes the byte length of a counted spritemap containing the given number of OAM parts.</summary>
    /// <param name="parts">Number of five-byte sprite records following the two-byte count.</param>
    /// <returns>The total record-chain size in bytes.</returns>
    private static int RecordBytes(int parts) => sizeof(ushort) + 5 * parts;

    /// <summary>Calculates ending-explosion frame definitions in published order without materializing a backing array.</summary>
    private sealed class FrameView : IReadOnlyList<EndingExplosionSpriteFrameDefinition>
    {
        /// <summary>Number of named compositions in the ending sequence.</summary>
        public int Count => FrameCount;

        /// <summary>Calculates the frame definition at the requested sequence position.</summary>
        /// <param name="index">Zero-based frame position.</param>
        /// <returns>The matching name, bank pointer, and expected OAM part count.</returns>
        /// <exception cref="ArgumentOutOfRangeException">The index is outside the sequence.</exception>
        public EndingExplosionSpriteFrameDefinition this[int index] => Get(index);

        /// <summary>Enumerates calculated frame definitions in cinematic order.</summary>
        /// <returns>An iterator over all sixteen frame definitions.</returns>
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
