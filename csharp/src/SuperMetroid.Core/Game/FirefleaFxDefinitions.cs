namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled fixed-color shade definitions for the Fireflea room effect. The flashing
/// phase and enemy-death darkness level remain mutable WRAM state in
/// <see cref="FirefleaRoomFx"/>.
/// </summary>
internal static class FirefleaFxDefinitions
{
    /// <summary>
    /// <c>Fireflea_Flashing_Shades</c> at $88:B058: the twelve packed shade words
    /// selected by the room effect's six-frame phase clock.
    /// </summary>
    private static ReadOnlySpan<ushort> FlashingShades =>
    [
        0x0000, 0x0100, 0x0200, 0x0300, 0x0400, 0x0500,
        0x0600, 0x0500, 0x0400, 0x0300, 0x0200, 0x0100,
    ];

    /// <summary>
    /// <c>Fireflea_Darkness_Shades</c> at $88:B070 plus the cartridge-visible
    /// offset-twelve word at $88:B07C. Retail enemy logic advances the byte offset
    /// through 0, 2, ... 12, so the final state intentionally observes the adjacent
    /// <c>PHP/SEP #$20</c> opcode bytes as packed shade $C208.
    /// </summary>
    private static ReadOnlySpan<ushort> DarknessShades =>
    [
        0x0000, 0x0600, 0x0c00, 0x1200, 0x1800, 0x1900, 0xc208,
    ];

    /// <summary>$88:B07F/$88:B0D5: six effect passes per flashing shade.</summary>
    internal const ushort FlashDuration = 6;

    /// <summary>$88:B0EC: twelve entries before the flashing cycle wraps.</summary>
    internal const ushort FlashCount = 12;

    /// <summary>$88:B0DE: darkness byte offset at which flashing stops.</summary>
    internal const ushort SteadyDarknessThreshold = 10;

    /// <summary>$88:B0E3: fixed flashing-table index used at maximum darkness.</summary>
    internal const ushort SteadyFlashIndex = 6;

    internal static ushort FlashingShade(ushort index)
    {
        if (index >= FlashingShades.Length)
        {
            throw new InvalidDataException(
                $"Fireflea flashing index {index} is outside the {FlashingShades.Length}-entry native cycle.");
        }

        return FlashingShades[index];
    }

    internal static ushort DarknessShade(ushort byteOffset)
    {
        if ((byteOffset & 1) != 0 || byteOffset / 2 >= DarknessShades.Length)
        {
            throw new InvalidDataException(
                $"Fireflea darkness byte offset {byteOffset} is outside the retail 0-through-12 even domain.");
        }

        return DarknessShades[byteOffset / 2];
    }
}
