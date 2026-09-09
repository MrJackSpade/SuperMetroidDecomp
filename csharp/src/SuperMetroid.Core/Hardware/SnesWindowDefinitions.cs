namespace SuperMetroid.Core.Hardware;

/// <summary>Composable bits of one layer's nibble in W12SEL, W34SEL or WOBJSEL.</summary>
[Flags]
public enum SnesWindowSelection : byte
{
    None = 0,
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
    Or = 0,
    And = 1,
    Xor = 2,
    Xnor = 3,
}
