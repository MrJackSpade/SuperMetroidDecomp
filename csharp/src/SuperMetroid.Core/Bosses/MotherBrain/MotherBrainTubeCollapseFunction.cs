namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Native function pointers stored in the head record's secondary Mother Brain dispatcher.
/// During fake death this independent state machine serializes the physical tube actors,
/// ceiling projectiles, and one-frame bank-$84 room mutations.
/// </summary>
public enum MotherBrainTubeCollapseFunction : ushort
{
    /// <summary><c>$A9:8949 Function_MotherBrainBody_SpawnTubesFallingWhenLessThan4Proj</c>: admits collapse when four projectile slots are free, then requests physical bottom-left tube enemy 0.</summary>
    WaitForFourFreeProjectileSlots = 1,
    /// <summary><c>$A9:896E Function_MotherBrainBody_ClearBottomLeftTube</c>: requests the bank-$84 removal at block (5,9) and seeds the 32-count top-right delay.</summary>
    ClearBottomLeftTube,
    /// <summary><c>$A9:8983 Function_MotherBrainBody_SpawnTopRightTubeFallingProjectile</c>: after timer underflow requests the ceiling projectile at pixel (152,47).</summary>
    SpawnTopRightTube,
    /// <summary><c>$A9:89A0 Function_MotherBrainBody_ClearCeilingBlockColumn9</c>: clears ceiling block (9,2) and seeds the next 32-count tube delay.</summary>
    ClearCeilingColumn9,
    /// <summary><c>$A9:89B5 Function_MotherBrainBody_SpawnTopLeftTubeFallingProjectile</c>: after timer underflow requests the ceiling projectile at pixel (104,47).</summary>
    SpawnTopLeftTube,
    /// <summary><c>$A9:89D2 Function_MotherBrainBody_ClearCeilingBlockColumn6</c>: clears ceiling block (6,2), selects the next physical tube, and seeds its shared timer.</summary>
    ClearCeilingColumn6,
    /// <summary><c>$A9:89E7 Function_MotherBrainBody_SpawnTubesFalling1</c>: requests physical bottom-right tube enemy 1, independently of the ceiling-projectile pool.</summary>
    SpawnBottomRightTube,
    /// <summary><c>$A9:89FA Function_MotherBrainBody_ClearBottomRightTube</c>: requests bottom-right removal at block (10,9) and selects physical tube enemy 2.</summary>
    ClearBottomRightTube,
    /// <summary><c>$A9:8A0F Function_MotherBrainBody_SpawnTubesFalling2</c>: requests physical bottom-middle-left tube enemy 2.</summary>
    SpawnBottomMiddleLeftTube,
    /// <summary><c>$A9:8A22 Function_MotherBrainBody_ClearBottomMiddleLeftTube</c>: removes the side tube at block (6,10) and starts the 32-count upper-middle-left delay.</summary>
    ClearBottomMiddleLeftTube,
    /// <summary><c>$A9:8A37 Function_MotherBrainBody_SpawnTopMiddleLeftTubeFallingProj</c>: after timer underflow requests the ceiling projectile at pixel (120,59).</summary>
    SpawnTopMiddleLeftTube,
    /// <summary><c>$A9:8A54 Function_MotherBrainBody_ClearCeilingTubeColumn7</c>: clears ceiling tube (7,2) and seeds the 32-count upper-middle-right delay.</summary>
    ClearCeilingColumn7,
    /// <summary><c>$A9:8A69 Function_MotherBrainBody_SpawnTopMiddleRightTubeFallingProj</c>: after timer underflow requests the ceiling projectile at pixel (136,59).</summary>
    SpawnTopMiddleRightTube,
    /// <summary><c>$A9:8A86 Function_MotherBrainBody_ClearCeilingTubeColumn8</c>: clears ceiling tube (8,2) and selects physical tube enemy 3.</summary>
    ClearCeilingColumn8,
    /// <summary><c>$A9:8A9B Function_MotherBrainBody_SpawnTubesFalling3</c>: requests physical bottom-middle-right tube enemy 3.</summary>
    SpawnBottomMiddleRightTube,
    /// <summary><c>$A9:8AAE Function_MotherBrainBody_ClearBottomMiddleRightTube</c>: removes the side tube at block (9,10) and seeds the two-count main-tube delay.</summary>
    ClearBottomMiddleRightTube,
    /// <summary><c>$A9:8AC3 Function_MotherBrainBody_SpawnTubesFalling4</c>: after timer underflow requests the main falling-tube enemy whose descent carries the head into ascent setup.</summary>
    SpawnMainTube,
    /// <summary><c>$A9:8AD6 Function_MotherBrainBody_ClearBottomMiddleTubes</c>: removes the remaining tubes at block (7,7) and installs its own RTS as the terminal subfunction.</summary>
    ClearBottomMiddleTubes,
    /// <summary><c>$A9:8AE4 Function_MotherBrainBody_ClearBottomMiddleTubes.return</c>: inert tube-subfunction endpoint while physical falling actors continue independently.</summary>
    Finished,
}
