namespace SuperMetroid.Core.Game;

/// <summary>Authored bank-$A6 palette sources for Lower Norfair Ridley's arena reveal.</summary>
public static class NorfairRidleyPaletteRomData
{

    /// <summary>Thirty-two initial OBJ colors at $A6:E1CF, installed before the hidden palette slots are cleared.</summary>
    public const int InitialColors = EnemyRomTablePointers.Ridley.InitialPaletteWords;
    /// <summary>Thirty-two RGB5 color words copied by the initializer: two complete sixteen-color OBJ palettes, not a byte count.</summary>
    public const int InitialColorCount = 32;
    /// <summary>CGRAM entry 160, converted from native color-memory byte offset $0140; the initial copy covers entries 160..191, OBJ palettes 2 and 3.</summary>
    public const int InitialCgramIndex = 0x140 / 2;

    /// <summary>Fifteen nonzero fixed-bank source pointers and a zero terminator at $A6:A4EB.</summary>
    public const int RevealSourcePointers = EnemyRomTablePointers.Ridley.RevealPaletteSourcePointers;
    /// <summary>Fifteen arena-background reveal rows, selected as indices 0..14 every third reveal-AI call; index 15 is the zero-pointer terminator, not color data.</summary>
    public const int RevealRowCount = 15;
    /// <summary>Fourteen RGB5 words per row, covering BG palette 7 colors 1..14 and leaving its color-zero and color-fifteen slots untouched.</summary>
    public const int RevealColorCount = 14;
    /// <summary>CGRAM entry 113, converted from $A6:A4DF's byte offset $00E2; each arena reveal row overwrites entries 113..126, not Ridley's OBJ body palette.</summary>
    public const int RevealCgramIndex = 0x00e2 / 2;
}
