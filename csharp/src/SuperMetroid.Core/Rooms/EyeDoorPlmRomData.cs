namespace SuperMetroid.Core.Rooms;

/// <summary>Named bank-$84 state-machine identities shared by both eye-door orientations.</summary>
internal static class EyeDoorPlmRomData
{
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
