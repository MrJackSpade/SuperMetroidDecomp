namespace SuperMetroid.Core.Hardware;

/// <summary>
/// Bit assignments of the SNES main-screen designation register <c>TM ($212C)</c>.
/// </summary>
/// <remarks>
/// These are hardware-defined composable bits. Door-transition IRQ handlers write either
/// <see cref="Obj"/> alone or <see cref="Bg1"/> plus <see cref="Obj"/> below the HUD.
/// </remarks>
[Flags]
public enum SnesMainScreenLayers : byte
{
    None = 0,
    Bg1 = 0x01,
    Bg2 = 0x02,
    Bg3 = 0x04,
    Bg4 = 0x08,
    Obj = 0x10,
}
