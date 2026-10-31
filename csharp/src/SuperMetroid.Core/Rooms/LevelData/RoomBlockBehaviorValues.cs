namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>
/// Block behaviours several collision families name. These are typed values rather than a global enum because equal
/// parameters mean different things to different collision kinds.
/// </summary>
public static class RoomBlockBehaviorValues
{
    /// <summary>Zero terminates an extension chain and is the first entry in many tables.</summary>
    public static readonly RoomBlockBehavior None = new(0);

    /// <summary>Left-facing blue-door shootable-cap dispatcher.</summary>
    public static readonly RoomBlockBehavior BlueDoorFacingLeft = new(64);

    /// <summary>Right-facing blue-door shootable-cap dispatcher.</summary>
    public static readonly RoomBlockBehavior BlueDoorFacingRight = new(65);

    /// <summary>Resident colored-door or special PLM projectile-notification seam.</summary>
    public static readonly RoomBlockBehavior ResidentPlmProjectileTrigger = new(68);

    /// <summary>Resident permanent-collectible touch/projectile-notification seam.</summary>
    public static readonly RoomBlockBehavior CollectibleTrigger = new(69);

    /// <summary>Resident scroll-trigger PLM collision seam.</summary>
    public static readonly RoomBlockBehavior ScrollTrigger = new(70);

    /// <summary>Entry 16 on a shootable block: the collision probe's no-op sentinel.</summary>
    public static readonly RoomBlockBehavior ShootableCollisionProbe = new(16);
}
