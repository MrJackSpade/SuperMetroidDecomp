using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Fx;
namespace SuperMetroid.Core.Enemies.Choot;

/// <summary>
/// Typed debugger view of Choot's six common-slot words and eight words in per-enemy extra
/// WRAM. Every property aliases the same native-width storage used by the translated AI, so
/// frame stepping exposes the actual table cursors instead of a second host-only model.
/// </summary>
public sealed class ChootEnemyState
{
    /// <summary>Common enemy slot holding Choot's native-width variable words.</summary>
    private readonly RoomEnemySlot _slot;
    /// <summary>The slot's Choot extension words, kept for the room across occupants.</summary>
    private readonly ChootExtensionWords _words;

    /// <summary>Binds the common slot and the per-enemy WRAM arrays without copying their state.</summary>
    internal ChootEnemyState(
        RoomEnemySlot slot, ChootExtensionWords words)
    {
        _slot = slot;
        _words = words;
    }

    /// <summary>Where the typed animation (#641) resumes, in place of the native instruction pointer.</summary>
    internal AnimationProgramCursor<ChootProgram> Program { get; set; }

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
    public NativeCountdown FallingPatternLoopCounter
    {
        get => NativeCountdown.FromRaw(_slot.VariableD);
        internal set => _slot.VariableD = value.RawValue;
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
        get => _words.SpawnPosition;
        internal set => _words.SpawnPosition = value;
    }

    /// <summary>Native <c>initialFallingXPosition</c>/<c>initialFallingYPosition</c> at <c>$7E:7804</c>/<c>$7E:7806 + enemy index</c>: spawn X and the wrapped whole-pixel spawn Y minus the computed jump height, installed as the position and falling origin at the apex transition.</summary>
    public Position16<RoomPixels> InitialFallingPosition
    {
        get => _words.InitialFallingPosition;
        internal set => _words.InitialFallingPosition = value;
    }

    /// <summary>Native <c>fallingXOrigin</c>/<c>fallingYOrigin</c> at <c>$7E:7808</c>/<c>$7E:780A + enemy index</c>: the whole-pixel base added to each signed falling-pattern offset; initialized at the apex, and its Y advanced by <see cref="FallingPatternYDistance"/> at every stream terminator.</summary>
    public Position16<RoomPixels> FallingOrigin
    {
        get => _words.FallingOrigin;
        internal set => _words.FallingOrigin = value;
    }

    /// <summary>Native <c>initialYSpeedTableIndex</c> at <c>$7E:780C + enemy index</c>: the saved encoded ascent cursor calculated from the population's loop count and pattern height; copied back to <see cref="YSpeedTableIndex"/> when the jump cycle finishes.</summary>
    public ushort InitialYSpeedTableIndex
    {
        get => _words.InitialYSpeedTableIndex;
        internal set => _words.InitialYSpeedTableIndex = value;
    }

    /// <summary>
    /// Parameter-two countdown. The jump begins only after decrementing through zero to
    /// <c>$FFFF</c>, so a population delay of zero launches on the first preparation frame.
    /// </summary>
    public NativeCountdown JumpDelayTimer
    {
        get => NativeCountdown.FromRaw(_words.JumpDelayTimer);
        internal set => _words.JumpDelayTimer = value.RawValue;
    }
}
