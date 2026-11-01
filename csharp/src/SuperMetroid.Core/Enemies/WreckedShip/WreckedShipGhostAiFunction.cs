namespace SuperMetroid.Core.Enemies.WreckedShip;

/// <summary>
/// The routine the Wrecked Ship ghost's main AI runs.
/// Its values are arbitrary.
/// </summary>
public enum WreckedShipGhostAiFunction
{
    /// <summary>Raises the palette toward white while advancing the flicker table.</summary>
    BrighteningAndFlickering = 1,
    /// <summary>Fades from white to the installed ghost palette while flickering.</summary>
    FadingToGhostPalette,
    /// <summary>Waits for the post-visible fade to reach white before hiding.</summary>
    WaitingForWhiteFade,
    /// <summary>Oscillates vertically during the vulnerable visible interval.</summary>
    BobbingWhileVisible,
    /// <summary>Counts down the invisible delay before Samus tracking begins.</summary>
    InitialInvisibleDelay,
    /// <summary>Observes Samus movement to select one of nine relative spawn positions.</summary>
    TrackingSamusForSpawn,
}
