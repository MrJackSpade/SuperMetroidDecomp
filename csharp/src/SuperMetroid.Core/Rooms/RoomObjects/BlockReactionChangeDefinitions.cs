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

    /// <summary>How long a single crumble block's images stay as it breaks under Samus: longer than other blocks'.</summary>
    private static BlockBreakupPace ContactCrumbleSinglePace { get; } = new(8, 6, 4);

    /// <summary>How fast a Power Bomb block breaks for good: quicker than other blocks.</summary>
    private static BlockBreakupPace PowerBombBlockPace { get; } = new(3, 2, 1);

    /// <summary>How long a single or wide crumble block's space stays open before it grows back.</summary>
    private const int ContactCrumbleOpenUpdates = 16;

    /// <summary>How long a tall or square crumble block's space stays open before it grows back.</summary>
    private const int TallContactCrumbleOpenUpdates = 32;

    /// <summary>How long a respawning shot, Super Missile, Power Bomb or bomb block's space stays open before it grows back.</summary>
    private const int ShotBlockOpenUpdates = 384;

    /// <summary>The crumble most blocks break with, joining the sound queue only while it is idle.</summary>
    private static BlockBreakupSound CrumbleSound { get; } = new(EnemySound.BlockCrumble, SoundRequestPriority.WhenIdle);

    /// <summary>A Super Missile block's crumble, joining the sound queue as a normal request.</summary>
    private static BlockBreakupSound SuperMissileBlockSound { get; } = new(EnemySound.BlockCrumble, SoundRequestPriority.Normal);

    /// <summary>The crumble of a bomb block a Power Bomb sets off, a low-priority request.</summary>
    private static BlockBreakupSound ReactionCrumbleSound { get; } = new(EnemySound.BlockCrumble, SoundRequestPriority.Low);

    /// <summary>The break sound of a bomb block Samus's contact sets off, a low-priority request.</summary>
    private static BlockBreakupSound ContactBreakSound { get; } = new(EnemySound.ContactBlockBreak, SoundRequestPriority.Low);

    /// <summary>A bombed crumble block's reveal: its image for one update, 1x1.</summary>
    private static LastingBlockReactionChange CrumbleReveal1x1 { get; } =
        BlockBreakups.CrumbleReveal(BlockArrangement.Single);

    /// <summary>A bombed crumble block's reveal: its image for one update, 2x1.</summary>
    private static LastingBlockReactionChange CrumbleReveal2x1 { get; } =
        BlockBreakups.CrumbleReveal(BlockArrangement.Horizontal);

    /// <summary>A bombed crumble block's reveal: its image for one update, 1x2.</summary>
    private static LastingBlockReactionChange CrumbleReveal1x2 { get; } =
        BlockBreakups.CrumbleReveal(BlockArrangement.Vertical);

    /// <summary>A bombed crumble block's reveal: its image for one update, 2x2.</summary>
    private static LastingBlockReactionChange CrumbleReveal2x2 { get; } =
        BlockBreakups.CrumbleReveal(BlockArrangement.Square);

    /// <summary>A normal bomb's one-update reveal of a Power Bomb block, which it does not break.</summary>
    internal static LastingBlockReactionChange BombedPowerBombBlockReveal { get; } =
        BlockBreakups.Reveal(BombedRevealObjectDraws.Family.Of(RoomObjectBombedRevealDraw.PowerBomb));

    /// <summary>A normal bomb's one-update reveal of a Super Missile block, which it does not break.</summary>
    internal static LastingBlockReactionChange BombedSuperMissileBlockReveal { get; } =
        BlockBreakups.Reveal(BombedRevealObjectDraws.Family.Of(RoomObjectBombedRevealDraw.SuperMissile));

    /// <summary>A crumble block Samus stands on breaking up, after its delay: a 1x1 block, growing back.</summary>
    internal static BlockReactionChange ContactCrumble1x1Respawning { get; } =
        BlockBreakups.Respawning(ContactCrumbleDelay, BlockArrangement.Single, ContactCrumbleSinglePace, CrumbleSound, ContactCrumbleOpenUpdates, BlockRestore.ContactCrumble);

    /// <summary>A crumble block Samus stands on breaking up, after its delay: a 2x1 block, growing back.</summary>
    internal static BlockReactionChange ContactCrumble2x1Respawning { get; } =
        BlockBreakups.Respawning(ContactCrumbleDelay, BlockArrangement.Horizontal, BlockBreakupPace.Steady, CrumbleSound, ContactCrumbleOpenUpdates, BlockRestore.ContactCrumble);

    /// <summary>A crumble block Samus stands on breaking up, after its delay: a 1x2 block, growing back.</summary>
    internal static BlockReactionChange ContactCrumble1x2Respawning { get; } =
        BlockBreakups.Respawning(ContactCrumbleDelay, BlockArrangement.Vertical, BlockBreakupPace.Steady, CrumbleSound, TallContactCrumbleOpenUpdates, BlockRestore.ContactCrumble);

    /// <summary>A crumble block Samus stands on breaking up, after its delay: a 2x2 block, growing back.</summary>
    internal static BlockReactionChange ContactCrumble2x2Respawning { get; } =
        BlockBreakups.Respawning(ContactCrumbleDelay, BlockArrangement.Square, BlockBreakupPace.Steady, CrumbleSound, TallContactCrumbleOpenUpdates, BlockRestore.ContactCrumble);

    /// <summary>A crumble block Samus stands on breaking up, after its delay: a 1x1 block, gone for good.</summary>
    internal static BlockReactionChange ContactCrumble1x1Permanent { get; } =
        BlockBreakups.Permanent(ContactCrumbleDelay, BlockArrangement.Single, BlockBreakupPace.Steady, CrumbleSound);

    /// <summary>A crumble block Samus stands on breaking up, after its delay: a 2x1 block, gone for good.</summary>
    internal static BlockReactionChange ContactCrumble2x1Permanent { get; } =
        BlockBreakups.Permanent(ContactCrumbleDelay, BlockArrangement.Horizontal, BlockBreakupPace.Steady, CrumbleSound);

    /// <summary>A crumble block Samus stands on breaking up, after its delay: a 1x2 block, gone for good.</summary>
    internal static BlockReactionChange ContactCrumble1x2Permanent { get; } =
        BlockBreakups.Permanent(ContactCrumbleDelay, BlockArrangement.Vertical, BlockBreakupPace.Steady, CrumbleSound);

    /// <summary>A crumble block Samus stands on breaking up, after its delay: a 2x2 block, gone for good.</summary>
    internal static BlockReactionChange ContactCrumble2x2Permanent { get; } =
        BlockBreakups.Permanent(ContactCrumbleDelay, BlockArrangement.Square, BlockBreakupPace.Steady, CrumbleSound);

    /// <summary>A respawning shot block breaking up and growing back, 1x1.</summary>
    private static BlockReactionChange RespawningShotBlock1x1 { get; } =
        BlockBreakups.Respawning(1, BlockArrangement.Single, BlockBreakupPace.Steady, CrumbleSound, ShotBlockOpenUpdates, BlockRestore.ShotBlock);

    /// <summary>A respawning shot block breaking up and growing back, 2x1.</summary>
    private static BlockReactionChange RespawningShotBlock2x1 { get; } =
        BlockBreakups.Respawning(1, BlockArrangement.Horizontal, BlockBreakupPace.Steady, CrumbleSound, ShotBlockOpenUpdates, BlockRestore.ShotBlock);

    /// <summary>A respawning shot block breaking up and growing back, 1x2.</summary>
    private static BlockReactionChange RespawningShotBlock1x2 { get; } =
        BlockBreakups.Respawning(1, BlockArrangement.Vertical, BlockBreakupPace.Steady, CrumbleSound, ShotBlockOpenUpdates, BlockRestore.ShotBlock);

    /// <summary>A respawning shot block breaking up and growing back, 2x2.</summary>
    private static BlockReactionChange RespawningShotBlock2x2 { get; } =
        BlockBreakups.Respawning(1, BlockArrangement.Square, BlockBreakupPace.Steady, CrumbleSound, ShotBlockOpenUpdates, BlockRestore.ShotBlock);

    /// <summary>A respawning Super Missile block breaking up and growing back.</summary>
    internal static BlockReactionChange RespawningSuperMissileBlock { get; } =
        BlockBreakups.Respawning(1, BlockArrangement.Single, BlockBreakupPace.Steady, SuperMissileBlockSound, ShotBlockOpenUpdates, BlockRestore.ShotBlock);

    /// <summary>A respawning Power Bomb block breaking up and growing back.</summary>
    internal static BlockReactionChange RespawningPowerBombBlock { get; } =
        BlockBreakups.Respawning(1, BlockArrangement.Single, BlockBreakupPace.Steady, CrumbleSound, ShotBlockOpenUpdates, BlockRestore.ShotBlock);

    /// <summary>A shot block breaking up for good, 1x1.</summary>
    private static LastingBlockReactionChange PermanentShotBlock1x1 { get; } =
        BlockBreakups.LastingPermanent(1, BlockArrangement.Single, BlockBreakupPace.Steady, CrumbleSound);

    /// <summary>A shot block breaking up for good, 2x1.</summary>
    private static LastingBlockReactionChange PermanentShotBlock2x1 { get; } =
        BlockBreakups.LastingPermanent(1, BlockArrangement.Horizontal, BlockBreakupPace.Steady, CrumbleSound);

    /// <summary>A shot block breaking up for good, 1x2.</summary>
    private static LastingBlockReactionChange PermanentShotBlock1x2 { get; } =
        BlockBreakups.LastingPermanent(1, BlockArrangement.Vertical, BlockBreakupPace.Steady, CrumbleSound);

    /// <summary>A shot block breaking up for good, 2x2.</summary>
    private static LastingBlockReactionChange PermanentShotBlock2x2 { get; } =
        BlockBreakups.LastingPermanent(1, BlockArrangement.Square, BlockBreakupPace.Steady, CrumbleSound);

    /// <summary>A Super Missile block breaking up for good.</summary>
    internal static BlockReactionChange PermanentSuperMissileBlock { get; } =
        BlockBreakups.Permanent(1, BlockArrangement.Single, BlockBreakupPace.Steady, SuperMissileBlockSound);

    /// <summary>A Power Bomb block breaking up for good.</summary>
    internal static BlockReactionChange PermanentPowerBombBlock { get; } =
        BlockBreakups.Permanent(1, BlockArrangement.Single, PowerBombBlockPace, CrumbleSound);

    /// <summary>A bomb block Samus's contact sets off, with its break sound: a 1x1 block, growing back.</summary>
    private static BlockReactionChange CollisionBombBlock1x1Respawning { get; } =
        BlockBreakups.Respawning(1, BlockArrangement.Single, BlockBreakupPace.Steady, ContactBreakSound, ShotBlockOpenUpdates, BlockRestore.BombBlock);

    /// <summary>A bomb block a Power Bomb sets off, with its crumble sound: a 1x1 block, growing back.</summary>
    private static BlockReactionChange ReactionBombBlock1x1Respawning { get; } =
        BlockBreakups.Respawning(1, BlockArrangement.Single, BlockBreakupPace.Steady, ReactionCrumbleSound, ShotBlockOpenUpdates, BlockRestore.BombBlock);

    /// <summary>A bomb block a normal bomb sets off, without a sound: a 1x1 block, growing back.</summary>
    private static BlockReactionChange BombBlock1x1RespawningBreak { get; } =
        BlockBreakups.Respawning(1, BlockArrangement.Single, BlockBreakupPace.Steady, null, ShotBlockOpenUpdates, BlockRestore.BombBlock);

    /// <summary>A bomb block Samus's contact sets off, with its break sound: a 2x1 block, growing back.</summary>
    private static BlockReactionChange CollisionBombBlock2x1Respawning { get; } =
        BlockBreakups.Respawning(1, BlockArrangement.Horizontal, BlockBreakupPace.Steady, ContactBreakSound, ShotBlockOpenUpdates, BlockRestore.BombBlock);

    /// <summary>A bomb block a Power Bomb sets off, with its crumble sound: a 2x1 block, growing back.</summary>
    private static BlockReactionChange ReactionBombBlock2x1Respawning { get; } =
        BlockBreakups.Respawning(1, BlockArrangement.Horizontal, BlockBreakupPace.Steady, ReactionCrumbleSound, ShotBlockOpenUpdates, BlockRestore.BombBlock);

    /// <summary>A bomb block a normal bomb sets off, without a sound: a 2x1 block, growing back.</summary>
    private static BlockReactionChange BombBlock2x1RespawningBreak { get; } =
        BlockBreakups.Respawning(1, BlockArrangement.Horizontal, BlockBreakupPace.Steady, null, ShotBlockOpenUpdates, BlockRestore.BombBlock);

    /// <summary>A bomb block Samus's contact sets off, with its break sound: a 1x2 block, growing back.</summary>
    private static BlockReactionChange CollisionBombBlock1x2Respawning { get; } =
        BlockBreakups.Respawning(1, BlockArrangement.Vertical, BlockBreakupPace.Steady, ContactBreakSound, ShotBlockOpenUpdates, BlockRestore.BombBlock);

    /// <summary>A bomb block a Power Bomb sets off, with its crumble sound: a 1x2 block, growing back.</summary>
    private static BlockReactionChange ReactionBombBlock1x2Respawning { get; } =
        BlockBreakups.Respawning(1, BlockArrangement.Vertical, BlockBreakupPace.Steady, ReactionCrumbleSound, ShotBlockOpenUpdates, BlockRestore.BombBlock);

    /// <summary>A bomb block a normal bomb sets off, without a sound: a 1x2 block, growing back.</summary>
    private static BlockReactionChange BombBlock1x2RespawningBreak { get; } =
        BlockBreakups.Respawning(1, BlockArrangement.Vertical, BlockBreakupPace.Steady, null, ShotBlockOpenUpdates, BlockRestore.BombBlock);

    /// <summary>A bomb block Samus's contact sets off, with its break sound: a 2x2 block, growing back.</summary>
    private static BlockReactionChange CollisionBombBlock2x2Respawning { get; } =
        BlockBreakups.Respawning(1, BlockArrangement.Square, BlockBreakupPace.Steady, ContactBreakSound, ShotBlockOpenUpdates, BlockRestore.BombBlock);

    /// <summary>A bomb block a Power Bomb sets off, with its crumble sound: a 2x2 block, growing back.</summary>
    private static BlockReactionChange ReactionBombBlock2x2Respawning { get; } =
        BlockBreakups.Respawning(1, BlockArrangement.Square, BlockBreakupPace.Steady, ReactionCrumbleSound, ShotBlockOpenUpdates, BlockRestore.BombBlock);

    /// <summary>A bomb block a normal bomb sets off, without a sound: a 2x2 block, growing back.</summary>
    private static BlockReactionChange BombBlock2x2RespawningBreak { get; } =
        BlockBreakups.Respawning(1, BlockArrangement.Square, BlockBreakupPace.Steady, null, ShotBlockOpenUpdates, BlockRestore.BombBlock);

    /// <summary>A bomb block Samus's contact sets off, with its break sound: a 1x1 block, gone for good.</summary>
    private static BlockReactionChange CollisionBombBlock1x1Permanent { get; } =
        BlockBreakups.Permanent(1, BlockArrangement.Single, BlockBreakupPace.Steady, ContactBreakSound);

    /// <summary>A bomb block a Power Bomb sets off, with its crumble sound: a 1x1 block, gone for good.</summary>
    private static BlockReactionChange ReactionBombBlock1x1Permanent { get; } =
        BlockBreakups.Permanent(1, BlockArrangement.Single, BlockBreakupPace.Steady, ReactionCrumbleSound);

    /// <summary>A bomb block a normal bomb sets off, without a sound: a 1x1 block, gone for good.</summary>
    private static BlockReactionChange BombBlock1x1PermanentBreak { get; } =
        BlockBreakups.Permanent(1, BlockArrangement.Single, BlockBreakupPace.Steady, null);

    /// <summary>A bomb block Samus's contact sets off, with its break sound: a 2x1 block, gone for good.</summary>
    private static BlockReactionChange CollisionBombBlock2x1Permanent { get; } =
        BlockBreakups.Permanent(1, BlockArrangement.Horizontal, BlockBreakupPace.Steady, ContactBreakSound);

    /// <summary>A bomb block a Power Bomb sets off, with its crumble sound: a 2x1 block, gone for good.</summary>
    private static BlockReactionChange ReactionBombBlock2x1Permanent { get; } =
        BlockBreakups.Permanent(1, BlockArrangement.Horizontal, BlockBreakupPace.Steady, ReactionCrumbleSound);

    /// <summary>A bomb block a normal bomb sets off, without a sound: a 2x1 block, gone for good.</summary>
    private static BlockReactionChange BombBlock2x1PermanentBreak { get; } =
        BlockBreakups.Permanent(1, BlockArrangement.Horizontal, BlockBreakupPace.Steady, null);

    /// <summary>A bomb block Samus's contact sets off, with its break sound: a 1x2 block, gone for good.</summary>
    private static BlockReactionChange CollisionBombBlock1x2Permanent { get; } =
        BlockBreakups.Permanent(1, BlockArrangement.Vertical, BlockBreakupPace.Steady, ContactBreakSound);

    /// <summary>A bomb block a Power Bomb sets off, with its crumble sound: a 1x2 block, gone for good.</summary>
    private static BlockReactionChange ReactionBombBlock1x2Permanent { get; } =
        BlockBreakups.Permanent(1, BlockArrangement.Vertical, BlockBreakupPace.Steady, ReactionCrumbleSound);

    /// <summary>A bomb block a normal bomb sets off, without a sound: a 1x2 block, gone for good.</summary>
    private static BlockReactionChange BombBlock1x2PermanentBreak { get; } =
        BlockBreakups.Permanent(1, BlockArrangement.Vertical, BlockBreakupPace.Steady, null);

    /// <summary>A bomb block Samus's contact sets off, with its break sound: a 2x2 block, gone for good.</summary>
    private static BlockReactionChange CollisionBombBlock2x2Permanent { get; } =
        BlockBreakups.Permanent(1, BlockArrangement.Square, BlockBreakupPace.Steady, ContactBreakSound);

    /// <summary>A bomb block a Power Bomb sets off, with its crumble sound: a 2x2 block, gone for good.</summary>
    private static BlockReactionChange ReactionBombBlock2x2Permanent { get; } =
        BlockBreakups.Permanent(1, BlockArrangement.Square, BlockBreakupPace.Steady, ReactionCrumbleSound);

    /// <summary>A bomb block a normal bomb sets off, without a sound: a 2x2 block, gone for good.</summary>
    private static BlockReactionChange BombBlock2x2PermanentBreak { get; } =
        BlockBreakups.Permanent(1, BlockArrangement.Square, BlockBreakupPace.Steady, null);

    /// <summary>A respawning breakable Grapple block: it holds, breaks up, grows back and breaks again.</summary>
    internal static BlockReactionChange RespawningBreakableGrappleBlock { get; } = new(1,
    [
        new(GrappleBlockObjectDraws.Family.Of(GrappleBlockDraw.Intact), 240),
        new(GrappleBlockObjectDraws.Family.Of(GrappleBlockDraw.Rock), 4, SoundBefore: EnemySound.BlockCrumble) { SoundPriority = SoundRequestPriority.Normal },
        new(GrappleBlockObjectDraws.Family.Of(GrappleBlockDraw.Pieces), 4),
        new(GrappleBlockObjectDraws.Family.Of(GrappleBlockDraw.Dust), 4),
        new(GrappleBlockObjectDraws.Family.Of(GrappleBlockDraw.Blank), 6),
        new(GrappleBlockObjectDraws.Family.Of(GrappleBlockDraw.Dust), 4),
        new(GrappleBlockObjectDraws.Family.Of(GrappleBlockDraw.Pieces), 4),
        new(GrappleBlockObjectDraws.Family.Of(GrappleBlockDraw.Rock), 4),
        BlockReactionFrame.RestoredBlock,
    ]);

    /// <summary>A breakable Grapple block: it holds, then breaks up for good.</summary>
    internal static BlockReactionChange PermanentBreakableGrappleBlock { get; } = new(1,
    [
        new(GrappleBlockObjectDraws.Family.Of(GrappleBlockDraw.Intact), 120),
        new(GrappleBlockObjectDraws.Family.Of(GrappleBlockDraw.Rock), 4, SoundBefore: EnemySound.BlockCrumble) { SoundPriority = SoundRequestPriority.Normal },
        new(GrappleBlockObjectDraws.Family.Of(GrappleBlockDraw.Pieces), 4),
        new(GrappleBlockObjectDraws.Family.Of(GrappleBlockDraw.Dust), 4),
        new(GrappleBlockObjectDraws.Family.Of(GrappleBlockDraw.Blank), 1),
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
    internal static IReadOnlyDictionary<SpecialBlockReaction, LastingBlockReactionChange> CrumbleReveal { get; } =
        new Dictionary<SpecialBlockReaction, LastingBlockReactionChange>
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
    internal static IReadOnlyDictionary<ShootableReaction, LastingBlockReactionChange> PermanentShot { get; } =
        new Dictionary<ShootableReaction, LastingBlockReactionChange>
        {
            [ShootableReaction.PermanentBreak1x1] = PermanentShotBlock1x1,
            [ShootableReaction.PermanentBreak2x1] = PermanentShotBlock2x1,
            [ShootableReaction.PermanentBreak1x2] = PermanentShotBlock1x2,
            [ShootableReaction.PermanentBreak2x2] = PermanentShotBlock2x2,
        };
}
