using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>Authored bank-$A6 palette sources for Lower Norfair Ridley's arena reveal.</summary>
public static class NorfairRidleyPaletteRomData
{

    /// <summary>Thirty-two initial OBJ colors at $A6:E1CF, installed before the hidden palette slots are cleared.</summary>
    public const int InitialColors = EnemyRomTablePointers.Ridley.InitialPaletteWords;
    /// <summary>Thirty-two RGB5 color words copied by the initializer: two complete sixteen-color OBJ palettes, not a byte count.</summary>
    public const int InitialColorCount = 32;

    /// <summary>Fifteen arena-background reveal rows, selected as indices 0..14 every third reveal-AI call; index 15 is the zero-pointer terminator, not color data.</summary>
    public const int RevealRowCount = 15;
    /// <summary>Fourteen RGB5 words per row, covering BG palette 7 colors 1..14 and leaving its color-zero and color-fifteen slots untouched.</summary>
    public const int RevealColorCount = 14;
}
