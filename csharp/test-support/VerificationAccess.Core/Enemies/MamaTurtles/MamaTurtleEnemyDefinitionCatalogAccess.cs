namespace SuperMetroid.Core.Enemies.MamaTurtles;
using SuperMetroid.Core.Samus.Grapple;

/// <summary>Independent compiled Mama Turtle family headers that verification compares with the cartridge.</summary>
internal static class MamaTurtleEnemyDefinitionCatalogAccess
{
    /// <summary>Compiled Mama Turtle enemy definition.</summary>
    private static readonly RoomEnemyDefinition Mama = new(
        TileByteCount: 0x0c00,
        TilePlacement: EnemyTilePlacement.FollowingPrevious,
        Health: 0x4e20,
        Damage: 0x00c8,
        Radius: new(0x0014, 0x0010),
        HurtFlashFrames: 4,
        HurtSoundEffect: null,
        Boss: null,
        InitializationAi: EnemyAiRoutine.SpeciesRoutine,
        MainAi: EnemyAiRoutine.SpeciesRoutine,
        GrappleAi: GrappleAiRoutine.Cancel,
        HurtAi: EnemyAiRoutine.CommonNoOp,
        DeathExplosion: EnemyDeathExplosion.Big,
        PowerBombReaction: null,
        PowerBombReactionBody: EnemyCallbackBody.None,
        VariantIndex: 0x0000,
        Touch: EnemyInteractionCallback.MamaTurtleTouch,
        TouchBody: EnemyCallbackBody.Routine,
        Shot: EnemyInteractionCallback.NormalEnemyShot,
        ShotBody: EnemyCallbackBody.Routine,
        NoOpCallbackBody: EnemyCallbackBody.ImmediateReturn,
        Layer: 0x05,
        DropChances: new EnemyDropChances(80, 80, 80, 5, 5, 5),
        Vulnerability: EnemyVulnerabilityDefinitions.Invulnerable);

    /// <summary>Compiled baby turtle enemy definition.</summary>
    private static readonly RoomEnemyDefinition Baby = new(
        TileByteCount: 0x0c00,
        TilePlacement: EnemyTilePlacement.FollowingPrevious,
        Health: 0x4e20,
        Damage: 0x0000,
        Radius: new(0x0008, 0x0005),
        HurtFlashFrames: 4,
        HurtSoundEffect: null,
        Boss: null,
        InitializationAi: EnemyAiRoutine.SpeciesRoutine,
        MainAi: EnemyAiRoutine.SpeciesRoutine,
        GrappleAi: GrappleAiRoutine.Cancel,
        HurtAi: EnemyAiRoutine.CommonNoOp,
        DeathExplosion: EnemyDeathExplosion.Small,
        PowerBombReaction: null,
        PowerBombReactionBody: EnemyCallbackBody.None,
        VariantIndex: 0x0000,
        Touch: EnemyInteractionCallback.BabyTurtleTouch,
        TouchBody: EnemyCallbackBody.Routine,
        Shot: EnemyInteractionCallback.BabyTurtleShot,
        ShotBody: EnemyCallbackBody.Routine,
        NoOpCallbackBody: EnemyCallbackBody.ImmediateReturn,
        Layer: 0x05,
        DropChances: new EnemyDropChances(80, 80, 80, 5, 5, 5),
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
