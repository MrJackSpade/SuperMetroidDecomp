namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// The steps of the tubes' collapse, run beside Mother Brain's own during her fake death: the falling tubes, the
/// ceiling's falling pieces and the room's blocks they clear, one after another.
/// </summary>
public enum MotherBrainTubeCollapseStep
{
    /// <summary>Admits collapse when four enemy projectiles can still be placed, then requests the bottom-left tube's fall.</summary>
    WaitForFourFreeProjectileSlots,
    /// <summary>Requests the removal at block (5,9) and seeds the 32-count top-right delay.</summary>
    ClearBottomLeftTube,
    /// <summary>After its wait runs out, requests the ceiling projectile at pixel (152,47).</summary>
    SpawnTopRightTube,
    /// <summary>Clears ceiling block (9,2) and seeds the next 32-count tube delay.</summary>
    ClearCeilingColumn9,
    /// <summary>After its wait runs out, requests the ceiling projectile at pixel (104,47).</summary>
    SpawnTopLeftTube,
    /// <summary>Clears ceiling block (6,2), selects the next falling tube, and starts its wait.</summary>
    ClearCeilingColumn6,
    /// <summary>Requests the bottom-right falling tube, whatever room the ceiling's pieces leave.</summary>
    SpawnBottomRightTube,
    /// <summary>Requests the bottom-right removal at block (10,9) and selects the bottom-middle-left falling tube.</summary>
    ClearBottomRightTube,
    /// <summary>Requests the bottom-middle-left falling tube.</summary>
    SpawnBottomMiddleLeftTube,
    /// <summary>Removes the side tube at block (6,10) and starts the 32-count upper-middle-left delay.</summary>
    ClearBottomMiddleLeftTube,
    /// <summary>After its wait runs out, requests the ceiling projectile at pixel (120,59).</summary>
    SpawnTopMiddleLeftTube,
    /// <summary>Clears ceiling tube (7,2) and seeds the 32-count upper-middle-right delay.</summary>
    ClearCeilingColumn7,
    /// <summary>After its wait runs out, requests the ceiling projectile at pixel (136,59).</summary>
    SpawnTopMiddleRightTube,
    /// <summary>Clears ceiling tube (8,2) and selects the bottom-middle-right tube.</summary>
    ClearCeilingColumn8,
    /// <summary>Requests the bottom-middle-right tube.</summary>
    SpawnBottomMiddleRightTube,
    /// <summary>Removes the side tube at block (9,10) and seeds the two-count main-tube delay.</summary>
    ClearBottomMiddleRightTube,
    /// <summary>After its wait runs out, requests the main falling-tube enemy whose descent carries the head into ascent setup.</summary>
    SpawnMainTube,
    /// <summary>Removes the remaining tubes at block (7,7) and finishes the collapse.</summary>
    ClearBottomMiddleTubes,
    /// <summary>The finished collapse, which does nothing while the falling tubes fall on their own.</summary>
    Finished,
}
