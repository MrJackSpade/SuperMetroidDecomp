using SuperMetroid.Core.Assets;
using static SuperMetroid.Core.Enemies.Artwork.ChozoStrideGeometryDefinitions;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>
/// The Chozo statue's stride: where its support foot is planted in each phase, and the stride poses' foot glyphs placed
/// there while the appearance and order of their other artwork parts are preserved.
/// </summary>
internal static class ChozoStride
{
    /// <summary>Gets the left support-foot origin for one of the four stride phases.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="phase"/> is outside zero through three.</exception>
    internal static int SupportFootX(int phase) =>
        (uint)phase < (uint)SupportFootXs.Length ? SupportFootXs[phase] : throw new ArgumentOutOfRangeException(nameof(phase));

    /// <summary>
    /// Uses phase-derived foot positions only when the supplied pose has exactly one matching glyph pair. Parts drawn
    /// with neither foot character are the body's other artwork and keep their supplied positions.
    /// </summary>
    internal static EnemySpritemapParts Compile(EnemyFrameId frame, EnemySpritemapParts supplied)
    {
        int pose = Poses.IndexOf(frame);
        if (pose < 0) return supplied;
        int phase = pose % 4;
        int left = -1, right = -1;
        for (int index = 0; index < supplied.Count; index++)
        {
            SpritePart part = supplied[index];
            if (part.Character == LeftHalf.Tile)
            {
                if (left >= 0 || part.Offset.X != SupportFootX(phase) + LeftHalf.OffsetFromLeft) return supplied;
                left = index;
            }
            else if (part.Character == RightHalf.Tile)
            {
                if (right >= 0 || part.Offset.X != SupportFootX(phase) + RightHalf.OffsetFromLeft) return supplied;
                right = index;
            }
        }
        if (left < 0 || right < 0) return supplied;
        return new FootParts(supplied, phase, left, right);
    }

    /// <summary>Retains a foot glyph's size, row, character, palette, priority and mirroring when relocating its horizontal origin.</summary>
    private readonly record struct FootAppearance(SpritePart Part)
    {
        /// <summary>Keeps the glyph's whole appearance; only its horizontal coordinate is recalculated.</summary>
        internal static FootAppearance From(SpritePart part) => new(part);
        /// <summary>The glyph at a new X coordinate, wrapped across the sprite plane, with its other fields intact.</summary>
        internal SpritePart At(int x) => Part.At(Offset16<ScreenPixels>.Of(SpritePart.WrapX(x), Part.Offset.Y));
    }

    /// <summary>Projects relocated foot glyphs into their original slots among the supplied pose parts.</summary>
    private sealed class FootParts : EnemySpritemapParts
    {
        /// <summary>All supplied parts other than the two relocated support-foot glyphs.</summary>
        private readonly SpritePart[] otherParts;
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
            otherParts = new SpritePart[supplied.Count - 2];
            int output = 0;
            for (int index = 0; index < supplied.Count; index++)
                if (index != leftIndex && index != rightIndex) otherParts[output++] = supplied[index];
        }
        /// <summary>Gets the original part count, including both relocated support feet.</summary>
        public override int Count => otherParts.Length + 2;
        /// <summary>Returns the original part at this index or its relocated support-foot equivalent.</summary>
        public override SpritePart this[int index]
        {
            get
            {
                if ((uint)index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
                if (index == leftIndex) return left.At(SupportFootX(phase));
                if (index == rightIndex) return right.At(SupportFootX(phase) + RightHalf.OffsetFromLeft);
                return otherParts[index - (index > leftIndex ? 1 : 0) - (index > rightIndex ? 1 : 0)];
            }
        }
    }
}
