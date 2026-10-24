
namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>Cartridge values and helpers of <see cref="NorfairRidleyPaletteRomData"/> that only verification reads.</summary>
internal static class NorfairRidleyPaletteRomDataConstants
{
    /// <summary>CGRAM entry 160, converted from native color-memory byte offset $0140; the initial copy covers entries 160..191, OBJ palettes 2 and 3.</summary>
    public const int InitialCgramIndex = 0x140 / 2;
    /// <summary>CGRAM entry 113, converted from $A6:A4DF's byte offset $00E2; each arena reveal row overwrites entries 113..126, not Ridley's OBJ body palette.</summary>
    public const int RevealCgramIndex = 0x00e2 / 2;
}
