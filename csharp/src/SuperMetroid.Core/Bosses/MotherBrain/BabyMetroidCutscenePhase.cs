namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The phases of the Baby Metroid's entrance.</summary>
public enum BabyMetroidCutscenePhase
{
    /// <summary>No Baby Metroid cutscene phase is active.</summary>
    Inactive,

    /// <summary>Moves the Baby rapidly into the room until it reaches the first scripted X threshold.</summary>
    DashOntoScreen,

    /// <summary>Curves the Baby toward Mother Brain's head using the angle and speed updates.</summary>
    CurveTowardMotherBrainHead,

    /// <summary>Closes the final gap to Mother Brain before latching onto her.</summary>
    GetRightUpInMotherBrainsFace,

    /// <summary>Anchors the Baby to Mother Brain's head and queues the latch sound edge.</summary>
    LatchOntoMotherBrain,

    /// <summary>Requests Mother Brain's stumble-back body phase before draining begins.</summary>
    SetMotherBrainToStumbleBack,

    /// <summary>Activates Mother Brain's body and rainbow-beam owners for the energy-drain sequence.</summary>
    ActivateRainbowBeamAndMotherBrainBody,

    /// <summary>Retains the drain attachment until Mother Brain's body phase reaches her corpse.</summary>
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

    /// <summary>Raises Samus's energy toward the full-health target while the Baby remains attached.</summary>
    HealSamusToFullHealth,

    /// <summary>Holds the attachment until the Baby's transfer reserve reaches zero.</summary>
    IdleUntilNoHealth,

    /// <summary>Detaches the depleted Baby from Samus and restores her post-heal state.</summary>
    ReleaseSamus,

    /// <summary>Turns the Baby toward Mother Brain for the pause before its return attack.</summary>
    StareDownMotherBrain,

    /// <summary>Flies the Baby up to the point just past the screen's right edge, to set up the final charge.</summary>
    FlyToRightEdge,

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

    /// <summary>Runs the falling, colour-fading death sequence and periodic dust explosions.</summary>
    DeathSequence,

    /// <summary>
    /// Pauses, Samus's rainbow cycling, once the Baby's body has left the active cutscene; the pause's last update is the
    /// first of letting her rainbow some more.
    /// </summary>
    PauseAfterFade,

    /// <summary>Keeps Samus's restored rainbow colours cycling through the remaining scripted delay.</summary>
    LetSamusRainbowSomeMore,

    /// <summary>Hands control to the final Mother Brain combat/cutscene state after the Baby sequence completes.</summary>
    FinalCutscene,

    /// <summary>
    /// The Baby has deleted itself at the end of <see cref="FinalCutscene"/>: the cutscene is over, the room frees its
    /// place, and Mother Brain's rings no longer look for it. It never steps again.
    /// </summary>
    Deleted,
}
