
namespace SuperMetroid.Core.Enemies.MaridiaSnails;

/// <summary>Cartridge values and helpers of <see cref="MaridiaLargeSnailCollisionDefinitions"/> that only verification reads.</summary>
internal static class MaridiaLargeSnailCollisionDefinitionsConstants
{
    /// <summary>First Oum extended frame, <c>ExtendedSpritemap_Oum_FacingLeft_0</c> at $A2:CB87.</summary>
    internal const ushort FirstFrame = 0xcb87;
    /// <summary>Last Oum extended frame, <c>ExtendedSpritemap_Oum_FacingRight_E</c> at $A2:CCA9.</summary>
    internal const ushort LastFrame = 0xcca9;
    /// <summary>Each native one-component extended frame occupies ten bytes.</summary>
    internal const int FrameStride = 10;
    /// <summary>Enumerates all thirty one-component Oum frames in native address order.</summary>
    internal static FramePointerSequence FramePointers => default;
    /// <summary>Consecutive ten-byte one-component frame identities at $A2:CB87-$CCA9.</summary>
    internal readonly struct FramePointerSequence : IReadOnlyList<ushort>
    {
        /// <summary>Thirty native frames cover both facing directions.</summary>
        public int Count => (LastFrame - FirstFrame) / FrameStride + 1;
        /// <summary>Alias for the compiled frame count.</summary>
        public int Length => Count;
        /// <summary>Calculates an aligned frame pointer from its native ordinal.</summary>
        public ushort this[int index] => (uint)index < Count
            ? (ushort)(FirstFrame + index * FrameStride)
            : throw new ArgumentOutOfRangeException(nameof(index));
        /// <summary>Visits Oum frame pointers in ascending native address order.</summary>
        public IEnumerator<ushort> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return this[index];
        }
        /// <summary>Exposes the same frame ordering through non-generic enumeration.</summary>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
