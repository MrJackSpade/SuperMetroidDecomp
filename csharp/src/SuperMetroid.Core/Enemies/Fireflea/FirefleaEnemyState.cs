using SuperMetroid.Core.Programs;
namespace SuperMetroid.Core.Enemies.Fireflea;

/// <summary>
/// The state of a Fireflea. The speed and the vertical extents are the Fireflea's own: setup writes the speed, and
/// writes the extents for every Fireflea that moves vertically, the only kind that reads them, so nothing a previous
/// Fireflea in the slot left reaches a new one.
/// </summary>
public sealed class FirefleaEnemyState
{
    /// <summary>The slot the Fireflea occupies, whose spawn settings choose its path.</summary>
    private readonly RoomEnemySlot _slot;

    /// <summary>Starts the state of the Fireflea in <paramref name="slot"/>.</summary>
    /// <param name="slot">The slot the Fireflea occupies.</param>
    internal FirefleaEnemyState(RoomEnemySlot slot) => _slot = slot;

    /// <summary>Where the animation resumes.</summary>
    internal AnimationProgramCursor<FirefleaProgram> Program { get; set; }

    /// <summary>
    /// The signed angular speed, its spawn speed toward its spawn sign, in 256ths of a turn per update with a fraction
    /// in 65536ths. The circle advances by an 8.8 turn delta: its whole 256ths and the fraction's top 256ths.
    /// </summary>
    public SignedSixteenSixteen AngularSpeed { get; internal set; }

    /// <summary>The movement radius in pixels, 8 through 64, from its spawn settings.</summary>
    public int Radius { get; internal set; }

    /// <summary>
    /// The circle angle, wrapping around the turn. Main AI uses its table index as the circle's trigonometry angle.
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

    /// <summary>The speed the Fireflea moves at, from its spawn settings, and its negation.</summary>
    public EnemyLinearSpeedRow Speeds { get; internal set; }

    /// <summary>Which of its speeds the Fireflea moves at; vertical movement reverses it at each extent.</summary>
    internal EnemySpeedSign SpeedSign { get; set; }

    /// <summary>Whether the spawn settings choose circular motion; otherwise the actor moves vertically between its saved extrema.</summary>
    public bool UsesCircularMovement => _slot.SpawnSettings<FirefleaSpawnSettings>().Path == FirefleaPath.Circling;
}
