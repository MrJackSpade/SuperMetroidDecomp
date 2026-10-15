namespace SuperMetroid.Core.Hardware;

/// <summary>Combinable CGADSUB ($2131) source-enable and arithmetic bits.</summary>
[Flags]
public enum SnesColorMathControl : byte
{
    /// <summary>Disables color arithmetic for every main-screen source.</summary>
    None = 0,
    /// <summary>Applies color arithmetic to BG1 pixels.</summary>
    Bg1 = 1,
    /// <summary>Applies color arithmetic to BG2 pixels.</summary>
    Bg2 = 2,
    /// <summary>Applies color arithmetic to BG3 pixels.</summary>
    Bg3 = 4,
    /// <summary>Applies color arithmetic to eligible OBJ pixels.</summary>
    Obj = 16,
    /// <summary>Applies color arithmetic when the backdrop wins the main screen.</summary>
    Backdrop = 32,
    /// <summary>Halves the color arithmetic result where SNES conditions permit.</summary>
    Half = 64,
    /// <summary>Subtracts the operand instead of adding it.</summary>
    Subtract = 128,
}
