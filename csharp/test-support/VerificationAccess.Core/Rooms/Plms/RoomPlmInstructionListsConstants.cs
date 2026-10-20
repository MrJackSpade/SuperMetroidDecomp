namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Cartridge values and helpers of <see cref="RoomPlmInstructionLists"/> that only verification reads.</summary>
internal static class RoomPlmInstructionListsConstants
{
    /// <summary>Maridia elevatube's delay and sound list at $84:B8F0.</summary>
    public const ushort MaridiaElevatube = 0xb8f0;
    /// <summary>
    /// <c>$84:BB44 InstList_PLM_GateThatClosesDuringEscapeAfterMotherBrain_1</c>:
    /// animate open, half-closed, and closed at two frames apiece, then release the slot.
    /// </summary>
    public const ushort MotherBrainEscapeRoomGateClosing = 0xbb44;
    /// <summary><c>$84:C8EC InstList_BombReaction_PLM_1x1RespawningCrumbleBlock</c>: reveals one crumble block for one frame, then deletes without breaking it.</summary>
    public const ushort CrumbleReveal1x1 = 0xc8ec;
    /// <summary><c>$84:C8F2 InstList_BombReaction_PLM_2x1RespawningCrumbleBlock</c>: reveals a horizontal two-block crumble parent, then deletes.</summary>
    public const ushort CrumbleReveal2x1 = 0xc8f2;
    /// <summary><c>$84:C8F8 InstList_BombReaction_PLM_1x2RespawningCrumbleBlock</c>: reveals a vertical two-block crumble parent, then deletes.</summary>
    public const ushort CrumbleReveal1x2 = 0xc8f8;
    /// <summary><c>$84:C8FE InstList_BombReaction_PLM_2x2RespawningCrumbleBlock</c>: reveals a square four-block crumble parent, then deletes.</summary>
    public const ushort CrumbleReveal2x2 = 0xc8fe;
    /// <summary>Respawning 1x1 contact-crumble list at <c>$84:C9F9</c>.</summary>
    public const ushort ContactCrumble1x1Respawning = 0xc9f9;
    /// <summary>Respawning 2x1 contact-crumble list at <c>$84:CA1C</c>.</summary>
    public const ushort ContactCrumble2x1Respawning = 0xca1c;
    /// <summary>Respawning 1x2 contact-crumble list at <c>$84:CA41</c>.</summary>
    public const ushort ContactCrumble1x2Respawning = 0xca41;
    /// <summary>Respawning 2x2 contact-crumble list at <c>$84:CA66</c>.</summary>
    public const ushort ContactCrumble2x2Respawning = 0xca66;
    /// <summary>Permanent 1x1 contact-crumble list at <c>$84:CA8B</c>.</summary>
    public const ushort ContactCrumble1x1Permanent = 0xca8b;
    /// <summary>Permanent 2x1 contact-crumble list at <c>$84:CAA0</c>.</summary>
    public const ushort ContactCrumble2x1Permanent = 0xcaa0;
    /// <summary>Permanent 1x2 contact-crumble list at <c>$84:CAB5</c>.</summary>
    public const ushort ContactCrumble1x2Permanent = 0xcab5;
    /// <summary>Permanent 2x2 contact-crumble list at <c>$84:CACA</c>.</summary>
    public const ushort ContactCrumble2x2Permanent = 0xcaca;
    /// <summary><c>$84:C91C UNUSED_InstList_PLM_PowerBombBlockBombed_84C91C</c>: bomb setup uses this one-frame reveal/delete list despite its unused native name.</summary>
    public const ushort BombedPowerBombBlockUnused = 0xc91c;
    /// <summary><c>$84:C922 UNUSED_InstList_PLM_SuperMissileBlockBombed_84C922</c>: bomb setup reveals the gated Super Missile parent without breaking it, then deletes.</summary>
    public const ushort BombedSuperMissileBlockUnused = 0xc922;
    /// <summary><c>$84:CADF InstList_PLM_1x1RespawningShotBlock</c>: sounds and animates a single block's break, waits 384 ticks, then restores its saved level word.</summary>
    public const ushort RespawningShotBlock1x1 = 0xcadf;
    /// <summary><c>$84:CB02 InstList_PLM_2x1RespawningShotBlock</c>: breaks a horizontal pair, waits 384 ticks, and restores its linked two-block layout.</summary>
    public const ushort RespawningShotBlock2x1 = 0xcb02;
    /// <summary><c>$84:CB27 InstList_PLM_1x2RespawningShotBlock</c>: breaks a vertical pair, waits 384 ticks, and restores its linked two-block layout.</summary>
    public const ushort RespawningShotBlock1x2 = 0xcb27;
    /// <summary><c>$84:CB4C InstList_PLM_2x2RespawningShotBlock</c>: breaks a four-block square, waits 384 ticks, and restores its linked layout.</summary>
    public const ushort RespawningShotBlock2x2 = 0xcb4c;
    /// <summary><c>$84:CB71 InstList_PLM_RespawningSuperMissileBlock</c>: breaks the accepted Super Missile parent with library-two maximum-six sound routing, then restores after the respawn wait.</summary>
    public const ushort RespawningSuperMissileBlock = 0xcb71;
    /// <summary><c>$84:CB94 InstList_PLM_RespawningPowerBombBlock</c>: animates the accepted Power Bomb parent's break and restores its saved level word after the respawn wait.</summary>
    public const ushort RespawningPowerBombBlock = 0xcb94;
    /// <summary><c>$84:CBB7 InstList_PLM_1x1ShotBlock</c>: sounds and animates a single block's permanent removal, then deletes without restoration.</summary>
    public const ushort PermanentShotBlock1x1 = 0xcbb7;
    /// <summary><c>$84:CBCC InstList_PLM_2x1ShotBlock</c>: animates permanent removal of a horizontal pair, then deletes without restoration.</summary>
    public const ushort PermanentShotBlock2x1 = 0xcbcc;
    /// <summary><c>$84:CBE1 InstList_PLM_1x2ShotBlock</c>: animates permanent removal of a vertical pair, then deletes without restoration.</summary>
    public const ushort PermanentShotBlock1x2 = 0xcbe1;
    /// <summary><c>$84:CBF6 InstList_PLM_2x2ShotBlock</c>: animates permanent removal of a four-block square, then deletes without restoration.</summary>
    public const ushort PermanentShotBlock2x2 = 0xcbf6;
    /// <summary><c>$84:CC0B InstList_PLM_SuperMissileBlock</c>: sounds and animates the accepted Super Missile parent's permanent removal.</summary>
    public const ushort PermanentSuperMissileBlock = 0xcc0b;
    /// <summary><c>$84:CC20 InstList_PLM_PowerBombBlock</c>: permanently removes the accepted Power Bomb parent with the native 3/2/1/1-tick draw sequence.</summary>
    public const ushort PermanentPowerBombBlock = 0xcc20;
    /// <summary><c>$84:CC35 InstList_PLM_CollisionReaction_1x1RespawningBombBlock</c>: collision sound $06, then the shared single-block break/wait/restore tail.</summary>
    public const ushort CollisionBombBlock1x1Respawning = 0xcc35;
    /// <summary><c>$84:CC3C InstList_PLM_Reaction_1x1RespawningBombBlock_0</c>: projectile sound $0A, then the shared single-block break/wait/restore tail.</summary>
    public const ushort ReactionBombBlock1x1Respawning = 0xcc3c;
    /// <summary><c>$84:CC5F InstList_PLM_Collision_2x1RespawningBombBlock</c>: collision sound $06, then the shared horizontal-pair respawn animation.</summary>
    public const ushort CollisionBombBlock2x1Respawning = 0xcc5f;
    /// <summary><c>$84:CC66 InstList_PLM_Reaction_2x1RespawningBombBlock</c>: projectile sound $0A, then the shared horizontal-pair respawn animation.</summary>
    public const ushort ReactionBombBlock2x1Respawning = 0xcc66;
    /// <summary><c>$84:CC8B InstList_PLM_Collision_1x2RespawningBombBlock</c>: collision sound $06, then the shared vertical-pair respawn animation.</summary>
    public const ushort CollisionBombBlock1x2Respawning = 0xcc8b;
    /// <summary><c>$84:CC92 InstList_PLM_Reaction_1x2RespawningBombBlock</c>: projectile sound $0A, then the shared vertical-pair respawn animation.</summary>
    public const ushort ReactionBombBlock1x2Respawning = 0xcc92;
    /// <summary><c>$84:CCB7 InstList_PLM_Collision_2x2RespawningBombBlock</c>: collision sound $06, then the shared four-block square respawn animation.</summary>
    public const ushort CollisionBombBlock2x2Respawning = 0xccb7;
    /// <summary><c>$84:CCBE InstList_PLM_Reaction_2x2RespawningBombBlock</c>: projectile sound $0A, then the shared four-block square respawn animation.</summary>
    public const ushort ReactionBombBlock2x2Respawning = 0xccbe;
    /// <summary><c>$84:CCE3 InstList_PLM_Collision_1x1RespawningBombBlock</c>: despite the native spelling, collision sound $06 enters the permanent single-block removal tail.</summary>
    public const ushort CollisionBombBlock1x1Permanent = 0xcce3;
    /// <summary><c>$84:CCEA InstList_PLM_Reaction_1x1RespawningBombBlock_4</c>: projectile sound $0A enters permanent single-block removal without restoration.</summary>
    public const ushort ReactionBombBlock1x1Permanent = 0xccea;
    /// <summary><c>$84:CCFF InstList_PLM_Collision_2x1BombBlock</c>: collision sound $06 enters permanent removal of a horizontal pair.</summary>
    public const ushort CollisionBombBlock2x1Permanent = 0xccff;
    /// <summary><c>$84:CD06 InstList_PLM_Reaction_2x1BombBlock</c>: projectile sound $0A enters permanent removal of a horizontal pair.</summary>
    public const ushort ReactionBombBlock2x1Permanent = 0xcd06;
    /// <summary><c>$84:CD1B InstList_PLM_Collision_1x2BombBlock</c>: collision sound $06 enters permanent removal of a vertical pair.</summary>
    public const ushort CollisionBombBlock1x2Permanent = 0xcd1b;
    /// <summary><c>$84:CD22 InstList_PLM_Reaction_1x2BombBlock</c>: projectile sound $0A enters permanent removal of a vertical pair.</summary>
    public const ushort ReactionBombBlock1x2Permanent = 0xcd22;
    /// <summary><c>$84:CD37 InstList_PLM_Collision_2x2BombBlock</c>: collision sound $06 enters permanent removal of a four-block square.</summary>
    public const ushort CollisionBombBlock2x2Permanent = 0xcd37;
    /// <summary><c>$84:CD3E InstList_PLM_Reaction_2x2BombBlock</c>: projectile sound $0A enters permanent removal of a four-block square.</summary>
    public const ushort ReactionBombBlock2x2Permanent = 0xcd3e;
    /// <summary><c>$84:CD6A InstList_PLM_RespawningBreakableGrappleBlock</c>.</summary>
    public const ushort RespawningBreakableGrappleBlock = 0xcd6a;
    /// <summary><c>$84:CDA9 InstList_PLM_BreakableGrappleBlock</c>.</summary>
    public const ushort PermanentBreakableGrappleBlock = 0xcda9;
    /// <summary>Selects the named collision bomb program from native $94:936B PLM headers.
    /// Indices encode 1x1, 2x1, 1x2, 2x2; where present, permanent forms follow respawning forms.</summary>
    public static ushort CollisionBombByReactionIndex(int index) => index switch
    {
        0 => CollisionBombBlock1x1Respawning,
        1 => CollisionBombBlock2x1Respawning,
        2 => CollisionBombBlock1x2Respawning,
        3 => CollisionBombBlock2x2Respawning,
        4 => CollisionBombBlock1x1Permanent,
        5 => CollisionBombBlock2x1Permanent,
        6 => CollisionBombBlock1x2Permanent,
        7 => CollisionBombBlock2x2Permanent,
        _ => throw new ArgumentOutOfRangeException(nameof(index)),
    };
    /// <summary>Selects the named bomb reaction program from native $94:A012 PLM headers.
    /// Indices encode 1x1, 2x1, 1x2, 2x2; where present, permanent forms follow respawning forms.</summary>
    public static ushort ReactionBombByReactionIndex(int index) => index switch
    {
        0 => ReactionBombBlock1x1Respawning,
        1 => ReactionBombBlock2x1Respawning,
        2 => ReactionBombBlock1x2Respawning,
        3 => ReactionBombBlock2x2Respawning,
        4 => ReactionBombBlock1x1Permanent,
        5 => ReactionBombBlock2x1Permanent,
        6 => ReactionBombBlock1x2Permanent,
        7 => ReactionBombBlock2x2Permanent,
        _ => throw new ArgumentOutOfRangeException(nameof(index)),
    };
    /// <summary>Selects the named contact crumble program from native $94:9139 PLM headers.
    /// Indices encode 1x1, 2x1, 1x2, 2x2; where present, permanent forms follow respawning forms.</summary>
    public static ushort ContactCrumbleByReactionIndex(int index) => index switch
    {
        0 => ContactCrumble1x1Respawning,
        1 => ContactCrumble2x1Respawning,
        2 => ContactCrumble1x2Respawning,
        3 => ContactCrumble2x2Respawning,
        4 => ContactCrumble1x1Permanent,
        5 => ContactCrumble2x1Permanent,
        6 => ContactCrumble1x2Permanent,
        7 => ContactCrumble2x2Permanent,
        _ => throw new ArgumentOutOfRangeException(nameof(index)),
    };
}
