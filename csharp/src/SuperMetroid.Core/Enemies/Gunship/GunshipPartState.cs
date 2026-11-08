namespace SuperMetroid.Core.Enemies.Gunship;

/// <summary>
/// The state of one gunship hull or pad slot. Each value is the part's own: the game keeps the top hull's bob timer
/// with the bottom hull's engine-sound timer, its base Y and landing-brake step with the bottom hull's rumble frame,
/// and its function with the bottom hull's escape speed, but each hull reads only its own meanings, and the top hull's
/// landing brake ends by setting the base Y before anything reads it.
/// </summary>
internal sealed class GunshipPartState
{
    /// <summary>The top hull's function timer, decremented by the waiting functions; 144 updates per pad movement.</summary>
    public int FunctionTimer { get; internal set; }

    /// <summary>
    /// The updates the takeoff has spent readying the engines so far, fewer than
    /// <see cref="GunshipTakeoffDefinitions.EngineReadyingDuration"/>; zero once they fire up.
    /// </summary>
    public int EngineReadyingElapsed
    {
        get;
        internal set => field = Ensure.BetweenInclusive(0, ushort.MaxValue, value);
    }

    /// <summary>The top hull's next idle-bob entry, 0 through 3.</summary>
    public int BobPhase
    {
        get;
        internal set => field = Ensure.BetweenInclusive(0, ushort.MaxValue, value);
    }

    /// <summary>The top hull's updates until its next idle-bob step.</summary>
    public int BobTimer { get; internal set; }

    /// <summary>The bottom hull's updates until its next engine sound.</summary>
    public int EngineSoundTimer { get; internal set; }

    /// <summary>The top hull's landed room Y, from which Samus's entry and exit heights are measured.</summary>
    public int BaseY
    {
        get;
        internal set => field = Ensure.BetweenInclusive(0, ushort.MaxValue, value);
    }

    /// <summary>The top hull's next landing-brake entry, counting through the seventeen authored offsets.</summary>
    public int LandingBrakeStep
    {
        get;
        internal set => field = Ensure.BetweenInclusive(0, ushort.MaxValue, value);
    }

    /// <summary>The bottom hull's engine-rumble frame, counting the 128 frames before liftoff.</summary>
    public int RumbleFrame
    {
        get;
        internal set => field = Ensure.BetweenInclusive(0, ushort.MaxValue, value);
    }

    /// <summary>The function the top hull dispatches.</summary>
    public GunshipFunction Function { get; internal set; }

    /// <summary>The bottom hull's unsigned 8.8 escape speed in pixels per update, raised each update up to its cap.</summary>
    public int EscapeVelocity
    {
        get;
        internal set => field = Ensure.BetweenInclusive(0, ushort.MaxValue, value);
    }

    /// <summary>The state of a gunship slot, failing if the slot never initialized as a gunship part.</summary>
    internal static GunshipPartState Of(RoomEnemySlot slot) =>
        slot.SpeciesState<GunshipPartState>() ?? throw new InvalidOperationException("A gunship slot has no state.");
}
