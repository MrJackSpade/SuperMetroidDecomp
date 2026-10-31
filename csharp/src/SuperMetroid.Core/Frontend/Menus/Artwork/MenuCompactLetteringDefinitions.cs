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
        new("CRATERIA", [new(0, 3)], new(1, 1), false),
        new("BRINSTAR", [new(4, 7)], new(1, 1), false),
        new("TOURIAN", [new(8, 9), new(32), new(50)], new(3, 1), false),
        new("NORFAIR", [new(16), new(18, 20)], new(3, 1), false),
        new("MARIDIA", [new(21, 22), new(24, 25)], new(3, 1), false),
        new("SHIP", [new(68, 69)], new(1, 0), false),
        new("WRECKED", [new(83, 86)], new(1, 1), false),
        new("EXIT", [new(95), new(183)], new(1, 1), false),
        new("SELECT", [new(165, 167)], new(0, 1), true),
        new("START", [new(168, 170)], new(2, 1), true),
    ];
}
