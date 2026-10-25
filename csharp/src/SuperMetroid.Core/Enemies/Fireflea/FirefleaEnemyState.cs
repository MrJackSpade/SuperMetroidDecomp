using SuperMetroid.Core.Programs;
namespace SuperMetroid.Core.Enemies.Fireflea;

/// <summary>
/// The state of a Fireflea, with its three per-slot words in extra WRAM <c>$7E:7800</c>, which stay in parallel arrays
/// because they are genuinely outside the 64-byte enemy record on the cartridge. The values the cartridge keeps in the
/// enemy's common record are emptied with it when the enemy is killed (<see cref="ClearWithRecord"/>).
/// </summary>
public sealed class FirefleaEnemyState
{
    /// <summary>Common 64-byte enemy record holding variables A through F.</summary>
    private readonly RoomEnemySlot _slot;
    /// <summary>The slot's Fireflea extension words, kept for the room across occupants.</summary>
    private readonly FirefleaExtensionWords _words;

    /// <summary>Binds a physical enemy record to its three parallel extra-WRAM state arrays.</summary>
    internal FirefleaEnemyState(
        RoomEnemySlot slot, FirefleaExtensionWords words)
    {
        _slot = slot;
        _words = words;
    }

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
        internal set => field = checked((ushort)value);
    }

    /// <summary>
    /// The circle angle, wrapping around the turn. Main AI uses its table index as the eight-bit circle angle.
    /// </summary>
    public RotationAngle Angle { get; internal set; }

    /// <summary>
    /// Native Fireflea.XCenter/YCenter: initial whole-pixel room position used as the fixed circular-motion center.
    /// </summary>
    public Position16<RoomPixels> Center { get; internal set; }

    /// <summary>Per-slot upper extent of vertical motion, the spawn Y minus the radius with room-coordinate wraparound; only its Y is meaningful. Passing above it, by the wrapped spatial difference, reverses speed after movement.</summary>
    public Position16<RoomPixels> MinimumYExtent
    {
        get => _words.MinimumYExtent;
        internal set => _words.MinimumYExtent = value;
    }

    /// <summary>Per-slot lower extent of vertical motion, the spawn Y plus the radius with room-coordinate wraparound; only its Y is meaningful. Reaching or passing it, by the wrapped spatial difference, reverses speed without clamping.</summary>
    public Position16<RoomPixels> MaximumYExtent
    {
        get => _words.MaximumYExtent;
        internal set => _words.MaximumYExtent = value;
    }

    /// <summary>Byte offset of the current speed entry in the linear speed table, 0 through 2044: eight per speed selector, plus four for the negative half, which reversal toggles.</summary>
    public int SpeedTableIndex
    {
        get => _words.SpeedTableIndex;
        internal set => _words.SpeedTableIndex = value;
    }

    /// <summary>Native init0/parameter-one bit $0002 selects circular motion; otherwise the actor moves vertically between its saved extrema.</summary>
    public bool UsesCircularMovement => (_slot.Spawn.Population.Parameter1 & FirefleaMovementDefinitions.CircularMovementParameterBit) != 0;

    /// <summary>Empties the values the cartridge keeps in the enemy's common record.</summary>
    internal void ClearWithRecord()
    {
        AngularSpeed = SignedSixteenSixteen.Zero;
        Radius = 0;
        Angle = RotationAngle.FromTableIndex(0);
        Center = Position16<RoomPixels>.Zero;
    }
}
