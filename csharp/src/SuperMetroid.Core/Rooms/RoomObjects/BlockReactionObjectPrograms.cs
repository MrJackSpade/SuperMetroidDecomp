namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Selects a block-reaction program by reaction index or block size.</summary>
internal static class BlockReactionObjectPrograms
{
    /// <summary>The collision bomb program for reaction index 0 through 7: 1x1, 2x1, 1x2, 2x2 respawning, then permanent.</summary>
    internal static BlockReactionObjectProgram CollisionBomb(int index) => index switch
    {
        0 => BlockReactionObjectProgram.CollisionBombBlock1x1Respawning,
        1 => BlockReactionObjectProgram.CollisionBombBlock2x1Respawning,
        2 => BlockReactionObjectProgram.CollisionBombBlock1x2Respawning,
        3 => BlockReactionObjectProgram.CollisionBombBlock2x2Respawning,
        4 => BlockReactionObjectProgram.CollisionBombBlock1x1Permanent,
        5 => BlockReactionObjectProgram.CollisionBombBlock2x1Permanent,
        6 => BlockReactionObjectProgram.CollisionBombBlock1x2Permanent,
        7 => BlockReactionObjectProgram.CollisionBombBlock2x2Permanent,
        _ => throw new ArgumentOutOfRangeException(nameof(index)),
    };

    /// <summary>The bomb reaction program for reaction index 0 through 7: 1x1, 2x1, 1x2, 2x2 respawning, then permanent.</summary>
    internal static BlockReactionObjectProgram ReactionBomb(int index) => index switch
    {
        0 => BlockReactionObjectProgram.ReactionBombBlock1x1Respawning,
        1 => BlockReactionObjectProgram.ReactionBombBlock2x1Respawning,
        2 => BlockReactionObjectProgram.ReactionBombBlock1x2Respawning,
        3 => BlockReactionObjectProgram.ReactionBombBlock2x2Respawning,
        4 => BlockReactionObjectProgram.ReactionBombBlock1x1Permanent,
        5 => BlockReactionObjectProgram.ReactionBombBlock2x1Permanent,
        6 => BlockReactionObjectProgram.ReactionBombBlock1x2Permanent,
        7 => BlockReactionObjectProgram.ReactionBombBlock2x2Permanent,
        _ => throw new ArgumentOutOfRangeException(nameof(index)),
    };

    /// <summary>The bomb blocks' shared tail for reaction index 0 through 7, which a normal bomb enters past the sound.</summary>
    internal static BlockReactionObjectProgram BombBreak(int index) => index switch
    {
        0 => BlockReactionObjectProgram.BombBlock1x1RespawningBreak,
        1 => BlockReactionObjectProgram.BombBlock2x1RespawningBreak,
        2 => BlockReactionObjectProgram.BombBlock1x2RespawningBreak,
        3 => BlockReactionObjectProgram.BombBlock2x2RespawningBreak,
        4 => BlockReactionObjectProgram.BombBlock1x1PermanentBreak,
        5 => BlockReactionObjectProgram.BombBlock2x1PermanentBreak,
        6 => BlockReactionObjectProgram.BombBlock1x2PermanentBreak,
        7 => BlockReactionObjectProgram.BombBlock2x2PermanentBreak,
        _ => throw new ArgumentOutOfRangeException(nameof(index)),
    };

    /// <summary>The crumble reveal program for size 0 through 3: 1x1, 2x1, 1x2, 2x2.</summary>
    internal static BlockReactionObjectProgram CrumbleReveal(int index) => index switch
    {
        0 => BlockReactionObjectProgram.CrumbleReveal1x1,
        1 => BlockReactionObjectProgram.CrumbleReveal2x1,
        2 => BlockReactionObjectProgram.CrumbleReveal1x2,
        3 => BlockReactionObjectProgram.CrumbleReveal2x2,
        _ => throw new ArgumentOutOfRangeException(nameof(index)),
    };

    /// <summary>The contact crumble program for reaction index 0 through 7: 1x1, 2x1, 1x2, 2x2 respawning, then permanent.</summary>
    internal static BlockReactionObjectProgram ContactCrumble(int index) => index switch
    {
        0 => BlockReactionObjectProgram.ContactCrumble1x1Respawning,
        1 => BlockReactionObjectProgram.ContactCrumble2x1Respawning,
        2 => BlockReactionObjectProgram.ContactCrumble1x2Respawning,
        3 => BlockReactionObjectProgram.ContactCrumble2x2Respawning,
        4 => BlockReactionObjectProgram.ContactCrumble1x1Permanent,
        5 => BlockReactionObjectProgram.ContactCrumble2x1Permanent,
        6 => BlockReactionObjectProgram.ContactCrumble1x2Permanent,
        7 => BlockReactionObjectProgram.ContactCrumble2x2Permanent,
        _ => throw new ArgumentOutOfRangeException(nameof(index)),
    };

    /// <summary>The respawning shot-block program for size 0 through 3.</summary>
    internal static BlockReactionObjectProgram RespawningShot(int index) => index switch
    {
        0 => BlockReactionObjectProgram.RespawningShotBlock1x1,
        1 => BlockReactionObjectProgram.RespawningShotBlock2x1,
        2 => BlockReactionObjectProgram.RespawningShotBlock1x2,
        3 => BlockReactionObjectProgram.RespawningShotBlock2x2,
        _ => throw new ArgumentOutOfRangeException(nameof(index)),
    };

    /// <summary>The permanent shot-block program for size 0 through 3.</summary>
    internal static BlockReactionObjectProgram PermanentShot(int index) => index switch
    {
        0 => BlockReactionObjectProgram.PermanentShotBlock1x1,
        1 => BlockReactionObjectProgram.PermanentShotBlock2x1,
        2 => BlockReactionObjectProgram.PermanentShotBlock1x2,
        3 => BlockReactionObjectProgram.PermanentShotBlock2x2,
        _ => throw new ArgumentOutOfRangeException(nameof(index)),
    };
}
