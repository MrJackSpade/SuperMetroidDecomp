using SuperMetroid.Core.Game;
namespace SuperMetroid.Core.Rooms;

/// <summary>Named bank-$84 state-machine identities shared by both eye-door orientations.</summary>
internal static class EyeDoorPlmRomData
{
    /// <summary>Pre-instruction <c>$84:D753</c>, used by passive components awaiting the door bit.</summary>
    public const ushort WakeWhenDoorBitSetPreInstruction = 0xd753;

    /// <summary>Pre-instruction <c>$84:BD50</c>, accepting missiles and Super Missiles.</summary>
    public const ushort MissileHitPreInstruction = 0xbd50;

    /// <summary>Library-two firing sound queued by <c>$84:D77A</c>.</summary>
    public const byte ProjectileSound = 0x4c;

    /// <summary>Library-two rejected-shot sound queued by <c>$84:BD50</c>.</summary>
    public const byte RejectedShotSound = 0x57;

    /// <summary>Parameter consumed by each randomized pair smoke actor.</summary>
    public const ushort RandomizedSmokeParameter = 0x030a;

    /// <summary>Parameter consumed by the centered terminal smoke actor.</summary>
    public const ushort CenteredSmokeParameter = 0x000b;

    /// <summary>Native collision/BTS word installed at the live eye origin.</summary>
    public const ushort EyeCollisionWord = 0xc044;

    /// <summary>Native vertical extension immediately below the eye.</summary>
    public const ushort EyeExtensionWord = 0xd0ff;

    /// <summary>Native special-solid word installed by the passive door components.</summary>
    public const ushort ClosedComponentWord = 0xa000;

    /// <summary>Number of vertical extension cells constructed beneath the blue-door cap.</summary>
    public const int BlueDoorExtensionCount = 3;
}

/// <summary>The three independently authored PLMs which together form one eye door.</summary>
public enum EyeDoorComponent : byte
{
    /// <summary>Live eye PLM, native headers $84:DB48/$DB56: receives missile hits, fires attack/sweat actors, sets the persistent opened-door bit, and ultimately becomes the ordinary blue-door cap.</summary>
    Eye,
    /// <summary>Passive main door body, native headers $84:DB4C/$DB5A: installs special-solid collision and follows the shared opened-door bit rather than accepting the eye's projectile hits itself.</summary>
    Door,
    /// <summary>Separately animated lower component, native headers $84:DB52/$DB60: installs special-solid collision and waits for the eye-owned opened-door bit before its terminal removal.</summary>
    Bottom,
}

/// <summary>Mirrored horizontal orientation of an eye-door component.</summary>
public enum EyeDoorOrientation : byte
{
    /// <summary>Left-facing native component set $84:DB56/$DB5A/$DB60, selecting mirrored artwork, projectile operands, and the left-facing blue-door replacement.</summary>
    Left,
    /// <summary>Right-facing native component set $84:DB48/$DB4C/$DB52, selecting mirrored artwork, projectile operands, and the right-facing blue-door replacement.</summary>
    Right,
}

/// <summary>One bank-$84 request to allocate an actor from bank $86's shared projectile pool.</summary>
/// <param name="DefinitionPointer">Bank-$86 definition word: attack $B743, sweat $B751, or shared PLM dust/smoke $E517; allocation and actor lifetime belong to the enemy-projectile owner.</param>
/// <param name="Parameter">Definition-specific native operand: an even byte offset into attack-origin or sweat-velocity words, or packed dust/smoke animation and placement flags.</param>
/// <param name="PlmBlockIndex">Zero-based row-major room-cell index of the spawning PLM, in 16-pixel blocks rather than the native level-word byte offset; the consumer supplies the room row stride.</param>
/// <param name="DoorBit">Raw spawning PLM room-argument word; attack initialization retains it for the persistent opened-door-bit check, while sweat and smoke do not consume it.</param>
public readonly record struct EyeDoorProjectileRequest(
    EyeDoorProjectileDefinition DefinitionPointer,
    ushort Parameter,
    int PlmBlockIndex,
    ushort DoorBit);
