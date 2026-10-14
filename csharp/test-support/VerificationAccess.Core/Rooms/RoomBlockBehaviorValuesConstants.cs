using SuperMetroid.Core.Rooms;

/// <summary>Cartridge values of <see cref="RoomBlockBehaviorValues"/> that only verification reads.</summary>
internal static class RoomBlockBehaviorValuesConstants
{
    /// <summary>Down-facing blue-door shootable-cap dispatcher.</summary>
    public static readonly RoomBlockBehavior BlueDoorFacingDown = new(0x43);
    /// <summary>Up-facing blue-door shootable-cap dispatcher.</summary>
    public static readonly RoomBlockBehavior BlueDoorFacingUp = new(0x42);

    /// <summary>Provides native up- and down-facing blue door block values for room collision checks.</summary>
    extension(RoomBlockBehaviorValues)
    {
        /// <inheritdoc cref="RoomBlockBehaviorValuesConstants.BlueDoorFacingDown"/>
        internal static RoomBlockBehavior BlueDoorFacingDown => RoomBlockBehaviorValuesConstants.BlueDoorFacingDown;
        /// <inheritdoc cref="RoomBlockBehaviorValuesConstants.BlueDoorFacingUp"/>
        internal static RoomBlockBehavior BlueDoorFacingUp => RoomBlockBehaviorValuesConstants.BlueDoorFacingUp;
    }
}
