using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Samus.Abilities;

namespace SuperMetroid.Core.Samus.Palettes;

/// <summary>Cartridge values of <see cref="SamusPaletteRomData.Banks"/> that only verification reads.</summary>
internal static class SamusPaletteRomDataBanksConstants
{
    /// <summary>Bank <c>$91</c>, which owns Samus palette pointer lists.</summary>
    public const int Movement = 0x910000;

    /// <summary>Provides the native movement palette bank for Samus palette imports.</summary>
    extension(SamusPaletteRomData.Banks)
    {
        /// <inheritdoc cref="SamusPaletteRomDataBanksConstants.Movement"/>
        internal static int Movement => SamusPaletteRomDataBanksConstants.Movement;
    }
        /// <summary>Bank <c>$8D</c>, which owns generic palette-FX programs.</summary>
        public const int PaletteFx = 0x8d0000;
}
