using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.Sbug;

/// <summary>
/// The state of an Sbug, with its parallel WRAM extension arrays. The names follow the behavior established by
/// $A3:A14D-$A67C rather than the anonymous var00/var20 labels required by the raw decompilation. The values the
/// cartridge keeps in the enemy's common record are emptied with it when the enemy is killed (<see
/// cref="ClearWithRecord"/>).
/// </summary>
public sealed class SbugEnemyState
{
    /// <summary>The Sbug's slot, whose population parameters the state reads.</summary>
    private readonly RoomEnemySlot _slot;

    /// <summary>Creates the state for an Sbug's slot.</summary>
    internal SbugEnemyState(RoomEnemySlot slot) => _slot = slot;

    /// <summary>The long lifetime used only by behavior four.</summary>
    public NativeCountdown StraightMovementTimer { get; internal set; }

    /// <summary>The current same-bank AI dispatch pointer; none once a kill has emptied the record.</summary>
    public SbugEnemyFunction? Function { get; internal set; }

    /// <summary>
    /// The custom movement heading, in standard-mathematics orientation (zero right, a quarter turn up). The cartridge
    /// adds random turns to a whole word whose high byte nothing steers by; the heading is its low byte. It changes
    /// only through <see cref="FaceCustomHeading"/> and <see cref="TurnCustomHeading"/>.
    /// </summary>
    public SnesAngle CustomHeading { get; private set; }

    /// <summary>Sets the custom heading outright, as the toward- and away-from-Samus activations do.</summary>
    internal void FaceCustomHeading(SnesAngle heading) => CustomHeading = heading;

    /// <summary>
    /// Turns the custom heading by <paramref name="tableUnits"/> 256ths of a turn (0 through 255), wrapping around the
    /// turn.
    /// </summary>
    internal void TurnCustomHeading(int tableUnits) =>
        CustomHeading = CustomHeading.AddTableUnits(Ensure.BetweenInclusive(0, byte.MaxValue, tableUnits));

    /// <summary>The 32-frame segment/wait countdown.</summary>
    public NativeCountdown MovementTimer { get; internal set; }

    /// <summary>Parameter one's low byte, 0 through 255: the speed the trigonometry tables are multiplied by.</summary>
    public int Speed => SnesLittleEndian.LowByte(_slot.Spawn.Population.Parameter1);

    /// <summary>
    /// Parameter one's high byte as a heading: a table index of zero points right, 64 up, 128 left and 192 down.
    /// </summary>
    public SnesAngle InitialAngle => SnesAngle.FromTableIndex(SnesLittleEndian.HighByte(_slot.Spawn.Population.Parameter1));

    /// <summary>Parameter two's low byte, 0 through 255: the pixel distance Samus must be within on both axes for activation.</summary>
    public int ActivationRadius => SnesLittleEndian.LowByte(_slot.Spawn.Population.Parameter2);

    /// <summary>Parameter two high byte; the activation-table selector.</summary>
    public SbugActivationBehavior ActivationBehavior =>
        ClosedNativeWords.Decode<SbugActivationBehavior>(SnesLittleEndian.HighByte(_slot.Spawn.Population.Parameter2), "Sbug activation behavior");

    /// <summary>Spawn-computed unsigned 16.16 horizontal magnitude in pixels per AI update; the spawn angle supplies its sign during forward movement.</summary>
    public SnesUnsignedSixteenSixteen ForwardXVelocity { get; internal set; }
    /// <summary>Spawn-computed unsigned 16.16 vertical magnitude in pixels per AI update; the spawn angle supplies its sign during forward movement.</summary>
    public SnesUnsignedSixteenSixteen ForwardYVelocity { get; internal set; }
    /// <summary>Spawn-computed signed 16.16 horizontal displacement for angle <c>InitialAngle - $20</c>, used by the native left zigzag leg.</summary>
    public SnesSignedSixteenSixteen LeftXVelocity { get; internal set; }
    /// <summary>Spawn-computed signed 16.16 vertical displacement for angle <c>InitialAngle - $20</c>, used by the native left zigzag leg.</summary>
    public SnesSignedSixteenSixteen LeftYVelocity { get; internal set; }
    /// <summary>Spawn-computed signed 16.16 horizontal displacement for angle <c>InitialAngle + $20</c>, used by the native right zigzag leg.</summary>
    public SnesSignedSixteenSixteen RightXVelocity { get; internal set; }
    /// <summary>Spawn-computed signed 16.16 vertical displacement for angle <c>InitialAngle + $20</c>, used by the native right zigzag leg.</summary>
    public SnesSignedSixteenSixteen RightYVelocity { get; internal set; }
    /// <summary>Current signed 16.16 horizontal displacement in pixels per AI update, computed from the custom angle and independently word-negated on reversal.</summary>
    public SnesSignedSixteenSixteen CustomXVelocity { get; internal set; }
    /// <summary>Current signed 16.16 vertical displacement in pixels per AI update, computed from the custom angle and independently word-negated on reversal.</summary>
    public SnesSignedSixteenSixteen CustomYVelocity { get; internal set; }

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

    /// <summary>Empties the values the cartridge keeps in the enemy's common record.</summary>
    internal void ClearWithRecord()
    {
        StraightMovementTimer = NativeCountdown.Start(0);
        Function = null;
        CustomHeading = SnesAngle.FromTableIndex(0);
        MovementTimer = NativeCountdown.Start(0);
    }
}
