using SuperMetroid.Core.Game;

/// <summary>Cartridge values of <see cref="RoomEnemyRomLayout"/> that only verification reads.</summary>
internal static class RoomEnemyRomLayoutConstants
{
    /// <summary>Bank containing room enemy population records.</summary>
    public const int PopulationBank = 0xa10000;

    /// <summary>Provides the population bank used to resolve native room enemy records.</summary>
    extension(RoomEnemyRomLayout)
    {
        /// <inheritdoc cref="RoomEnemyRomLayoutConstants.PopulationBank"/>
        internal static int PopulationBank => RoomEnemyRomLayoutConstants.PopulationBank;
    }
}
