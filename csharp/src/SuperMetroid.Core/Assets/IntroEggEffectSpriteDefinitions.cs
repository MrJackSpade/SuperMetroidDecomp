using System.Collections;

namespace SuperMetroid.Core.Assets;

/// <summary>Six shell fragments and five slime-drop OAM frames from the SR388 scene.</summary>
internal static class IntroEggEffectSpriteDefinitions
{
    /// <summary>$8C:8F7E, first shell-fragment one-part composition.</summary>
    internal const ushort Start = 0x8f7e;
    /// <summary>Defines stock part count.</summary>
    internal const int StockPartCount = 1;

    /// <summary>Eleven native one-part records, each two count bytes plus five part bytes.</summary>
    internal static ushort FramePointer(int frame)
    {
        if ((uint)frame >= 11) throw new ArgumentOutOfRangeException(nameof(frame));
        return (ushort)(Start + frame * 7);
    }
    /// <summary>Gets frames.</summary>
    internal static IReadOnlyList<IntroEggEffectSpriteFrameDefinition> Frames { get; } = new FrameList();
    /// <summary>Represents frame list.</summary>
    private sealed class FrameList : IReadOnlyList<IntroEggEffectSpriteFrameDefinition>
    {
        /// <summary>Gets count.</summary>
        public int Count => 11;
        /// <summary>Gets or sets an indexed value.</summary>
        public IntroEggEffectSpriteFrameDefinition this[int index]
        {
            get
            {
                ushort pointer = FramePointer(index);
                return new(pointer, index < 6 ? $"fragment-{index}" : index == 6 ? "slime-moving" : $"slime-impact-{index - 7}");
            }
        }
        /// <summary>Gets enumerator.</summary>
        public IEnumerator<IntroEggEffectSpriteFrameDefinition> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return this[index];
        }
        /// <summary>Gets enumerator.</summary>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
/// <summary>Represents intro egg effect sprite frame definition.</summary>
internal readonly record struct IntroEggEffectSpriteFrameDefinition(ushort Pointer, string Name);
