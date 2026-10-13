using SuperMetroid.Core.Hardware;
namespace SuperMetroid.Core.Game;

/// <summary>The literal bank-$A2 function pointer stored in a Choot's variable A.</summary>
public enum ChootEnemyFunction : ushort
{
    /// <summary><c>$A2:E035</c>, native <c>Function_Choot_WaitForSamusToGetNear</c>: waits for Samus within 80 horizontal pixels, then loads the population's jump delay.</summary>
    WaitingForSamus = 0xe035,
    /// <summary><c>$A2:E04F</c>, native <c>Function_Choot_PrepareToJump</c>: decrements the jump delay through signed underflow before selecting the jump animation and ascent state.</summary>
    PreparingJump = 0xe04f,
    /// <summary><c>$A2:E06A</c>, native <c>Function_Choot_Jumping</c>: applies negative quadratic-table Y displacement until the ascent cursor underflows, then snaps whole coordinates to the precalculated falling origin.</summary>
    Jumping = 0xe06a,
    /// <summary><c>$A2:E0CD</c>, native <c>Function_Choot_Falling</c>: plays and repeats the selected X/Y offset stream, advancing its Y origin between loops, then restores the spawn position and rearms proximity activation.</summary>
    Falling = 0xe0cd,
}

/// <summary>
/// Typed debugger view of Choot's six common-slot words and eight words in per-enemy extra
/// WRAM. Every property aliases the same native-width storage used by the translated AI, so
/// frame stepping exposes the actual table cursors instead of a second host-only model.
/// </summary>
public sealed class ChootEnemyState
{
    /// <summary>Common enemy slot holding Choot's native-width variable words.</summary>
    private readonly RoomEnemySlot _slot;
    /// <summary>Per-slot whole-pixel room positions restored after a falling cycle.</summary>
    private readonly Position16<RoomPixels>[] _spawnPositions;
    /// <summary>Per-slot apex positions calculated from each Choot's jump height.</summary>
    private readonly Position16<RoomPixels>[] _initialFallingPositions;
    /// <summary>Per-slot base positions to which falling-pattern offsets are added.</summary>
    private readonly Position16<RoomPixels>[] _fallingOrigins;
    /// <summary>Per-slot ascent cursors restored when the jump cycle finishes.</summary>
    private readonly ushort[] _initialYSpeedTableIndexes;
    /// <summary>Per-slot delays that underflow before the next jump begins.</summary>
    private readonly ushort[] _jumpDelayTimers;

    /// <summary>Binds the common slot and the per-enemy WRAM arrays without copying their state.</summary>
    internal ChootEnemyState(
        RoomEnemySlot slot,
        Position16<RoomPixels>[] spawnPositions,
        Position16<RoomPixels>[] initialFallingPositions,
        Position16<RoomPixels>[] fallingOrigins,
        ushort[] initialYSpeedTableIndexes,
        ushort[] jumpDelayTimers)
    {
        _slot = slot;
        _spawnPositions = spawnPositions;
        _initialFallingPositions = initialFallingPositions;
        _fallingOrigins = fallingOrigins;
        _initialYSpeedTableIndexes = initialYSpeedTableIndexes;
        _jumpDelayTimers = jumpDelayTimers;
    }

    /// <summary>Native <c>function</c> at <c>$0FA8 + enemy index</c>, stored in variable A: the bank-$A2 callback selected by Choot's main AI dispatcher.</summary>
    public ChootEnemyFunction Function
    {
        get => (ChootEnemyFunction)_slot.VariableA;
        internal set => _slot.VariableA = (ushort)value;
    }

    /// <summary>
    /// Encoded ascent cursor: its high byte is the quadratic-table index and native AI
    /// changes the word by <c>$0200</c> per frame.
    /// </summary>
    public ushort YSpeedTableIndex
    {
        get => _slot.VariableB;
        internal set => _slot.VariableB = value;
    }

    /// <summary>8.8 falling-pattern frame cursor; main AI advances it by <c>$0100</c>.</summary>
    public ushort FallingPatternIndex
    {
        get => _slot.VariableC;
        internal set => _slot.VariableC = value;
    }

    /// <summary>Signed-underflow loop counter initialized to parameter-one low minus one.</summary>
    public ushort FallingPatternLoopCounter
    {
        get => _slot.VariableD;
        internal set => _slot.VariableD = value;
    }

    /// <summary>Same-bank pointer to one of five ROM-authored X/Y offset streams.</summary>
    public ushort FallingPatternPointer
    {
        get => _slot.VariableE;
        internal set => _slot.VariableE = value;
    }

    /// <summary>Whole-pixel Y origin advance applied between falling-pattern loops.</summary>
    public ushort FallingPatternYDistance
    {
        get => _slot.VariableF;
        internal set => _slot.VariableF = value;
    }

    /// <summary>Native <c>spawnXPosition</c>/<c>spawnYPosition</c> at <c>$7E:7800</c>/<c>$7E:7802 + enemy index</c>: the initial room position in whole pixels, restored after the last falling-pattern loop.</summary>
    public Position16<RoomPixels> SpawnPosition
    {
        get => _spawnPositions[_slot.SlotIndex];
        internal set => _spawnPositions[_slot.SlotIndex] = value;
    }

    /// <summary>Native <c>initialFallingXPosition</c>/<c>initialFallingYPosition</c> at <c>$7E:7804</c>/<c>$7E:7806 + enemy index</c>: spawn X and the wrapped whole-pixel spawn Y minus the computed jump height, installed as the position and falling origin at the apex transition.</summary>
    public Position16<RoomPixels> InitialFallingPosition
    {
        get => _initialFallingPositions[_slot.SlotIndex];
        internal set => _initialFallingPositions[_slot.SlotIndex] = value;
    }

    /// <summary>Native <c>fallingXOrigin</c>/<c>fallingYOrigin</c> at <c>$7E:7808</c>/<c>$7E:780A + enemy index</c>: the whole-pixel base added to each signed falling-pattern offset; initialized at the apex, and its Y advanced by <see cref="FallingPatternYDistance"/> at every stream terminator.</summary>
    public Position16<RoomPixels> FallingOrigin
    {
        get => _fallingOrigins[_slot.SlotIndex];
        internal set => _fallingOrigins[_slot.SlotIndex] = value;
    }

    /// <summary>Native <c>initialYSpeedTableIndex</c> at <c>$7E:780C + enemy index</c>: the saved encoded ascent cursor calculated from the population's loop count and pattern height; copied back to <see cref="YSpeedTableIndex"/> when the jump cycle finishes.</summary>
    public ushort InitialYSpeedTableIndex
    {
        get => _initialYSpeedTableIndexes[_slot.SlotIndex];
        internal set => _initialYSpeedTableIndexes[_slot.SlotIndex] = value;
    }

    /// <summary>
    /// Parameter-two countdown. The jump begins only after decrementing through zero to
    /// <c>$FFFF</c>, so a population delay of zero launches on the first preparation frame.
    /// </summary>
    public ushort JumpDelayTimer
    {
        get => _jumpDelayTimers[_slot.SlotIndex];
        internal set => _jumpDelayTimers[_slot.SlotIndex] = value;
    }
}
