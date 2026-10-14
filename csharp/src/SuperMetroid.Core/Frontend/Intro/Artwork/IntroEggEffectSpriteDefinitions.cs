using System.Collections;

namespace SuperMetroid.Core.Frontend.Intro.Artwork;

/// <summary>Six shell fragments and five slime-drop OAM frames from the SR388 scene.</summary>
internal static class IntroEggEffectSpriteDefinitions
{
    /// <summary>$8C:8F7E, first shell-fragment one-part composition.</summary>
    internal const ushort Start = 0x8f7e;
    /// <summary>Every shell-fragment and slime-drop record contains one OAM part.</summary>
    internal const int StockPartCount = 1;

    /// <summary>Eleven native one-part records, each two count bytes plus five part bytes.</summary>
    internal static ushort FramePointer(int frame)
    {
        if ((uint)frame >= 11) throw new ArgumentOutOfRangeException(nameof(frame));
        return (ushort)(Start + frame * 7);
    }
    /// <summary>Ordered six shell fragments, one moving slime frame, and four impact frames.</summary>
    internal static IReadOnlyList<IntroEggEffectSpriteFrameDefinition> Frames { get; } = new FrameList();
    /// <summary>Calculates native pointers and asset names without a stored frame array.</summary>
    private sealed class FrameList : IReadOnlyList<IntroEggEffectSpriteFrameDefinition>
    {
        /// <summary>Six fragments and five slime-drop states occupy eleven consecutive records.</summary>
        public int Count => 11;
        /// <summary>Maps an indexed seven-byte record to its shell or slime asset identity.</summary>
        public IntroEggEffectSpriteFrameDefinition this[int index]
        {
            get
            {
                ushort pointer = FramePointer(index);
                return new(pointer, index < 6 ? $"fragment-{index}" : index == 6 ? "slime-moving" : $"slime-impact-{index - 7}");
            }
        }
        /// <summary>Visits the eleven native records in pointer order.</summary>
        public IEnumerator<IntroEggEffectSpriteFrameDefinition> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return this[index];
        }
        /// <summary>Uses the same pointer-order traversal for non-generic enumeration.</summary>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
