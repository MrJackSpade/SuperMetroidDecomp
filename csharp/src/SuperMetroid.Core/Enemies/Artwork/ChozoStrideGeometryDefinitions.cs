using System.Collections.Immutable;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>
/// Applies the four native Chozo stride foot positions to matching supplied spritemaps
/// while preserving the appearance and order of their other artwork parts.
/// </summary>
internal static class ChozoStrideGeometryDefinitions
{
    /// <summary>The eight stride compositions, Spritemaps_Chozo_4 onwards: two strides of four phases.</summary>
    internal static ImmutableArray<EnemyFrameId> Poses { get; } =
    [
        EnemyFrameId.ChozoStatueAaE943, EnemyFrameId.ChozoStatueAaE9ae, EnemyFrameId.ChozoStatueAaEa1e, EnemyFrameId.ChozoStatueAaEa8e,
        EnemyFrameId.ChozoStatueAaEafe, EnemyFrameId.ChozoStatueAaEb69, EnemyFrameId.ChozoStatueAaEbd9, EnemyFrameId.ChozoStatueAaEc49,
    ];
    /// <summary>Width of one OBJ glyph, the distance from the left foot to the adjacent right foot.</summary>
    private const int TilePixels = 8;

    /// <summary>$AA:E954/E959 and following poses: the adjacent support-foot glyphs.</summary>
    private enum FootGlyph
    {
        /// <summary>OBJ tile $170, the left half of the support foot.</summary>
        Left = 0x170,
        /// <summary>OBJ tile $171, the right half of the support foot.</summary>
        Right = 0x171,
    }
    /// <summary>$AA:E959/E9C4/EA34/EAA4: chosen left-foot origins -31/-28/-14/-7.</summary>
    private const int TransferFootX = -31, EarlySupportFootX = -28,
        LateSupportFootX = -14, PushOffFootX = -7;

    /// <summary>Gets the left support-foot origin for one of the four stride phases.</summary>
    internal static int SupportFootX(int phase) => phase switch
    {
        0 => TransferFootX,
        1 => EarlySupportFootX,
        2 => LateSupportFootX,
        3 => PushOffFootX,
        _ => throw new ArgumentOutOfRangeException(nameof(phase)),
    };

    /// <summary>Uses phase-derived foot positions only when the supplied pose has exactly one matching glyph pair.</summary>
    internal static EnemySpritemapParts Compile(EnemyFrameId frame, EnemySpritemapParts supplied)
    {
        int pose = Poses.IndexOf(frame);
        if (pose < 0) return supplied;
        int phase = pose % 4;
        int left = -1, right = -1;
        for (int index = 0; index < supplied.Count; index++)
        {
            SpritemapPart part = supplied[index];
            if (!Enum.IsDefined((FootGlyph)part.Attributes.TileNumber))
                continue;
            switch ((FootGlyph)part.Attributes.TileNumber)
            {
                case FootGlyph.Left:
                    if (left >= 0 || part.X.SignedOffset != SupportFootX(phase)) return supplied;
                    left = index;
                    break;
                case FootGlyph.Right:
                    if (right >= 0 || part.X.SignedOffset != SupportFootX(phase) + TilePixels) return supplied;
                    right = index;
                    break;
                default:
                    throw new InvalidOperationException(
                        $"Undefined {nameof(FootGlyph)} {part.Attributes.TileNumber}.");
            }
        }
        if (left < 0 || right < 0) return supplied;
        return new FootParts(supplied, phase, left, right);
    }

    /// <summary>Retains a foot glyph's flags, row, and tile attributes when relocating its horizontal origin.</summary>
    private readonly record struct FootAppearance(ushort XFlags, SnesSpritemapYByte Y, SnesObjAttributeWord Attributes)
    {
        /// <summary>Separates the glyph's non-position X flags from its current horizontal coordinate.</summary>
        internal static FootAppearance From(SpritemapPart part) =>
            new((ushort)(part.X.Raw & ~0x1ff), part.Y, part.Attributes);
        /// <summary>Reconstructs the glyph at a new X coordinate with its other appearance bits intact.</summary>
        internal SpritemapPart At(int x) =>
            new(new SnesSpritemapXWord((ushort)(XFlags | (x & 0x1ff))), Y, Attributes);
    }

    /// <summary>Projects relocated foot glyphs into their original slots among the supplied pose parts.</summary>
    private sealed class FootParts : EnemySpritemapParts
    {
        /// <summary>All supplied parts other than the two relocated support-foot glyphs.</summary>
        private readonly SpritemapPart[] otherParts;
        /// <summary>The preserved appearances of the left and right support-foot glyphs.</summary>
        private readonly FootAppearance left, right;
        /// <summary>The stride phase and original indices used to reinsert the two foot glyphs.</summary>
        private readonly int phase, leftIndex, rightIndex;

        /// <summary>Extracts the support feet and retains the remaining parts in their supplied order.</summary>
        internal FootParts(EnemySpritemapParts supplied, int phase, int leftIndex, int rightIndex)
        {
            this.phase = phase;
            this.leftIndex = leftIndex;
            this.rightIndex = rightIndex;
            left = FootAppearance.From(supplied[leftIndex]);
            right = FootAppearance.From(supplied[rightIndex]);
            otherParts = new SpritemapPart[supplied.Count - 2];
            int output = 0;
            for (int index = 0; index < supplied.Count; index++)
                if (index != leftIndex && index != rightIndex) otherParts[output++] = supplied[index];
        }
        /// <summary>Gets the original part count, including both relocated support feet.</summary>
        public override int Count => otherParts.Length + 2;
        /// <summary>Returns the original part at this index or its relocated support-foot equivalent.</summary>
        public override SpritemapPart this[int index]
        {
            get
            {
                if ((uint)index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
                if (index == leftIndex) return left.At(SupportFootX(phase));
                if (index == rightIndex) return right.At(SupportFootX(phase) + TilePixels);
                return otherParts[index - (index > leftIndex ? 1 : 0) - (index > rightIndex ? 1 : 0)];
            }
        }
    }
}
