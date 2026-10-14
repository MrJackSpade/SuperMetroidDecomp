namespace SuperMetroid.Core.Game;

/// <summary>The literal bank-$A8 function pointer stored in Alcoon's common variable zero.</summary>
public enum AlcoonEnemyFunction : ushort
{
    /// <summary>$A8:DD71, <c>Function_Alcoon_WaitForSamusToGetNear</c>: starts the emergence jump when Samus is strictly within 80 horizontal pixels and 32 pixels of the previously discovered landing Y.</summary>
    WaitingForSamus = 0xdd71,
    /// <summary>$A8:DDC6, <c>Function_Alcoon_Emerging_Rising</c>: integrates upward vertical motion with half-pixel acceleration until the whole velocity becomes nonnegative, then selects the falling pose.</summary>
    EmergingRising = 0xddc6,
    /// <summary>$A8:DE05, <c>Function_Alcoon_Emerging_Falling</c>: descends through the terrain collision mover, then faces Samus and begins walking with one walking cycle remaining.</summary>
    EmergingFalling = 0xde05,
    /// <summary>$A8:DE4B, <c>Function_Alcoon_MoveHorizontally_SpitFireballsAtSamus</c>: follows the floor, hides after travelling 112 pixels from spawn, or requests a volley when the step counter is zero and Samus is ahead; animation commands perform horizontal movement.</summary>
    WalkingAndFiring = 0xde4b,
    /// <summary>$A8:DECC, <c>RTL_A8DECC</c>: performs no main-AI movement while the fire-volley animation runs, until its command restores walking.</summary>
    WaitingForFireAnimation = 0xdecc,
    /// <summary>$A8:DECD, <c>Function_Alcoon_Hiding_Rising</c>: integrates the retreat jump's upward motion until acceleration changes its whole velocity to nonnegative.</summary>
    HidingRising = 0xdecd,
    /// <summary>$A8:DEEC, <c>Function_Alcoon_Hiding_Falling</c>: falls without terrain collision until reaching spawn Y, then restores the whole spawn coordinates and proximity-wait state while preserving subpositions.</summary>
    HidingFalling = 0xdeec,
}
