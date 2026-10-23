using SuperMetroid.Core.Escape;

namespace SuperMetroid.Core.Enemies.Drawing;

/// <summary>
/// CGRAM ranges that bosses and enemy actors write directly (#630). Each cell is the colour
/// range named by the native routine; ranges shared between actors alias the hardware palette
/// row that holds them.
/// </summary>
[PpuLayoutBoundary("catalog: enemy and boss CGRAM ranges")]
public static class EnemyPaletteLayout
{

    /// <summary>CGADD $7D-$7E (BG palette 7, colours 13-14): the escape typewriter's source text colours.</summary>
    public static readonly PpuLayoutCell EscapeTypewriterSource = PpuMemoryLayout.Colors(nameof(EscapeTypewriterSource),
        EscapeTypewriterRomData.ColorSource, 2, "Bgr555 BG palette colours");

    /// <summary>CGADD $00-$6F (BG palettes 0-6): the Wrecked Ship room colours faded in at power-on.</summary>
    public static readonly PpuLayoutCell WreckedShipPowerOn = PpuMemoryLayout.Colors(nameof(WreckedShipPowerOn), 0x00, 0x70, "Bgr555 BG palettes");
}
