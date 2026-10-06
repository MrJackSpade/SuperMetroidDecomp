namespace SuperMetroid.Core.Frontend;

/// <summary>Draw_FileSelection_Energy ($81:A0A4-A111) tank cells and row traversal.</summary>
internal static class FileSelectEnergyTankDefinitions
{
    /// <summary>$81:A0B0/A0CD divides current and maximum energy by 100.</summary>
    internal const int EnergyPerTank = 100;
    /// <summary>$81:A0A4 advances four tile cells from the ENERGY anchor.</summary>
    internal const int ColumnOffset = 4;
    /// <summary>$81:A0E4 starts one tilemap row below the ENERGY anchor.</summary>
    internal const int RowOffset = 1;
    /// <summary>$81:A0DD draws seven tanks on the lower row first.</summary>
    internal const int FirstRowCount = 7;
    /// <summary>$81:A10C permits eight cells after wrapping (native harmless off-by-one).</summary>
    internal const int WrappedRowCount = 8;
    /// <summary>$81:A108 subtracts $4E bytes after incrementing past the row.</summary>
    internal const int RowRewindCells = 39;
    /// <summary>$81:A0F5 selects the filled-tank tile before OR-ing the menu palette.</summary>
    internal const ushort FilledTile = 0x0098;
    /// <summary>$81:A0EC selects the empty-tank tile before OR-ing the menu palette.</summary>
    internal const ushort EmptyTile = 0x0099;
    /// <summary>BG tilemap palette bits, inherited from the installed health digit.</summary>
    internal const ushort PaletteMask = 0x1c00;
}
