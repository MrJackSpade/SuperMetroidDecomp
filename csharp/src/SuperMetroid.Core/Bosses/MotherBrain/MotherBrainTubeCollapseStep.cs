namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// The steps of the collapse of the tubes around Mother Brain's tank during her fake death, run beside her own: each tube
/// falls in turn, and the room is cleared where it stood. The steps name the tube each drops or clears (#2258).
/// </summary>
public enum MotherBrainTubeCollapseStep
{
    /// <summary>Waits until four enemy projectiles can be placed, then drops the bottom-left tube.</summary>
    SpawnBottomLeftTube,
    /// <summary>Clears the bottom-left tube from the room and starts the wait before the top-right tube falls.</summary>
    ClearBottomLeftTube,
    /// <summary>Once its wait runs out, drops the top-right tube from the ceiling.</summary>
    SpawnTopRightTube,
    /// <summary>Clears the ceiling the top-right tube hung from and starts the wait before the top-left tube falls.</summary>
    ClearTopRightTube,
    /// <summary>Once its wait runs out, drops the top-left tube from the ceiling.</summary>
    SpawnTopLeftTube,
    /// <summary>Clears the ceiling the top-left tube hung from and starts the wait before the bottom-right tube falls.</summary>
    ClearTopLeftTube,
    /// <summary>Once its wait runs out, drops the bottom-right tube.</summary>
    SpawnBottomRightTube,
    /// <summary>Clears the bottom-right tube from the room and starts the wait before the bottom-middle-left tube falls.</summary>
    ClearBottomRightTube,
    /// <summary>Once its wait runs out, drops the bottom-middle-left tube.</summary>
    SpawnBottomMiddleLeftTube,
    /// <summary>Clears the bottom-middle-left tube from the room and starts the wait before the top-middle-left tube falls.</summary>
    ClearBottomMiddleLeftTube,
    /// <summary>Once its wait runs out, drops the top-middle-left tube from the ceiling.</summary>
    SpawnTopMiddleLeftTube,
    /// <summary>Clears the top-middle-left tube from the ceiling and starts the wait before the top-middle-right tube falls.</summary>
    ClearTopMiddleLeftTube,
    /// <summary>Once its wait runs out, drops the top-middle-right tube from the ceiling.</summary>
    SpawnTopMiddleRightTube,
    /// <summary>Clears the top-middle-right tube from the ceiling and starts the wait before the bottom-middle-right tube falls.</summary>
    ClearTopMiddleRightTube,
    /// <summary>Once its wait runs out, drops the bottom-middle-right tube.</summary>
    SpawnBottomMiddleRightTube,
    /// <summary>Clears the bottom-middle-right tube from the room and starts the short wait before the main tube falls.</summary>
    ClearBottomMiddleRightTube,
    /// <summary>Once its wait runs out, drops the main tube, whose fall carries her brain down into the ascent.</summary>
    SpawnMainTube,
    /// <summary>Clears the main tube's place in the room: the two middle tubes under her tank.</summary>
    ClearBottomMiddleTubes,
    /// <summary>The collapse has dropped every tube; the falling tubes finish their falls on their own.</summary>
    Finished,
}
