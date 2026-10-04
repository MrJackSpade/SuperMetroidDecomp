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
            frames.Add(definition.Pointer,
                IntroCinematicSpriteCompiler.Compile(visual, definition.Name));
        }
        return new EndingExplosionSpritePresentation(frames);
    }

    public static void Write(Stream json, EndingExplosionSpriteDocument document)
    {
        ArgumentNullException.ThrowIfNull(json);
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document,
            MapPresentationFormat.JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        json.Write(bytes);
    }
}

public sealed record EndingExplosionSpriteDocument
{
    public required int Version { get; init; }
    public required Dictionary<string, SpriteVisualPart[]> Frames { get; init; }
}

/// <summary>Distinct bank-$8C frame identities consumed by the eight explosion actors.</summary>
public static class EndingExplosionSpriteDefinitions
{
    /// <summary>Mutually exclusive poses in the published explosion frame order.</summary>
    internal enum Pose
    {
        DamageFirst, DamageSecond, DamageThird, DamageFourth,
        FlashFirst, FlashSecond, FlashThird, FlashFourth, LavaFirst, LavaSecond,
        Glow, SupernovaFirst, SupernovaSecond, Stars, Silhouette, Afterglow,
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

public readonly record struct EndingExplosionSpriteFrameDefinition(
    string Name, ushort Pointer, int StockPartCount);

public static class EndingExplosionSpriteFormat
{
    public const int Version = 1;
    public const string FileName = "ending-explosion-sprites.json";
}
