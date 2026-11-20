namespace SuperMetroid.Core.Frontend.FileSelect;

/// <summary>Where file select shows a slot's energy tanks, in rows (<see cref="TanksPerRow"/>), in steps of one tank.</summary>
internal static class FileSelectEnergyTankDefinitions
{
    /// <summary>Pixels across and down one tank: each tank is shown this far right of the one before, each row this far above the one before.</summary>
    internal const int TankPixels = 8;
    /// <summary>How many tanks' widths right of the ENERGY label the first tank is shown.</summary>
    internal const int ColumnOffset = 4;
    /// <summary>How many tanks' heights below the ENERGY label the first tank is shown.</summary>
    internal const int RowOffset = 1;
    /// <summary>
    /// The tanks each row holds: the first row's fill the row below ENERGY, the rest the row above it. Samus never has
    /// more tanks than two rows hold.
    /// </summary>
    internal const int TanksPerRow = 7;
}
