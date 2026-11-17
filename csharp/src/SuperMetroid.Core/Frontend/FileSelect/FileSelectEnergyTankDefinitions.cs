namespace SuperMetroid.Core.Frontend.FileSelect;

/// <summary>Where file select shows a slot's energy tanks, in rows of seven, in steps of one tank.</summary>
internal static class FileSelectEnergyTankDefinitions
{
    /// <summary>Pixels across and down one tank: each tank is shown this far right of the one before, each row this far above the one before.</summary>
    internal const int TankPixels = 8;
    /// <summary>The first tank is shown four tanks' widths right of the ENERGY label.</summary>
    internal const int ColumnOffset = 4;
    /// <summary>The first tank is shown one tank's height below the ENERGY label.</summary>
    internal const int RowOffset = 1;
    /// <summary>
    /// Each row holds seven tanks: the first seven fill the row below ENERGY, the rest the row above it. Samus has at most
    /// fourteen tanks, so two rows hold them all.
    /// </summary>
    internal const int TanksPerRow = 7;
}
