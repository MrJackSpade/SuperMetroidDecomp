using SuperMetroid.Core.Fx;
namespace SuperMetroid.Core.Enemies.LavaJumpers;

/// <summary>
/// Typed projection of the common C-F enemy words and the family's two extra-RAM words used
/// by $A2:BE99-$C02B. Common values remain in the physical slot; only the two words which
/// genuinely live in family-extra WRAM receive host fields.
/// </summary>
public sealed class NorfairLavaJumpingEnemyState
{
    /// <summary>Physical parent or follower slot carrying the family's common enemy variables.</summary>
    private readonly RoomEnemySlot _slot;

    /// <summary>Projects a Squeept part's common variables from its occupied enemy slot.</summary>
    internal NorfairLavaJumpingEnemyState(RoomEnemySlot slot) => _slot = slot;

    /// <summary>
    /// Native family-extra variable $00. Instruction $BE8E sets this handshake after the
    /// jump animation reaches the frame where main AI may begin the final falling phase.
    /// It is deliberately host-backed: this word lives outside the six common A-F words.
    /// </summary>
    public bool AnimationFinished { get; internal set; }

    /// <summary>
    /// Native family-extra variable $01. Function six remembers the last started animation so it does not restart one
    /// which is already running; none since initialization. Like variable $00, this is separate from the ordinary A-F
    /// words and therefore has explicit host storage.
    /// </summary>
    internal NorfairLavaJumperProgram? InstalledProgram { get; set; }

    /// <summary>Where the typed animation (#641) resumes, in place of the native instruction pointer.</summary>
    internal AnimationProgramCursor<NorfairLavaJumperProgram> Program { get; set; }

    /// <summary>Signed 8.8 vertical velocity, stored in native enemy variable C.</summary>
    public ushort YVelocity
    {
        get => _slot.VariableC;
        internal set => _slot.VariableC = value;
    }

    /// <summary>
    /// Parent variant's immutable population position (Y at the lava surface), stored in native
    /// enemy variables D (X) and E (Y).
    /// </summary>
    public Position16<RoomPixels> SpawnPosition
    {
        get => new(_slot.VariableD, _slot.VariableE);
        internal set
        {
            _slot.VariableD = value.X;
            _slot.VariableE = value.Y;
        }
    }

    /// <summary>Same-bank dispatch word in common variable F: parent slots traverse the four jump phases, while parameter-one bit 15 selects the persistent follower function; every main call advances RNG before dispatch.</summary>
    public NorfairLavaJumpingEnemyFunction Function
    {
        get => (NorfairLavaJumpingEnemyFunction)_slot.VariableF;
        internal set => _slot.VariableF = (ushort)value;
    }

    /// <summary>
    /// Population parameter-one bit 15 selects the visual follower half. It is not a
    /// freely combinable runtime flag, so expose it as a predicate rather than an enum.
    /// </summary>
    public bool IsFollower => (_slot.Parameter1 & EnemyParameterMasks.FollowerBit) != 0;
}
