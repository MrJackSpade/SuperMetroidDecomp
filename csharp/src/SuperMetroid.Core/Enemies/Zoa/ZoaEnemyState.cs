using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Fx;
namespace SuperMetroid.Core.Enemies.Zoa;

/// <summary>
/// Named view of Zoa's common slot words and its one parallel $7E:7800 speed-table word.
/// “Shooting” is the ROM's name for the creature launching itself horizontally; it does not
/// create a separate enemy projectile.
/// </summary>
public sealed class ZoaEnemyState
{
    /// <summary>Native Zoa slot whose variables A, B, C, D, and F hold movement and animation state.</summary>
    private readonly RoomEnemySlot _slot;

    /// <summary>Binds typed Zoa state to its existing native enemy slot.</summary>
    internal ZoaEnemyState(RoomEnemySlot slot) => _slot = slot;

    /// <summary>
    /// Whole-pixel population position captured at initialization and restored when a launch
    /// ends offscreen, stored in native enemy variables A (X) and B (Y). Only the whole words
    /// are reset: the subpixel fractions accumulated in flight survive.
    /// </summary>
    public Position16<RoomPixels> SpawnPosition
    {
        get => new(_slot.VariableA, _slot.VariableB);
        internal set
        {
            _slot.VariableA = value.X;
            _slot.VariableB = value.Y;
        }
    }

    /// <summary>Element index zero through three in the instruction-list pointer table.</summary>
    public ZoaAnimationSelector InstructionListTableIndex
    {
        get => (ZoaAnimationSelector)_slot.VariableC;
        internal set => _slot.VariableC = value.ToNativeWord();
    }

    /// <summary>Last installed animation selector, initially zero; $A3:B537 compares it with the requested selector so unchanged lists keep their timer and loop state.</summary>
    public ZoaAnimationSelector PreviousInstructionListTableIndex
    {
        get => (ZoaAnimationSelector)_slot.VariableD;
        internal set => _slot.VariableD = value.ToNativeWord();
    }

    /// <summary>Same-bank AI dispatch word in slot variable F, initialized to wait and transitioned through rise, horizontal launch, and offscreen return to wait.</summary>
    public ZoaEnemyFunction Function
    {
        get => (ZoaEnemyFunction)_slot.VariableF;
        internal set => _slot.VariableF = (ushort)value;
    }

    /// <summary>Byte offset zero, four, eight, or twelve into the five-row X-speed table, set by the shooting animation's cues.</summary>
    public int XSpeedTableIndex { get; internal set; }

    /// <summary>Where Zoa's typed animation (#641) resumes, in place of the native instruction pointer.</summary>
    internal AnimationProgramCursor<ZoaProgram> Program { get; set; }
}
