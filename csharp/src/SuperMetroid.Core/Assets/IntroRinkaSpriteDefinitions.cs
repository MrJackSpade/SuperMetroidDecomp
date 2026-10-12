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
    /// <summary>Gets frames.</summary>
    internal static IReadOnlyList<IntroRinkaSpriteFrameDefinition> Frames { get; } = new FrameList();
    /// <summary>Represents frame list.</summary>
    private sealed class FrameList : IReadOnlyList<IntroRinkaSpriteFrameDefinition>
    {
        /// <summary>Gets the number of items in this collection.</summary>
        public int Count => 3;
        /// <summary>Accesses a value by index or range.</summary>
        public IntroRinkaSpriteFrameDefinition this[int index] => new(FramePointer(index), $"rinka-{index}");
        /// <summary>Returns an enumerator over this collection.</summary>
        public IEnumerator<IntroRinkaSpriteFrameDefinition> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return this[index];
        }
        /// <summary>Returns an enumerator over this collection.</summary>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
    /// <summary>Defines stock part count.</summary>
    internal const int StockPartCount = 4;
}

/// <summary>Represents intro rinka sprite frame definition.</summary>
internal readonly record struct IntroRinkaSpriteFrameDefinition(ushort Pointer, string Name);
