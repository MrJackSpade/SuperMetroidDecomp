
namespace SuperMetroid.Core.Fx;

/// <summary>Verification access to <see cref="AreaBlockAnimationDefinitions"/> members production does not use.</summary>
internal static class AreaBlockAnimationDefinitionsAccess
{
    /// <summary>Exposes the eight physical area rows of block-animation list and object pointers for source audits.</summary>
    extension(AreaBlockAnimationDefinitions)
    {
        /// <summary>Calculates a native list identity as AC76+20h*area, for indices0..7.</summary>
        /// <remarks>Each eight-word animation list follows an eight-word palette list,
        /// producing the native32-byte area stride. All eight original pointer words
        /// are independently verified; invalid indices reject before arithmetic.</remarks>
        internal static ushort NativeListPointer(int nativeAreaIndex)
        {
            AreaBlockAnimationDefinitions.ValidateNativeAreaIndex(nativeAreaIndex);
            return (ushort)(0xac76 + 0x20 * nativeAreaIndex);
        }

        /// <summary>
        /// Returns one object pointer from any native row, including the non-retail eighth
        /// row retained solely for a complete immutable-source audit.
        /// </summary>
        internal static ushort NativeObjectPointer(int nativeAreaIndex, int bit)
        {
            AreaBlockAnimationDefinitions.ValidateNativeAreaIndex(nativeAreaIndex);
            if ((uint)bit >= AreaBlockAnimationDefinitions.ObjectsPerArea)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(bit), bit, $"Block-animation bit must be 0..{AreaBlockAnimationDefinitions.ObjectsPerArea - 1}.");
            }

            return AreaBlockAnimations.SelectObject(nativeAreaIndex, bit) is { } selected
                ? FxNativeWords.AnimatedTileObjects.Word(selected)
                : FxNativeWords.UnselectedAnimatedTileObject(nativeAreaIndex, bit);
        }

        /// <summary>Restricts native animation-list lookup to its eight physical table rows.</summary>
        internal static void ValidateNativeAreaIndex(int nativeAreaIndex)
        {
            if ((uint)nativeAreaIndex >= AreaBlockAnimationDefinitions.NativeAreaCount)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(nativeAreaIndex), nativeAreaIndex,
                    $"Native block-animation area index must be 0..{AreaBlockAnimationDefinitions.NativeAreaCount - 1}.");
            }
        }
    }
}
