using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Earthquakes;

namespace SuperMetroid.Core.Fx;

/// <summary>How the room's earthquake shakes, how rising lava and acid rumble, and the rooms whose quakes are silent (#1415).</summary>
public static class RoomEarthquakeDefinitions
{
    /// <summary>Frames the shake shows on one side of the origin before it switches to the other.</summary>
    public const int StepsPerShakeSide = 2;

    /// <summary>Updates for which an enemy retains one applied shake displacement.</summary>
    public const int EnemyShakeDuration = 2;

    /// <summary>
    /// The frames a sustained earthquake keeps shaking at least (<see cref="RoomEarthquake.Sustain"/>): rising lava and
    /// acid and the Tourian statues renew their earthquake for this long every update they shake the room.
    /// </summary>
    public const int SustainedShakeFrames = 32;

    /// <summary>
    /// The light side-to-side shake of the foreground alone that a room no producer has shaken yet shows when it is made
    /// to shake: the Zebes escape's continuous shaking before its rumbles give a shake. It is the shake a new game
    /// starts with.
    /// </summary>
    public static readonly EarthquakeShake UnshakenRoomShake =
        new(EarthquakeStrength.Light, EarthquakeDirection.Horizontal, EarthquakeReach.Foreground);

    /// <summary>The medium side-to-side earthquake, enemies included, every active lava or acid rise sustains.</summary>
    public static readonly EarthquakeShake RisingLiquidShake =
        new(EarthquakeStrength.Medium, EarthquakeDirection.Horizontal, EarthquakeReach.ForegroundBackgroundAndEnemies);

    /// <summary>
    /// Base delays, in updates, between rising lava's or acid's repeated rumble sounds, in emission order. The live random
    /// <see cref="RisingLiquidSoundJitter"/> is added at each emission.
    /// </summary>
    /// <remarks>
    /// Retained as the particular irregular rhythm of repeated identical rumble
    /// sounds. The index advances chronologically by one emission, not by angle,
    /// distance, intensity or a semantic sound choice. The complete cycle has
    /// neither a ramp nor half-cycle reflection/repetition; equal delay values
    /// have different successors. Live RNG supplies separate variation. Reciting
    /// these eight choices in cases or fitting them would merely disguise the
    /// same rhythmic content, while replacing the rhythm changes sound cadence.
    /// This is the arbitrary-sequence/nonsense exception, not a cost or size exemption.
    /// </remarks>
    public static IReadOnlyList<int> RisingLiquidSoundBaseDelays { get; } =
        [1, 3, 2, 1, 1, 2, 2, 1];

    /// <summary>
    /// The random selection, read from the current sample without drawing, of the extra updates added to a rumble
    /// sound's base delay: 0 through 3.
    /// </summary>
    internal static readonly RandomSampleField RisingLiquidSoundJitter = new(0, 2);

    /// <summary>
    /// The six rooms whose special-FX initialization silences quake sounds, in the order the game compares them;
    /// every other room keeps them.
    /// </summary>
    public static IReadOnlyList<RoomDefinition> SoundSuppressedRooms { get; } =
    [
        RoomDefinitions.BombTorizo,
        RoomDefinitions.Climb,
        RoomDefinitions.Ridley,
        RoomDefinitions.Pillar,
        RoomDefinitions.MotherBrain,
        RoomDefinitions.TourianEscape4,
    ];
}
