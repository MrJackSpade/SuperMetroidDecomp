namespace SuperMetroid.Core.Game;

/// <summary>
/// Bank-$A8 function words stored in the Wrecked Ship ghost's native enemy variable A.
/// Keeping the cartridge addresses makes debugger watches directly comparable with WRAM.
/// </summary>
public enum WreckedShipGhostAiFunction : ushort
{
    /// <summary>$A8:9B42 raises the palette toward white while advancing the flicker table.</summary>
    BrighteningAndFlickering = 0x9b42,
    /// <summary>$A8:9BAD fades from white to the installed ghost palette while flickering.</summary>
    FadingToGhostPalette = 0x9bad,
    /// <summary>$A8:9C69 waits for the post-visible fade to reach white before hiding.</summary>
    WaitingForWhiteFade = 0x9c69,
    /// <summary>$A8:9C8A oscillates vertically during the vulnerable visible interval.</summary>
    BobbingWhileVisible = 0x9c8a,
    /// <summary>$A8:9D13 counts down the invisible delay before Samus tracking begins.</summary>
    InitialInvisibleDelay = 0x9d13,
    /// <summary>$A8:9D36 observes Samus movement to select one of nine relative spawn positions.</summary>
    TrackingSamusForSpawn = 0x9d36,
}
