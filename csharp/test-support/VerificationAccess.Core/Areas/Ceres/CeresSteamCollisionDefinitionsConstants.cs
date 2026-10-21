
namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>Cartridge values and helpers of <see cref="CeresSteamCollisionDefinitions"/> that only verification reads.</summary>
internal static class CeresSteamCollisionDefinitionsConstants
{
    /// <summary>First ten-byte extended frame, ExtendedSpritemap_CeresSteam_Up_0 at $A6:F142.</summary>
    internal const ushort FirstFrame = 0xf142;
    /// <summary>Twenty-eight contiguous extended-frame pointers in native direction and pose order.</summary>
    internal static FrameSequence FramePointers { get; } = new();
    /// <summary>Enumerates the twenty-eight extended-frame addresses without storing a pointer array.</summary>
    internal sealed class FrameSequence : IReadOnlyList<ushort>
    {
        /// <summary>Number of steam frames across four directions and seven poses.</summary>
        public int Count => 28;
        /// <summary>Alias for the frame count used by existing indexing callers.</summary>
        public int Length => Count;
        /// <summary>Returns an indexed frame pointer at a ten-byte native stride.</summary>
        public ushort this[int index] => (uint)index < Count
            ? (ushort)(FirstFrame + 10 * index) : throw new ArgumentOutOfRangeException(nameof(index));
        /// <summary>Yields all frame pointers in native order.</summary>
        public IEnumerator<ushort> GetEnumerator()
        {
            for (int index = 0; index < Count; index++)
                yield return this[index];
        }
        /// <summary>Exposes the same frame order through the non-generic collection interface.</summary>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
