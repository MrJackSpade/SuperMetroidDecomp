namespace SuperMetroid.Core.Hardware;

/// <summary>Composable bits of one layer's nibble in W12SEL, W34SEL or WOBJSEL.</summary>
[Flags]
public enum SnesWindowSelection : byte
{
    /// <summary>Nibble bit 0 inverts the first window's inclusive interval.</summary>
    InvertFirst = 1,
    /// <summary>Nibble bit 1 enables the first window.</summary>
    EnableFirst = 2,
    /// <summary>Nibble bit 2 inverts the second window's inclusive interval.</summary>
    InvertSecond = 4,
    /// <summary>Nibble bit 3 enables the second window.</summary>
    EnableSecond = 8,
}

/// <summary>Two-bit WBGLOG/WOBJLOG operation when both windows are enabled.</summary>
public enum SnesWindowLogic : byte
{
    /// <summary>Includes a pixel when either enabled window includes it.</summary>
    Or = 0,
    /// <summary>Includes a pixel only when both enabled windows include it.</summary>
    And = 1,
    /// <summary>Includes a pixel when exactly one of the two enabled windows includes it.</summary>
    Xor = 2,
    /// <summary>Includes a pixel when both enabled windows agree, including pixels outside both.</summary>
    Xnor = 3,
}

/// <summary>Hardware ordering of window targets across W12SEL/W34SEL/WOBJSEL.</summary>
public enum SnesWindowTarget : byte
{
    /// <summary>BG1 target, using the low W12SEL nibble and WBGLOG bits 0 and 1.</summary>
    Bg1 = 0,
    /// <summary>BG2 target, using the high W12SEL nibble and WBGLOG bits 2 and 3.</summary>
    Bg2 = 1,
    /// <summary>BG3 target, using the low W34SEL nibble and WBGLOG bits 4 and 5.</summary>
    Bg3 = 2,
    /// <summary>BG4 target, using the high W34SEL nibble and WBGLOG bits 6 and 7.</summary>
    Bg4 = 3,
    /// <summary>OBJ target, using the low WOBJSEL nibble and WOBJLOG bits 0 and 1.</summary>
    Obj = 4,
    /// <summary>Color-window target, using the high WOBJSEL nibble and WOBJLOG bits 2 and 3.</summary>
    ColorMath = 5,
}
