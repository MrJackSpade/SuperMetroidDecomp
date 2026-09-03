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
    Eye,
    Door,
    Bottom,
}

/// <summary>Mirrored horizontal orientation of an eye-door component.</summary>
public enum EyeDoorOrientation : byte
{
    Left,
    Right,
}

/// <summary>One bank-$84 request to allocate an actor from bank $86's shared projectile pool.</summary>
public readonly record struct EyeDoorProjectileRequest(
    ushort DefinitionPointer,
    ushort Parameter,
    int PlmBlockIndex,
    ushort DoorBit);
