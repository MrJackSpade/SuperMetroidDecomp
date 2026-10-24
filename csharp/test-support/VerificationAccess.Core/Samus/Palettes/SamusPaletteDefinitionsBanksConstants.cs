namespace SuperMetroid.Core.Samus.Palettes;

/// <summary>Cartridge values of <see cref="SamusPaletteDefinitions"/> that only verification reads.</summary>
internal static class SamusPaletteDefinitionsBanksConstants
{
    /// <summary>Bank <c>$91</c>, which owns Samus palette pointer lists.</summary>
    public const int Movement = 0x910000;

        /// <summary>Bank <c>$8D</c>, which owns generic palette-FX programs.</summary>
        public const int PaletteFx = 0x8d0000;
}
