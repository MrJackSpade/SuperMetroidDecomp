namespace SuperMetroid.Core.Hardware;

/// <summary>
/// Literal uploaded bytes, including unused high bits. Layer renderers may mask
/// those bits, but a later native read or overwrite must retain the raw values.
/// </summary>
public readonly record struct GameplayWindowRegisterSnapshot(
    SnesWindowRegisters Windows, byte MainScreen,     byte MainScreenWindow);
