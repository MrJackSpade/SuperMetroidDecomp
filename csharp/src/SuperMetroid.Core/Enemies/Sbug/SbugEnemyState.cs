using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.Sbug;

/// <summary>
/// Typed debugger view of Sbug's common-slot words and its parallel WRAM extension arrays.
/// The names follow the behavior established by $A3:A14D-$A67C rather than the anonymous
/// var00/var20 labels required by the raw decompilation.
/// </summary>
public sealed class SbugEnemyState
{
    /// <summary>Live enemy slot backing Sbug's four common variables and population parameters.</summary>
    private readonly RoomEnemySlot _slot;

    /// <summary>Projects Sbug's existing slot into named AI and motion state.</summary>
    internal SbugEnemyState(RoomEnemySlot slot) => _slot = slot;

    /// <summary>Variable A; the long lifetime used only by behavior four.</summary>
    public NativeCountdown StraightMovementTimer
    {
        get => NativeCountdown.FromRaw(_slot.VariableA);
        internal set => _slot.VariableA = value.RawValue;
    }

    /// <summary>Variable B; the current same-bank AI dispatch pointer.</summary>
    public SbugEnemyFunction Function
    {
        get => (SbugEnemyFunction)_slot.VariableB;
        internal set => _slot.VariableB = (ushort)value;
    }

    /// <summary>
    /// Variable C's custom movement heading, in standard-mathematics orientation (zero right, a quarter turn up). The
    /// cartridge adds random turns to the whole variable word, so whole turns carry into its high byte, which nothing
    /// steers by; the heading is its low byte. The word changes only through <see cref="FaceCustomHeading"/> and
    /// <see cref="TurnCustomHeading"/>, so other enemies' aliased enemy-RAM reads see the cartridge's exact word.
    /// </summary>
    public SnesAngle CustomHeading => SnesAngle.FromTableIndex(SnesLittleEndian.LowByte(_slot.VariableC));

    /// <summary>Sets the custom heading outright, as the toward- and away-from-Samus activations do.</summary>
    internal void FaceCustomHeading(SnesAngle heading) => _slot.VariableC = heading.TableIndex;

    /// <summary>
    /// Turns the custom heading by <paramref name="tableUnits"/> 256ths of a turn (0 through 255), added to the whole
    /// variable word with wraparound as the cartridge does, so a whole turn carries into its high byte.
    /// </summary>
    internal void TurnCustomHeading(int tableUnits) =>
        _slot.VariableC = unchecked((ushort)(_slot.VariableC + tableUnits));

    /// <summary>Variable F; the 32-frame segment/wait countdown.</summary>
    public NativeCountdown MovementTimer
    {
        get => NativeCountdown.FromRaw(_slot.VariableF);
        internal set => _slot.VariableF = value.RawValue;
    }

    /// <summary>Parameter one's low byte, 0 through 255: the speed the trigonometry tables are multiplied by.</summary>
    public int Speed => SnesLittleEndian.LowByte(_slot.Parameter1);

    /// <summary>
    /// Parameter one's high byte as a heading: a table index of zero points right, 64 up, 128 left and 192 down.
    /// </summary>
    public SnesAngle InitialAngle => SnesAngle.FromTableIndex(SnesLittleEndian.HighByte(_slot.Parameter1));

    /// <summary>Parameter two's low byte, 0 through 255: the pixel distance Samus must be within on both axes for activation.</summary>
    public int ActivationRadius => SnesLittleEndian.LowByte(_slot.Parameter2);

    /// <summary>Parameter two high byte; the activation-table selector.</summary>
    public SbugActivationBehavior ActivationBehavior =>
        ClosedNativeWords.Decode<SbugActivationBehavior>(SnesLittleEndian.HighByte(_slot.Parameter2), "Sbug activation behavior");

    /// <summary>Spawn-computed unsigned 16.16 horizontal magnitude in pixels per AI update; the spawn angle supplies its sign during forward movement.</summary>
    public SbugVelocityWords ForwardXVelocity { get; internal set; }
    /// <summary>Spawn-computed unsigned 16.16 vertical magnitude in pixels per AI update; the spawn angle supplies its sign during forward movement.</summary>
    public SbugVelocityWords ForwardYVelocity { get; internal set; }
    /// <summary>Spawn-computed signed 16.16 horizontal displacement for angle <c>InitialAngle - $20</c>, used by the native left zigzag leg.</summary>
    public SbugVelocityWords LeftXVelocity { get; internal set; }
    /// <summary>Spawn-computed signed 16.16 vertical displacement for angle <c>InitialAngle - $20</c>, used by the native left zigzag leg.</summary>
    public SbugVelocityWords LeftYVelocity { get; internal set; }
    /// <summary>Spawn-computed signed 16.16 horizontal displacement for angle <c>InitialAngle + $20</c>, used by the native right zigzag leg.</summary>
    public SbugVelocityWords RightXVelocity { get; internal set; }
    /// <summary>Spawn-computed signed 16.16 vertical displacement for angle <c>InitialAngle + $20</c>, used by the native right zigzag leg.</summary>
    public SbugVelocityWords RightYVelocity { get; internal set; }
    /// <summary>Current signed 16.16 horizontal displacement in pixels per AI update, computed from the custom angle and independently word-negated on reversal.</summary>
    public SbugVelocityWords CustomXVelocity { get; internal set; }
    /// <summary>Current signed 16.16 vertical displacement in pixels per AI update, computed from the custom angle and independently word-negated on reversal.</summary>
    public SbugVelocityWords CustomYVelocity { get; internal set; }

    /// <summary>Spawn-computed facing, 0 through 7 counter-clockwise from right, for the spawn heading; selects the forward loop.</summary>
    public int ForwardFacing { get; internal set; }
    /// <summary>Spawn-computed facing, 0 through 7, for the spawn heading less an eighth of a turn; used by the left zigzag leg.</summary>
    public int LeftFacing { get; internal set; }
    /// <summary>Spawn-computed facing, 0 through 7, for the spawn heading plus an eighth of a turn; used by the right zigzag leg.</summary>
    public int RightFacing { get; internal set; }
    /// <summary>Facing, 0 through 7, for the custom heading; a reversal folds it into the first four facings as the cartridge does.</summary>
    public int CustomFacing { get; internal set; }

    /// <summary>The loop most recently requested by direction-selection code.</summary>
    internal SbugProgram? RequestedProgram { get; set; }

    /// <summary>The loop already started, or none before the first direction is selected.</summary>
    internal SbugProgram? InstalledProgram { get; set; }

    /// <summary>Where the typed animation (#641) resumes, in place of the native instruction pointer.</summary>
    internal AnimationProgramCursor<SbugProgram> Program { get; set; }
}
