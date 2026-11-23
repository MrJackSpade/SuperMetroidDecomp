using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>What each of Samus's weapon families sets off at the blocks only some weapons break (#1554, #1742, #2559).</summary>
internal static partial class WeaponBlockReactionDefinitions
{
    /// <summary>
    /// The bomb-block breaks each bomb family sets off, by the block's reaction: a normal bomb sets off the break directly,
    /// without the reaction sound, and a Power Bomb sets off the reaction that plays it. No other family bombs a block.
    /// </summary>
    internal static readonly IReadOnlyDictionary<SamusProjectileFamily, IReadOnlyDictionary<BombableReaction, BlockReactionChange>> BombedBlockBreaks =
        new Dictionary<SamusProjectileFamily, IReadOnlyDictionary<BombableReaction, BlockReactionChange>>
        {
            [SamusProjectileFamily.Bomb] = BlockReactionChangeDefinitions.BombBreak,
            [SamusProjectileFamily.PowerBomb] = BlockReactionChangeDefinitions.ReactionBomb,
        };

    /// <summary>
    /// Whether each bomb family sets off the special block it hits: a normal bomb does, and a Power Bomb leaves special
    /// blocks alone. No other family bombs a special block.
    /// </summary>
    internal static readonly IReadOnlyDictionary<SamusProjectileFamily, bool> SetsOffSpecialBlocks = new Dictionary<SamusProjectileFamily, bool>
    {
        [SamusProjectileFamily.Bomb] = true,
        [SamusProjectileFamily.PowerBomb] = false,
    };

    /// <summary>The families a Power Bomb block yields to: its own weapon breaks it, and a normal bomb shows its reveal.</summary>
    private static readonly IReadOnlyDictionary<SamusProjectileFamily, ShotBlockWeaponEffect> PowerBombBlockWeapons =
        new Dictionary<SamusProjectileFamily, ShotBlockWeaponEffect>
        {
            [SamusProjectileFamily.PowerBomb] = ShotBlockWeaponEffect.Breaks,
            [SamusProjectileFamily.Bomb] = ShotBlockWeaponEffect.ShowsReveal,
        };

    /// <summary>The families a Super Missile block yields to: its own weapon breaks it, and a normal bomb shows its reveal.</summary>
    private static readonly IReadOnlyDictionary<SamusProjectileFamily, ShotBlockWeaponEffect> SuperMissileBlockWeapons =
        new Dictionary<SamusProjectileFamily, ShotBlockWeaponEffect>
        {
            [SamusProjectileFamily.SuperMissile] = ShotBlockWeaponEffect.Breaks,
            [SamusProjectileFamily.Bomb] = ShotBlockWeaponEffect.ShowsReveal,
        };

    /// <summary>
    /// The shot blocks only some weapons break, by their reaction: the respawning and permanent Power Bomb and Super
    /// Missile blocks. Every other shot-block reaction yields to every weapon.
    /// </summary>
    internal static readonly IReadOnlyDictionary<ShotBlockReaction, WeaponShotBlock> WeaponShotBlocks =
        new Dictionary<ShotBlockReaction, WeaponShotBlock>
        {
            [ShotBlockReaction.RespawningPowerBombBreak] = new(RoomObjectSetupBlock.PowerBombBlockParent,
                BlockReactionChangeDefinitions.RespawningPowerBombBlock, BlockReactionChangeDefinitions.BombedPowerBombBlockReveal,
                PowerBombBlockWeapons),
            [ShotBlockReaction.PermanentPowerBombBreak] = new(RoomObjectSetupBlock.PowerBombBlockParent,
                BlockReactionChangeDefinitions.PermanentPowerBombBlock, BlockReactionChangeDefinitions.BombedPowerBombBlockReveal,
                PowerBombBlockWeapons),
            [ShotBlockReaction.RespawningSuperMissileBreak] = new(RoomObjectSetupBlock.SuperMissileBlockParent,
                BlockReactionChangeDefinitions.RespawningSuperMissileBlock, BlockReactionChangeDefinitions.BombedSuperMissileBlockReveal,
                SuperMissileBlockWeapons),
            [ShotBlockReaction.PermanentSuperMissileBreak] = new(RoomObjectSetupBlock.SuperMissileBlockParent,
                BlockReactionChangeDefinitions.PermanentSuperMissileBlock, BlockReactionChangeDefinitions.BombedSuperMissileBlockReveal,
                SuperMissileBlockWeapons),
        };
}
