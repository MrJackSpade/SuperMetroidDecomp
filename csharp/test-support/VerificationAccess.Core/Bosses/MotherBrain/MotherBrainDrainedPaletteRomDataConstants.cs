
namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Cartridge values of <see cref="MotherBrainDrainedPaletteRomData"/> that only verification reads.</summary>
internal static class MotherBrainDrainedPaletteRomDataConstants
{
    /// <summary>$AD:EF44/EF81 write the trailing source word directly to low WRAM $017C.</summary>
    public const int TrailingWordWram = 0x7e017c;

    /// <summary>Exposes the drained-palette trailing-word destination to verification callers.</summary>
    extension(MotherBrainDrainedPaletteRomData)
    {
        /// <inheritdoc cref="MotherBrainDrainedPaletteRomDataConstants.TrailingWordWram"/>
        internal static int TrailingWordWram => MotherBrainDrainedPaletteRomDataConstants.TrailingWordWram;
    }
}
