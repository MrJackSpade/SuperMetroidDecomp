using System.Collections;

namespace SuperMetroid.Core.Assets;

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

    internal static IReadOnlyList<IntroScientistSpriteFrameDefinition> Frames { get; } = new FrameList();
    private sealed class FrameList : IReadOnlyList<IntroScientistSpriteFrameDefinition>
    {
        public int Count => 10;
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
        public IEnumerator<IntroScientistSpriteFrameDefinition> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return this[index];
        }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
internal readonly record struct IntroScientistSpriteFrameDefinition(
    ushort Pointer, string Name, int StockPartCount);
