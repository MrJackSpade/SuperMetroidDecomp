
namespace SuperMetroid.Core.Fx;

/// <summary>Verification access to <see cref="AreaAnimatedTileObjectDefinitions"/> members production does not use.</summary>
internal static class AreaAnimatedTileObjectDefinitionsAccess
{
    /// <summary>Exposes the eight physical area rows of animated-tile list and object pointers for source audits.</summary>
    extension(AreaAnimatedTileObjectDefinitions)
    {
        /// <summary>Calculates a native list identity as AC76+20h*area, for indices0..7.</summary>
        /// <remarks>Each eight-word animation list follows an eight-word palette list,
        /// producing the native32-byte area stride. All eight original pointer words
        /// are independently verified; invalid indices reject before arithmetic.</remarks>
        internal static ushort NativeListPointer(int nativeAreaIndex)
        {
            AreaAnimatedTileObjectDefinitions.ValidateNativeAreaIndex(nativeAreaIndex);
            return (ushort)(0xac76 + 0x20 * nativeAreaIndex);
        }

        /// <summary>
        /// Returns one object pointer from any native row, including the non-retail eighth
        /// row retained solely for a complete immutable-source audit.
        /// </summary>
        internal static ushort NativeObjectPointer(int nativeAreaIndex, int bit)
        {
            AreaAnimatedTileObjectDefinitions.ValidateNativeAreaIndex(nativeAreaIndex);
            if ((uint)bit >= AreaAnimatedTileObjectDefinitions.ObjectsPerArea)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(bit), bit, $"Animated-tile bit must be 0..{AreaAnimatedTileObjectDefinitions.ObjectsPerArea - 1}.");
            }

            return ((ushort)(AreaAnimatedTileObjectDefinitions.SelectObject((int)(nativeAreaIndex), (int)(bit))));
        }

        /// <summary>Restricts native animation-list lookup to its eight physical table rows.</summary>
        internal static void ValidateNativeAreaIndex(int nativeAreaIndex)
        {
            if ((uint)nativeAreaIndex >= AreaAnimatedTileObjectDefinitions.NativeAreaCount)
            {
                throw new ArgumentOutOfRangeException(
                    "nativeAreaIndex", nativeAreaIndex,
                    $"Native animated-tile area index must be 0..{AreaAnimatedTileObjectDefinitions.NativeAreaCount - 1}.");
            }
        }
    }
}
