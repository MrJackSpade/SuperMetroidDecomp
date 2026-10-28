namespace SuperMetroid.Core.Enemies.WreckedShip;

/// <summary>
/// The routine the Wrecked Ship ghost's main AI runs.
/// Its values are arbitrary; verification's native view names each member's cartridge routine (#1412).
/// </summary>
public enum WreckedShipGhostAiFunction
{
    /// <summary>$A8:9B42 raises the palette toward white while advancing the flicker table.</summary>
    BrighteningAndFlickering = 1,
    /// <summary>$A8:9BAD fades from white to the installed ghost palette while flickering.</summary>
    FadingToGhostPalette,
    /// <summary>$A8:9C69 waits for the post-visible fade to reach white before hiding.</summary>
    WaitingForWhiteFade,
    /// <summary>$A8:9C8A oscillates vertically during the vulnerable visible interval.</summary>
    BobbingWhileVisible,
    /// <summary>$A8:9D13 counts down the invisible delay before Samus tracking begins.</summary>
    InitialInvisibleDelay,
    /// <summary>$A8:9D36 observes Samus movement to select one of nine relative spawn positions.</summary>
    TrackingSamusForSpawn,
}
