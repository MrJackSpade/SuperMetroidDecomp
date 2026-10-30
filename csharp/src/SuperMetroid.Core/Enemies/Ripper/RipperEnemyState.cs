namespace SuperMetroid.Core.Enemies.Ripper;

/// <summary>
/// The state of one Ripper, GRipper or Ripper II: its patrol velocity, its linear-speed record, and GRipper's two patrol
/// bounds. The velocity and speed record are values the cartridge keeps in the enemy's common record, so they are
/// emptied with that record when the enemy is killed (<see cref="ClearWithRecord"/>); the bounds outlive the kill.
/// </summary>
public sealed class RipperEnemyState
{
    /// <summary>The slot's Ripper extension words, kept for the room across occupants.</summary>
    private readonly RipperExtensionWords _words;

    /// <summary>Binds a variant to its slot's GRipper patrol-bound storage.</summary>
    internal RipperEnemyState(RipperExtensionWords words) => _words = words;

    /// <summary>Signed 16.16 horizontal velocity per update, one of the selected linear-speed row's speeds.</summary>
    public SignedSixteenSixteen XVelocity { get; internal set; }

    /// <summary>The selected row of the shared linear speed table, set by initialization and read back when the velocity is loaded.</summary>
    public EnemyLinearSpeedRow Speeds { get; internal set; }

    /// <summary>
    /// GRipper extra word $00: exclusive left patrol reversal threshold, a room X coordinate in pixels
    /// from population parameter one. The patrol compares it with a wrapped signed word subtraction.
    /// </summary>
    public int MinimumXPosition
    {
        get => _words.MinimumXPosition;
        internal set => _words.MinimumXPosition = Ensure.BetweenInclusive(0, ushort.MaxValue, value);
    }

    /// <summary>
    /// GRipper extra word $01: inclusive right patrol reversal threshold, a room X coordinate in pixels
    /// from population parameter two. The patrol compares it with a wrapped signed word subtraction.
    /// </summary>
    public int MaximumXPosition
    {
        get => _words.MaximumXPosition;
        internal set => _words.MaximumXPosition = Ensure.BetweenInclusive(0, ushort.MaxValue, value);
    }

    /// <summary>Empties the velocity and speed row, when a kill empties the enemy's common record.</summary>
    internal void ClearWithRecord()
    {
        XVelocity = SignedSixteenSixteen.Zero;
        Speeds = default;
    }
}
