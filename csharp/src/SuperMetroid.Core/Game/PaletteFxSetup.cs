namespace SuperMetroid.Core.Game;

/// <summary>Native setup callbacks stored in bank-$8D palette-FX definitions.</summary>
public enum PaletteFxSetup : ushort
{
    /// <summary><c>PaletteFxSetup_Null</c> at $8D:C685.</summary>
    Null = 0xc685,
    /// <summary><c>PaletteFxSetup_Intro</c> at $8D:E204.</summary>
    Intro = 0xe204,
    /// <summary><c>PaletteFxSetup_Norfair</c> at $8D:E440.</summary>
    Norfair = 0xe440,
    /// <summary><c>PaletteFxSetup_Brinstar</c> at $8D:F730.</summary>
    Brinstar = 0xf730,
}
