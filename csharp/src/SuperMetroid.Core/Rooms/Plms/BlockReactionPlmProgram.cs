namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// The block-reaction PLMs' programs (#1296): crumble reveals, contact crumbles and the shot, Super Missile, Power Bomb,
/// bomb and grapple blocks, in place of their bank-$84 lists. A <c>Break</c> program is the bomb blocks' shared tail,
/// which normal bombs enter past the reaction sound.
/// </summary>
internal enum BlockReactionPlmProgram
{
    /// <summary>The CrumbleReveal1x1 list.</summary>
    CrumbleReveal1x1,
    /// <summary>The CrumbleReveal2x1 list.</summary>
    CrumbleReveal2x1,
    /// <summary>The CrumbleReveal1x2 list.</summary>
    CrumbleReveal1x2,
    /// <summary>The CrumbleReveal2x2 list.</summary>
    CrumbleReveal2x2,
    /// <summary>The BombedPowerBombBlockUnused list.</summary>
    BombedPowerBombBlockUnused,
    /// <summary>The BombedSuperMissileBlockUnused list.</summary>
    BombedSuperMissileBlockUnused,
    /// <summary>The ContactCrumble1x1Respawning list.</summary>
    ContactCrumble1x1Respawning,
    /// <summary>The ContactCrumble2x1Respawning list.</summary>
    ContactCrumble2x1Respawning,
    /// <summary>The ContactCrumble1x2Respawning list.</summary>
    ContactCrumble1x2Respawning,
    /// <summary>The ContactCrumble2x2Respawning list.</summary>
    ContactCrumble2x2Respawning,
    /// <summary>The ContactCrumble1x1Permanent list.</summary>
    ContactCrumble1x1Permanent,
    /// <summary>The ContactCrumble2x1Permanent list.</summary>
    ContactCrumble2x1Permanent,
    /// <summary>The ContactCrumble1x2Permanent list.</summary>
    ContactCrumble1x2Permanent,
    /// <summary>The ContactCrumble2x2Permanent list.</summary>
    ContactCrumble2x2Permanent,
    /// <summary>The RespawningShotBlock1x1 list.</summary>
    RespawningShotBlock1x1,
    /// <summary>The RespawningShotBlock2x1 list.</summary>
    RespawningShotBlock2x1,
    /// <summary>The RespawningShotBlock1x2 list.</summary>
    RespawningShotBlock1x2,
    /// <summary>The RespawningShotBlock2x2 list.</summary>
    RespawningShotBlock2x2,
    /// <summary>The RespawningSuperMissileBlock list.</summary>
    RespawningSuperMissileBlock,
    /// <summary>The RespawningPowerBombBlock list.</summary>
    RespawningPowerBombBlock,
    /// <summary>The PermanentShotBlock1x1 list.</summary>
    PermanentShotBlock1x1,
    /// <summary>The PermanentShotBlock2x1 list.</summary>
    PermanentShotBlock2x1,
    /// <summary>The PermanentShotBlock1x2 list.</summary>
    PermanentShotBlock1x2,
    /// <summary>The PermanentShotBlock2x2 list.</summary>
    PermanentShotBlock2x2,
    /// <summary>The PermanentSuperMissileBlock list.</summary>
    PermanentSuperMissileBlock,
    /// <summary>The PermanentPowerBombBlock list.</summary>
    PermanentPowerBombBlock,
    /// <summary>The CollisionBombBlock1x1Respawning list.</summary>
    CollisionBombBlock1x1Respawning,
    /// <summary>The ReactionBombBlock1x1Respawning list.</summary>
    ReactionBombBlock1x1Respawning,
    /// <summary>The BombBlock1x1RespawningBreak list.</summary>
    BombBlock1x1RespawningBreak,
    /// <summary>The CollisionBombBlock2x1Respawning list.</summary>
    CollisionBombBlock2x1Respawning,
    /// <summary>The ReactionBombBlock2x1Respawning list.</summary>
    ReactionBombBlock2x1Respawning,
    /// <summary>The BombBlock2x1RespawningBreak list.</summary>
    BombBlock2x1RespawningBreak,
    /// <summary>The CollisionBombBlock1x2Respawning list.</summary>
    CollisionBombBlock1x2Respawning,
    /// <summary>The ReactionBombBlock1x2Respawning list.</summary>
    ReactionBombBlock1x2Respawning,
    /// <summary>The BombBlock1x2RespawningBreak list.</summary>
    BombBlock1x2RespawningBreak,
    /// <summary>The CollisionBombBlock2x2Respawning list.</summary>
    CollisionBombBlock2x2Respawning,
    /// <summary>The ReactionBombBlock2x2Respawning list.</summary>
    ReactionBombBlock2x2Respawning,
    /// <summary>The BombBlock2x2RespawningBreak list.</summary>
    BombBlock2x2RespawningBreak,
    /// <summary>The CollisionBombBlock1x1Permanent list.</summary>
    CollisionBombBlock1x1Permanent,
    /// <summary>The ReactionBombBlock1x1Permanent list.</summary>
    ReactionBombBlock1x1Permanent,
    /// <summary>The BombBlock1x1PermanentBreak list.</summary>
    BombBlock1x1PermanentBreak,
    /// <summary>The CollisionBombBlock2x1Permanent list.</summary>
    CollisionBombBlock2x1Permanent,
    /// <summary>The ReactionBombBlock2x1Permanent list.</summary>
    ReactionBombBlock2x1Permanent,
    /// <summary>The BombBlock2x1PermanentBreak list.</summary>
    BombBlock2x1PermanentBreak,
    /// <summary>The CollisionBombBlock1x2Permanent list.</summary>
    CollisionBombBlock1x2Permanent,
    /// <summary>The ReactionBombBlock1x2Permanent list.</summary>
    ReactionBombBlock1x2Permanent,
    /// <summary>The BombBlock1x2PermanentBreak list.</summary>
    BombBlock1x2PermanentBreak,
    /// <summary>The CollisionBombBlock2x2Permanent list.</summary>
    CollisionBombBlock2x2Permanent,
    /// <summary>The ReactionBombBlock2x2Permanent list.</summary>
    ReactionBombBlock2x2Permanent,
    /// <summary>The BombBlock2x2PermanentBreak list.</summary>
    BombBlock2x2PermanentBreak,
    /// <summary>The RespawningBreakableGrappleBlock list.</summary>
    RespawningBreakableGrappleBlock,
    /// <summary>The PermanentBreakableGrappleBlock list.</summary>
    PermanentBreakableGrappleBlock,
}
