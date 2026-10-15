namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>
/// Verified singleton BTS meanings used by several collision-family dispatchers. These
/// are typed values rather than a global enum because equal bytes can mean different
/// things after a different collision nibble selects another native routine.
/// </summary>
public static class RoomBlockBehaviorValues
{
    /// <summary>Zero terminates an extension chain and is the first entry in many tables.</summary>
    public static readonly RoomBlockBehavior None = new(0x00);

    /// <summary>Left-facing blue-door shootable-cap dispatcher.</summary>
    public static readonly RoomBlockBehavior BlueDoorFacingLeft = new(0x40);

    /// <summary>Right-facing blue-door shootable-cap dispatcher.</summary>
    public static readonly RoomBlockBehavior BlueDoorFacingRight = new(0x41);

    /// <summary>Resident colored-door or special PLM projectile-notification seam.</summary>
    public static readonly RoomBlockBehavior ResidentPlmProjectileTrigger = new(0x44);

    /// <summary>Resident permanent-collectible touch/projectile-notification seam.</summary>
    public static readonly RoomBlockBehavior CollectibleTrigger = new(0x45);

    /// <summary>Resident scroll-trigger PLM collision seam.</summary>
    public static readonly RoomBlockBehavior ScrollTrigger = new(0x46);

    /// <summary>BTS $10 on a shootable block: the collision probe's no-op sentinel.</summary>
    public static readonly RoomBlockBehavior ShootableCollisionProbe = new(0x10);
}
