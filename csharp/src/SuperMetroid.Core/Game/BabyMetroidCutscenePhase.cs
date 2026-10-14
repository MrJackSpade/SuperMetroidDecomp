namespace SuperMetroid.Core.Game;

/// <summary>Named equivalents of the entrance's bank-$A9 function pointers.</summary>
public enum BabyMetroidCutscenePhase
{
    /// <summary>No Baby Metroid cutscene function is active.</summary>
    Inactive,

    /// <summary>Moves the Baby rapidly into the room until it reaches the first scripted X threshold.</summary>
    DashOntoScreen,

    /// <summary>Curves the Baby toward Mother Brain's head using the native angle and speed updates.</summary>
    CurveTowardMotherBrainHead,

    /// <summary>Closes the final gap to Mother Brain before the attachment state begins.</summary>
    GetRightUpInMotherBrainsFace,

    /// <summary>Anchors the Baby to Mother Brain's head and queues the latch sound edge.</summary>
    LatchOntoMotherBrain,

    /// <summary>Requests Mother Brain's stumble-back body function before draining begins.</summary>
    SetMotherBrainToStumbleBack,

    /// <summary>Activates Mother Brain's body and rainbow-beam owners for the energy-drain sequence.</summary>
    ActivateRainbowBeamAndMotherBrainBody,

    /// <summary>Retains the drain attachment until Mother Brain's body function reaches its corpse state.</summary>
    WaitForMotherBrainToTurnToCorpse,

    /// <summary>Stops transferring energy from Mother Brain and starts the release delay.</summary>
    StopDraining,

    /// <summary>Detaches from Mother Brain and emits the three scripted dust-cloud projectiles.</summary>
    LetGoAndSpawnDustClouds,

    /// <summary>Moves the Baby upward to its ceiling waypoint before it crosses toward Samus.</summary>
    MoveToTheCeiling,

    /// <summary>Flies from the ceiling waypoint to Samus's scripted attachment point.</summary>
    MoveToSamus,

    /// <summary>Anchors the Baby to Samus and begins transferring drained energy to her.</summary>
    LatchOntoSamus,

    /// <summary>Raises Samus's energy toward the native full-health target while the Baby remains attached.</summary>
    HealSamusToFullHealth,

    /// <summary>Holds the attachment until the Baby's transfer reserve reaches zero.</summary>
    IdleUntilNoHealth,

    /// <summary>Detaches the depleted Baby from Samus and restores her post-heal state.</summary>
    ReleaseSamus,

    /// <summary>Turns the Baby toward Mother Brain for the pause before its return attack.</summary>
    StareDownMotherBrain,

    /// <summary>Moves the Baby beyond the visible playfield to set up the final charge.</summary>
    FlyOffScreen,

    /// <summary>Moves to the off-screen waypoint from which the final charge is launched.</summary>
    MoveToFinalChargeStart,

    /// <summary>Initializes the final-charge velocity, animation, and attack presentation.</summary>
    InitiateFinalCharge,

    /// <summary>Advances the charge toward Mother Brain until the fatal projectile collision edge.</summary>
    FinalCharge,

    /// <summary>Applies Mother Brain's killing blow and runs the Baby's impact-shake delay.</summary>
    TakeFinalBlow,

    /// <summary>Waits out the post-impact timer, then queues Samus's theme.</summary>
    PlaySamusTheme,

    /// <summary>Transfers the Hyper Beam setup to Samus while preparing the Baby's death fall.</summary>
    PrepareSamusForHyperBeam,

    /// <summary>Runs the falling, palette-fading death sequence and periodic dust explosions.</summary>
    DeathSequence,

    /// <summary>Removes the Baby's sprite tiles after its body leaves the active cutscene.</summary>
    UnloadTiles,

    /// <summary>Keeps Samus's restored rainbow palette cycling through the remaining scripted delay.</summary>
    LetSamusRainbowSomeMore,

    /// <summary>Hands control to the final Mother Brain combat/cutscene state after the Baby sequence completes.</summary>
    FinalCutscene,
}
