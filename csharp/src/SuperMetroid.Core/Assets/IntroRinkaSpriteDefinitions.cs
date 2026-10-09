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
    /// <summary>Provides the three frame records referenced by the intro Rinka animation list.</summary>
    internal static IReadOnlyList<IntroRinkaSpriteFrameDefinition> Frames { get; } = new FrameList();

    /// <summary>Exposes the fixed intro frame pointers as a read-only indexed sequence.</summary>
    private sealed class FrameList : IReadOnlyList<IntroRinkaSpriteFrameDefinition>
    {
        /// <summary>The intro list contains three Rinka frame records.</summary>
        public int Count => 3;

        /// <summary>Creates the frame record for the requested intro frame.</summary>
        /// <param name="index">Zero-based frame index, from zero through two.</param>
        /// <returns>The frame pointer and its resource name.</returns>
        /// <exception cref="ArgumentOutOfRangeException">The index is outside the three-frame sequence.</exception>
        public IntroRinkaSpriteFrameDefinition this[int index] => new(FramePointer(index), $"rinka-{index}");

        /// <summary>Enumerates frame records in their native intro-list order.</summary>
        /// <returns>An iterator over the three frame definitions.</returns>
        public IEnumerator<IntroRinkaSpriteFrameDefinition> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return this[index];
        }
        /// <summary>Returns the non-generic enumerator used by <see cref="IEnumerable"/> consumers.</summary>
        /// <returns>An iterator over the three frame definitions.</returns>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    /// <summary>Number of five-byte sprite-part records stored in each native Rinka frame.</summary>
    internal const int StockPartCount = 4;
}

/// <summary>Identifies one intro Rinka sprite frame by its native record address and resource name.</summary>
/// <param name="Pointer">Address of the frame's count bytes and four sprite-part records.</param>
/// <param name="Name">Stable resource name used to associate the frame with installed artwork.</param>
internal readonly record struct IntroRinkaSpriteFrameDefinition(ushort Pointer, string Name);
