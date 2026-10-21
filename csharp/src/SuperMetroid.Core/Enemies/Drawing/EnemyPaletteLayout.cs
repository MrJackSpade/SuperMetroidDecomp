using SuperMetroid.Core.Enemies.Chozo;
using SuperMetroid.Core.Enemies.SpacePirates;
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
    /// <summary>CGADD $F0-$FF (OBJ palette 7): the gold Space Pirate colours the ninja Pirate copies on spawn.</summary>
    public static readonly PpuLayoutCell NinjaSpacePirate = PpuMemoryLayout.Colors(nameof(NinjaSpacePirate),
        NinjaSpacePiratePaletteDefinitions.TargetColor, PpuMemoryLayout.ColorsPerRow, "Bgr555 OBJ palette");

    /// <summary>CGADD $91-$9F (OBJ palette 1, colours 1-15): the dead sidehopper's transformation palette.</summary>
    public static readonly PpuLayoutCell DeadSidehopper = PpuMemoryLayout.Colors(nameof(DeadSidehopper), 0x91, 15, "Bgr555 OBJ palette colours");

    /// <summary>CGADD $7D-$7E (BG palette 7, colours 13-14): the escape typewriter's source text colours.</summary>
    public static readonly PpuLayoutCell EscapeTypewriterSource = PpuMemoryLayout.Colors(nameof(EscapeTypewriterSource),
        EscapeTypewriterRomData.ColorSource, 2, "Bgr555 BG palette colours");

    /// <summary>CGADD $9D-$9E (OBJ palette 1, colours 13-14): where the escape typewriter copies its text colours.</summary>
    public static readonly PpuLayoutCell EscapeTypewriterText = PpuMemoryLayout.Colors(nameof(EscapeTypewriterText),
        EscapeTypewriterRomData.ColorDestination, 2, "Bgr555 OBJ palette colours");

    /// <summary>CGADD $00-$6F (BG palettes 0-6): the Wrecked Ship room colours faded in at power-on.</summary>
    public static readonly PpuLayoutCell WreckedShipPowerOn = PpuMemoryLayout.Colors(nameof(WreckedShipPowerOn), 0x00, 0x70, "Bgr555 BG palettes");

    /// <summary>CGADD $90-$AF: the Chozo statue / glass tube OBJ palettes 1-2 ($AA:E716).</summary>
    public static readonly PpuLayoutCell ChozoAndTube = PpuMemoryLayout.Colors(nameof(ChozoAndTube),
        ChozoAndTubeColorRomData.Destination, ChozoAndTubeColorRomData.ColorCount, "Bgr555 OBJ palettes 1-2");
}
