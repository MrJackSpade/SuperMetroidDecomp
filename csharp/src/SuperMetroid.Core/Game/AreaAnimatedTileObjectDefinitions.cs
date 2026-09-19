namespace SuperMetroid.Core.Game;

/// <summary>
/// Immutable bank-$83 area-to-animated-tile-object selection metadata consumed by
/// <c>LoadFxHeader</c>.
/// </summary>
/// <remarks>
/// Each area owns eight bank-$87 object-header pointers, selected by the eight bits in
/// an FX record's animated-tile bitset. These pointers choose object behavior; the
/// selected objects' character-frame operands and graphics remain presentation data.
/// </remarks>
internal static class AreaAnimatedTileObjectDefinitions
{
    /// <summary>
    /// Native <c>kArea_AnimtilesObjectListPtrs</c> at <c>$83:AC56</c>; eight words point
    /// to the eight native area lists below.
    /// </summary>
    public const int NativeListPointerTable = 0x83ac56;

    /// <summary>Eight object-header words are addressable by one FX bitset.</summary>
    public const int ObjectsPerArea = 8;

    /// <summary>
    /// The cartridge stores an eighth non-retail list after the seven typed areas.
    /// Production <see cref="Read(AreaId, int)"/> deliberately accepts only typed retail
    /// areas, while verification retains this row so every native table word is audited.
    /// </summary>
    public const int NativeAreaCount = 8;

    private static readonly ushort[] NativeListPointers =
    [
        0xac76, 0xac96, 0xacb6, 0xacd6,
        0xacf6, 0xad16, 0xad36, 0xad56,
    ];

    private static readonly ushort[][] Objects =
    [
        // Crateria: two common entries, two area-specific entries, then the shared header.
        [0x8257, 0x8251, 0x825d, 0x8263, 0x824b, 0x824b, 0x824b, 0x824b],

        // Brinstar.
        [0x8257, 0x8251, 0x8281, 0x824b, 0x824b, 0x824b, 0x824b, 0x824b],

        // Norfair.
        [0x8257, 0x8251, 0x824b, 0x824b, 0x824b, 0x824b, 0x824b, 0x824b],

        // Wrecked Ship: bits two and three select the two treadmill directions.
        [0x8257, 0x8251,
            AnimatedTileObjectPointers.WreckedShipTreadmillRightwards,
            AnimatedTileObjectPointers.WreckedShipTreadmillLeftwards,
            0x826f, 0x824b, 0x824b, 0x824b],

        // Maridia: bits two and three select ceiling and falling sand respectively.
        [0x8257, 0x8251,
            AnimatedTileObjectPointers.MaridiaSandCeiling,
            AnimatedTileObjectPointers.MaridiaSandFalling,
            0x824b, 0x824b, 0x824b, 0x824b],

        // Tourian.
        [0x8257, 0x8251, 0x824b, 0x824b, 0x824b, 0x824b, 0x824b, 0x824b],

        // Ceres.
        [0x8257, 0x8251, 0x824b, 0x824b, 0x824b, 0x824b, 0x824b, 0x824b],

        // Native eighth row, unreachable through the seven-value retail AreaId domain.
        [0x8257, 0x8251, 0x824b, 0x824b, 0x824b, 0x824b, 0x824b, 0x824b],
    ];

    /// <summary>Returns the bank-$87 object header selected by one retail area/bit pair.</summary>
    public static ushort Read(AreaId area, int bit)
    {
        if ((uint)bit >= ObjectsPerArea)
        {
            throw new ArgumentOutOfRangeException(
                nameof(bit), bit, $"Animated-tile bit must be 0..{ObjectsPerArea - 1}.");
        }

        return Objects[AreaIds.ToIndex(area)][bit];
    }

    /// <summary>Returns one native list pointer for exhaustive cartridge comparison.</summary>
    internal static ushort NativeListPointer(int nativeAreaIndex)
    {
        ValidateNativeAreaIndex(nativeAreaIndex);
        return NativeListPointers[nativeAreaIndex];
    }

    /// <summary>
    /// Returns one object pointer from any native row, including the non-retail eighth
    /// row retained solely for a complete immutable-source audit.
    /// </summary>
    internal static ushort NativeObjectPointer(int nativeAreaIndex, int bit)
    {
        ValidateNativeAreaIndex(nativeAreaIndex);
        if ((uint)bit >= ObjectsPerArea)
        {
            throw new ArgumentOutOfRangeException(
                nameof(bit), bit, $"Animated-tile bit must be 0..{ObjectsPerArea - 1}.");
        }

        return Objects[nativeAreaIndex][bit];
    }

    private static void ValidateNativeAreaIndex(int nativeAreaIndex)
    {
        if ((uint)nativeAreaIndex >= NativeAreaCount)
        {
            throw new ArgumentOutOfRangeException(
                nameof(nativeAreaIndex), nativeAreaIndex,
                $"Native animated-tile area index must be 0..{NativeAreaCount - 1}.");
        }
    }
}
