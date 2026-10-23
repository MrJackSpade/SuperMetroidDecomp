
namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Cartridge values of <see cref="MotherBrainFakeDeathPaletteRomData"/> that only verification reads.</summary>
internal static class MotherBrainFakeDeathPaletteRomDataConstants
{
    /// <summary>The resurrection uses the same $AD:ED9C pointer table as the later revival fade.</summary>
    public const int FromGreyPointerTable = MotherBrainDrainedPaletteRomData.FromGreyTable;

    /// <summary>Provides the pointer table for Mother Brain's fake-death transition from grey.</summary>
    extension(MotherBrainFakeDeathPaletteRomData)
    {
        /// <inheritdoc cref="MotherBrainFakeDeathPaletteRomDataConstants.FromGreyPointerTable"/>
        internal static int FromGreyPointerTable => MotherBrainFakeDeathPaletteRomDataConstants.FromGreyPointerTable;
    }
    /// <summary>The fade handlers write from CGRAM byte offset $0122.</summary>
    public const int BrainColor = 0x0122 / sizeof(ushort);
}
