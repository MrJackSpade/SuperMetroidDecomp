namespace SuperMetroid.Core.Fx;

/// <summary>
/// The palette-FX words room loading never clears: the handler enable bit and the Norfair heat palette phases. Each
/// room's <see cref="RoomPaletteFxSystem"/> reads and writes them here.
/// </summary>
public sealed class PaletteFxSessionState
{
    /// <summary>
    /// Bit 15 of <c>PaletteFXObject_Enable</c> ($1E79), checked first by the handler. Game
    /// data loading sets it; the escape timer's expiry (<c>$90:E106</c>) clears it.
    /// </summary>
    public bool HandlerEnabled { get; private set; } = true;

    /// <summary>
    /// Heat palette phase published by the Norfair object for the next descending-slot pass. Only compared for
    /// equality and used to select a heat program, so it is held as a plain phase number.
    /// </summary>
    internal int SamusInHeatPaletteIndex { get; set; }

    /// <summary>Last heat palette phase applied to Samus; prevents redundant program restarts.</summary>
    internal int PreviousSamusInHeatPaletteIndex { get; set; }

    /// <summary><c>Disable_PaletteFXObjects</c> ($8D:C4CD).</summary>
    public void DisableHandler() => HandlerEnabled = false;
}
