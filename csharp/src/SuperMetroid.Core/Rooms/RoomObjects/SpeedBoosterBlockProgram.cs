namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>The Speed Booster blocks' PLM programs (#1296), in place of their lists.</summary>
internal enum SpeedBoosterBlockProgram
{
    /// <summary>Crumbles slowly and grows back.</summary>
    BrinstarSlowRespawning,
    /// <summary>Crumbles and grows back.</summary>
    Respawning,
    /// <summary>Crumbles and grows back through the cloned restore.</summary>
    DachoraRespawning,
    /// <summary>Crumbles slowly for good.</summary>
    BrinstarSlowPermanent,
    /// <summary>Crumbles for good.</summary>
    Permanent,
    /// <summary>A bomb reveals the block for one frame.</summary>
    BombReveal,
}
