using System.Collections;

namespace SuperMetroid.Core.Assets;

/// <summary>The three four-quadrant Rinka frames selected by the intro list.</summary>
internal static class IntroRinkaSpriteDefinitions
{
    /// <summary>$8C:8C8D, first intro Rinka composition.</summary>
    internal const ushort First = 0x8c8d;

    /// <summary>Three native four-part records: two count bytes plus four five-byte parts.</summary>
    internal static ushort FramePointer(int frame)
    {
        if ((uint)frame >= 3) throw new ArgumentOutOfRangeException(nameof(frame));
        return (ushort)(First + frame * (2 + 5 * StockPartCount));
    }
    /// <summary>Ordered view of the three native intro Rinka frame identities.</summary>
    internal static IReadOnlyList<IntroRinkaSpriteFrameDefinition> Frames { get; } = new FrameList();
    /// <summary>Calculates each pointer and stable asset name from its checked native frame index.</summary>
    private sealed class FrameList : IReadOnlyList<IntroRinkaSpriteFrameDefinition>
    {
        /// <summary>Exactly three compositions are present in the intro sequence.</summary>
        public int Count => 3;
        /// <summary>Pairs a checked native pointer with its stable <c>rinka-N</c> asset name.</summary>
        public IntroRinkaSpriteFrameDefinition this[int index] => new(FramePointer(index), $"rinka-{index}");
        /// <summary>Yields the native compositions in their three-frame order.</summary>
        public IEnumerator<IntroRinkaSpriteFrameDefinition> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return this[index];
        }
        /// <summary>Exposes the same ordered frames through the nongeneric collection interface.</summary>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
    /// <summary>Number of five-byte OAM parts in each stock Rinka composition.</summary>
    internal const int StockPartCount = 4;
}

/// <summary>Native spritemap pointer paired with the installed intro Rinka frame name.</summary>
internal readonly record struct IntroRinkaSpriteFrameDefinition(ushort Pointer, string Name);
