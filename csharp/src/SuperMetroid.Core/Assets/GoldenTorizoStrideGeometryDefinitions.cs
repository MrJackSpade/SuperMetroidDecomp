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

    /// <summary>Returns the horizontal anchor of the planted foot for one of the ten walking phases.</summary>
    /// <param name="phase">Walking phase from the first through the final pose.</param>
    /// <returns>The support-foot X origin in local sprite coordinates.</returns>
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

    /// <summary>Replaces verified floor-strip coordinates in a Golden Torizo walking body frame with phase-derived placement.</summary>
    /// <param name="frameIdentity">Identity of the extended frame being rendered.</param>
    /// <param name="componentIndex">Spritemap component index within that frame.</param>
    /// <param name="supplied">Original parts to retain when the frame is unrelated or its foot geometry does not match the catalog.</param>
    /// <returns>Parts that preserve supplied artwork while deriving the recognized foot strips from the walking phase.</returns>
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

    /// <summary>Preserves one foot tile's appearance while allowing its local horizontal origin to follow the stride phase.</summary>
    /// <param name="XFlags">Non-coordinate bits of the SNES horizontal-position word.</param>
    /// <param name="Y">Unchanged vertical sprite coordinate.</param>
    /// <param name="Attributes">Tile, palette, and other object attributes retained from the source part.</param>
    private readonly record struct FootAppearance(ushort XFlags, byte Y, SnesObjAttributeWord Attributes)
    {
        /// <summary>Captures the foot tile's horizontal flags, vertical coordinate, and object attributes.</summary>
        /// <param name="part">Supplied sprite part whose horizontal coordinate will later be recomputed.</param>
        /// <returns>A retained appearance record for the tile.</returns>
        internal static FootAppearance From(EnemySpritemapPart part) =>
            new((ushort)(part.X.Raw & ~0x1ff), part.Y, part.Attributes);

        /// <summary>Places the tile at its support or departing-foot anchor for the requested stride phase.</summary>
        /// <param name="phase">Walking phase used to choose the planted-foot anchor.</param>
        /// <returns>The sprite part with its computed horizontal coordinate and retained appearance data.</returns>
        internal EnemySpritemapPart At(int phase)
        {
            int supportPalette = phase < 5 ? 1 : 2;
            int anchor = Attributes.PaletteIndex == supportPalette ? SupportFootX(phase) : DepartingFootX;
            int x = anchor + (Attributes.TileNumber - FirstFootTile) * TilePixels;
            return new(new SnesSpritemapXWord((ushort)(XFlags | (x & 0x1ff))), Y, Attributes);
        }
    }

    /// <summary>Lazy spritemap view that recalculates recognized foot-strip parts and preserves every other part.</summary>
    private sealed class FootParts : EnemySpritemapParts
    {
        /// <summary>Unmodified supplied parts that are not part of the recognized foot strips.</summary>
        private readonly EnemySpritemapPart[] otherParts;
        /// <summary>Appearance data for foot parts, stored in the same order as their original indices.</summary>
        private readonly FootAppearance[] footAppearance;
        /// <summary>Original positions of foot parts in the complete supplied part sequence.</summary>
        private readonly int[] footIndices;
        /// <summary>Walking phase used when lazily recomputing each foot tile's horizontal coordinate.</summary>
        private readonly int phase;

        /// <summary>Builds a view that keeps non-foot parts intact and derives foot placement from the supplied phase.</summary>
        /// <param name="supplied">Original ordered spritemap parts.</param>
        /// <param name="phase">Walking phase for the recognized frame.</param>
        /// <param name="footIndices">Ascending original indices of the foot-strip parts.</param>
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
        /// <summary>Gets the number of parts in the original sprite sequence.</summary>
        public override int Count => otherParts.Length + footIndices.Length;

        /// <summary>Gets a part at its original sequence index, recomputing its X coordinate when it is a foot tile.</summary>
        /// <param name="index">Zero-based index in the original sequence.</param>
        /// <returns>The preserved non-foot part or phase-adjusted foot part at that position.</returns>
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
