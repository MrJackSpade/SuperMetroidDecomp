using SuperMetroid.Core.Animation;
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

    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<FirefleaSegment> Playback { get; set; }

    /// <summary>
    /// The turn the circle makes each update: its spawn speed toward its spawn sign read as a turn
    /// (<see cref="FirefleaBehavior.TurnPerUpdate"/>).
    /// </summary>
    public RotationAngle AngularSpeed { get; internal set; }

    /// <summary>The movement radius in pixels, 8 through 64, from its spawn settings.</summary>
    public int Radius { get; internal set; }

    /// <summary>
    /// The circle angle, wrapping around the turn. Main AI uses its whole step as the circle's trigonometry angle.
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

    /// <summary>The speed the Fireflea moves at, from its spawn settings, toward increasing coordinates.</summary>
    public SignedSixteenSixteen Speed { get; internal set; }

    /// <summary>Which way the Fireflea moves at its speed; vertical movement reverses it at each extent.</summary>
    internal EnemySpeedSign SpeedSign { get; set; }

    /// <summary>The Fireflea's speed in the direction <see cref="SpeedSign"/> names: the speed itself or its negation.</summary>
    internal SignedSixteenSixteen Velocity => SpeedSign switch
    {
        EnemySpeedSign.Positive => Speed,
        EnemySpeedSign.Negative => -Speed,
        _ => throw new InvalidOperationException($"Undefined enemy speed sign {SpeedSign}."),
    };

    /// <summary>Whether the spawn settings choose circular motion; otherwise the actor moves vertically between its saved extrema.</summary>
    public bool UsesCircularMovement => _slot.SpawnSettings<FirefleaSpawnSettings>().Path == FirefleaPath.Circling;
}
