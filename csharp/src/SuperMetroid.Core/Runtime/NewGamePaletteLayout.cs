using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Runtime;

/// <summary>The palette entries fresh-game loading writes outside the ordinary palette copies (#630).</summary>
[PpuLayoutBoundary("catalog: fresh-game palette writes")]
public static class NewGamePaletteLayout
{
    /// <summary>
    /// CGADD $DF (OBJ palette 5, colour 15): the opaque pixel of common OBJ tile $20 that conceals
    /// the Ceres elevator recess; <c>$82:8190</c> clears it before the elevator projectiles spawn.
    /// </summary>
    public static readonly PpuLayoutCell CeresElevatorConcealer = PpuMemoryLayout.Colors(nameof(CeresElevatorConcealer), 0xdf, 1,
        "Bgr555 colour");
}

/// <summary>Typed view of the fresh-game palette writes (#630).</summary>
[PpuLayoutBoundary("rendering view: fresh-game palette writes")]
public readonly struct NewGamePalettes(SnesCgram cgram)
{
    /// <summary>Clears the Ceres elevator concealer colour to black, as <c>$82:8190</c> does.</summary>
    public void ClearCeresElevatorConcealer() => new CgramPalette(cgram, NewGamePaletteLayout.CeresElevatorConcealer).Write(0, Bgr555.Black);
}
