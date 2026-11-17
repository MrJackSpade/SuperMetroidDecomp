using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Earthquakes;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The definition data of Mother Brain's death and the escape it starts: the escape door and the subtitle.</summary>
public static class MotherBrainDeathDefinitions
{
    /// <summary>The escape's song, played once the escape's data set has loaded.</summary>
    public const MusicTrack EscapeMusicTrack = MusicTrack.Escape;

    /// <summary>The strong diagonal foreground shake the rainbow beam starts as it pins Samus and while it drains her.</summary>
    public static readonly EarthquakeShake RainbowBeamEarthquake =
        new(EarthquakeStrength.Strong, EarthquakeDirection.Diagonal, EarthquakeReach.Foreground);

    /// <summary>The light diagonal foreground shake the brain starts when it hits the ground.</summary>
    public static readonly EarthquakeShake BrainLandingEarthquake =
        new(EarthquakeStrength.Light, EarthquakeDirection.Diagonal, EarthquakeReach.Foreground);

    /// <summary>The fall speed her severed brain gains each update it falls: an eighth of a pixel an update.</summary>
    public static readonly SignedEightEight BrainFallAcceleration = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(32);

    /// <summary>The medium diagonal foreground shake the escape starts and keeps going.</summary>
    public static readonly EarthquakeShake EscapeEarthquake =
        new(EarthquakeStrength.Medium, EarthquakeDirection.Diagonal, EarthquakeReach.Foreground);

    /// <summary>The updates an escape-door fragment falls; on the last it turns to dust.</summary>
    public const int DoorFragmentLifetimeUpdates = 33;

    /// <summary>The drag subtracted from a door fragment's 1/256-pixel horizontal speed each update.</summary>
    public static readonly SignedEightEight DoorFragmentDrag = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(16);

    /// <summary>The gravity added to a door fragment's 1/256-pixel vertical speed each update.</summary>
    public static readonly SignedEightEight DoorFragmentGravity = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(32);

    /// <summary>The room position the door fragments spawn about.</summary>
    public const int DoorX = 16, DoorY = 128;

    /// <summary>How far above an expiring fragment its dust appears.</summary>
    public const int DoorDustYOffset = 4;

    /// <summary>The dust an expiring door fragment leaves.</summary>
    public const MiscDustAnimation DoorDustParameter = MiscDustAnimation.SmallDustCloud;

    /// <summary>The screen position of the escape subtitle.</summary>
    public const int SubtitleX = 128, SubtitleY = 192;

    /// <summary>
    /// Where the escape door's dust bursts, in the order it bursts (#1872, #2315), continuing the place her burst cycle
    /// stands at and starting again after the last.
    /// </summary>
    internal static IReadOnlyList<Position16<RoomPixels>> EscapeDoorDustPositions { get; } =
    [
        new(24, 116),
        new(9, 144),
        new(24, 128),
        new(8, 108),
    ];

    /// <summary>The updates between the escape door's dust bursts.</summary>
    internal const int EscapeDoorDustIntervalUpdates = 4;

    /// <summary>The block the escape door opens at, in the room's left wall.</summary>
    internal static readonly Position16<RoomBlocks> EscapeDoorBlock = new(0, 6);

    /// <summary>The door the escape door's block leads through: from Mother Brain's room into the escape's first room.</summary>
    public static readonly DoorDefinition EscapeDoor = TourianDoorDefinitions.MotherBrainToTourianEscape1;

    /// <summary>Each escape-door extension's behaviour: a link to the block one row upward.</summary>
    public static readonly RoomBlockBehavior EscapeDoorExtensionBehavior = RoomBlockBehavior.Extending(-1);
}
