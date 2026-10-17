using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Fx;
namespace SuperMetroid.Core.Enemies.WorkRobots;

/// <summary>
/// Typed views of the six common enemy words bank-$A8's Work Robot uses. Each view writes the slot
/// variables exactly, so other enemies' aliased enemy-RAM reads see the cartridge's words: the laser
/// velocity is a signed 8.8 word and the falling speed a signed 16.16 whole/fraction pair.
/// </summary>
public sealed class WorkRobotEnemyState
{
    /// <summary>Live enemy slot whose variable words back the debugger-facing Work Robot state.</summary>
    private readonly RoomEnemySlot _slot;

    /// <summary>Projects the bank-$A8 Work Robot aliases over its current native enemy slot.</summary>
    internal WorkRobotEnemyState(RoomEnemySlot slot) => _slot = slot;

    /// <summary>Where the typed animation (#641) resumes, in place of the native instruction pointer.</summary>
    internal AnimationProgramCursor<WorkRobotProgram> Program { get; set; }

    /// <summary>
    /// True only when definition $E8FF saw the area's boss bit during initialization.
    /// Definition $E8FF can therefore own a deactivated state before Phantoon is defeated.
    /// </summary>
    public bool Powered { get; internal set; }

    /// <summary>
    /// Variable A: the signed 8.8 velocity, in pixels per update, given to newly fired robot lasers; only its X is
    /// meaningful, and its sign is the robot's facing.
    /// </summary>
    public EightEightVector<RoomPixels> LaserVelocity
    {
        get => new(new SnesSignedEightEight(_slot.VariableA), default);
        internal set => _slot.VariableA = value.X.RawValue;
    }

    /// <summary>Gets or sets variable B, the wrapping countdown that gates the next laser attempt.</summary>
    public NativeCountdown LaserCooldown
    {
        get => NativeCountdown.FromRaw(_slot.VariableB);
        internal set => _slot.VariableB = value.RawValue;
    }

    /// <summary>
    /// Gets or sets the whole-pixel room position saved during a temporary ledge probe: X in
    /// variable C, Y in variable D.
    /// </summary>
    public Position16<RoomPixels> BackupPosition
    {
        get => new(_slot.VariableC, _slot.VariableD);
        internal set
        {
            _slot.VariableC = value.X;
            _slot.VariableD = value.Y;
        }
    }

    /// <summary>
    /// Variables E (fraction) and F (whole pixels): an unpowered robot's signed 16.16 falling speed in pixels per
    /// update, which gravity accumulates with the cartridge's carry and wraparound.
    /// </summary>
    public SnesSignedSixteenSixteen FallVelocity
    {
        get => SnesSignedSixteenSixteen.FromParts(unchecked((short)_slot.VariableF), _slot.VariableE);
        internal set
        {
            _slot.VariableE = value.Fraction;
            _slot.VariableF = value.WholeWord;
        }
    }
}
