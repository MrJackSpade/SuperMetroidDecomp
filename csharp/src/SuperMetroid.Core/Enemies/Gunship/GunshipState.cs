namespace SuperMetroid.Core.Enemies.Gunship;

/// <summary>
/// The gunship's state, one for the whole ship, which its top hull keeps and runs: its phase, landing brakes, bob and
/// engine timers, and its takeoff.
/// </summary>
internal sealed class GunshipState
{
    /// <summary>Starts the ship in <paramref name="phase"/>.</summary>
    internal GunshipState(GunshipPhase phase) => Phase = phase;

    /// <summary>
    /// The updates left of a waiting phase, the one it ends on included: each pad movement starts it at
    /// <see cref="GunshipBoardingDefinitions.PadMovementDuration"/> and the wait for the engines at
    /// <see cref="GunshipTakeoffDefinitions.EngineStartUpdates"/>, and the update that takes it to zero ends the phase (#2340, #2469).
    /// </summary>
    public UpdateCountdown PhaseTimer { get; internal set; } = UpdateCountdown.Ended();

    /// <summary>The ship's next idle-bob step.</summary>
    public GunshipIdleBob Bob { get; internal set; }

    /// <summary>The ship's updates until its next idle-bob step.</summary>
    public UpdateCountdown BobTimer { get; internal set; } = UpdateCountdown.Ended();

    /// <summary>The ship's updates until its next engine sound, the one it sounds on included.</summary>
    public UpdateCountdown EngineSoundTimer { get; internal set; } = UpdateCountdown.Ended();

    /// <summary>The ship's landed room Y, its top hull's, from which Samus's entry and exit heights are measured.</summary>
    public int BaseY { get; internal set; }

    /// <summary>The ship's next landing brake.</summary>
    public GunshipLandingBrake LandingBrake { get; internal set; }

    /// <summary>
    /// The ship's engine rumble, the <see cref="GunshipTakeoffDefinitions.EngineRumbleFrames"/> updates before liftoff,
    /// counted once each: its age chooses each update's shake, and the rumble strengthens at
    /// <see cref="GunshipTakeoffDefinitions.RumbleStrengthensFrame"/>.
    /// </summary>
    public UpdateCountdown Rumble { get; } = UpdateCountdown.Lasting(GunshipTakeoffDefinitions.EngineRumbleFrames);

    /// <summary>The phase the ship runs.</summary>
    public GunshipPhase Phase { get; internal set; }

    /// <summary>
    /// The ship's upward escape speed, raised each update up to its cap (<see cref="AccelerateEscape"/>). It
    /// starts slow and grows each update; reaching its trigger speed sets it back to its cap, so it never passes the cap by
    /// a whole pixel.
    /// </summary>
    public SignedEightEight EscapeVelocity { get; internal set; }

    /// <summary>
    /// One update of the escape's acceleration: the speed grows by <see cref="GunshipMotionDefinitions.EscapeAcceleration"/>,
    /// and an update whose speed reaches <see cref="GunshipMotionDefinitions.EscapeVelocityCapTrigger"/>
    /// sets it back to <see cref="GunshipMotionDefinitions.EscapeVelocityCap"/>, so once it first gets there the speed
    /// saws between nine pixels an update and nine and three quarters.
    /// </summary>
    internal void AccelerateEscape()
    {
        EscapeVelocity += GunshipMotionDefinitions.EscapeAcceleration;
        if (EscapeVelocity >= GunshipMotionDefinitions.EscapeVelocityCapTrigger)
            EscapeVelocity = GunshipMotionDefinitions.EscapeVelocityCap;
    }
}
