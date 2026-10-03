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

    /// <summary>Direct area/bit behavior selection, shared by retail and native audit views.</summary>
    /// <remarks>All eight original lists at $83:AC76..AD65 share spike objects for
    /// bits0/1. Later bits select named area effects or the empty object. This dispatch
    /// is independently verified against NTSC J/U v1.0 and pinned bank_83.asm
    /// (362be646929cf8e483f692b73a6561cfc2dc1d0d); no selection matrix remains.
    /// Callers retain their distinct area bounds and validation order.</remarks>
    private static ushort SelectObject(int area, int bit) => ((AreaId)area, bit) switch
    {
        (_, 0) => AnimatedTileObjectPointers.HorizontalSpikes,
        (_, 1) => AnimatedTileObjectPointers.VerticalSpikes,
        (AreaId.Crateria, 2) => AnimatedTileObjectPointers.CrateriaLake,
        (AreaId.Crateria, 3) => AnimatedTileObjectPointers.UnusedCrateriaLava,
        (AreaId.Brinstar, 2) => AnimatedTileObjectPointers.BrinstarPlant,
        (AreaId.WreckedShip, 2) => AnimatedTileObjectPointers.WreckedShipTreadmillRightwards,
        (AreaId.WreckedShip, 3) => AnimatedTileObjectPointers.WreckedShipTreadmillLeftwards,
        (AreaId.WreckedShip, 4) => AnimatedTileObjectPointers.WreckedShipScreen,
        (AreaId.Maridia, 2) => AnimatedTileObjectPointers.MaridiaSandCeiling,
        (AreaId.Maridia, 3) => AnimatedTileObjectPointers.MaridiaSandFalling,
        _ => AnimatedTileObjectPointers.Empty,
    };
    /// <summary>Returns the bank-$87 object header selected by one retail area/bit pair.</summary>
    public static ushort Read(AreaId area, int bit)
    {
        if ((uint)bit >= ObjectsPerArea)
        {
            throw new ArgumentOutOfRangeException(
                nameof(bit), bit, $"Animated-tile bit must be 0..{ObjectsPerArea - 1}.");
        }

        return SelectObject(AreaIds.ToIndex(area), bit);
    }

    /// <summary>Calculates a native list identity as AC76+20h*area, for indices0..7.</summary>
    /// <remarks>Each eight-word animation list follows an eight-word palette list,
    /// producing the native32-byte area stride. All eight original pointer words
    /// are independently verified; invalid indices reject before arithmetic.</remarks>
    internal static ushort NativeListPointer(int nativeAreaIndex)
    {
        ValidateNativeAreaIndex(nativeAreaIndex);
        return (ushort)(0xac76 + 0x20 * nativeAreaIndex);
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

        return SelectObject(nativeAreaIndex, bit);
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
