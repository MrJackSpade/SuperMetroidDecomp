namespace SuperMetroid.Core.Enemies.Ripper;

/// <summary>
/// The state of one GRipper or Ripper II: its patrol velocity, its linear-speed record, and GRipper's two patrol
/// bounds. The velocity and speed record are values the cartridge keeps in the enemy's common record, so they are
/// emptied with that record when the enemy is killed (<see cref="ClearWithRecord"/>); the bounds outlive the kill.
/// </summary>
public sealed class RipperVariantEnemyState
{
    /// <summary>The slot's Ripper extension words, kept for the room across occupants.</summary>
    private readonly RipperExtensionWords _words;

    /// <summary>Binds a variant to its slot's GRipper patrol-bound storage.</summary>
    internal RipperVariantEnemyState(RipperExtensionWords words) => _words = words;

    /// <summary>Signed 16.16 horizontal velocity per update, one half of the selected linear-speed record.</summary>
    public SnesSignedSixteenSixteen XVelocity { get; internal set; }

    /// <summary>
    /// The selected linear-speed record's byte offset in the shared speed table, eight bytes per record, set by
    /// initialization (wrapping at the word boundary there, 0 through 65535) and read back when the velocity is loaded.
    /// </summary>
    public int SpeedTableByteOffset { get; internal set; }

    /// <summary>
    /// GRipper extra word $00: exclusive left patrol reversal threshold, a room X coordinate in pixels
    /// from population parameter one. The patrol compares it with a wrapped signed word subtraction.
    /// </summary>
    public int MinimumXPosition
    {
        get => _words.MinimumXPosition;
        internal set => _words.MinimumXPosition = checked((ushort)value);
    }

    /// <summary>
    /// GRipper extra word $01: inclusive right patrol reversal threshold, a room X coordinate in pixels
    /// from population parameter two. The patrol compares it with a wrapped signed word subtraction.
    /// </summary>
    public int MaximumXPosition
    {
        get => _words.MaximumXPosition;
        internal set => _words.MaximumXPosition = checked((ushort)value);
    }

    /// <summary>Empties the velocity and speed record, when a kill empties the enemy's common record.</summary>
    internal void ClearWithRecord()
    {
        XVelocity = SnesSignedSixteenSixteen.Zero;
        SpeedTableByteOffset = 0;
    }
}
