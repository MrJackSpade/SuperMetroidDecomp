namespace SuperMetroid.Core.Frontend.Menus.Artwork;

/// <summary>
/// The compact-lettering labels in the menu atlas: the seven elevator area labels selected by $82:C4DB..C568, the ship
/// label, EXIT (cells $5F/$B7) and SELECT/START (cells $A5..$AA). Norfair and Maridia skip the large-font stem cells
/// $11 and $17, Tourian occupies $08, $09, $20, $32 left to right ($82:C51D), and EXIT spans $5F then $B7.
/// </summary>
internal static class MenuCompactLetteringDefinitions
{
    /// <summary>The labels in ascending order of their first cell, the order their glyphs are first captured in.</summary>
    internal static readonly MenuLetteringStrip[] Strips =
    [
        new("CRATERIA", [new(0x00, 0x03)], new(1, 1), false),
        new("BRINSTAR", [new(0x04, 0x07)], new(1, 1), false),
        new("TOURIAN", [new(0x08, 0x09), new(0x20), new(0x32)], new(3, 1), false),
        new("NORFAIR", [new(0x10), new(0x12, 0x14)], new(3, 1), false),
        new("MARIDIA", [new(0x15, 0x16), new(0x18, 0x19)], new(3, 1), false),
        new("SHIP", [new(0x44, 0x45)], new(1, 0), false),
        new("WRECKED", [new(0x53, 0x56)], new(1, 1), false),
        new("EXIT", [new(0x5f), new(0xb7)], new(1, 1), false),
        new("SELECT", [new(0xa5, 0xa7)], new(0, 1), true),
        new("START", [new(0xa8, 0xaa)], new(2, 1), true),
    ];

    /// <summary>The label strip spelling <paramref name="text"/>.</summary>
    internal static MenuLetteringStrip Strip(string text) =>
        Strips.FirstOrDefault(strip => strip.Text == text) ??
        throw new ArgumentOutOfRangeException(nameof(text), text, "No compact-lettering label spells this text.");
}
