namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>The Speed Booster blocks' PLM programs (#1296), in place of their bank-$84 lists at $84:C928..C9F8.</summary>
internal enum SpeedBoosterBlockProgram
{
    /// <summary>Crumbles slowly and grows back (<c>InstList_PLM_RespawningSpeedBlock_SlowerCrumbleAnimation</c>).</summary>
    BrinstarSlowRespawning,
    /// <summary>Crumbles and grows back (<c>InstList_PLM_RespawningSpeedBoostBlock</c>).</summary>
    Respawning,
    /// <summary>Crumbles and grows back through the cloned restore (<c>InstList_PLM_RespawningSpeedBlock</c>).</summary>
    DachoraRespawning,
    /// <summary>Crumbles slowly for good (<c>InstList_PLM_SpeedBlockSlowerCrumbleAnimation</c>).</summary>
    BrinstarSlowPermanent,
    /// <summary>Crumbles for good (<c>InstList_PLM_SpeedBoostBlock</c>).</summary>
    Permanent,
    /// <summary>A bomb reveals the block for one frame (<c>InstList_PLM_BombReaction_SpeedBlock</c>).</summary>
    BombReveal,
}
