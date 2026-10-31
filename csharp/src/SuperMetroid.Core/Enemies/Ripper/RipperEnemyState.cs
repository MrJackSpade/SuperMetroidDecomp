using SuperMetroid.Core.Programs;
namespace SuperMetroid.Core.Enemies.Ripper;

/// <summary>
/// The state of one Ripper, GRipper or Ripper II: its flying animation, its patrol velocity, its linear-speed record, and
/// GRipper's two patrol bounds. The velocity and speed record are values the cartridge keeps in the enemy's common record,
/// so they are emptied with that record when the enemy is killed (<see cref="ClearWithRecord"/>); the animation and the
/// bounds outlive the kill. All are the enemy's own: setup starts the animation for every variant and writes the bounds
/// for the GRipper, the only variant that reads them, so nothing a previous occupant of the slot left reaches a new one.
/// </summary>
public sealed class RipperEnemyState
{
    /// <summary>The Ripper-family typed flying loop (#641), started by setup and restarted at each reversal.</summary>
    internal AnimationProgramCursor<RipperProgram> Program { get; set; }

    /// <summary>Signed 16.16 horizontal velocity per update, one of the selected linear-speed row's speeds.</summary>
    public SignedSixteenSixteen XVelocity { get; internal set; }

    /// <summary>The selected row of the shared linear speed table, set by initialization and read back when the velocity is loaded.</summary>
    public EnemyLinearSpeedRow Speeds { get; internal set; }

    /// <summary>
    /// GRipper's exclusive left patrol reversal threshold, a room X coordinate in pixels from its spawn settings. The
    /// patrol compares it by the sign of the wrapped difference.
    /// </summary>
    public int MinimumXPosition
    {
        get;
        internal set => field = Ensure.BetweenInclusive(0, ushort.MaxValue, value);
    }

    /// <summary>
    /// GRipper's inclusive right patrol reversal threshold, a room X coordinate in pixels from its spawn settings. The
    /// patrol compares it by the sign of the wrapped difference.
    /// </summary>
    public int MaximumXPosition
    {
        get;
        internal set => field = Ensure.BetweenInclusive(0, ushort.MaxValue, value);
    }

    /// <summary>Empties the velocity and speed row, when a kill empties the enemy's common record.</summary>
    internal void ClearWithRecord()
    {
        XVelocity = SignedSixteenSixteen.Zero;
        Speeds = default;
    }
}
