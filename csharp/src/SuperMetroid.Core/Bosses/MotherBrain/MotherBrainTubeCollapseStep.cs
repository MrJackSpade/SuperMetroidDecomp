namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// The steps the head record's secondary Mother Brain dispatcher runs.
/// During fake death this independent state machine serializes the physical tube actors,
/// ceiling projectiles, and one-frame room mutations.
/// </summary>
public enum MotherBrainTubeCollapseStep
{
    /// <summary>Admits collapse when four projectile slots are free, then requests physical bottom-left tube enemy 0.</summary>
    WaitForFourFreeProjectileSlots = 1,
    /// <summary>Requests the removal at block (5,9) and seeds the 32-count top-right delay.</summary>
    ClearBottomLeftTube,
    /// <summary>After the timer passes zero, requests the ceiling projectile at pixel (152,47).</summary>
    SpawnTopRightTube,
    /// <summary>Clears ceiling block (9,2) and seeds the next 32-count tube delay.</summary>
    ClearCeilingColumn9,
    /// <summary>After the timer passes zero, requests the ceiling projectile at pixel (104,47).</summary>
    SpawnTopLeftTube,
    /// <summary>Clears ceiling block (6,2), selects the next physical tube, and seeds its shared timer.</summary>
    ClearCeilingColumn6,
    /// <summary>Requests physical bottom-right tube enemy 1, independently of the ceiling-projectile pool.</summary>
    SpawnBottomRightTube,
    /// <summary>Requests bottom-right removal at block (10,9) and selects physical tube enemy 2.</summary>
    ClearBottomRightTube,
    /// <summary>Requests physical bottom-middle-left tube enemy 2.</summary>
    SpawnBottomMiddleLeftTube,
    /// <summary>Removes the side tube at block (6,10) and starts the 32-count upper-middle-left delay.</summary>
    ClearBottomMiddleLeftTube,
    /// <summary>After the timer passes zero, requests the ceiling projectile at pixel (120,59).</summary>
    SpawnTopMiddleLeftTube,
    /// <summary>Clears ceiling tube (7,2) and seeds the 32-count upper-middle-right delay.</summary>
    ClearCeilingColumn7,
    /// <summary>After the timer passes zero, requests the ceiling projectile at pixel (136,59).</summary>
    SpawnTopMiddleRightTube,
    /// <summary>Clears ceiling tube (8,2) and selects physical tube enemy 3.</summary>
    ClearCeilingColumn8,
    /// <summary>Requests physical bottom-middle-right tube enemy 3.</summary>
    SpawnBottomMiddleRightTube,
    /// <summary>Removes the side tube at block (9,10) and seeds the two-count main-tube delay.</summary>
    ClearBottomMiddleRightTube,
    /// <summary>After the timer passes zero, requests the main falling-tube enemy whose descent carries the head into ascent setup.</summary>
    SpawnMainTube,
    /// <summary>Removes the remaining tubes at block (7,7) and installs its own empty step as the last one.</summary>
    ClearBottomMiddleTubes,
    /// <summary>Inert last tube-collapse step while physical falling actors continue independently.</summary>
    Finished,
}
