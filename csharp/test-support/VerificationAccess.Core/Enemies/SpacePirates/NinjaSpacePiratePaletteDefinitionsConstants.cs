
namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>Cartridge values of <see cref="NinjaSpacePiratePaletteDefinitions"/> that only verification reads.</summary>
internal static class NinjaSpacePiratePaletteDefinitionsConstants
{
    /// <summary>Native source of the shared gold-Pirate color words at $B2:8727.</summary>
    public const int SharedGoldPirateSource = 0xb28727;

    /// <summary>Provides the shared gold pirate palette source used to verify Ninja Space Pirate colors.</summary>
    extension(NinjaSpacePiratePaletteDefinitions)
    {
        /// <inheritdoc cref="NinjaSpacePiratePaletteDefinitionsConstants.SharedGoldPirateSource"/>
        internal static int SharedGoldPirateSource => NinjaSpacePiratePaletteDefinitionsConstants.SharedGoldPirateSource;
    }
}
