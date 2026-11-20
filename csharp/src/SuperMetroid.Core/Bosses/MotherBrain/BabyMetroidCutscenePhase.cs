namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The phases of the Baby Metroid's entrance.</summary>
public abstract record BabyMetroidCutscenePhase
{
    /// <summary>Allows only the cases below.</summary>
    private BabyMetroidCutscenePhase()
    {
    }

    /// <summary>No Baby Metroid cutscene phase is active.</summary>
    public sealed record Inactive : BabyMetroidCutscenePhase;

    /// <summary>
    /// The Baby has deleted itself at the end of <see cref="Flying.FinalCutscene"/>: the cutscene is over, the room frees its
    /// place, and Mother Brain's rings no longer look for it. It never steps again.
    /// </summary>
    public sealed record Deleted : BabyMetroidCutscenePhase;

    /// <summary>The phases in which the cutscene Baby Metroid flies its course, from its dash onto the screen until it is deleted.</summary>
    public abstract record Flying : BabyMetroidCutscenePhase
    {
        /// <summary>Allows only the cases below.</summary>
        private Flying()
        {
        }

        /// <summary>Moves the Baby rapidly into the room until it reaches the first scripted X threshold.</summary>
        public sealed record DashOntoScreen : Flying;

        /// <summary>Curves the Baby toward Mother Brain's head using the angle and speed updates.</summary>
        public sealed record CurveTowardMotherBrainHead : Flying;

        /// <summary>Closes the final gap to Mother Brain before latching onto her.</summary>
        public sealed record GetRightUpInMotherBrainsFace : Flying;

        /// <summary>Anchors the Baby to Mother Brain's head and queues the latch sound edge.</summary>
        public sealed record LatchOntoMotherBrain : Flying;

        /// <summary>Requests Mother Brain's stumble-back body phase before draining begins.</summary>
        public sealed record SetMotherBrainToStumbleBack : Flying;

        /// <summary>Activates Mother Brain's body and rainbow-beam owners for the energy-drain sequence.</summary>
        public sealed record ActivateRainbowBeamAndMotherBrainBody : Flying;

        /// <summary>Retains the drain attachment until Mother Brain's body phase reaches her corpse.</summary>
        public sealed record WaitForMotherBrainToTurnToCorpse : Flying;

        /// <summary>Stops transferring energy from Mother Brain and starts the release delay.</summary>
        public sealed record StopDraining : Flying;

        /// <summary>Detaches from Mother Brain and emits the three scripted dust-cloud projectiles.</summary>
        public sealed record LetGoAndSpawnDustClouds : Flying;

        /// <summary>Moves the Baby upward to its ceiling waypoint before it crosses toward Samus.</summary>
        public sealed record MoveToTheCeiling : Flying;

        /// <summary>Flies from the ceiling waypoint to Samus's scripted attachment point.</summary>
        public sealed record MoveToSamus : Flying;

        /// <summary>Anchors the Baby to Samus and begins transferring drained energy to her.</summary>
        public sealed record LatchOntoSamus : Flying;

        /// <summary>Raises Samus's energy toward the full-health target while the Baby remains attached.</summary>
        public sealed record HealSamusToFullHealth : Flying;

        /// <summary>Holds the attachment until the Baby's transfer reserve reaches zero.</summary>
        public sealed record IdleUntilNoHealth : Flying;

        /// <summary>Detaches the depleted Baby from Samus and restores her post-heal state.</summary>
        public sealed record ReleaseSamus : Flying;

        /// <summary>Turns the Baby toward Mother Brain for the pause before its return attack.</summary>
        public sealed record StareDownMotherBrain : Flying;

        /// <summary>Flies the Baby up to the point just past the screen's right edge, to set up the final charge.</summary>
        public sealed record FlyToRightEdge : Flying;

        /// <summary>Moves to the off-screen waypoint from which the final charge is launched.</summary>
        public sealed record MoveToFinalChargeStart : Flying;

        /// <summary>Initializes the final-charge velocity, animation, and attack presentation.</summary>
        public sealed record InitiateFinalCharge : Flying;

        /// <summary>Advances the charge toward Mother Brain until the fatal projectile collision edge.</summary>
        public sealed record FinalCharge : Flying;

        /// <summary>Applies Mother Brain's killing blow and runs the Baby's impact-shake delay.</summary>
        public sealed record TakeFinalBlow : Flying;

        /// <summary>Waits out the post-impact timer, then queues Samus's theme.</summary>
        public sealed record PlaySamusTheme : Flying;

        /// <summary>Transfers the Hyper Beam setup to Samus while preparing the Baby's death fall.</summary>
        public sealed record PrepareSamusForHyperBeam : Flying;

        /// <summary>Runs the falling, colour-fading death sequence and periodic dust explosions.</summary>
        public sealed record DeathSequence : Flying;

        /// <summary>
        /// Pauses, Samus's rainbow cycling, once the Baby's body has left the active cutscene; the pause's last update is the
        /// first of letting her rainbow some more.
        /// </summary>
        public sealed record PauseAfterFade : Flying;

        /// <summary>Keeps Samus's restored rainbow colours cycling through the remaining scripted delay.</summary>
        public sealed record LetSamusRainbowSomeMore : Flying;

        /// <summary>Hands control to the final Mother Brain combat/cutscene state after the Baby sequence completes.</summary>
        public sealed record FinalCutscene : Flying;
    }
}
