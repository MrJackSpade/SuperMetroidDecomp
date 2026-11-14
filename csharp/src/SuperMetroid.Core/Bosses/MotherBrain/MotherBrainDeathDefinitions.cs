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

    /// <summary>The medium diagonal foreground shake the escape starts and keeps going.</summary>
    public static readonly EarthquakeShake EscapeEarthquake =
        new(EarthquakeStrength.Medium, EarthquakeDirection.Diagonal, EarthquakeReach.Foreground);

    /// <summary>The updates an escape-door fragment lives, counted down until it passes zero.</summary>
    public const int DoorFragmentLifetime = 32;

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

    /// <summary>The escape door's behaviour: it leads through door-list entry one.</summary>
    public static readonly RoomBlockBehavior EscapeDoorBehavior = RoomBlockBehavior.LinkingDoor(1);

    /// <summary>Each escape-door extension's behaviour: a link to the block one row upward.</summary>
    public static readonly RoomBlockBehavior EscapeDoorExtensionBehavior = RoomBlockBehavior.Extending(-1);
}
