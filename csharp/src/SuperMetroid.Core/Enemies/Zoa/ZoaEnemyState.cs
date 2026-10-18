using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Fx;
namespace SuperMetroid.Core.Enemies.Zoa;

/// <summary>
/// The state of a Zoa, with its one parallel $7E:7800 speed-table word. The values the cartridge keeps in the enemy's
/// common record are emptied with it when the enemy is killed (<see cref="ClearWithRecord"/>). “Shooting” is the ROM's
/// name for the creature launching itself horizontally; it does not create a separate enemy projectile.
/// </summary>
public sealed class ZoaEnemyState
{
    /// <summary>
    /// Whole-pixel population position captured at initialization and restored when a launch ends offscreen (X) and B
    /// (Y). Only the whole words are reset: the subpixel fractions accumulated in flight survive.
    /// </summary>
    public Position16<RoomPixels> SpawnPosition { get; internal set; }

    /// <summary>Element index zero through three in the instruction-list pointer table.</summary>
    public ZoaAnimationSelector InstructionListTableIndex { get; internal set; }

    /// <summary>
    /// Last installed animation selector, initially zero; $A3:B537 compares it with the requested selector so unchanged
    /// lists keep their timer and loop state.
    /// </summary>
    public ZoaAnimationSelector PreviousInstructionListTableIndex { get; internal set; }

    /// <summary>
    /// Same-bank AI dispatch word, initialized to wait and transitioned through rise, horizontal launch, and offscreen
    /// return to wait; none once a kill has emptied the record.
    /// </summary>
    public ZoaEnemyFunction? Function { get; internal set; }

    /// <summary>Byte offset zero, four, eight, or twelve into the five-row X-speed table, set by the shooting animation's cues.</summary>
    public int XSpeedTableIndex { get; internal set; }

    /// <summary>Where Zoa's typed animation (#641) resumes, in place of the native instruction pointer.</summary>
    internal AnimationProgramCursor<ZoaProgram> Program { get; set; }

    /// <summary>Empties the values the cartridge keeps in the enemy's common record.</summary>
    internal void ClearWithRecord()
    {
        SpawnPosition = Position16<RoomPixels>.Zero;
        InstructionListTableIndex = default;
        PreviousInstructionListTableIndex = default;
        Function = null;
    }
}
