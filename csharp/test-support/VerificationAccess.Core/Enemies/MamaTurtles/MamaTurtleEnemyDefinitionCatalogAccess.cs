namespace SuperMetroid.Core.Enemies.MamaTurtles;
using SuperMetroid.Core.Samus.Grapple;

/// <summary>Independent compiled Mama Turtle family headers that verification compares with the cartridge.</summary>
internal static class MamaTurtleEnemyDefinitionCatalogAccess
{
    /// <summary>Compiled Mama Turtle enemy definition.</summary>
    private static readonly RoomEnemyDefinition Mama = new(
        TileDataSize: 0x0c00,
        PalettePointer: 0x8b60,
        Health: 0x4e20,
        Damage: 0x00c8,
        Radius: new(0x0014, 0x0010),
        Bank: 0xa2,
        HurtAiTime: 0x00,
        HurtSoundEffect: 0x0000,
        BossId: 0x0000,
        InitializationAiPointer: 0x8d6c,
        PartCount: 0x0005,
        Unused16: 0x0000,
        MainAiPointer: 0x8dd2,
        GrappleAiPointer: GrappleAiRoutine.Cancel,
        HurtAiPointer: 0x804c,
        FrozenAiPointer: 0x8041,
        TimeFrozenAiPointer: 0x0000,
        DeathAnimation: 0x0004,
        Unused24: 0x0000,
        Unused26: 0x0000,
        PowerBombReactionPointer: 0x0000,
        VariantIndex: 0x0000,
        Unused2C: 0x0000,
        Unused2E: 0x0000,
        TouchAiPointer: 0x9281,
        ShotAiPointer: 0x802d,
        InitialSpritemapPointer: 0x0000,
        TileDataAddress: 0xacd400,
        Layer: 0x05,
        ItemDropChancesPointer: 0xf3bc,
        VulnerabilityPointer: 0xeec6,
        NamePointer: 0xdf11);

    /// <summary>Compiled baby turtle enemy definition.</summary>
    private static readonly RoomEnemyDefinition Baby = new(
        TileDataSize: 0x0c00,
        PalettePointer: 0x8b60,
        Health: 0x4e20,
        Damage: 0x0000,
        Radius: new(0x0008, 0x0005),
        Bank: 0xa2,
        HurtAiTime: 0x00,
        HurtSoundEffect: 0x0000,
        BossId: 0x0000,
        InitializationAiPointer: 0x8d9d,
        PartCount: 0x0001,
        Unused16: 0x0000,
        MainAiPointer: 0x912e,
        GrappleAiPointer: GrappleAiRoutine.Cancel,
        HurtAiPointer: 0x804c,
        FrozenAiPointer: 0x8041,
        TimeFrozenAiPointer: 0x0000,
        DeathAnimation: 0x0000,
        Unused24: 0x0000,
        Unused26: 0x0000,
        PowerBombReactionPointer: 0x0000,
        VariantIndex: 0x0000,
        Unused2C: 0x0000,
        Unused2E: 0x0000,
        TouchAiPointer: 0x929f,
        ShotAiPointer: 0x930f,
        InitialSpritemapPointer: 0x0000,
        TileDataAddress: 0xacd400,
        Layer: 0x05,
        ItemDropChancesPointer: 0xf3bc,
        VulnerabilityPointer: 0xeec6,
        NamePointer: 0x0000);

    /// <summary>Resolves a compiled family header by its native bank-$A0 pointer.</summary>
    internal static bool TryGet(EnemyDefinitionId pointer, out RoomEnemyDefinition definition)
    {
        definition = pointer == EnemyDefinitionId.MamaTurtle ? Mama
            : pointer == EnemyDefinitionId.BabyTurtle ? Baby
            : default;
        return pointer is EnemyDefinitionId.MamaTurtle or EnemyDefinitionId.BabyTurtle;
    }
}
