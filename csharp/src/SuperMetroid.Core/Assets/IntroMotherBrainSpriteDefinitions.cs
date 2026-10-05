using System.Collections;

namespace SuperMetroid.Core.Assets;

/// <summary>Three visual frames selected by the intro Mother Brain's compiled lists.</summary>
internal static class IntroMotherBrainSpriteDefinitions
{
    /// <summary>$8C:8C00, first nine-part Mother Brain frame.</summary>
    internal const ushort FrameZero = 0x8c00;
    /// <summary>$8C:8C2F, second nine-part Mother Brain frame.</summary>
    internal const ushort FrameOne = FrameZero + 2 + 5 * StockPartCount;
    /// <summary>$8C:8C5E, third nine-part Mother Brain frame.</summary>
    internal const ushort FrameTwo = FrameOne + 2 + 5 * StockPartCount;
    /// <summary>Every retail frame has nine OAM parts; $8C:8C8D is unrelated.</summary>
    internal const int StockPartCount = 9;

    internal static ushort FramePointer(int frame)
    {
        if ((uint)frame >= 3) throw new ArgumentOutOfRangeException(nameof(frame));
        return (ushort)(FrameZero + frame * (2 + 5 * StockPartCount));
    }
    internal static IReadOnlyList<IntroMotherBrainSpriteFrameDefinition> Frames { get; } = new FrameList();
    private sealed class FrameList : IReadOnlyList<IntroMotherBrainSpriteFrameDefinition>
    {
        public int Count => 3;
        public IntroMotherBrainSpriteFrameDefinition this[int index] =>
            new(FramePointer(index), $"mother-brain-frame-{index}");
        public IEnumerator<IntroMotherBrainSpriteFrameDefinition> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return this[index];
        }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}

internal readonly record struct IntroMotherBrainSpriteFrameDefinition(ushort Pointer, string Name);