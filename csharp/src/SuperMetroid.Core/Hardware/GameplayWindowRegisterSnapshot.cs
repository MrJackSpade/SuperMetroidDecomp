namespace SuperMetroid.Core.Hardware;

/// <summary>
/// Uploaded window and screen-designation values, including bits no layer uses. Layer renderers may mask
/// those bits, but a later native read or overwrite must retain the uploaded values.
/// </summary>
/// <param name="Windows">The uploaded window registers.</param>
/// <param name="MainScreen">The uploaded main-screen designation (TM), including any unused bits.</param>
/// <param name="MainScreenWindow">The uploaded main-screen window mask (TMW), including any unused bits.</param>
public readonly record struct GameplayWindowRegisterSnapshot(
    SnesWindowRegisters Windows, SnesMainScreenLayers MainScreen, SnesMainScreenLayers MainScreenWindow);
