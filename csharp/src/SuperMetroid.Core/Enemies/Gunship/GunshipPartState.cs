namespace SuperMetroid.Core.Enemies.Gunship;

/// <summary>
/// The state of one gunship hull or pad slot. The cartridge reuses these common-record words between phases and parts,
/// and a phase can read what an earlier one left in them, so the state keeps each record word once and every named view
/// reads and writes it. The words are emptied with the record when the slot is killed (<see cref="ClearWithRecord"/>).
/// </summary>
internal sealed class GunshipPartState
{
    /// <summary>The function timer word.</summary>
    private int _functionTimerWord;

    /// <summary>The next liftoff page word.</summary>
    private int _nextLiftoffPageWord;

    /// <summary>The idle-bob phase word.</summary>
    private int _bobPhaseWord;

    /// <summary>The word holding the top hull's bob timer or the bottom hull's engine-sound timer.</summary>
    private int _timerWord;

    /// <summary>The word holding the top hull's base Y or landing-brake step, or the bottom hull's rumble frame.</summary>
    private int _heightOrStepWord;

    /// <summary>The top hull's function timer, decremented by the waiting functions; 144 updates per pad movement.</summary>
    public int FunctionTimer
    {
        get => _functionTimerWord;
        internal set => _functionTimerWord = Ensure.BetweenInclusive(0, ushort.MaxValue, value);
    }

    /// <summary>The top hull's next liftoff page to show, 0 through the last of the five.</summary>
    public int NextLiftoffPage
    {
        get => _nextLiftoffPageWord;
        internal set => _nextLiftoffPageWord = Ensure.BetweenInclusive(0, ushort.MaxValue, value);
    }

    /// <summary>The top hull's next idle-bob entry, 0 through 3.</summary>
    public int BobPhase
    {
        get => _bobPhaseWord;
        internal set => _bobPhaseWord = Ensure.BetweenInclusive(0, ushort.MaxValue, value);
    }

    /// <summary>The top hull's updates until its next idle-bob step.</summary>
    public int BobTimer
    {
        get => _timerWord;
        internal set => _timerWord = Ensure.BetweenInclusive(0, ushort.MaxValue, value);
    }

    /// <summary>The bottom hull's updates until its next engine sound.</summary>
    public int EngineSoundTimer
    {
        get => _timerWord;
        internal set => _timerWord = Ensure.BetweenInclusive(0, ushort.MaxValue, value);
    }

    /// <summary>The top hull's landed room Y, from which Samus's entry and exit heights are measured.</summary>
    public int BaseY
    {
        get => _heightOrStepWord;
        internal set => _heightOrStepWord = Ensure.BetweenInclusive(0, ushort.MaxValue, value);
    }

    /// <summary>The top hull's next landing-brake entry, counting through the seventeen authored offsets.</summary>
    public int LandingBrakeStep
    {
        get => _heightOrStepWord;
        internal set => _heightOrStepWord = Ensure.BetweenInclusive(0, ushort.MaxValue, value);
    }

    /// <summary>The bottom hull's engine-rumble frame, counting the 128 frames before liftoff.</summary>
    public int RumbleFrame
    {
        get => _heightOrStepWord;
        internal set => _heightOrStepWord = Ensure.BetweenInclusive(0, ushort.MaxValue, value);
    }

    /// <summary>
    /// The function the top hull dispatches. The cartridge keeps it in the record word the bottom hull uses for its
    /// escape speed; each hull reads only its own meaning, so the two are kept apart.
    /// </summary>
    public GunshipFunction Function { get; internal set; }

    /// <summary>The record word holding the bottom hull's escape speed.</summary>
    internal int FunctionWord { get; private set; }

    /// <summary>The bottom hull's unsigned 8.8 escape speed in pixels per update, raised each update up to its cap.</summary>
    public int EscapeVelocity
    {
        get => FunctionWord;
        internal set => FunctionWord = Ensure.BetweenInclusive(0, ushort.MaxValue, value);
    }

    /// <summary>The state of a gunship slot, failing if the slot never initialized as a gunship part.</summary>
    internal static GunshipPartState Of(RoomEnemySlot slot) =>
        slot.SpeciesState<GunshipPartState>() ?? throw new InvalidOperationException("A gunship slot has no state.");

    /// <summary>Empties the values the cartridge keeps in the enemy's common record.</summary>
    [IntendedDuplicate("each state clears its own native words; the shared shape is only the assignment of zero")]
    internal void ClearWithRecord()
    {
        _functionTimerWord = 0;
        _nextLiftoffPageWord = 0;
        _bobPhaseWord = 0;
        _timerWord = 0;
        _heightOrStepWord = 0;
        FunctionWord = 0;
        Function = GunshipFunction.None;
    }
}
