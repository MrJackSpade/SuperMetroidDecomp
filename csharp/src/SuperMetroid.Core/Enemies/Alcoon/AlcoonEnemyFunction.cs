namespace SuperMetroid.Core.Enemies.Alcoon;

/// <summary>
/// The routine Alcoon's main AI runs each frame.
/// Its values are arbitrary; verification's native view names each member's cartridge routine (#1412).
/// </summary>
public enum AlcoonEnemyFunction : ushort
{
    /// <summary>$A8:DD71, <c>Function_Alcoon_WaitForSamusToGetNear</c>: starts the emergence jump when Samus is strictly within 80 horizontal pixels and 32 pixels of the previously discovered landing Y.</summary>
    WaitingForSamus = 1,
    /// <summary>$A8:DDC6, <c>Function_Alcoon_Emerging_Rising</c>: integrates upward vertical motion with half-pixel acceleration until the whole velocity becomes nonnegative, then selects the falling pose.</summary>
    EmergingRising,
    /// <summary>$A8:DE05, <c>Function_Alcoon_Emerging_Falling</c>: descends through the terrain collision mover, then faces Samus and begins walking with one walking cycle remaining.</summary>
    EmergingFalling,
    /// <summary>$A8:DE4B, <c>Function_Alcoon_MoveHorizontally_SpitFireballsAtSamus</c>: follows the floor, hides after travelling 112 pixels from spawn, or requests a volley when the step counter is zero and Samus is ahead; animation commands perform horizontal movement.</summary>
    WalkingAndFiring,
    /// <summary>$A8:DECC, <c>RTL_A8DECC</c>: performs no main-AI movement while the fire-volley animation runs, until its command restores walking.</summary>
    WaitingForFireAnimation,
    /// <summary>$A8:DECD, <c>Function_Alcoon_Hiding_Rising</c>: integrates the retreat jump's upward motion until acceleration changes its whole velocity to nonnegative.</summary>
    HidingRising,
    /// <summary>$A8:DEEC, <c>Function_Alcoon_Hiding_Falling</c>: falls without terrain collision until reaching spawn Y, then restores the whole spawn coordinates and proximity-wait state while preserving subpositions.</summary>
    HidingFalling,
}
