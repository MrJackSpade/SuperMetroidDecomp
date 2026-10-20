using System.Collections;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Intro.Artwork;

/// <summary>Three visual frames selected by the intro Mother Brain's compiled lists.</summary>
internal static class IntroMotherBrainSpriteDefinitions
{
    /// <summary>$8C:8C00, first nine-part Mother Brain frame.</summary>
    internal const ushort FrameZero = 0x8c00;
    /// <summary>Every retail frame has nine OAM parts; $8C:8C8D is unrelated.</summary>
    internal const int StockPartCount = 9;

    /// <summary>Advances through three contiguous nine-part OAM records beginning at $8C:8C00, rejecting other frame indices.</summary>
    internal static ushort FramePointer(int frame)
    {
        if ((uint)frame >= 3) throw new ArgumentOutOfRangeException(nameof(frame));
        return (ushort)(FrameZero + frame * (2 + 5 * StockPartCount));
    }
    /// <summary>Lazy definitions for the intro's three native Mother Brain OAM frames.</summary>
    internal static IReadOnlyList<SpritemapFrameDefinition> Frames { get; } = new FrameList();
    /// <summary>Computes each frame's native pointer and stable artwork key without storing duplicate definitions.</summary>
    private sealed class FrameList : IReadOnlyList<SpritemapFrameDefinition>
    {
        /// <summary>The three consecutive frames selected by the intro instruction lists.</summary>
        public int Count => 3;
        /// <summary>Returns the native OAM pointer and artwork key for a validated frame index.</summary>
        public SpritemapFrameDefinition this[int index] =>
            new(FramePointer(index), $"mother-brain-frame-{index}");
        /// <summary>Yields all three definitions in native frame order.</summary>
        public IEnumerator<SpritemapFrameDefinition> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return this[index];
        }
        /// <summary>Shares the typed native-order enumerator with non-generic collection consumers.</summary>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
