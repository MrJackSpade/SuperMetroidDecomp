namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Selects a block-reaction program by reaction index or block size.</summary>
internal static class BlockReactionPlmPrograms
{
    /// <summary>The collision bomb program for reaction index 0 through 7: 1x1, 2x1, 1x2, 2x2 respawning, then permanent.</summary>
    internal static BlockReactionPlmProgram CollisionBomb(int index) => index switch
    {
        0 => BlockReactionPlmProgram.CollisionBombBlock1x1Respawning,
        1 => BlockReactionPlmProgram.CollisionBombBlock2x1Respawning,
        2 => BlockReactionPlmProgram.CollisionBombBlock1x2Respawning,
        3 => BlockReactionPlmProgram.CollisionBombBlock2x2Respawning,
        4 => BlockReactionPlmProgram.CollisionBombBlock1x1Permanent,
        5 => BlockReactionPlmProgram.CollisionBombBlock2x1Permanent,
        6 => BlockReactionPlmProgram.CollisionBombBlock1x2Permanent,
        7 => BlockReactionPlmProgram.CollisionBombBlock2x2Permanent,
        _ => throw new IndexOutOfRangeException(),
    };

    /// <summary>The bomb reaction program for reaction index 0 through 7: 1x1, 2x1, 1x2, 2x2 respawning, then permanent.</summary>
    internal static BlockReactionPlmProgram ReactionBomb(int index) => index switch
    {
        0 => BlockReactionPlmProgram.ReactionBombBlock1x1Respawning,
        1 => BlockReactionPlmProgram.ReactionBombBlock2x1Respawning,
        2 => BlockReactionPlmProgram.ReactionBombBlock1x2Respawning,
        3 => BlockReactionPlmProgram.ReactionBombBlock2x2Respawning,
        4 => BlockReactionPlmProgram.ReactionBombBlock1x1Permanent,
        5 => BlockReactionPlmProgram.ReactionBombBlock2x1Permanent,
        6 => BlockReactionPlmProgram.ReactionBombBlock1x2Permanent,
        7 => BlockReactionPlmProgram.ReactionBombBlock2x2Permanent,
        _ => throw new IndexOutOfRangeException(),
    };

    /// <summary>The bomb blocks' shared tail for reaction index 0 through 7, which a normal bomb enters past the sound.</summary>
    internal static BlockReactionPlmProgram BombBreak(int index) => index switch
    {
        0 => BlockReactionPlmProgram.BombBlock1x1RespawningBreak,
        1 => BlockReactionPlmProgram.BombBlock2x1RespawningBreak,
        2 => BlockReactionPlmProgram.BombBlock1x2RespawningBreak,
        3 => BlockReactionPlmProgram.BombBlock2x2RespawningBreak,
        4 => BlockReactionPlmProgram.BombBlock1x1PermanentBreak,
        5 => BlockReactionPlmProgram.BombBlock2x1PermanentBreak,
        6 => BlockReactionPlmProgram.BombBlock1x2PermanentBreak,
        7 => BlockReactionPlmProgram.BombBlock2x2PermanentBreak,
        _ => throw new IndexOutOfRangeException(),
    };

    /// <summary>The crumble reveal program for size 0 through 3: 1x1, 2x1, 1x2, 2x2.</summary>
    internal static BlockReactionPlmProgram CrumbleReveal(int index) => index switch
    {
        0 => BlockReactionPlmProgram.CrumbleReveal1x1,
        1 => BlockReactionPlmProgram.CrumbleReveal2x1,
        2 => BlockReactionPlmProgram.CrumbleReveal1x2,
        3 => BlockReactionPlmProgram.CrumbleReveal2x2,
        _ => throw new IndexOutOfRangeException(),
    };

    /// <summary>The contact crumble program for reaction index 0 through 7: 1x1, 2x1, 1x2, 2x2 respawning, then permanent.</summary>
    internal static BlockReactionPlmProgram ContactCrumble(int index) => index switch
    {
        0 => BlockReactionPlmProgram.ContactCrumble1x1Respawning,
        1 => BlockReactionPlmProgram.ContactCrumble2x1Respawning,
        2 => BlockReactionPlmProgram.ContactCrumble1x2Respawning,
        3 => BlockReactionPlmProgram.ContactCrumble2x2Respawning,
        4 => BlockReactionPlmProgram.ContactCrumble1x1Permanent,
        5 => BlockReactionPlmProgram.ContactCrumble2x1Permanent,
        6 => BlockReactionPlmProgram.ContactCrumble1x2Permanent,
        7 => BlockReactionPlmProgram.ContactCrumble2x2Permanent,
        _ => throw new IndexOutOfRangeException(),
    };

    /// <summary>The respawning shot-block program for size 0 through 3.</summary>
    internal static BlockReactionPlmProgram RespawningShot(int index) => index switch
    {
        0 => BlockReactionPlmProgram.RespawningShotBlock1x1,
        1 => BlockReactionPlmProgram.RespawningShotBlock2x1,
        2 => BlockReactionPlmProgram.RespawningShotBlock1x2,
        3 => BlockReactionPlmProgram.RespawningShotBlock2x2,
        _ => throw new IndexOutOfRangeException(),
    };

    /// <summary>The permanent shot-block program for size 0 through 3.</summary>
    internal static BlockReactionPlmProgram PermanentShot(int index) => index switch
    {
        0 => BlockReactionPlmProgram.PermanentShotBlock1x1,
        1 => BlockReactionPlmProgram.PermanentShotBlock2x1,
        2 => BlockReactionPlmProgram.PermanentShotBlock1x2,
        3 => BlockReactionPlmProgram.PermanentShotBlock2x2,
        _ => throw new IndexOutOfRangeException(),
    };
}
