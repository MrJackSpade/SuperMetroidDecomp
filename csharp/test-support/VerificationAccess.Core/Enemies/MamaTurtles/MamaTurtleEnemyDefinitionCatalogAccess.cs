using SuperMetroid.Core.Samus.Grapple;

namespace SuperMetroid.Core.Enemies.MamaTurtles;

/// <summary>Independent compiled Mama Turtle family headers that verification compares with the cartridge.</summary>
internal static class MamaTurtleEnemyDefinitionCatalogAccess
{
    /// <summary>Compiled Mama Turtle enemy definition.</summary>
    private static readonly RoomEnemyDefinition Mama = new(
        Health: 0x4e20,
        Damage: 0x00c8,
        Radius: new(0x0014, 0x0010),
        HurtFlashFrames: 4,
        HurtSoundEffect: null,
        Boss: null,
        GrappleResponse: GrappleEnemyReaction.Cancel,
        HurtResponse: EnemyHurtResponse.Inert,
        DeathExplosion: EnemyDeathExplosion.Big,
        PowerBombReaction: EnemyReaction.CommonPowerBomb,
        Touch: EnemyReaction.SpeciesOwn,
        Shot: EnemyReaction.OrdinaryShot,
        TouchHitboxScan: EnemyHitboxScan.Scanned,
        ShotHitboxScan: EnemyHitboxScan.Scanned,
        Drops: EnemyDrops.Rolling(new EnemyDropChances(80, 80, 80, 5, 5, 5)),
        Vulnerability: EnemyVulnerabilityDefinitions.Invulnerable);

    /// <summary>Compiled baby turtle enemy definition.</summary>
    private static readonly RoomEnemyDefinition Baby = new(
        Health: 0x4e20,
        Damage: 0x0000,
        Radius: new(0x0008, 0x0005),
        HurtFlashFrames: 4,
        HurtSoundEffect: null,
        Boss: null,
        GrappleResponse: GrappleEnemyReaction.Cancel,
        HurtResponse: EnemyHurtResponse.Inert,
        DeathExplosion: EnemyDeathExplosion.Small,
        PowerBombReaction: EnemyReaction.CommonPowerBomb,
        Touch: EnemyReaction.SpeciesOwn,
        Shot: EnemyReaction.SpeciesOwn,
        TouchHitboxScan: EnemyHitboxScan.Scanned,
        ShotHitboxScan: EnemyHitboxScan.Scanned,
        Drops: EnemyDrops.Rolling(new EnemyDropChances(80, 80, 80, 5, 5, 5)),
        Vulnerability: EnemyVulnerabilityDefinitions.Invulnerable);

    /// <summary>Resolves a compiled family header by its native bank-$A0 pointer.</summary>
    internal static bool TryGet(EnemyDefinitionId pointer, out RoomEnemyDefinition definition)
    {
        definition = pointer == EnemyDefinitionId.MamaTurtle ? Mama
            : pointer == EnemyDefinitionId.BabyTurtle ? Baby
            : default;
        return pointer is EnemyDefinitionId.MamaTurtle or EnemyDefinitionId.BabyTurtle;
    }
}
