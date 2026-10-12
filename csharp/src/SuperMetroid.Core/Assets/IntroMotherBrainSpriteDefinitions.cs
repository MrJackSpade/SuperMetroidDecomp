using System.Collections;

namespace SuperMetroid.Core.Assets;

/// <summary>Three visual frames selected by the intro Mother Brain's compiled lists.</summary>
internal static class IntroMotherBrainSpriteDefinitions
{
    /// <summary>$8C:8C00, first nine-part Mother Brain frame.</summary>
    internal const ushort FrameZero = 0x8c00;
    /// <summary>Every retail frame has nine OAM parts; $8C:8C8D is unrelated.</summary>
    internal const int StockPartCount = 9;

    /// <summary>Frame pointer.</summary>
    internal static ushort FramePointer(int frame)
    {
        if ((uint)frame >= 3) throw new ArgumentOutOfRangeException(nameof(frame));
        return (ushort)(FrameZero + frame * (2 + 5 * StockPartCount));
    }
    /// <summary>Gets frames.</summary>
    internal static IReadOnlyList<IntroMotherBrainSpriteFrameDefinition> Frames { get; } = new FrameList();
    /// <summary>Represents frame list.</summary>
    private sealed class FrameList : IReadOnlyList<IntroMotherBrainSpriteFrameDefinition>
    {
        /// <summary>Gets count.</summary>
        public int Count => 3;
        /// <summary>Gets an indexed value.</summary>
        public IntroMotherBrainSpriteFrameDefinition this[int index] =>
            new(FramePointer(index), $"mother-brain-frame-{index}");
        /// <summary>Returns an enumerator over this sequence.</summary>
        public IEnumerator<IntroMotherBrainSpriteFrameDefinition> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return this[index];
        }
        /// <summary>Returns an enumerator over this sequence.</summary>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}

/// <summary>Represents intro mother brain sprite frame definition.</summary>
internal readonly record struct IntroMotherBrainSpriteFrameDefinition(ushort Pointer, string Name);