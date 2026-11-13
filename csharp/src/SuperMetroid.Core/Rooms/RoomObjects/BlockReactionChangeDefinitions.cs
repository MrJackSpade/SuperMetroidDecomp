using D = SuperMetroid.Core.Rooms.RoomObjects.BlockReactionObjectDrawDefinitions;
using F = SuperMetroid.Core.Rooms.RoomObjects.BlockReactionObjectFrame;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The authored timing of the block reactions (#1554), change for change: the reaction sound, the breakup
/// images with their durations, the respawn and the restored block. A change ends on the update after its last image.
/// </summary>
internal static class BlockReactionChangeDefinitions
{
    /// <summary>The updates a contact crumble waits, once Samus stands on it, before its first image.</summary>
    internal const int ContactCrumbleDelay = 4;

    /// <summary>A bombed crumble block's reveal: its image for one update, 1x1.</summary>
    internal static BlockReactionChange CrumbleReveal1x1 { get; } = new(1,
    [
        new(D.DrawLists[F.CrumbleRevealSingle], 1),
    ]);

    /// <summary>A bombed crumble block's reveal: its image for one update, 2x1.</summary>
    internal static BlockReactionChange CrumbleReveal2x1 { get; } = new(1,
    [
        new(D.DrawLists[F.CrumbleRevealHorizontal], 1),
    ]);

    /// <summary>A bombed crumble block's reveal: its image for one update, 1x2.</summary>
    internal static BlockReactionChange CrumbleReveal1x2 { get; } = new(1,
    [
        new(D.DrawLists[F.CrumbleRevealVertical], 1),
    ]);

    /// <summary>A bombed crumble block's reveal: its image for one update, 2x2.</summary>
    internal static BlockReactionChange CrumbleReveal2x2 { get; } = new(1,
    [
        new(D.DrawLists[F.CrumbleRevealSquare], 1),
    ]);

    /// <summary>A normal bomb's one-update reveal of a Power Bomb block, which it does not break.</summary>
    internal static BlockReactionChange BombedPowerBombBlockReveal { get; } = new(1,
    [
        new(D.DrawLists[F.PowerBombReveal], 1),
    ]);

    /// <summary>A normal bomb's one-update reveal of a Super Missile block, which it does not break.</summary>
    internal static BlockReactionChange BombedSuperMissileBlockReveal { get; } = new(1,
    [
        new(D.DrawLists[F.SuperMissileReveal], 1),
    ]);

    /// <summary>A crumble block Samus stands on breaking up, after its delay: a 1x1 block, growing back.</summary>
    internal static BlockReactionChange ContactCrumble1x1Respawning { get; } = new(ContactCrumbleDelay,
    [
        new(D.DrawLists[F.SingleBreakup0], 8, SoundBefore: EnemySound.BlockCrumble) { SoundPriority = SoundRequestPriority.WhenIdle },
        new(D.DrawLists[F.SingleBreakup1], 6),
        new(D.DrawLists[F.SingleBreakup2], 4),
        new(D.DrawLists[F.SingleBreakup3], 16),
        new(D.DrawLists[F.SingleBreakup2], 4),
        new(D.DrawLists[F.SingleBreakup1], 4),
        new(D.DrawLists[F.SingleBreakup0], 4),
        BlockReactionFrame.RestoredBlock,
    ]);

    /// <summary>A crumble block Samus stands on breaking up, after its delay: a 2x1 block, growing back.</summary>
    internal static BlockReactionChange ContactCrumble2x1Respawning { get; } = new(ContactCrumbleDelay,
    [
        new(D.DrawLists[F.HorizontalBreakup0], 4, SoundBefore: EnemySound.BlockCrumble) { SoundPriority = SoundRequestPriority.WhenIdle },
        new(D.DrawLists[F.HorizontalBreakup1], 4),
        new(D.DrawLists[F.HorizontalBreakup2], 4),
        new(D.DrawLists[F.HorizontalBreakup3], 16),
        new(D.DrawLists[F.HorizontalBreakup2], 4),
        new(D.DrawLists[F.HorizontalBreakup1], 4),
        new(D.DrawLists[F.HorizontalBreakup0], 4),
        new(D.DrawLists[F.CrumbleRevealHorizontal], 1),
    ]);

    /// <summary>A crumble block Samus stands on breaking up, after its delay: a 1x2 block, growing back.</summary>
    internal static BlockReactionChange ContactCrumble1x2Respawning { get; } = new(ContactCrumbleDelay,
    [
        new(D.DrawLists[F.VerticalBreakup0], 4, SoundBefore: EnemySound.BlockCrumble) { SoundPriority = SoundRequestPriority.WhenIdle },
        new(D.DrawLists[F.VerticalBreakup1], 4),
        new(D.DrawLists[F.VerticalBreakup2], 4),
        new(D.DrawLists[F.VerticalBreakup3], 32),
        new(D.DrawLists[F.VerticalBreakup2], 4),
        new(D.DrawLists[F.VerticalBreakup1], 4),
        new(D.DrawLists[F.VerticalBreakup0], 4),
        new(D.DrawLists[F.CrumbleRevealVertical], 1),
    ]);

    /// <summary>A crumble block Samus stands on breaking up, after its delay: a 2x2 block, growing back.</summary>
    internal static BlockReactionChange ContactCrumble2x2Respawning { get; } = new(ContactCrumbleDelay,
    [
        new(D.DrawLists[F.SquareBreakup0], 4, SoundBefore: EnemySound.BlockCrumble) { SoundPriority = SoundRequestPriority.WhenIdle },
        new(D.DrawLists[F.SquareBreakup1], 4),
        new(D.DrawLists[F.SquareBreakup2], 4),
        new(D.DrawLists[F.SquareBreakup3], 32),
        new(D.DrawLists[F.SquareBreakup2], 4),
        new(D.DrawLists[F.SquareBreakup1], 4),
        new(D.DrawLists[F.SquareBreakup0], 4),
        new(D.DrawLists[F.CrumbleRevealSquare], 1),
    ]);

    /// <summary>A crumble block Samus stands on breaking up, after its delay: a 1x1 block, gone for good.</summary>
    internal static BlockReactionChange ContactCrumble1x1Permanent { get; } = new(ContactCrumbleDelay,
    [
        new(D.DrawLists[F.SingleBreakup0], 4, SoundBefore: EnemySound.BlockCrumble) { SoundPriority = SoundRequestPriority.WhenIdle },
        new(D.DrawLists[F.SingleBreakup1], 4),
        new(D.DrawLists[F.SingleBreakup2], 4),
        new(D.DrawLists[F.SingleBreakup3], 1),
    ]);

    /// <summary>A crumble block Samus stands on breaking up, after its delay: a 2x1 block, gone for good.</summary>
    internal static BlockReactionChange ContactCrumble2x1Permanent { get; } = new(ContactCrumbleDelay,
    [
        new(D.DrawLists[F.HorizontalBreakup0], 4, SoundBefore: EnemySound.BlockCrumble) { SoundPriority = SoundRequestPriority.WhenIdle },
        new(D.DrawLists[F.HorizontalBreakup1], 4),
        new(D.DrawLists[F.HorizontalBreakup2], 4),
        new(D.DrawLists[F.HorizontalBreakup3], 1),
    ]);

    /// <summary>A crumble block Samus stands on breaking up, after its delay: a 1x2 block, gone for good.</summary>
    internal static BlockReactionChange ContactCrumble1x2Permanent { get; } = new(ContactCrumbleDelay,
    [
        new(D.DrawLists[F.VerticalBreakup0], 4, SoundBefore: EnemySound.BlockCrumble) { SoundPriority = SoundRequestPriority.WhenIdle },
        new(D.DrawLists[F.VerticalBreakup1], 4),
        new(D.DrawLists[F.VerticalBreakup2], 4),
        new(D.DrawLists[F.VerticalBreakup3], 1),
    ]);

    /// <summary>A crumble block Samus stands on breaking up, after its delay: a 2x2 block, gone for good.</summary>
    internal static BlockReactionChange ContactCrumble2x2Permanent { get; } = new(ContactCrumbleDelay,
    [
        new(D.DrawLists[F.SquareBreakup0], 4, SoundBefore: EnemySound.BlockCrumble) { SoundPriority = SoundRequestPriority.WhenIdle },
        new(D.DrawLists[F.SquareBreakup1], 4),
        new(D.DrawLists[F.SquareBreakup2], 4),
        new(D.DrawLists[F.SquareBreakup3], 1),
    ]);

    /// <summary>A respawning shot block breaking up and growing back, 1x1.</summary>
    internal static BlockReactionChange RespawningShotBlock1x1 { get; } = new(1,
    [
        new(D.DrawLists[F.SingleBreakup0], 4, SoundBefore: EnemySound.BlockCrumble) { SoundPriority = SoundRequestPriority.WhenIdle },
        new(D.DrawLists[F.SingleBreakup1], 4),
        new(D.DrawLists[F.SingleBreakup2], 4),
        new(D.DrawLists[F.SingleBreakup3], 384),
        new(D.DrawLists[F.SingleBreakup2], 4),
        new(D.DrawLists[F.SingleBreakup1], 4),
        new(D.DrawLists[F.SingleBreakup0], 4),
        BlockReactionFrame.RestoredBlock,
    ]);

    /// <summary>A respawning shot block breaking up and growing back, 2x1.</summary>
    internal static BlockReactionChange RespawningShotBlock2x1 { get; } = new(1,
    [
        new(D.DrawLists[F.HorizontalBreakup0], 4, SoundBefore: EnemySound.BlockCrumble) { SoundPriority = SoundRequestPriority.WhenIdle },
        new(D.DrawLists[F.HorizontalBreakup1], 4),
        new(D.DrawLists[F.HorizontalBreakup2], 4),
        new(D.DrawLists[F.HorizontalBreakup3], 384),
        new(D.DrawLists[F.HorizontalBreakup2], 4),
        new(D.DrawLists[F.HorizontalBreakup1], 4),
        new(D.DrawLists[F.HorizontalBreakup0], 4),
        new(D.DrawLists[F.ShotRestoreHorizontal], 1),
    ]);

    /// <summary>A respawning shot block breaking up and growing back, 1x2.</summary>
    internal static BlockReactionChange RespawningShotBlock1x2 { get; } = new(1,
    [
        new(D.DrawLists[F.VerticalBreakup0], 4, SoundBefore: EnemySound.BlockCrumble) { SoundPriority = SoundRequestPriority.WhenIdle },
        new(D.DrawLists[F.VerticalBreakup1], 4),
        new(D.DrawLists[F.VerticalBreakup2], 4),
        new(D.DrawLists[F.VerticalBreakup3], 384),
        new(D.DrawLists[F.VerticalBreakup2], 4),
        new(D.DrawLists[F.VerticalBreakup1], 4),
        new(D.DrawLists[F.VerticalBreakup0], 4),
        new(D.DrawLists[F.ShotRestoreVertical], 1),
    ]);

    /// <summary>A respawning shot block breaking up and growing back, 2x2.</summary>
    internal static BlockReactionChange RespawningShotBlock2x2 { get; } = new(1,
    [
        new(D.DrawLists[F.SquareBreakup0], 4, SoundBefore: EnemySound.BlockCrumble) { SoundPriority = SoundRequestPriority.WhenIdle },
        new(D.DrawLists[F.SquareBreakup1], 4),
        new(D.DrawLists[F.SquareBreakup2], 4),
        new(D.DrawLists[F.SquareBreakup3], 384),
        new(D.DrawLists[F.SquareBreakup2], 4),
        new(D.DrawLists[F.SquareBreakup1], 4),
        new(D.DrawLists[F.SquareBreakup0], 4),
        new(D.DrawLists[F.ShotRestoreSquare], 1),
    ]);

    /// <summary>A respawning Super Missile block breaking up and growing back.</summary>
    internal static BlockReactionChange RespawningSuperMissileBlock { get; } = new(1,
    [
        new(D.DrawLists[F.SingleBreakup0], 4, SoundBefore: EnemySound.BlockCrumble) { SoundPriority = SoundRequestPriority.Normal },
        new(D.DrawLists[F.SingleBreakup1], 4),
        new(D.DrawLists[F.SingleBreakup2], 4),
        new(D.DrawLists[F.SingleBreakup3], 384),
        new(D.DrawLists[F.SingleBreakup2], 4),
        new(D.DrawLists[F.SingleBreakup1], 4),
        new(D.DrawLists[F.SingleBreakup0], 4),
        BlockReactionFrame.RestoredBlock,
    ]);

    /// <summary>A respawning Power Bomb block breaking up and growing back.</summary>
    internal static BlockReactionChange RespawningPowerBombBlock { get; } = new(1,
    [
        new(D.DrawLists[F.SingleBreakup0], 4, SoundBefore: EnemySound.BlockCrumble) { SoundPriority = SoundRequestPriority.WhenIdle },
        new(D.DrawLists[F.SingleBreakup1], 4),
        new(D.DrawLists[F.SingleBreakup2], 4),
        new(D.DrawLists[F.SingleBreakup3], 384),
        new(D.DrawLists[F.SingleBreakup2], 4),
        new(D.DrawLists[F.SingleBreakup1], 4),
        new(D.DrawLists[F.SingleBreakup0], 4),
        BlockReactionFrame.RestoredBlock,
    ]);

    /// <summary>A shot block breaking up for good, 1x1.</summary>
    internal static BlockReactionChange PermanentShotBlock1x1 { get; } = new(1,
    [
        new(D.DrawLists[F.SingleBreakup0], 4, SoundBefore: EnemySound.BlockCrumble) { SoundPriority = SoundRequestPriority.WhenIdle },
        new(D.DrawLists[F.SingleBreakup1], 4),
        new(D.DrawLists[F.SingleBreakup2], 4),
        new(D.DrawLists[F.SingleBreakup3], 1),
    ]);

    /// <summary>A shot block breaking up for good, 2x1.</summary>
    internal static BlockReactionChange PermanentShotBlock2x1 { get; } = new(1,
    [
        new(D.DrawLists[F.HorizontalBreakup0], 4, SoundBefore: EnemySound.BlockCrumble) { SoundPriority = SoundRequestPriority.WhenIdle },
        new(D.DrawLists[F.HorizontalBreakup1], 4),
        new(D.DrawLists[F.HorizontalBreakup2], 4),
        new(D.DrawLists[F.HorizontalBreakup3], 1),
    ]);

    /// <summary>A shot block breaking up for good, 1x2.</summary>
    internal static BlockReactionChange PermanentShotBlock1x2 { get; } = new(1,
    [
        new(D.DrawLists[F.VerticalBreakup0], 4, SoundBefore: EnemySound.BlockCrumble) { SoundPriority = SoundRequestPriority.WhenIdle },
        new(D.DrawLists[F.VerticalBreakup1], 4),
        new(D.DrawLists[F.VerticalBreakup2], 4),
        new(D.DrawLists[F.VerticalBreakup3], 1),
    ]);

    /// <summary>A shot block breaking up for good, 2x2.</summary>
    internal static BlockReactionChange PermanentShotBlock2x2 { get; } = new(1,
    [
        new(D.DrawLists[F.SquareBreakup0], 4, SoundBefore: EnemySound.BlockCrumble) { SoundPriority = SoundRequestPriority.WhenIdle },
        new(D.DrawLists[F.SquareBreakup1], 4),
        new(D.DrawLists[F.SquareBreakup2], 4),
        new(D.DrawLists[F.SquareBreakup3], 1),
    ]);

    /// <summary>A Super Missile block breaking up for good.</summary>
    internal static BlockReactionChange PermanentSuperMissileBlock { get; } = new(1,
    [
        new(D.DrawLists[F.SingleBreakup0], 4, SoundBefore: EnemySound.BlockCrumble) { SoundPriority = SoundRequestPriority.Normal },
        new(D.DrawLists[F.SingleBreakup1], 4),
        new(D.DrawLists[F.SingleBreakup2], 4),
        new(D.DrawLists[F.SingleBreakup3], 1),
    ]);

    /// <summary>A Power Bomb block breaking up for good.</summary>
    internal static BlockReactionChange PermanentPowerBombBlock { get; } = new(1,
    [
        new(D.DrawLists[F.SingleBreakup0], 3, SoundBefore: EnemySound.BlockCrumble) { SoundPriority = SoundRequestPriority.WhenIdle },
        new(D.DrawLists[F.SingleBreakup1], 2),
        new(D.DrawLists[F.SingleBreakup2], 1),
        new(D.DrawLists[F.SingleBreakup3], 1),
    ]);

    /// <summary>A bomb block Samus's contact sets off, with its break sound: a 1x1 block, growing back.</summary>
    internal static BlockReactionChange CollisionBombBlock1x1Respawning { get; } = new(1,
    [
        new(D.DrawLists[F.SingleBreakup0], 4, SoundBefore: EnemySound.ContactBlockBreak) { SoundPriority = SoundRequestPriority.Low },
        new(D.DrawLists[F.SingleBreakup1], 4),
        new(D.DrawLists[F.SingleBreakup2], 4),
        new(D.DrawLists[F.SingleBreakup3], 384),
        new(D.DrawLists[F.SingleBreakup2], 4),
        new(D.DrawLists[F.SingleBreakup1], 4),
        new(D.DrawLists[F.SingleBreakup0], 4),
        BlockReactionFrame.RestoredBlock,
    ]);

    /// <summary>A bomb block a Power Bomb sets off, with its crumble sound: a 1x1 block, growing back.</summary>
    internal static BlockReactionChange ReactionBombBlock1x1Respawning { get; } = new(1,
    [
        new(D.DrawLists[F.SingleBreakup0], 4, SoundBefore: EnemySound.BlockCrumble) { SoundPriority = SoundRequestPriority.Low },
        new(D.DrawLists[F.SingleBreakup1], 4),
        new(D.DrawLists[F.SingleBreakup2], 4),
        new(D.DrawLists[F.SingleBreakup3], 384),
        new(D.DrawLists[F.SingleBreakup2], 4),
        new(D.DrawLists[F.SingleBreakup1], 4),
        new(D.DrawLists[F.SingleBreakup0], 4),
        BlockReactionFrame.RestoredBlock,
    ]);

    /// <summary>A bomb block a normal bomb sets off, without a sound: a 1x1 block, growing back.</summary>
    internal static BlockReactionChange BombBlock1x1RespawningBreak { get; } = new(1,
    [
        new(D.DrawLists[F.SingleBreakup0], 4),
        new(D.DrawLists[F.SingleBreakup1], 4),
        new(D.DrawLists[F.SingleBreakup2], 4),
        new(D.DrawLists[F.SingleBreakup3], 384),
        new(D.DrawLists[F.SingleBreakup2], 4),
        new(D.DrawLists[F.SingleBreakup1], 4),
        new(D.DrawLists[F.SingleBreakup0], 4),
        BlockReactionFrame.RestoredBlock,
    ]);

    /// <summary>A bomb block Samus's contact sets off, with its break sound: a 2x1 block, growing back.</summary>
    internal static BlockReactionChange CollisionBombBlock2x1Respawning { get; } = new(1,
    [
        new(D.DrawLists[F.HorizontalBreakup0], 4, SoundBefore: EnemySound.ContactBlockBreak) { SoundPriority = SoundRequestPriority.Low },
        new(D.DrawLists[F.HorizontalBreakup1], 4),
        new(D.DrawLists[F.HorizontalBreakup2], 4),
        new(D.DrawLists[F.HorizontalBreakup3], 384),
        new(D.DrawLists[F.HorizontalBreakup2], 4),
        new(D.DrawLists[F.HorizontalBreakup1], 4),
        new(D.DrawLists[F.HorizontalBreakup0], 4),
        new(D.DrawLists[F.BombRestoreHorizontal], 1),
    ]);

    /// <summary>A bomb block a Power Bomb sets off, with its crumble sound: a 2x1 block, growing back.</summary>
    internal static BlockReactionChange ReactionBombBlock2x1Respawning { get; } = new(1,
    [
        new(D.DrawLists[F.HorizontalBreakup0], 4, SoundBefore: EnemySound.BlockCrumble) { SoundPriority = SoundRequestPriority.Low },
        new(D.DrawLists[F.HorizontalBreakup1], 4),
        new(D.DrawLists[F.HorizontalBreakup2], 4),
        new(D.DrawLists[F.HorizontalBreakup3], 384),
        new(D.DrawLists[F.HorizontalBreakup2], 4),
        new(D.DrawLists[F.HorizontalBreakup1], 4),
        new(D.DrawLists[F.HorizontalBreakup0], 4),
        new(D.DrawLists[F.BombRestoreHorizontal], 1),
    ]);

    /// <summary>A bomb block a normal bomb sets off, without a sound: a 2x1 block, growing back.</summary>
    internal static BlockReactionChange BombBlock2x1RespawningBreak { get; } = new(1,
    [
        new(D.DrawLists[F.HorizontalBreakup0], 4),
        new(D.DrawLists[F.HorizontalBreakup1], 4),
        new(D.DrawLists[F.HorizontalBreakup2], 4),
        new(D.DrawLists[F.HorizontalBreakup3], 384),
        new(D.DrawLists[F.HorizontalBreakup2], 4),
        new(D.DrawLists[F.HorizontalBreakup1], 4),
        new(D.DrawLists[F.HorizontalBreakup0], 4),
        new(D.DrawLists[F.BombRestoreHorizontal], 1),
    ]);

    /// <summary>A bomb block Samus's contact sets off, with its break sound: a 1x2 block, growing back.</summary>
    internal static BlockReactionChange CollisionBombBlock1x2Respawning { get; } = new(1,
    [
        new(D.DrawLists[F.VerticalBreakup0], 4, SoundBefore: EnemySound.ContactBlockBreak) { SoundPriority = SoundRequestPriority.Low },
        new(D.DrawLists[F.VerticalBreakup1], 4),
        new(D.DrawLists[F.VerticalBreakup2], 4),
        new(D.DrawLists[F.VerticalBreakup3], 384),
        new(D.DrawLists[F.VerticalBreakup2], 4),
        new(D.DrawLists[F.VerticalBreakup1], 4),
        new(D.DrawLists[F.VerticalBreakup0], 4),
        new(D.DrawLists[F.BombRestoreVertical], 1),
    ]);

    /// <summary>A bomb block a Power Bomb sets off, with its crumble sound: a 1x2 block, growing back.</summary>
    internal static BlockReactionChange ReactionBombBlock1x2Respawning { get; } = new(1,
    [
        new(D.DrawLists[F.VerticalBreakup0], 4, SoundBefore: EnemySound.BlockCrumble) { SoundPriority = SoundRequestPriority.Low },
        new(D.DrawLists[F.VerticalBreakup1], 4),
        new(D.DrawLists[F.VerticalBreakup2], 4),
        new(D.DrawLists[F.VerticalBreakup3], 384),
        new(D.DrawLists[F.VerticalBreakup2], 4),
        new(D.DrawLists[F.VerticalBreakup1], 4),
        new(D.DrawLists[F.VerticalBreakup0], 4),
        new(D.DrawLists[F.BombRestoreVertical], 1),
    ]);

    /// <summary>A bomb block a normal bomb sets off, without a sound: a 1x2 block, growing back.</summary>
    internal static BlockReactionChange BombBlock1x2RespawningBreak { get; } = new(1,
    [
        new(D.DrawLists[F.VerticalBreakup0], 4),
        new(D.DrawLists[F.VerticalBreakup1], 4),
        new(D.DrawLists[F.VerticalBreakup2], 4),
        new(D.DrawLists[F.VerticalBreakup3], 384),
        new(D.DrawLists[F.VerticalBreakup2], 4),
        new(D.DrawLists[F.VerticalBreakup1], 4),
        new(D.DrawLists[F.VerticalBreakup0], 4),
        new(D.DrawLists[F.BombRestoreVertical], 1),
    ]);

    /// <summary>A bomb block Samus's contact sets off, with its break sound: a 2x2 block, growing back.</summary>
    internal static BlockReactionChange CollisionBombBlock2x2Respawning { get; } = new(1,
    [
        new(D.DrawLists[F.SquareBreakup0], 4, SoundBefore: EnemySound.ContactBlockBreak) { SoundPriority = SoundRequestPriority.Low },
        new(D.DrawLists[F.SquareBreakup1], 4),
        new(D.DrawLists[F.SquareBreakup2], 4),
        new(D.DrawLists[F.SquareBreakup3], 384),
        new(D.DrawLists[F.SquareBreakup2], 4),
        new(D.DrawLists[F.SquareBreakup1], 4),
        new(D.DrawLists[F.SquareBreakup0], 4),
        new(D.DrawLists[F.BombRestoreSquare], 1),
    ]);

    /// <summary>A bomb block a Power Bomb sets off, with its crumble sound: a 2x2 block, growing back.</summary>
    internal static BlockReactionChange ReactionBombBlock2x2Respawning { get; } = new(1,
    [
        new(D.DrawLists[F.SquareBreakup0], 4, SoundBefore: EnemySound.BlockCrumble) { SoundPriority = SoundRequestPriority.Low },
        new(D.DrawLists[F.SquareBreakup1], 4),
        new(D.DrawLists[F.SquareBreakup2], 4),
        new(D.DrawLists[F.SquareBreakup3], 384),
        new(D.DrawLists[F.SquareBreakup2], 4),
        new(D.DrawLists[F.SquareBreakup1], 4),
        new(D.DrawLists[F.SquareBreakup0], 4),
        new(D.DrawLists[F.BombRestoreSquare], 1),
    ]);

    /// <summary>A bomb block a normal bomb sets off, without a sound: a 2x2 block, growing back.</summary>
    internal static BlockReactionChange BombBlock2x2RespawningBreak { get; } = new(1,
    [
        new(D.DrawLists[F.SquareBreakup0], 4),
        new(D.DrawLists[F.SquareBreakup1], 4),
        new(D.DrawLists[F.SquareBreakup2], 4),
        new(D.DrawLists[F.SquareBreakup3], 384),
        new(D.DrawLists[F.SquareBreakup2], 4),
        new(D.DrawLists[F.SquareBreakup1], 4),
        new(D.DrawLists[F.SquareBreakup0], 4),
        new(D.DrawLists[F.BombRestoreSquare], 1),
    ]);

    /// <summary>A bomb block Samus's contact sets off, with its break sound: a 1x1 block, gone for good.</summary>
    internal static BlockReactionChange CollisionBombBlock1x1Permanent { get; } = new(1,
    [
        new(D.DrawLists[F.SingleBreakup0], 4, SoundBefore: EnemySound.ContactBlockBreak) { SoundPriority = SoundRequestPriority.Low },
        new(D.DrawLists[F.SingleBreakup1], 4),
        new(D.DrawLists[F.SingleBreakup2], 4),
        new(D.DrawLists[F.SingleBreakup3], 1),
    ]);

    /// <summary>A bomb block a Power Bomb sets off, with its crumble sound: a 1x1 block, gone for good.</summary>
    internal static BlockReactionChange ReactionBombBlock1x1Permanent { get; } = new(1,
    [
        new(D.DrawLists[F.SingleBreakup0], 4, SoundBefore: EnemySound.BlockCrumble) { SoundPriority = SoundRequestPriority.Low },
        new(D.DrawLists[F.SingleBreakup1], 4),
        new(D.DrawLists[F.SingleBreakup2], 4),
        new(D.DrawLists[F.SingleBreakup3], 1),
    ]);

    /// <summary>A bomb block a normal bomb sets off, without a sound: a 1x1 block, gone for good.</summary>
    internal static BlockReactionChange BombBlock1x1PermanentBreak { get; } = new(1,
    [
        new(D.DrawLists[F.SingleBreakup0], 4),
        new(D.DrawLists[F.SingleBreakup1], 4),
        new(D.DrawLists[F.SingleBreakup2], 4),
        new(D.DrawLists[F.SingleBreakup3], 1),
    ]);

    /// <summary>A bomb block Samus's contact sets off, with its break sound: a 2x1 block, gone for good.</summary>
    internal static BlockReactionChange CollisionBombBlock2x1Permanent { get; } = new(1,
    [
        new(D.DrawLists[F.HorizontalBreakup0], 4, SoundBefore: EnemySound.ContactBlockBreak) { SoundPriority = SoundRequestPriority.Low },
        new(D.DrawLists[F.HorizontalBreakup1], 4),
        new(D.DrawLists[F.HorizontalBreakup2], 4),
        new(D.DrawLists[F.HorizontalBreakup3], 1),
    ]);

    /// <summary>A bomb block a Power Bomb sets off, with its crumble sound: a 2x1 block, gone for good.</summary>
    internal static BlockReactionChange ReactionBombBlock2x1Permanent { get; } = new(1,
    [
        new(D.DrawLists[F.HorizontalBreakup0], 4, SoundBefore: EnemySound.BlockCrumble) { SoundPriority = SoundRequestPriority.Low },
        new(D.DrawLists[F.HorizontalBreakup1], 4),
        new(D.DrawLists[F.HorizontalBreakup2], 4),
        new(D.DrawLists[F.HorizontalBreakup3], 1),
    ]);

    /// <summary>A bomb block a normal bomb sets off, without a sound: a 2x1 block, gone for good.</summary>
    internal static BlockReactionChange BombBlock2x1PermanentBreak { get; } = new(1,
    [
        new(D.DrawLists[F.HorizontalBreakup0], 4),
        new(D.DrawLists[F.HorizontalBreakup1], 4),
        new(D.DrawLists[F.HorizontalBreakup2], 4),
        new(D.DrawLists[F.HorizontalBreakup3], 1),
    ]);

    /// <summary>A bomb block Samus's contact sets off, with its break sound: a 1x2 block, gone for good.</summary>
    internal static BlockReactionChange CollisionBombBlock1x2Permanent { get; } = new(1,
    [
        new(D.DrawLists[F.VerticalBreakup0], 4, SoundBefore: EnemySound.ContactBlockBreak) { SoundPriority = SoundRequestPriority.Low },
        new(D.DrawLists[F.VerticalBreakup1], 4),
        new(D.DrawLists[F.VerticalBreakup2], 4),
        new(D.DrawLists[F.VerticalBreakup3], 1),
    ]);

    /// <summary>A bomb block a Power Bomb sets off, with its crumble sound: a 1x2 block, gone for good.</summary>
    internal static BlockReactionChange ReactionBombBlock1x2Permanent { get; } = new(1,
    [
        new(D.DrawLists[F.VerticalBreakup0], 4, SoundBefore: EnemySound.BlockCrumble) { SoundPriority = SoundRequestPriority.Low },
        new(D.DrawLists[F.VerticalBreakup1], 4),
        new(D.DrawLists[F.VerticalBreakup2], 4),
        new(D.DrawLists[F.VerticalBreakup3], 1),
    ]);

    /// <summary>A bomb block a normal bomb sets off, without a sound: a 1x2 block, gone for good.</summary>
    internal static BlockReactionChange BombBlock1x2PermanentBreak { get; } = new(1,
    [
        new(D.DrawLists[F.VerticalBreakup0], 4),
        new(D.DrawLists[F.VerticalBreakup1], 4),
        new(D.DrawLists[F.VerticalBreakup2], 4),
        new(D.DrawLists[F.VerticalBreakup3], 1),
    ]);

    /// <summary>A bomb block Samus's contact sets off, with its break sound: a 2x2 block, gone for good.</summary>
    internal static BlockReactionChange CollisionBombBlock2x2Permanent { get; } = new(1,
    [
        new(D.DrawLists[F.SquareBreakup0], 4, SoundBefore: EnemySound.ContactBlockBreak) { SoundPriority = SoundRequestPriority.Low },
        new(D.DrawLists[F.SquareBreakup1], 4),
        new(D.DrawLists[F.SquareBreakup2], 4),
        new(D.DrawLists[F.SquareBreakup3], 1),
    ]);

    /// <summary>A bomb block a Power Bomb sets off, with its crumble sound: a 2x2 block, gone for good.</summary>
    internal static BlockReactionChange ReactionBombBlock2x2Permanent { get; } = new(1,
    [
        new(D.DrawLists[F.SquareBreakup0], 4, SoundBefore: EnemySound.BlockCrumble) { SoundPriority = SoundRequestPriority.Low },
        new(D.DrawLists[F.SquareBreakup1], 4),
        new(D.DrawLists[F.SquareBreakup2], 4),
        new(D.DrawLists[F.SquareBreakup3], 1),
    ]);

    /// <summary>A bomb block a normal bomb sets off, without a sound: a 2x2 block, gone for good.</summary>
    internal static BlockReactionChange BombBlock2x2PermanentBreak { get; } = new(1,
    [
        new(D.DrawLists[F.SquareBreakup0], 4),
        new(D.DrawLists[F.SquareBreakup1], 4),
        new(D.DrawLists[F.SquareBreakup2], 4),
        new(D.DrawLists[F.SquareBreakup3], 1),
    ]);

    /// <summary>A respawning breakable Grapple block: it holds, breaks up, grows back and breaks again.</summary>
    internal static BlockReactionChange RespawningBreakableGrappleBlock { get; } = new(1,
    [
        new(D.DrawLists[F.GrappleBlock], 240),
        new(D.DrawLists[F.GrappleBreak0], 4, SoundBefore: EnemySound.BlockCrumble) { SoundPriority = SoundRequestPriority.Normal },
        new(D.DrawLists[F.GrappleBreak1], 4),
        new(D.DrawLists[F.GrappleBreak2], 4),
        new(D.DrawLists[F.GrappleBlank], 6),
        new(D.DrawLists[F.GrappleBreak2], 4),
        new(D.DrawLists[F.GrappleBreak1], 4),
        new(D.DrawLists[F.GrappleBreak0], 4),
        BlockReactionFrame.RestoredBlock,
    ]);

    /// <summary>A breakable Grapple block: it holds, then breaks up for good.</summary>
    internal static BlockReactionChange PermanentBreakableGrappleBlock { get; } = new(1,
    [
        new(D.DrawLists[F.GrappleBlock], 120),
        new(D.DrawLists[F.GrappleBreak0], 4, SoundBefore: EnemySound.BlockCrumble) { SoundPriority = SoundRequestPriority.Normal },
        new(D.DrawLists[F.GrappleBreak1], 4),
        new(D.DrawLists[F.GrappleBreak2], 4),
        new(D.DrawLists[F.GrappleBlank], 1),
    ]);

    /// <summary>The collision bomb block each bomb block's break sets off.</summary>
    internal static IReadOnlyDictionary<BombableReaction, BlockReactionChange> CollisionBomb { get; } =
        new Dictionary<BombableReaction, BlockReactionChange>
        {
            [BombableReaction.RespawningBreak1x1] = CollisionBombBlock1x1Respawning,
            [BombableReaction.RespawningBreak2x1] = CollisionBombBlock2x1Respawning,
            [BombableReaction.RespawningBreak1x2] = CollisionBombBlock1x2Respawning,
            [BombableReaction.RespawningBreak2x2] = CollisionBombBlock2x2Respawning,
            [BombableReaction.PermanentBreak1x1] = CollisionBombBlock1x1Permanent,
            [BombableReaction.PermanentBreak2x1] = CollisionBombBlock2x1Permanent,
            [BombableReaction.PermanentBreak1x2] = CollisionBombBlock1x2Permanent,
            [BombableReaction.PermanentBreak2x2] = CollisionBombBlock2x2Permanent,
        };

    /// <summary>The bomb reaction block a Power Bomb sets off for each bomb block's break.</summary>
    internal static IReadOnlyDictionary<BombableReaction, BlockReactionChange> ReactionBomb { get; } =
        new Dictionary<BombableReaction, BlockReactionChange>
        {
            [BombableReaction.RespawningBreak1x1] = ReactionBombBlock1x1Respawning,
            [BombableReaction.RespawningBreak2x1] = ReactionBombBlock2x1Respawning,
            [BombableReaction.RespawningBreak1x2] = ReactionBombBlock1x2Respawning,
            [BombableReaction.RespawningBreak2x2] = ReactionBombBlock2x2Respawning,
            [BombableReaction.PermanentBreak1x1] = ReactionBombBlock1x1Permanent,
            [BombableReaction.PermanentBreak2x1] = ReactionBombBlock2x1Permanent,
            [BombableReaction.PermanentBreak1x2] = ReactionBombBlock1x2Permanent,
            [BombableReaction.PermanentBreak2x2] = ReactionBombBlock2x2Permanent,
        };

    /// <summary>The bomb block a normal bomb sets off for each bomb block's break.</summary>
    internal static IReadOnlyDictionary<BombableReaction, BlockReactionChange> BombBreak { get; } =
        new Dictionary<BombableReaction, BlockReactionChange>
        {
            [BombableReaction.RespawningBreak1x1] = BombBlock1x1RespawningBreak,
            [BombableReaction.RespawningBreak2x1] = BombBlock2x1RespawningBreak,
            [BombableReaction.RespawningBreak1x2] = BombBlock1x2RespawningBreak,
            [BombableReaction.RespawningBreak2x2] = BombBlock2x2RespawningBreak,
            [BombableReaction.PermanentBreak1x1] = BombBlock1x1PermanentBreak,
            [BombableReaction.PermanentBreak2x1] = BombBlock2x1PermanentBreak,
            [BombableReaction.PermanentBreak1x2] = BombBlock1x2PermanentBreak,
            [BombableReaction.PermanentBreak2x2] = BombBlock2x2PermanentBreak,
        };

    /// <summary>The crumble reveal a normal bomb sets off for each crumble block, by its size.</summary>
    internal static IReadOnlyDictionary<SpecialBlockReaction, BlockReactionChange> CrumbleReveal { get; } =
        new Dictionary<SpecialBlockReaction, BlockReactionChange>
        {
            [SpecialBlockReaction.RespawningCrumble1x1] = CrumbleReveal1x1,
            [SpecialBlockReaction.RespawningCrumble2x1] = CrumbleReveal2x1,
            [SpecialBlockReaction.RespawningCrumble1x2] = CrumbleReveal1x2,
            [SpecialBlockReaction.RespawningCrumble2x2] = CrumbleReveal2x2,
            [SpecialBlockReaction.PermanentCrumble1x1] = CrumbleReveal1x1,
            [SpecialBlockReaction.PermanentCrumble2x1] = CrumbleReveal2x1,
            [SpecialBlockReaction.PermanentCrumble1x2] = CrumbleReveal1x2,
            [SpecialBlockReaction.PermanentCrumble2x2] = CrumbleReveal2x2,
        };

    /// <summary>The respawning shot block each respawning break sets off, by its size.</summary>
    internal static IReadOnlyDictionary<ShootableReaction, BlockReactionChange> RespawningShot { get; } =
        new Dictionary<ShootableReaction, BlockReactionChange>
        {
            [ShootableReaction.RespawningBreak1x1] = RespawningShotBlock1x1,
            [ShootableReaction.RespawningBreak2x1] = RespawningShotBlock2x1,
            [ShootableReaction.RespawningBreak1x2] = RespawningShotBlock1x2,
            [ShootableReaction.RespawningBreak2x2] = RespawningShotBlock2x2,
        };

    /// <summary>The permanent shot block each permanent break sets off, by its size.</summary>
    internal static IReadOnlyDictionary<ShootableReaction, BlockReactionChange> PermanentShot { get; } =
        new Dictionary<ShootableReaction, BlockReactionChange>
        {
            [ShootableReaction.PermanentBreak1x1] = PermanentShotBlock1x1,
            [ShootableReaction.PermanentBreak2x1] = PermanentShotBlock2x1,
            [ShootableReaction.PermanentBreak1x2] = PermanentShotBlock1x2,
            [ShootableReaction.PermanentBreak2x2] = PermanentShotBlock2x2,
        };
}
