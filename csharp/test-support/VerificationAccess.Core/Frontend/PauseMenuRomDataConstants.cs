using SuperMetroid.Core.Frontend;

/// <summary>Cartridge values of <see cref="PauseMenuRomData"/> that only verification reads.</summary>
internal static class PauseMenuRomDataConstants
{
    /// <summary>Equipment-set lookup table at $82:B257.</summary>
    public const int EquipmentSetTable = 0x82b257;

    /// <summary>Provides the pause equipment set table used to verify menu data import.</summary>
    extension(PauseMenuRomData)
    {
        /// <inheritdoc cref="PauseMenuRomDataConstants.EquipmentSetTable"/>
        internal static int EquipmentSetTable => PauseMenuRomDataConstants.EquipmentSetTable;
    }
}
