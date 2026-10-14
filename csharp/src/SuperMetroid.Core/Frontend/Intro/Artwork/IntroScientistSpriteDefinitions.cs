using System.Collections;

namespace SuperMetroid.Core.Frontend.Intro.Artwork;

/// <summary>Visual compositions selected by the scientist delivery and examination lists.</summary>
internal static class IntroScientistSpriteDefinitions
{
    /// <summary>$8C:8CCF, first subtitle-arrow composition in the shared scientist catalog.</summary>
    internal const ushort Start = 0x8ccf;

    /// <summary>Native record order: three two-part arrows, three six-part delivered
    /// poses, three one-part examined poses, then the one-part caret. Record size is
    /// two count bytes plus five bytes per part; index0..9 is bounded before summing.</summary>
    internal static ushort FramePointer(int index)
    {
        if ((uint)index >= 10) throw new ArgumentOutOfRangeException(nameof(index));
        int arrows = Math.Min(index, 3);
        int delivered = index > 3 ? Math.Min(index - 3, 3) : 0;
        int single = index > 6 ? index - 6 : 0;
        return (ushort)(Start + arrows * 12 + delivered * 32 + single * 7);
    }

    /// <summary>Ordered view of ten scientist records, retaining their published asset names and part counts.</summary>
    internal static IReadOnlyList<IntroScientistSpriteFrameDefinition> Frames { get; } = new FrameList();
    /// <summary>Calculates frame metadata from native record sizes without storing a second frame table.</summary>
    private sealed class FrameList : IReadOnlyList<IntroScientistSpriteFrameDefinition>
    {
        /// <summary>Three arrows, three delivery poses, three examination poses, and a final caret.</summary>
        public int Count => 10;
        /// <summary>Pairs a bounded native pointer with its installed key and original OAM part count.</summary>
        public IntroScientistSpriteFrameDefinition this[int index]
        {
            get
            {
                ushort pointer = FramePointer(index);
                // Retain the published asset keys, including legacy arrow/hold names.
                string name = index == 9 ? "examined-baby-hold"
                    : $"{(index < 3 ? "examined-loop" : index < 6 ? "delivered-baby" : "examined-baby")}-{index % 3 + 1}";
                return new(pointer, name, index < 3 ? 2 : index < 6 ? 6 : 1);
            }
        }
        /// <summary>Yields the ten records in native pointer order.</summary>
        public IEnumerator<IntroScientistSpriteFrameDefinition> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return this[index];
        }
        /// <summary>Uses the same ordered record view for non-generic callers.</summary>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
