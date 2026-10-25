using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The definition data of Mother Brain's death and the escape it starts: its colour fades, the escape door and the subtitle.</summary>
public static class MotherBrainDeathDefinitions
{
    /// <summary>The body and leg fade images, shown in order before the body is cleared.</summary>
    public const int BodyFadeFrameCount = 16;

    /// <summary>The corpse-head fade images, shown in order while the corpse rots away.</summary>
    public const int CorpseFadeFrameCount = 8;

    /// <summary>The colours each body or leg fade image rewrites; the exploded door's colours are as many.</summary>
    public const int BodyColorCount = 14;

    /// <summary>The colours each corpse fade image rewrites.</summary>
    public const int CorpseColorCount = 15;

    /// <summary>The music track the escape starts.</summary>
    public const byte EscapeMusicTrack = 7;

    /// <summary>The updates an escape-door fragment lives, counted through signed underflow.</summary>
    public const ushort DoorFragmentLifetime = 32;

    /// <summary>The drag subtracted from a door fragment's 8.8 horizontal speed each update.</summary>
    public const int DoorFragmentDrag = 16;

    /// <summary>The gravity added to a door fragment's 8.8 vertical speed each update.</summary>
    public const int DoorFragmentGravity = 32;

    /// <summary>The room position the door fragments spawn about.</summary>
    public const int DoorX = 16, DoorY = 128;

    /// <summary>How far above an expiring fragment its dust appears.</summary>
    public const int DoorDustYOffset = 4;

    /// <summary>The dust an expiring door fragment leaves.</summary>
    public const MiscDustAnimation DoorDustParameter = MiscDustAnimation.SmallDustCloud;

    /// <summary>The screen position of the escape subtitle.</summary>
    public const int SubtitleX = 128, SubtitleY = 192;

    /// <summary>The door-list entry the escape door leads through.</summary>
    public const int EscapeDoorListIndex = 1;

    /// <summary>The block each escape-door extension links to: one block upward.</summary>
    public const sbyte EscapeDoorExtensionOffset = -1;
}
