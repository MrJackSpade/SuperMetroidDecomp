using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Ending.Artwork;

/// <summary>Distinct bank-$8C frame identities consumed by the eight explosion actors.</summary>
public static class EndingExplosionSpriteDefinitions
{
    /// <summary>Mutually exclusive composite poses, valued by their published explosion frame
    /// order after the ten small planet records addressed by <see cref="PlanetPointer"/>.</summary>
    internal enum Pose
    {
        /// <summary>Large glowing planet frame immediately after the ten small planet records.</summary>
        Glow = PlanetRecordCount,
        /// <summary>First 20-part supernova pose in the consecutive composite records.</summary>
        SupernovaFirst = 11,
        /// <summary>Second 20-part supernova pose in the consecutive composite records.</summary>
        SupernovaSecond = 12,
        /// <summary>Independent starfield record at $8C:A28B.</summary>
        Stars = 13,
        /// <summary>20-part silhouette pose following the supernova records.</summary>
        Silhouette = 14,
        /// <summary>Final 37-part afterglow pose.</summary>
        Afterglow = 15,
    }
    /// <summary>$8C:A396, ExplodingPlanetZebesFrame1; ten four-part records
    /// contain four damage poses, four flash poses and two lava poses.</summary>
    private const ushort PlanetFirst = 0xa396;
    /// <summary>$8C:A28B, ZebesBoomStarryBackground, independent of the planet record chain.</summary>
    private const ushort Starfield = 0xa28b;
    /// <summary>Each damage, flash, and lava planet record contains four OAM parts.</summary>
    private const int SmallParts = 4;
    /// <summary>The first composite glow frame contains twelve OAM parts.</summary>
    private const int GlowParts = 12;
    /// <summary>Each supernova and silhouette record contains twenty OAM parts.</summary>
    private const int SupernovaParts = 20;
    /// <summary>Ten planet records followed by six composite or independent poses.</summary>
    private const int FrameCount = 16;
    /// <summary>Four damage, four flash and two lava records precede the composite poses.</summary>
    private const int PlanetRecordCount = 10;

    /// <summary>Ordered asset identities, calculated on demand without a stored frame array.</summary>
    public static IReadOnlyList<SpriteFrameDefinition> Frames { get; } = new FrameView();

    /// <summary>One of the ten consecutive four-part planet records from $8C:A396, in
    /// published order: four damage, four flash and two lava poses.</summary>
    internal static ushort PlanetPointer(int record)
    {
        if ((uint)record >= PlanetRecordCount) throw new ArgumentOutOfRangeException(nameof(record));
        return (ushort)(PlanetFirst + record * RecordBytes(SmallParts));
    }

    /// <summary>Composite records that follow the planet chain, except the independent
    /// $8C:A28B starfield. Shared by asset identity and executable display operands.</summary>
    internal static ushort Pointer(Pose pose)
    {
        int glow = PlanetFirst + PlanetRecordCount * RecordBytes(SmallParts);
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

    /// <summary>Maps the published frame order to its JSON name, cartridge pointer, and stock part count.</summary>
    private static SpriteFrameDefinition Get(int index)
    {
        if ((uint)index >= FrameCount) throw new ArgumentOutOfRangeException(nameof(index));
        if (index < PlanetRecordCount)
        {
            ushort planet = PlanetPointer(index);
            string family = index < 4 ? "planet-damage" : index < 8 ? "planet-flash" : "lava";
            int stage = index < 4 ? index : index < 8 ? index - 4 : index - 8;
            return new(family + "-" + stage.ToString(System.Globalization.CultureInfo.InvariantCulture),
                planet, SmallParts);
        }
        // The glow and supernova compositions follow the small planet/core records.
        var pose = (Pose)index;
        ushort pointer = Pointer(pose);
        return pose switch
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

    /// <summary>Counts the two-byte part count and five bytes per native OAM part in a spritemap record.</summary>
    private static int RecordBytes(int parts) => sizeof(ushort) + 5 * parts;

    /// <summary>Exposes the fixed frame order without allocating a parallel definition array.</summary>
    private sealed class FrameView : IReadOnlyList<SpriteFrameDefinition>
    {
        /// <summary>The ten planet records and six remaining cinematic poses.</summary>
        public int Count => FrameCount;
        /// <summary>Calculates a definition for its position in the cinematic asset order.</summary>
        public SpriteFrameDefinition this[int index] => Get(index);
        /// <summary>Visits the calculated frame definitions in published order.</summary>
        public IEnumerator<SpriteFrameDefinition> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return Get(index);
        }
        /// <summary>Shares the typed ordered-frame iterator with non-generic consumers.</summary>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }

}
