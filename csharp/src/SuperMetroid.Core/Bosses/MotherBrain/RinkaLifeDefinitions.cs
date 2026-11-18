namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>A Rinka's life (#2171): how long it waits before it flies at Samus, how fast it flies and the health it returns with.</summary>
internal static class RinkaLifeDefinitions
{
    /// <summary>The updates a newly fired Rinka waits before it aims at Samus, the last being the update it aims on.</summary>
    internal const int AimDelayUpdates = 27;

    /// <summary>
    /// The updates a killed special Rinka, one of Mother Brain's spawn-point Rinkas, waits hidden before it respawns, the
    /// last being the update it respawns on.
    /// </summary>
    internal const int RespawnDelayUpdates = 2;

    /// <summary>The health a special Rinka, one of Mother Brain's spawn-point Rinkas, returns with after its death delay.</summary>
    internal const int RespawnHealth = 10;

    /// <summary>How far past each of the screen's edges a Rinka flies before it leaves its flight rectangle.</summary>
    internal const int FlightMargin = 16;

    /// <summary>The width of a Rinka's flight rectangle: the screen's, widened by its margin on both sides.</summary>
    internal const int FlightWindowWidth = ScreenGeometry.ScreenWidthPixels + 2 * FlightMargin;

    /// <summary>The height of a Rinka's flight rectangle: the screen's, widened by its margin above and below.</summary>
    internal const int FlightWindowHeight = ScreenGeometry.ScreenHeightPixels + 2 * FlightMargin;

    /// <summary>The speed a Rinka flies at along its aim: a pixel and an eighth an update.</summary>
    internal static readonly SignedEightEight Speed = SignedEightEight.FromParts(1, 32);
}
