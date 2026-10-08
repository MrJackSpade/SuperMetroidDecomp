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
    /// <summary>Admits no represented background or OBJ layer to the main screen.</summary>
    None = 0,
    /// <summary>TM bit 0 admits background layer 1 to main-screen composition.</summary>
    Bg1 = 0x01,
    /// <summary>TM bit 1 admits background layer 2 to main-screen composition.</summary>
    Bg2 = 0x02,
    /// <summary>TM bit 2 admits background layer 3, including the gameplay HUD, to the main screen.</summary>
    Bg3 = 0x04,
    /// <summary>TM bit 4 admits OBJ sprites to main-screen composition.</summary>
    Obj = 0x10,
}
