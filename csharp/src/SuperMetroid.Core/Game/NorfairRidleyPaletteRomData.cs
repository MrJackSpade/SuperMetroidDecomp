namespace SuperMetroid.Core.Game;

/// <summary>Authored bank-$A6 palette sources for Lower Norfair Ridley's arena reveal.</summary>
public static class NorfairRidleyPaletteRomData
{
    /// <summary>Thirty-two initial OBJ colors at $A6:E1CF, installed before the hidden palette slots are cleared.</summary>
    public const int InitialColors = EnemyRomTablePointers.Ridley.InitialPaletteWords;
    public const int InitialColorCount = 32;
    public const int InitialCgramIndex = 0x140 / 2;

    /// <summary>Fifteen nonzero fixed-bank source pointers and a zero terminator at $A6:A4EB.</summary>
    public const int RevealSourcePointers = EnemyRomTablePointers.Ridley.RevealPaletteSourcePointers;
    public const int RevealRowCount = 15;
    public const int RevealColorCount = 14;
    public const int RevealCgramIndex = 0x00e2 / 2;
}
