using System.Collections;

namespace SuperMetroid.Core.Assets;

/// <summary>The twelve OAM compositions selected by the fourth-hit explosion loops.</summary>
internal static class IntroMotherBrainExplosionSpriteDefinitions
{
    /// <summary>$8C:97F7, first small-explosion frame; the last frame ends at $8C:985D.</summary>
    internal const ushort SmallStart = 0x97f7;
    /// <summary>$8C:985D, first large-explosion frame; the last frame ends at $8C:98D2.</summary>
    internal const ushort BigStart = 0x985d;
    /// <summary>$8C:98D2, exclusive end of the twelve consecutive composition records.</summary>
    internal const ushort End = 0x98d2;

    /// <summary>Six consecutive native records per effect. Small starts with two
    /// one-part records; big starts with one. Remaining records contain four parts.
    /// Each record is a two-byte count plus five bytes per part, hence strides7/22.</summary>
    internal static ushort FramePointer(bool big, int frame)
    {
        if ((uint)frame >= 6) throw new ArgumentOutOfRangeException(nameof(frame));
        int single = Math.Min(frame, big ? 1 : 2);
        return (ushort)((big ? BigStart : SmallStart) + single * 7 + (frame - single) * 22);
    }

    internal static IReadOnlyList<IntroMotherBrainExplosionSpriteFrameDefinition> Frames { get; } = new FrameList();
    private sealed class FrameList : IReadOnlyList<IntroMotherBrainExplosionSpriteFrameDefinition>
    {
        public int Count => 12;
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
        public IEnumerator<IntroMotherBrainExplosionSpriteFrameDefinition> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return this[index];
        }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}

internal readonly record struct IntroMotherBrainExplosionSpriteFrameDefinition(
    ushort Pointer, string Name, int StockPartCount);
