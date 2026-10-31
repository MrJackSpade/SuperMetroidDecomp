using SuperMetroid.Core.Programs;
namespace SuperMetroid.Core.Enemies.LavaJumpers;

/// <summary>
/// The state of one Squeept part, the jumping parent or its cosmetic follower. The velocity, spawn, function and
/// follower selection are values the cartridge keeps in the enemy's common record, so they are emptied with that
/// record when the part is killed (<see cref="ClearWithRecord"/>); the animation handshake and installed animation
/// outlive the kill.
/// </summary>
public sealed class NorfairLavaJumpingEnemyState
{
    /// <summary>
    /// Native family-extra variable $00. Instruction $BE8E sets this handshake after the
    /// jump animation reaches the frame where main AI may begin the final falling phase.
    /// </summary>
    public bool AnimationFinished { get; internal set; }

    /// <summary>
    /// Native family-extra variable $01. Function six remembers the last started animation so it does not restart one
    /// which is already running; none since initialization.
    /// </summary>
    internal NorfairLavaJumperProgram? InstalledProgram { get; set; }

    /// <summary>Where the typed animation (#641) resumes.</summary>
    internal AnimationProgramCursor<NorfairLavaJumperProgram> Program { get; set; }

    /// <summary>
    /// Signed 8.8 vertical velocity. Gravity adds to its word, which the jump compares unsigned with the animation
    /// switch velocity (<see cref="ApplyGravity"/>), and the follower shows only while it is negative.
    /// </summary>
    public SignedEightEight YVelocity { get; internal set; }

    /// <summary>Parent variant's immutable population position (Y at the lava surface).</summary>
    public Position16<RoomPixels> SpawnPosition { get; internal set; }

    /// <summary>Parent parts traverse the four jump phases, while followers keep the follow function; none once a kill has emptied the record.</summary>
    public NorfairLavaJumpingEnemyFunction? Function { get; internal set; }

    /// <summary>Whether population parameter-one bit 15 selects the visual follower half rather than the jumping parent.</summary>
    public bool IsFollower { get; internal set; }

    /// <summary>One update's gravity: adds <paramref name="gravity"/> to the velocity's word, wrapping as the cartridge's addition does.</summary>
    /// <param name="gravity">The 8.8 gravity step.</param>
    internal void ApplyGravity(int gravity) =>
        YVelocity = SignedEightEight.Wrapped(YVelocity.RawValue + gravity);

    /// <summary>Empties the values the cartridge keeps in the enemy's common record, when a kill empties that record.</summary>
    internal void ClearWithRecord()
    {
        YVelocity = default;
        SpawnPosition = Position16<RoomPixels>.Zero;
        Function = null;
        IsFollower = false;
    }
}
