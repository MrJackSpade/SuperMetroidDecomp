using System.Collections;

namespace SuperMetroid.Core.Assets;

/// <summary>The twelve OAM compositions selected by the fourth-hit explosion loops.</summary>
internal static class IntroMotherBrainExplosionSpriteDefinitions
{
    /// <summary>$8C:97F7, first small-explosion frame; the last frame ends at $8C:985D.</summary>
    internal const ushort SmallStart = 0x97f7;
    /// <summary>$8C:985D, first large-explosion frame; the last frame ends at $8C:98D2.</summary>
    internal const ushort BigStart = 0x985d;

    /// <summary>Six consecutive native records per effect. Small starts with two
    /// one-part records; big starts with one. Remaining records contain four parts.
    /// Each record is a two-byte count plus five bytes per part, hence strides7/22.</summary>
    internal static ushort FramePointer(bool big, int frame)
    {
        if ((uint)frame >= 6) throw new ArgumentOutOfRangeException(nameof(frame));
        int single = Math.Min(frame, big ? 1 : 2);
        return (ushort)((big ? BigStart : SmallStart) + single * 7 + (frame - single) * 22);
    }

    /// <summary>Ordered small-then-large explosion frames selected by the native fourth-hit loops.</summary>
    internal static IReadOnlyList<IntroMotherBrainExplosionSpriteFrameDefinition> Frames { get; } = new FrameList();

    /// <summary>Provides the twelve frame definitions without allocating a stored frame array.</summary>
    private sealed class FrameList : IReadOnlyList<IntroMotherBrainExplosionSpriteFrameDefinition>
    {
        /// <summary>Gets the six small-explosion and six large-explosion frame definitions.</summary>
        public int Count => 12;

        /// <summary>Gets the definition for one frame in small-then-large effect order.</summary>
        /// <param name="index">Zero-based position from 0 through 11.</param>
        /// <returns>The bank-$8C pointer, stable name, and stock OAM part count for that frame.</returns>
        public IntroMotherBrainExplosionSpriteFrameDefinition this[int index]
        {
            get
            {
                if ((uint)index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
                bool big = index >= 6;
                int frame = index % 6;
                return new(FramePointer(big, frame), $"{(big ? "big" : "small")}-explosion-{frame}",
                    frame < (big ? 1 : 2) ? 1 : 4);
            }
        }
        /// <summary>Enumerates all twelve definitions in the same order as the indexer.</summary>
        /// <returns>An enumerator over the small and large explosion frame definitions.</returns>
        public IEnumerator<IntroMotherBrainExplosionSpriteFrameDefinition> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return this[index];
        }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}

/// <summary>Identifies one native explosion spritemap and records its editable visual name and original OAM size.</summary>
/// <param name="Pointer">Bank-relative $8C spritemap pointer used by the explosion instruction loop.</param>
/// <param name="Name">Stable key used to select this composition from the editable asset.</param>
/// <param name="StockPartCount">Number of five-byte OAM parts in the original composition, excluding its count header.</param>
internal readonly record struct IntroMotherBrainExplosionSpriteFrameDefinition(
    ushort Pointer, string Name, int StockPartCount);
