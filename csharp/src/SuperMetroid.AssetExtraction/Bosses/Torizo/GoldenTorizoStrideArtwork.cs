using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Spatial;
using static SuperMetroid.AssetExtraction.Bosses.Torizo.GoldenTorizoStrideGeometryDefinitions;
using static SuperMetroid.Core.Bosses.Torizo.GoldenTorizoStrideDefinitions;

namespace SuperMetroid.AssetExtraction.Bosses.Torizo;

/// <summary>
/// Golden Torizo's walking frames as the import pipeline composes them: the body components' foot strips placed at the
/// stride's planted-foot positions.
/// </summary>
internal static class GoldenTorizoStrideArtwork
{
    /// <summary>Replaces matching body-component foot strips with calculated planted geometry.</summary>
    internal static EnemySpritemapParts Compile(EnemyFrameId frame, int componentIndex, EnemySpritemapParts supplied)
    {
        int phase = WalkingFrames.IndexOf(frame);
        if (componentIndex != BodyComponent || phase < 0)
            return supplied;
        GoldenTorizoLeg supportLeg = SupportLeg(phase);
        bool doublePlant = phase % PhasesPerHalf == 0;
        int seen = 0;
        var footIndices = new List<int>();
        for (int index = 0; index < supplied.Count; index++)
        {
            SpritePart part = supplied[index];
            int column = part.Character - FirstFootTile;
            if ((uint)column >= FootTiles) continue;
            if (!LegsByPalette.TryGetValue(part.Palette, out GoldenTorizoLeg leg) || (!doublePlant && leg != supportLeg)) return supplied;
            bool departing = leg != supportLeg;
            int bit = 1 << (column + (departing ? FootTiles : 0));
            int anchor = departing ? DepartingFootX : SupportFootXs[phase];
            if ((seen & bit) != 0 || part.Offset.X != anchor + column * TilePixels) return supplied;
            seen |= bit;
            footIndices.Add(index);
        }
        int expected = (1 << (doublePlant ? FootTiles * 2 : FootTiles)) - 1;
        return seen == expected ? new FootParts(supplied, phase, footIndices.ToArray()) : supplied;
    }

    /// <summary>The leg supporting the body in <paramref name="phase"/>: the first leg in the stride's first half.</summary>
    private static GoldenTorizoLeg SupportLeg(int phase) => phase < PhasesPerHalf ? GoldenTorizoLeg.First : GoldenTorizoLeg.Second;

    /// <summary>Retains each authored foot tile's non-X appearance while deriving its planted origin.</summary>
    private readonly record struct FootAppearance(SpritePart Part)
    {
        /// <summary>Keeps the foot tile's whole appearance; only its horizontal coordinate is recalculated.</summary>
        internal static FootAppearance From(SpritePart part) => new(part);
        /// <summary>Repositions a support or departing foot tile for the current stride pose.</summary>
        internal SpritePart At(int phase)
        {
            int anchor = Part.Palette == SupportPalette(phase) ? SupportFootXs[phase] : DepartingFootX;
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
