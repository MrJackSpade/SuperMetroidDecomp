using System.Collections.Immutable;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Earthquakes;

namespace SuperMetroid.Core.Fx;

/// <summary>How the room's earthquake shakes, how rising lava and acid rumble, and the rooms whose quakes are silent (#1415).</summary>
public static class RoomEarthquakeDefinitions
{
    /// <summary>
    /// The sides a shaking room shows in turn, one each frame (#2421): two frames on the far side, then two on its shake's
    /// own, over and over. A shake for a length runs through them as its frames run down, and a continuous shake goes on
    /// from where they stood.
    /// </summary>
    public static ImmutableArray<EarthquakeSide> ShakeSides { get; } =
        [EarthquakeSide.Far, EarthquakeSide.Far, EarthquakeSide.Near, EarthquakeSide.Near];

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
        new(EarthquakeStrength.Light, EarthquakeDirection.Horizontal, EarthquakeReach.RoomBlocks);

    /// <summary>The medium side-to-side earthquake, enemies included, every active lava or acid rise sustains.</summary>
    public static readonly EarthquakeShake RisingLiquidShake =
        new(EarthquakeStrength.Medium, EarthquakeDirection.Horizontal, EarthquakeReach.RoomBlocksSceneryAndEnemies);

    /// <summary>
    /// Base delays, in updates, between rising lava's or acid's repeated rumble sounds, in emission order. The live random
    /// <see cref="RisingLiquidSoundJitter"/> is added at each emission.
    /// </summary>
    /// <remarks>
    /// The authored irregular rhythm of the repeated rumble sound: each emission takes the next delay, whatever the
    /// rumble's strength or Samus's place, and the cycle has neither a ramp nor a repeating half. The live random jitter
    /// varies it further.
    /// </remarks>
    public static IReadOnlyList<int> RisingLiquidSoundBaseDelays { get; } =
        [1, 3, 2, 1, 1, 2, 2, 1];

    /// <summary>
    /// The random selection, read from the current sample without drawing, of the extra updates added to a rumble
    /// sound's base delay: 0 through 3.
    /// </summary>
    internal static readonly RandomSampleField RisingLiquidSoundJitter = new(0, 2);

    /// <summary>
    /// The six rooms whose quakes are silent: entering one silences quake sounds as its room effect loads, and every other
    /// room keeps them.
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
