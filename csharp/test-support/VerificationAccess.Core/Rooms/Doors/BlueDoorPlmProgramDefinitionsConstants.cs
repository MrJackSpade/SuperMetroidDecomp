namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>Cartridge values of <see cref="BlueDoorPlmProgramDefinitions"/> that only verification reads.</summary>
internal static class BlueDoorPlmProgramDefinitionsConstants
{
    /// <summary>Facing-down closing list at $84:C531.</summary>
    public const ushort ClosingDown = 0xc531;
    /// <summary>Facing-left closing list at $84:C49E.</summary>
    public const ushort ClosingLeft = 0xc49e;
    /// <summary>Facing-right closing list at $84:C4CF.</summary>
    public const ushort ClosingRight = 0xc4cf;
    /// <summary>Facing-up closing list at $84:C500.</summary>
    public const ushort ClosingUp = 0xc500;

    /// <summary>Provides the four native blue door closing programs used to verify directional PLM behavior.</summary>
    extension(BlueDoorPlmProgramDefinitions)
    {
        /// <inheritdoc cref="BlueDoorPlmProgramDefinitionsConstants.ClosingDown"/>
        internal static ushort ClosingDown => BlueDoorPlmProgramDefinitionsConstants.ClosingDown;
        /// <inheritdoc cref="BlueDoorPlmProgramDefinitionsConstants.ClosingLeft"/>
        internal static ushort ClosingLeft => BlueDoorPlmProgramDefinitionsConstants.ClosingLeft;
        /// <inheritdoc cref="BlueDoorPlmProgramDefinitionsConstants.ClosingRight"/>
        internal static ushort ClosingRight => BlueDoorPlmProgramDefinitionsConstants.ClosingRight;
        /// <inheritdoc cref="BlueDoorPlmProgramDefinitionsConstants.ClosingUp"/>
        internal static ushort ClosingUp => BlueDoorPlmProgramDefinitionsConstants.ClosingUp;
    }
}
