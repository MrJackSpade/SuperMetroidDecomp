namespace SuperMetroid.Core.Enemies.Gunship;

/// <summary>
/// The state of one gunship hull or pad slot. Each value is the part's own: the game keeps the top hull's bob timer
/// with the bottom hull's engine-sound timer, its base Y and landing-brake step with the bottom hull's rumble frame,
/// and its function with the bottom hull's escape speed, but each hull reads only its own meanings, and the top hull's
/// landing brake ends by setting the base Y before anything reads it. Each count is bounded where it is read: the
/// brake and bob schedules reject a step past their entries, and the engine readying rejects one past its duration.
/// </summary>
internal sealed class GunshipPartState
{
    /// <summary>The top hull's phase timer, decremented by the waiting functions; 144 updates per pad movement.</summary>
    public int PhaseTimer { get; internal set; }

    /// <summary>
    /// The updates the takeoff has spent readying the engines so far, fewer than
    /// <see cref="GunshipTakeoffDefinitions.EngineReadyingDuration"/>; zero once they fire up.
    /// </summary>
    public int EngineReadyingElapsed { get; internal set; }

    /// <summary>The top hull's next idle-bob entry, 0 through 3.</summary>
    public int BobPhase { get; internal set; }

    /// <summary>The top hull's updates until its next idle-bob step.</summary>
    public int BobTimer { get; internal set; }

    /// <summary>The bottom hull's updates until its next engine sound.</summary>
    public int EngineSoundTimer { get; internal set; }

    /// <summary>The top hull's landed room Y, from which Samus's entry and exit heights are measured.</summary>
    public int BaseY { get; internal set; }

    /// <summary>The top hull's next landing-brake entry, counting through the seventeen authored offsets.</summary>
    public int LandingBrakeStep { get; internal set; }

    /// <summary>
    /// The bottom hull's engine-rumble frame, counting the <see cref="GunshipTakeoffDefinitions.EngineRumbleFrames"/>
    /// frames before liftoff.
    /// </summary>
    public int RumbleFrame { get; internal set; }

    /// <summary>The phase the top hull dispatches.</summary>
    public GunshipPhase Phase { get; internal set; }

    /// <summary>
    /// The bottom hull's upward escape speed, raised each update up to its cap (<see cref="AccelerateEscape"/>). It
    /// starts at two pixels an update, grows by a quarter pixel and never passes ten pixels and three quarters, so it
    /// never reaches the type's wrap.
    /// </summary>
    public SignedEightEight EscapeVelocity { get; internal set; }

    /// <summary>
    /// One update of the escape's acceleration: the speed grows by <see cref="GunshipMotionDefinitions.EscapeAcceleration"/>,
    /// and an update whose speed's whole pixels reach <see cref="GunshipMotionDefinitions.EscapeVelocityCapTriggerPixels"/>
    /// sets it back to <see cref="GunshipMotionDefinitions.EscapeVelocityCap"/>, so the speed saws between nine and ten
    /// pixels an update once it first gets there.
    /// </summary>
    internal void AccelerateEscape()
    {
        EscapeVelocity += GunshipMotionDefinitions.EscapeAcceleration;
        if (EscapeVelocity.WholePixels >= GunshipMotionDefinitions.EscapeVelocityCapTriggerPixels)
            EscapeVelocity = GunshipMotionDefinitions.EscapeVelocityCap;
    }

    /// <summary>The state of a gunship slot, failing if the slot never initialized as a gunship part.</summary>
    internal static GunshipPartState Of(RoomEnemySlot slot) =>
        slot.SpeciesState<GunshipPartState>() ?? throw new InvalidOperationException("A gunship slot has no state.");
}
