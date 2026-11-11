namespace SuperMetroid.Core.Frontend.FileSelect;

/// <summary>Where file select shows a slot's energy tanks, row by row, in steps of one tank.</summary>
internal static class FileSelectEnergyTankDefinitions
{
    /// <summary>The display divides current and maximum energy by 100.</summary>
    internal const int EnergyPerTank = 100;
    /// <summary>Pixels across and down one tank: each tank is shown this far right of the one before, each row this far above the one before.</summary>
    internal const int TankPixels = 8;
    /// <summary>The first tank is shown four tanks' widths right of the ENERGY label.</summary>
    internal const int ColumnOffset = 4;
    /// <summary>The first tank is shown one tank's height below the ENERGY label.</summary>
    internal const int RowOffset = 1;
    /// <summary>Seven tanks are drawn on the lower row first.</summary>
    internal const int FirstRowCount = 7;
    /// <summary>Eight tanks are permitted after wrapping (a harmless off-by-one in the game).</summary>
    internal const int WrappedRowCount = 8;
    /// <summary>
    /// After a row the drawing steps up one tank and back seven tanks, so a wrapped row starts where the first row
    /// started, and a third row one tank right of that.
    /// </summary>
    internal const int RowRewindColumns = 7;
}
