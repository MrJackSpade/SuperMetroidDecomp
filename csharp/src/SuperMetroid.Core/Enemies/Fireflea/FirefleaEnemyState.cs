using SuperMetroid.Core.Programs;
namespace SuperMetroid.Core.Enemies.Fireflea;

/// <summary>
/// The state of a Fireflea. The values the cartridge keeps in the enemy's common record are emptied with it when the
/// enemy is killed (<see cref="ClearWithRecord"/>). The speed and the vertical extents are the Fireflea's own too: setup
/// writes the speed, and writes the extents for every Fireflea that moves vertically, the only kind that reads them, so
/// nothing a previous Fireflea in the slot left reaches a new one.
/// </summary>
public sealed class FirefleaEnemyState
{
    /// <summary>The slot the Fireflea occupies, whose spawn settings choose its path.</summary>
    private readonly RoomEnemySlot _slot;

    /// <summary>Starts the state of the Fireflea in <paramref name="slot"/>.</summary>
    /// <param name="slot">The slot the Fireflea occupies.</param>
    internal FirefleaEnemyState(RoomEnemySlot slot) => _slot = slot;

    /// <summary>Where the typed animation (#641) resumes, in place of the native instruction pointer.</summary>
    internal AnimationProgramCursor<FirefleaProgram> Program { get; set; }

    /// <summary>
    /// The signed angular speed selected from the linear speed table, in 256ths of a turn per update with a 16-bit
    /// fraction.  The circle advances by its middle sixteen bits, an 8.8 turn delta.
    /// </summary>
    public SignedSixteenSixteen AngularSpeed { get; internal set; }

    /// <summary>The movement radius in pixels, 8 through 64, selected by parameter two's high byte.</summary>
    public int Radius
    {
        get;
        internal set => field = Ensure.BetweenInclusive(0, ushort.MaxValue, value);
    }

    /// <summary>
    /// The circle angle, wrapping around the turn. Main AI uses its table index as the eight-bit circle angle.
    /// </summary>
    public RotationAngle Angle { get; internal set; }

    /// <summary>
    /// Initial whole-pixel room position used as the fixed circular-motion center.
    /// </summary>
    public Position16<RoomPixels> Center { get; internal set; }

    /// <summary>Upper extent of vertical motion, the spawn Y minus the radius with room-coordinate wraparound; only its Y is meaningful. Passing above it, by the wrapped spatial difference, reverses speed after movement.</summary>
    public Position16<RoomPixels> MinimumYExtent { get; internal set; }

    /// <summary>Lower extent of vertical motion, the spawn Y plus the radius with room-coordinate wraparound; only its Y is meaningful. Reaching or passing it, by the wrapped spatial difference, reverses speed without clamping.</summary>
    public Position16<RoomPixels> MaximumYExtent { get; internal set; }

    /// <summary>The row of the shared linear speed table the Fireflea moves at, from parameter two's low byte.</summary>
    public int SpeedRow { get; internal set; }

    /// <summary>Which of the row's speeds the Fireflea moves at; vertical movement reverses it at each extent.</summary>
    internal EnemySpeedSign SpeedSign { get; set; }

    /// <summary>Whether the spawn settings choose circular motion; otherwise the actor moves vertically between its saved extrema.</summary>
    public bool UsesCircularMovement => _slot.SpawnSettings<FirefleaSpawnSettings>().Path == FirefleaPath.Circling;

    /// <summary>Empties the values the cartridge keeps in the enemy's common record.</summary>
    internal void ClearWithRecord()
    {
        AngularSpeed = SignedSixteenSixteen.Zero;
        Radius = 0;
        Angle = RotationAngle.FromTableIndex(0);
        Center = Position16<RoomPixels>.Zero;
    }
}
