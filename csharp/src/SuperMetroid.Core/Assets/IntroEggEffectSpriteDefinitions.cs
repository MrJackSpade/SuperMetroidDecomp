using System.Collections;

namespace SuperMetroid.Core.Assets;

/// <summary>Six shell fragments and five slime-drop OAM frames from the SR388 scene.</summary>
internal static class IntroEggEffectSpriteDefinitions
{
    /// <summary>$8C:8F7E, first shell-fragment one-part composition.</summary>
    internal const ushort Start = 0x8f7e;

    /// <summary>Number of OAM parts in each original shell-fragment or slime-drop frame.</summary>
    internal const int StockPartCount = 1;

    /// <summary>Eleven native one-part records, each two count bytes plus five part bytes.</summary>
    internal static ushort FramePointer(int frame)
    {
        if ((uint)frame >= 11) throw new ArgumentOutOfRangeException(nameof(frame));
        return (ushort)(Start + frame * 7);
    }
    /// <summary>Ordered catalog of six shell-fragment frames followed by five slime-drop frames.</summary>
    internal static IReadOnlyList<IntroEggEffectSpriteFrameDefinition> Frames { get; } = new FrameList();

    /// <summary>Provides indexed frame metadata without allocating a separate catalog for each request.</summary>
    private sealed class FrameList : IReadOnlyList<IntroEggEffectSpriteFrameDefinition>
    {
        /// <summary>Number of native OAM records represented by this catalog.</summary>
        public int Count => 11;

        /// <summary>Creates the frame metadata at an index in shell-fragment-then-slime order.</summary>
        /// <param name="index">Zero-based frame index in the eleven-entry catalog.</param>
        /// <returns>The ROM pointer and stable extraction name for that frame.</returns>
        /// <exception cref="ArgumentOutOfRangeException">The index is outside the catalog.</exception>
        public IntroEggEffectSpriteFrameDefinition this[int index]
        {
            get
            {
                ushort pointer = FramePointer(index);
                return new(pointer, index < 6 ? $"fragment-{index}" : index == 6 ? "slime-moving" : $"slime-impact-{index - 7}");
            }
        }
        /// <summary>Enumerates frame definitions in the same order as the indexed catalog.</summary>
        /// <returns>An iterator over all eleven frame definitions.</returns>
        public IEnumerator<IntroEggEffectSpriteFrameDefinition> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return this[index];
        }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}

/// <summary>Identifies one native egg-effect spritemap and its stable extracted-artwork name.</summary>
/// <param name="Pointer">Bank-relative address of the native OAM frame record.</param>
/// <param name="Name">Stable label distinguishing this fragment or slime-drop frame in extracted assets.</param>
internal readonly record struct IntroEggEffectSpriteFrameDefinition(ushort Pointer, string Name);
