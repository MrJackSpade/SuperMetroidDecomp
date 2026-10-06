using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// Canonical planted-foot geometry shared by Golden Torizo artwork and walking motion.
/// The seven selected origins and all independent artwork remain required under Stream3's
/// EnemyExtendedFrameCatalog.frames; none is exempted as a motion-table conversion.
/// </summary>
internal static class GoldenTorizoStrideGeometryDefinitions
{
    /// <summary>$AA:A4FA-A64D: ten four-component left-walking extended frames, each2+4*8bytes.</summary>
    private const int FirstFrame = 0xaaa4fa, FrameBytes = 2 + 4 * 8, BodyComponent = 2;
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

    internal static int NativeFrameIdentity(int phase)
    {
        if ((uint)phase >= 10) throw new ArgumentOutOfRangeException(nameof(phase));
        return FirstFrame + FrameBytes * phase;
    }

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

    internal static EnemySpritemapParts Compile(int frameIdentity, int componentIndex, EnemySpritemapParts supplied)
    {
        int offset = frameIdentity - FirstFrame;
        if (componentIndex != BodyComponent || offset < 0 || offset % FrameBytes != 0 || offset / FrameBytes >= 10)
            return supplied;
        int phase = offset / FrameBytes, supportPalette = phase < 5 ? 1 : 2;
        bool doublePlant = phase % 5 == 0;
        int seen = 0;
        var footIndices = new List<int>();
        for (int index = 0; index < supplied.Count; index++)
        {
            EnemySpritemapPart part = supplied[index];
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

    private readonly record struct FootAppearance(ushort XFlags, byte Y, SnesObjAttributeWord Attributes)
    {
        internal static FootAppearance From(EnemySpritemapPart part) =>
            new((ushort)(part.X.Raw & ~0x1ff), part.Y, part.Attributes);
        internal EnemySpritemapPart At(int phase)
        {
            int supportPalette = phase < 5 ? 1 : 2;
            int anchor = Attributes.PaletteIndex == supportPalette ? SupportFootX(phase) : DepartingFootX;
            int x = anchor + (Attributes.TileNumber - FirstFootTile) * TilePixels;
            return new(new SnesSpritemapXWord((ushort)(XFlags | (x & 0x1ff))), Y, Attributes);
        }
    }

    private sealed class FootParts : EnemySpritemapParts
    {
        private readonly EnemySpritemapPart[] otherParts;
        private readonly FootAppearance[] footAppearance;
        private readonly int[] footIndices;
        private readonly int phase;
        internal FootParts(EnemySpritemapParts supplied, int phase, int[] footIndices)
        {
            this.phase = phase;
            this.footIndices = footIndices;
            footAppearance = new FootAppearance[footIndices.Length];
            otherParts = new EnemySpritemapPart[supplied.Count - footIndices.Length];
            int foot = 0, other = 0;
            for (int index = 0; index < supplied.Count; index++)
            {
                if (foot < footIndices.Length && index == footIndices[foot])
                    footAppearance[foot++] = FootAppearance.From(supplied[index]);
                else otherParts[other++] = supplied[index];
            }
        }
        public override int Count => otherParts.Length + footIndices.Length;
        public override EnemySpritemapPart this[int index]
        {
            get
            {
                if ((uint)index >= Count) throw new IndexOutOfRangeException();
                int foot = 0;
                while (foot < footIndices.Length && footIndices[foot] < index) foot++;
                if (foot < footIndices.Length && footIndices[foot] == index) return footAppearance[foot].At(phase);
                return otherParts[index - foot];
            }
        }
    }
}