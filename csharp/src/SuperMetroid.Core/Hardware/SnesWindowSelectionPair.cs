namespace SuperMetroid.Core.Hardware;

/// <summary>
/// The window selections of two layers that share one selection register: the lower layer (BG1, BG3 or OBJ) and the
/// upper layer (BG2, BG4 or the colour window). Each layer composes its own <see cref="SnesWindowSelection"/> bits.
/// </summary>
[Flags]
public enum SnesWindowSelectionPair : byte
{
    /// <summary>Neither layer selects a window.</summary>
    None = 0,
    /// <summary>The lower layer inverts the first window's interval.</summary>
    LowerInvertFirst = 0x01,
    /// <summary>The lower layer enables the first window.</summary>
    LowerEnableFirst = 0x02,
    /// <summary>The lower layer inverts the second window's interval.</summary>
    LowerInvertSecond = 0x04,
    /// <summary>The lower layer enables the second window.</summary>
    LowerEnableSecond = 0x08,
    /// <summary>The upper layer inverts the first window's interval.</summary>
    UpperInvertFirst = 0x10,
    /// <summary>The upper layer enables the first window.</summary>
    UpperEnableFirst = 0x20,
    /// <summary>The upper layer inverts the second window's interval.</summary>
    UpperInvertSecond = 0x40,
    /// <summary>The upper layer enables the second window.</summary>
    UpperEnableSecond = 0x80,
}
