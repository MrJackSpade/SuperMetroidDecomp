namespace SuperMetroid.Core.Fx;

/// <summary>
/// The colour-effect state room loading never clears: whether the effects run, and the Norfair heat phases. Each room's
/// <see cref="RoomPaletteFxSystem"/> reads and writes them here.
/// </summary>
public sealed class PaletteFxSessionState
{
    /// <summary>
    /// Whether the colour effects run, checked first each update. Loading a game turns them on; the escape timer's
    /// expiry turns them off.
    /// </summary>
    public bool HandlerEnabled { get; private set; } = true;

    /// <summary>
    /// The heat phase the Norfair glow published, which Samus's heat shimmer, taking its turn before the glow, follows on
    /// the next update. Only compared for equality and used to select a heat program, so it is held as a plain phase
    /// number.
    /// </summary>
    internal int SamusInHeatPaletteIndex { get; set; }

    /// <summary>The heat phase Samus's heat shimmer last restarted on; it restarts only when the published phase differs.</summary>
    internal int PreviousSamusInHeatPaletteIndex { get; set; }

    /// <summary>Stops the colour effects from running.</summary>
    public void DisableHandler() => HandlerEnabled = false;
}
