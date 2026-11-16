using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.Fireflea;

/// <summary>
/// The state of a Fireflea. The speed and the vertical extents are the Fireflea's own: setup writes the speed, and
/// writes the extents for every Fireflea that moves vertically, the only kind that reads them, so nothing a previous
/// Fireflea in the place left reaches a new one.
/// </summary>
public sealed class FirefleaEnemyState
{
    /// <summary>The place the Fireflea occupies, whose spawn settings choose its path.</summary>
    private readonly RoomEnemyPlace _place;

    /// <summary>Starts the state of the Fireflea in <paramref name="place"/>.</summary>
    /// <param name="place">The place the Fireflea occupies.</param>
    internal FirefleaEnemyState(RoomEnemyPlace place) => _place = place;

    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<FirefleaSegment> Playback { get; set; }

    /// <summary>How far, and which way, a circling Fireflea turns around its circle each update, from its spawn settings.</summary>
    public AngularSpeed AngularSpeed { get; internal set; }

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

    /// <summary>Upper extent of vertical motion, the spawn Y minus the radius with room-coordinate wraparound. Passing above it, by the wrapped difference, reverses speed after movement.</summary>
    public int MinimumY { get; internal set; }

    /// <summary>Lower extent of vertical motion, the spawn Y plus the radius with room-coordinate wraparound. Reaching or passing it, by the wrapped difference, reverses speed without clamping.</summary>
    public int MaximumY { get; internal set; }

    /// <summary>The speed a bobbing Fireflea moves at, from its spawn settings, toward increasing coordinates.</summary>
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
    public bool UsesCircularMovement => _place.SpawnSettings<FirefleaSpawnSettings>().Flight is FirefleaFlight.Circling;
}
