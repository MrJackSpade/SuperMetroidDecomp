namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Selects a block reaction by reaction index or block size from <see cref="BlockReactionChangeDefinitions"/>.</summary>
internal static class BlockReactionChanges
{
    /// <summary>The collision bomb block for reaction index 0 through 7: 1x1, 2x1, 1x2, 2x2 respawning, then permanent.</summary>
    internal static BlockReactionChange CollisionBomb(int index) => At(BlockReactionChangeDefinitions.CollisionBomb, index);

    /// <summary>The bomb reaction a Power Bomb sets off for reaction index 0 through 7.</summary>
    internal static BlockReactionChange ReactionBomb(int index) => At(BlockReactionChangeDefinitions.ReactionBomb, index);

    /// <summary>The bomb block a normal bomb sets off for reaction index 0 through 7, without the reaction sound.</summary>
    internal static BlockReactionChange BombBreak(int index) => At(BlockReactionChangeDefinitions.BombBreak, index);

    /// <summary>The crumble reveal for size 0 through 3: 1x1, 2x1, 1x2, 2x2.</summary>
    internal static BlockReactionChange CrumbleReveal(int index) => At(BlockReactionChangeDefinitions.CrumbleReveal, index);

    /// <summary>The contact crumble for reaction index 0 through 7: 1x1, 2x1, 1x2, 2x2 respawning, then permanent.</summary>
    internal static BlockReactionChange ContactCrumble(int index) => At(BlockReactionChangeDefinitions.ContactCrumble, index);

    /// <summary>The respawning shot block for size 0 through 3.</summary>
    internal static BlockReactionChange RespawningShot(int index) => At(BlockReactionChangeDefinitions.RespawningShot, index);

    /// <summary>The permanent shot block for size 0 through 3.</summary>
    internal static BlockReactionChange PermanentShot(int index) => At(BlockReactionChangeDefinitions.PermanentShot, index);

    /// <summary>The reaction at <paramref name="index"/> of <paramref name="changes"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is outside the selection.</exception>
    private static BlockReactionChange At(IReadOnlyList<BlockReactionChange> changes, int index) =>
        (uint)index < (uint)changes.Count ? changes[index] : throw new ArgumentOutOfRangeException(nameof(index));
}
