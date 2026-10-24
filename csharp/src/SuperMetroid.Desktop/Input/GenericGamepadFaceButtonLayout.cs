namespace SuperMetroid.Desktop.Input;

/// <summary>
/// Identifies how a host driver numbers the four physical face buttons before they are
/// converted to the SNES controller word.
/// </summary>
/// <remarks>
/// This is deliberately a host-input concern, not an in-game control binding. Super
/// Metroid still receives literal SNES X/A/B/Y bits and applies its configurable action
/// bindings afterward. Some generic SNES USB adapters enumerate buttons by their printed
/// labels instead of by the conventional DirectInput diamond positions.
/// </remarks>
internal enum GenericGamepadFaceButtonLayout
{
    /// <summary>WinMM buttons 1-4 are south, east, west, north: SNES B, A, Y, X.</summary>
    Positional,

    /// <summary>WinMM buttons 1-4 are printed X, A, B, Y on VID 0079 / PID 0011 pads.</summary>
    SnesUsbAdapter0079_0011,
}
