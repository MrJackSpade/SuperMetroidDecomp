namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>The setup a palette effect runs when it starts.
/// Its values are arbitrary; verification's native view names each member's cartridge word (#1415).
/// </summary>
public enum PaletteFxSetup
{
    /// <summary><c>PaletteFxSetup_Null</c> at $8D:C685.</summary>
    Null = 1,
    /// <summary><c>PaletteFxSetup_Intro</c> at $8D:E204.</summary>
    Intro,
    /// <summary><c>PaletteFxSetup_Norfair</c> at $8D:E440.</summary>
    Norfair,
    /// <summary><c>PaletteFxSetup_Brinstar</c> at $8D:F730.</summary>
    Brinstar,
}
