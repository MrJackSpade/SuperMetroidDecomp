namespace SuperMetroid.Core.Gameplay;

/// <summary>Typed view of the fresh-game palette writes (#630).</summary>
[PpuLayoutBoundary("rendering view: fresh-game palette writes")]
public readonly struct NewGamePalettes(SnesCgram cgram)
{
    /// <summary>Clears the Ceres elevator concealer colour to black, as <c>$82:8190</c> does.</summary>
    public void ClearCeresElevatorConcealer() => new CgramPalette(cgram, NewGamePaletteLayout.CeresElevatorConcealer).Write(0, Bgr555.Black);
}
