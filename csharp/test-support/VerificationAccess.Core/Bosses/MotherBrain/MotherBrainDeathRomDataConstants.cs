using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Bosses.MotherBrain;

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
    /// <summary>$A9:AF77 copies the live brain colors beginning at CGRAM entry 145.</summary>
    public const int BrainColors = 145;
    /// <summary>$A9:AF7D copies those fifteen colors to sprite palette seven, skipping color zero.</summary>
    public const int CorpseColors = 241;
    /// <summary>$AD:E9CC background palette two's first opaque color.</summary>
    public const int BodyColors = 65;
    /// <summary>$AD:E9DD back-leg palette three's first opaque color.</summary>
    public const int LegColors = 177;
}
