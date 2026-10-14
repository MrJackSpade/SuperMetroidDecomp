using SuperMetroid.Core.Game;

/// <summary>Cartridge values of <see cref="MotherBrainDeathRomData"/> that only verification reads.</summary>
internal static class MotherBrainDeathRomDataConstants
{
    /// <summary>$AD:E9E8 pointer table, fourteen-color body fades followed by a null word.</summary>
    public const int BodyFadeTable = 0xade9e8;
    /// <summary>$AD:F107 pointer table, fifteen-color decapitated-head fades followed by null.</summary>
    public const int CorpseFadeTable = 0xadf107;

    /// <summary>Provides the body and corpse fade tables used during Mother Brain death comparisons.</summary>
    extension(MotherBrainDeathRomData)
    {
        /// <inheritdoc cref="MotherBrainDeathRomDataConstants.BodyFadeTable"/>
        internal static int BodyFadeTable => MotherBrainDeathRomDataConstants.BodyFadeTable;
        /// <inheritdoc cref="MotherBrainDeathRomDataConstants.CorpseFadeTable"/>
        internal static int CorpseFadeTable => MotherBrainDeathRomDataConstants.CorpseFadeTable;
    }
}
