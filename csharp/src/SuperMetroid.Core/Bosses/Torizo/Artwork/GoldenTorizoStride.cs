using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.Artwork;
using static SuperMetroid.Core.Bosses.Torizo.Artwork.GoldenTorizoStrideGeometryDefinitions;

namespace SuperMetroid.Core.Bosses.Torizo.Artwork;

/// <summary>
/// Golden Torizo's planted-foot stride: how far each walking pose advances the body, and the walking frames' foot
/// strips placed at their calculated planted origins.
/// </summary>
internal static class GoldenTorizoStride
{
    /// <summary>
    /// $AA:D59A-D5C1: each pose transition cancels the existing planted foot's local
    /// displacement. At phases0/5 the outgoing foot moves to the double-plant origin;
    /// otherwise the current support strip remains fixed. No independent speed is stored.
    /// </summary>
    internal static int HorizontalAdvance(int phase)
    {
        if ((uint)phase >= 10) throw new ArgumentOutOfRangeException(nameof(phase));
        int next = phase % 5 == 0 ? DepartingFootX : SupportFootXs[phase];
        return next - SupportFootXs[(phase + 9) % 10];
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
            SpritePart part = supplied[index];
            int column = part.Character - FirstFootTile;
            if ((uint)column >= FootTiles) continue;
            int palette = part.Palette;
            if (palette is not (1 or 2) || (!doublePlant && palette != supportPalette)) return supplied;
            bool departing = palette != supportPalette;
            int bit = 1 << (column + (departing ? FootTiles : 0));
            int anchor = departing ? DepartingFootX : SupportFootXs[phase];
            if ((seen & bit) != 0 || part.Offset.X != anchor + column * TilePixels) return supplied;
            seen |= bit;
            footIndices.Add(index);
        }
        int expected = (1 << (doublePlant ? FootTiles * 2 : FootTiles)) - 1;
        return seen == expected ? new FootParts(supplied, phase, footIndices.ToArray()) : supplied;
    }

    /// <summary>Retains each authored foot tile's non-X appearance while deriving its planted origin.</summary>
    private readonly record struct FootAppearance(SpritePart Part)
    {
        /// <summary>Keeps the foot tile's whole appearance; only its horizontal coordinate is recalculated.</summary>
        internal static FootAppearance From(SpritePart part) => new(part);
        /// <summary>Repositions a support or departing foot tile for the current stride pose.</summary>
        internal SpritePart At(int phase)
        {
            int supportPalette = phase < 5 ? 1 : 2;
            int anchor = Part.Palette == supportPalette ? SupportFootXs[phase] : DepartingFootX;
            int x = anchor + (Part.Character - FirstFootTile) * TilePixels;
            return Part.At(Offset16<ScreenPixels>.Of(SpritePart.WrapX(x), Part.Offset.Y));
        }
    }

    /// <summary>Combines calculated foot strips with the remaining independently authored OAM parts.</summary>
    private sealed class FootParts : EnemySpritemapParts
    {
        /// <summary>Unmodified non-foot pieces retained in their original relative order.</summary>
        private readonly SpritePart[] otherParts;
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
            otherParts = new SpritePart[supplied.Count - footIndices.Length];
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
        public override SpritePart this[int index]
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
