using System.Collections;

namespace SuperMetroid.Core.Assets;

/// <summary>The three four-quadrant Rinka frames selected by the intro list.</summary>
internal static class IntroRinkaSpriteDefinitions
{
    /// <summary>$8C:8C8D, first intro Rinka composition.</summary>
    internal const ushort First = 0x8c8d;
    /// <summary>$8C:8CCF, exclusive end after three consecutive 22-byte records.</summary>
    internal const ushort End = 0x8ccf;

    /// <summary>Three native four-part records: two count bytes plus four five-byte parts.</summary>
    internal static ushort FramePointer(int frame)
    {
        if ((uint)frame >= 3) throw new ArgumentOutOfRangeException(nameof(frame));
        return (ushort)(First + frame * (2 + 5 * StockPartCount));
    }
    internal static IReadOnlyList<IntroRinkaSpriteFrameDefinition> Frames { get; } = new FrameList();
    private sealed class FrameList : IReadOnlyList<IntroRinkaSpriteFrameDefinition>
    {
        public int Count => 3;
        public IntroRinkaSpriteFrameDefinition this[int index] => new(FramePointer(index), $"rinka-{index}");
        public IEnumerator<IntroRinkaSpriteFrameDefinition> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return this[index];
        }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
    internal const int StockPartCount = 4;
}

internal readonly record struct IntroRinkaSpriteFrameDefinition(ushort Pointer, string Name);
