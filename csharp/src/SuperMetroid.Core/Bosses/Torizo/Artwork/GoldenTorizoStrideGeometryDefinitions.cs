using System.Collections.Immutable;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.Torizo.Artwork;

/// <summary>
/// Canonical planted-foot geometry shared by Golden Torizo artwork and walking motion.
/// The seven selected origins and all independent artwork remain required under Stream3's
/// EnemyExtendedFrameCatalog.frames; none is exempted as a motion-table conversion.
/// </summary>
internal static class GoldenTorizoStrideGeometryDefinitions
{
    /// <summary>The ten four-component left-walking extended frames, one per walking phase.</summary>
    internal static ImmutableArray<EnemyFrameId> WalkingFrames { get; } =
    [
        EnemyFrameId.GoldenTorizoWalkLeft0, EnemyFrameId.GoldenTorizoWalkLeft1, EnemyFrameId.GoldenTorizoWalkLeft2,
        EnemyFrameId.GoldenTorizoWalkLeft3, EnemyFrameId.GoldenTorizoWalkLeft4, EnemyFrameId.GoldenTorizoWalkLeft5,
        EnemyFrameId.GoldenTorizoWalkLeft6, EnemyFrameId.GoldenTorizoWalkLeft7, EnemyFrameId.GoldenTorizoWalkLeft8,
        EnemyFrameId.GoldenTorizoWalkLeft9,
    ];
    /// <summary>The walking frames' body component, which carries the planted foot.</summary>
    internal const int BodyComponent = 2;
    /// <summary>$AA:968D/8FE0/9720/91B4: the incoming support foot begins at X=-37.</summary>
    private const int ForwardPlantX = -37;
    /// <summary>$AA:905F/9233: early planted-foot origins for the two alternating legs.</summary>
    private const int FirstEarlySupportX = -32, SecondEarlySupportX = -30;
    /// <summary>$AA:90CA/929E: both legs pass the same middle support origin X=-13.</summary>
    private const int MiddleSupportX = -13;
    /// <summary>$AA:9135/930E: final single-foot support origins X=3/5.</summary>
    private const int FirstPushOffX = 3, SecondPushOffX = 5;
    /// <summary>$AA:967E/96F8: double-plant poses put the outgoing foot at X=10.</summary>
    private const int DepartingFootX = 10;
    /// <summary>$AA:8FD6-8FE4 and the other body poses: three8px floor-strip tiles $160/$161/$162.</summary>
    private const int FirstFootTile = 0x160, FootTiles = 3, TilePixels = 8;

    /// <summary>Finds the planted foot's local X origin across either five-pose walking half.</summary>
    private static int SupportFootX(int phase) => (phase % 5) switch
    {
        0 or 1 => ForwardPlantX,
        2 => phase < 5 ? FirstEarlySupportX : SecondEarlySupportX,
        3 => MiddleSupportX,
        _ => phase < 5 ? FirstPushOffX : SecondPushOffX,
    };

    /// <summary>
    /// $AA:D59A-D5C1: each pose transition cancels the existing planted foot's local
    /// displacement. At phases0/5 the outgoing foot moves to the double-plant origin;
    /// otherwise the current support strip remains fixed. No independent speed is stored.
    /// </summary>
    internal static int HorizontalAdvance(int phase)
    {
        if ((uint)phase >= 10) throw new ArgumentOutOfRangeException(nameof(phase));
        int next = phase % 5 == 0 ? DepartingFootX : SupportFootX(phase);
        return next - SupportFootX((phase + 9) % 10);
    }

    /// <summary>Replaces matching body-component foot strips with calculated planted geometry.</summary>
    internal static EnemySpritemapParts Compile(EnemyFrameId frame, int componentIndex, EnemySpritemapParts supplied)
    {
        int phase = WalkingFrames.IndexOf(frame);
        if (componentIndex != BodyComponent || phase < 0)
            return supplied;
        int supportPalette = phase < 5 ? 1 : 2;
        bool doublePlant = phase % 5 == 0;
        int seen = 0;
        var footIndices = new List<int>();
        for (int index = 0; index < supplied.Count; index++)
        {
            SpritemapPart part = supplied[index];
            int column = part.Attributes.TileNumber - FirstFootTile;
            if ((uint)column >= FootTiles) continue;
            int palette = part.Attributes.PaletteIndex;
            if (palette is not (1 or 2) || (!doublePlant && palette != supportPalette)) return supplied;
            bool departing = palette != supportPalette;
            int bit = 1 << (column + (departing ? FootTiles : 0));
            int anchor = departing ? DepartingFootX : SupportFootX(phase);
            if ((seen & bit) != 0 || part.X.SignedOffset != anchor + column * TilePixels) return supplied;
            seen |= bit;
            footIndices.Add(index);
        }
        int expected = (1 << (doublePlant ? FootTiles * 2 : FootTiles)) - 1;
        return seen == expected ? new FootParts(supplied, phase, footIndices.ToArray()) : supplied;
    }

    /// <summary>Retains each authored foot tile's non-X appearance while deriving its planted origin.</summary>
    private readonly record struct FootAppearance(ushort XFlags, SnesSpritemapYByte Y, SnesObjAttributeWord Attributes)
    {
        /// <summary>Separates high X/size flags from the nine-bit authored local coordinate.</summary>
        internal static FootAppearance From(SpritemapPart part) =>
            new((ushort)(part.X.Raw & ~0x1ff), part.Y, part.Attributes);
        /// <summary>Repositions a support or departing foot tile for the current stride pose.</summary>
        internal SpritemapPart At(int phase)
        {
            int supportPalette = phase < 5 ? 1 : 2;
            int anchor = Attributes.PaletteIndex == supportPalette ? SupportFootX(phase) : DepartingFootX;
            int x = anchor + (Attributes.TileNumber - FirstFootTile) * TilePixels;
            return new(new SnesSpritemapXWord((ushort)(XFlags | (x & 0x1ff))), Y, Attributes);
        }
    }

    /// <summary>Combines calculated foot strips with the remaining independently authored OAM parts.</summary>
    private sealed class FootParts : EnemySpritemapParts
    {
        /// <summary>Unmodified non-foot pieces retained in their original relative order.</summary>
        private readonly SpritemapPart[] otherParts;
        /// <summary>Foot tile palette, flags, and Y retained while X is calculated.</summary>
        private readonly FootAppearance[] footAppearance;
        /// <summary>Original OAM positions of the foot pieces among all body parts.</summary>
        private readonly int[] footIndices;
        /// <summary>Walking phase used to choose support and departing anchors.</summary>
        private readonly int phase;
        /// <summary>Partitions a matched body component into calculated feet and unchanged pieces.</summary>
        internal FootParts(EnemySpritemapParts supplied, int phase, int[] footIndices)
        {
            this.phase = phase;
            this.footIndices = footIndices;
            footAppearance = new FootAppearance[footIndices.Length];
            otherParts = new SpritemapPart[supplied.Count - footIndices.Length];
            int foot = 0, other = 0;
            for (int index = 0; index < supplied.Count; index++)
            {
                if (foot < footIndices.Length && index == footIndices[foot])
                    footAppearance[foot++] = FootAppearance.From(supplied[index]);
                else otherParts[other++] = supplied[index];
            }
        }
        /// <summary>Preserves the supplied OAM part count after replacing foot geometry.</summary>
        public override int Count => otherParts.Length + footIndices.Length;
        /// <summary>Restores original OAM ordering while calculating each foot's X coordinate.</summary>
        public override SpritemapPart this[int index]
        {
            get
            {
                if ((uint)index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
                int foot = 0;
                while (foot < footIndices.Length && footIndices[foot] < index) foot++;
                if (foot < footIndices.Length && footIndices[foot] == index) return footAppearance[foot].At(phase);
                return otherParts[index - foot];
            }
        }
    }
}
