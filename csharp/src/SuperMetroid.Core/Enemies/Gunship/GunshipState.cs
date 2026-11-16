namespace SuperMetroid.Core.Enemies.Gunship;

/// <summary>
/// The gunship's state, one for the whole ship, which its top hull keeps and runs: its phase, landing, bob and engine
/// timers, and its takeoff. Each count is bounded where it is read: the brake and bob schedules reject a step past their
/// entries, and the engine readying rejects one past its duration.
/// </summary>
internal sealed class GunshipState
{
    /// <summary>Starts the ship in <paramref name="phase"/>.</summary>
    internal GunshipState(GunshipPhase phase) => Phase = phase;

    /// <summary>The ship's phase timer, decremented by the waiting phases; 144 updates per pad movement.</summary>
    public int PhaseTimer { get; internal set; }

    /// <summary>
    /// The updates the takeoff has spent readying the engines so far, fewer than
    /// <see cref="GunshipTakeoffDefinitions.EngineReadyingDuration"/>; zero once they fire up.
    /// </summary>
    public int EngineReadyingElapsed { get; internal set; }

    /// <summary>The ship's next idle-bob entry, 0 through 3.</summary>
    public int BobPhase { get; internal set; }

    /// <summary>The ship's updates until its next idle-bob step.</summary>
    public int BobTimer { get; internal set; }

    /// <summary>The ship's updates until its next engine sound.</summary>
    public int EngineSoundTimer { get; internal set; }

    /// <summary>The ship's landed room Y, its top hull's, from which Samus's entry and exit heights are measured.</summary>
    public int BaseY { get; internal set; }

    /// <summary>The ship's next landing-brake entry, counting through the seventeen authored offsets.</summary>
    public int LandingBrakeStep { get; internal set; }

    /// <summary>
    /// The ship's engine-rumble frame, counting the <see cref="GunshipTakeoffDefinitions.EngineRumbleFrames"/>
    /// frames before liftoff.
    /// </summary>
    public int RumbleFrame { get; internal set; }

    /// <summary>The phase the ship runs.</summary>
    public GunshipPhase Phase { get; internal set; }

    /// <summary>
    /// The ship's upward escape speed, raised each update up to its cap (<see cref="AccelerateEscape"/>). It
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

    /// <summary>The state of the ship whose top hull <paramref name="top"/> holds, failing if it never initialized as one.</summary>
    internal static GunshipState Of(RoomEnemySlot top) =>
        top.SpeciesState<GunshipState>() ?? throw new InvalidOperationException("A gunship top hull has no ship state.");
}
