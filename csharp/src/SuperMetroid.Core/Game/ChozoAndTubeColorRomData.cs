namespace SuperMetroid.Core.Game;

/// <summary>Bank-$AA n00b-tube and Chozo-statue visual palette transfers.</summary>
public static class ChozoAndTubeColorRomData
{
    /// <summary>
    /// The n00b-tube crack initializer $AA:E716 copies sprite palettes one and two,
    /// 32 words beginning at $AA:E2DD, into CGRAM colors 144..175.
    /// </summary>
    public const int TubeCracksSource = 0xaae2dd;

    /// <summary>
    /// Wrecked Ship Chozo initializer $AA:E75C copies sprite palettes one and two,
    /// 32 words beginning at $AA:E31D, into CGRAM colors 144..175.
    /// </summary>
    public const int WreckedShipSource = 0xaae31d;

    /// <summary>
    /// Lower Norfair Chozo initializer $AA:E784 copies sprite palettes one and two,
    /// 32 words beginning at $AA:E35D, into CGRAM colors 144..175.
    /// </summary>
    public const int LowerNorfairSource = 0xaae35d;

    /// <summary>Color-word count in each 64-byte RGB5 image: two complete sixteen-color OBJ palettes, including each row's transparent color-zero entry.</summary>
    public const int ColorCount = 32;
    /// <summary>First CGRAM color-word index, not a byte address: 144 starts OBJ palette 1, and the 32-color transfer covers indices 144 through 175 (OBJ palettes 1 and 2), matching the native target-palette rows.</summary>
    public const int Destination = 144;
}
