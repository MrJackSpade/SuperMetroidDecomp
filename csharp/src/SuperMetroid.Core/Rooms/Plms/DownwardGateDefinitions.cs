namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>The downward gate's column, sounds and the placement and travel of its gate projectile.</summary>
internal static class DownwardGateDefinitions
{
    /// <summary>Five room blocks are marked solid while the closed gate occupies its column.</summary>
    public const byte GateHeightInBlocks = 5;
    /// <summary>The block behaviour written to every block in the closed gate column.</summary>
    public const byte ClosedGateBts = 0x10;
    /// <summary>The sound queued for a rejected non-blue gate trigger shot.</summary>
    public static readonly SoundEffectId RejectedShotSound = Library2Sound.DudShotOnDoor;
    /// <summary>The sound the gate's movement plays.</summary>
    public static readonly SoundEffectId MovementSound = Library3Sound.GateMoving;

    /// <summary>The gate projectile's accumulated travel, in 8.8 pixels, at which it advances one block: sixteen pixels.</summary>
    public const int OneBlockDistance = 16 * 256;
    /// <summary>Converts the gate PLM's room block coordinates into projectile pixel coordinates.</summary>
    public const int PixelsPerRoomBlock = 16;
    /// <summary>Positions the closed gate projectile four blocks below its PLM origin.</summary>
    public const int ClosedPositionOffsetPixels = 64;
    /// <summary>
    /// The gate projectile records its PLM's block as twice the block index; spawning and the wake search scale the
    /// index by this.
    /// </summary>
    public const int BlockAssociationScale = 2;
}
