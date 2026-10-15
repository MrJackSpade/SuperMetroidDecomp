
namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>Cartridge values of <see cref="DachoraColorRomData"/> that only verification reads.</summary>
internal static class DachoraColorRomDataConstants
{
    /// <summary>Four native shine-image pointers at $A7:F92D.</summary>
    public const int ShinePointerTable = 0xa7f92d;
    /// <summary>Four native speed-image pointers at $A7:F787.</summary>
    public const int SpeedPointerTable = 0xa7f787;

    /// <summary>Provides the Dachora speed and shine color table pointers used to check palette imports.</summary>
    extension(DachoraColorRomData)
    {
        /// <inheritdoc cref="DachoraColorRomDataConstants.ShinePointerTable"/>
        internal static int ShinePointerTable => DachoraColorRomDataConstants.ShinePointerTable;
        /// <inheritdoc cref="DachoraColorRomDataConstants.SpeedPointerTable"/>
        internal static int SpeedPointerTable => DachoraColorRomDataConstants.SpeedPointerTable;
    }
}
