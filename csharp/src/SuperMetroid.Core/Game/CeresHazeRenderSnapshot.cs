namespace SuperMetroid.Core.Game;

/// <summary>
/// Value-only render input for the Ceres haze's per-scanline COLDATA gradient. The HDMA object
/// writes its table during the main loop and HDMA reads it while the frame is displayed.
/// </summary>
/// <param name="IsRed">True for the Ridley-dead red ramp, false for the Ridley-alive blue ramp.</param>
/// <param name="Intensity">Last published ramp strength in RGB5 channel units.</param>
public readonly record struct CeresHazeRenderSnapshot(bool IsRed, int Intensity)
{
    /// <summary>Copies an enabled haze owner, or null when the room has none.</summary>
    public static CeresHazeRenderSnapshot? Capture(CeresHazeState haze)
    {
        Ensure.NotNull(haze);
        return haze.Enabled ? new(haze.IsRed, haze.Intensity) : null;
    }
}
