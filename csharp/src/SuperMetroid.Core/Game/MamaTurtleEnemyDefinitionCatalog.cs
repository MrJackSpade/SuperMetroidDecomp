namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled bank-$A0 enemy headers for the Mama Turtle family. Gameplay statistics,
/// callback identities, and presentation bindings stay application-owned while the
/// referenced artwork and animation programs can be separated into editable assets.
/// </summary>
internal static class MamaTurtleEnemyDefinitionCatalog
{
    /// <summary><c>EnemyHeaders_MamaTurtle</c> at <c>$A0:CF3F</c>.</summary>
    internal const ushort MamaPointer = 0xcf3f;

    /// <summary><c>EnemyHeaders_BabyTurtle</c> at <c>$A0:CF7F</c>.</summary>
    internal const ushort BabyPointer = 0xcf7f;

    /// <summary>First source byte occupied by the two contiguous native headers.</summary>
    internal const int SourceAddress = 0xa0cf3f;

    /// <summary>Total byte length of the two native 64-byte headers.</summary>
    internal const int SourceByteLength = 128;

    private static readonly RoomEnemyDefinition Mama = new(
        TileDataSize: 0x0c00,
        PalettePointer: 0x8b60,
        Health: 0x4e20,
        Damage: 0x00c8,
        XRadius: 0x0014,
        YRadius: 0x0010,
        Bank: 0xa2,
        HurtAiTime: 0x00,
        HurtSoundEffect: 0x0000,
        BossId: 0x0000,
        InitializationAiPointer: 0x8d6c,
        PartCount: 0x0005,
        Unused16: 0x0000,
        MainAiPointer: 0x8dd2,
        GrappleAiPointer: 0x800f,
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

    private static readonly RoomEnemyDefinition Baby = new(
        TileDataSize: 0x0c00,
        PalettePointer: 0x8b60,
        Health: 0x4e20,
        Damage: 0x0000,
        XRadius: 0x0008,
        YRadius: 0x0005,
        Bank: 0xa2,
        HurtAiTime: 0x00,
        HurtSoundEffect: 0x0000,
        BossId: 0x0000,
        InitializationAiPointer: 0x8d9d,
        PartCount: 0x0001,
        Unused16: 0x0000,
        MainAiPointer: 0x912e,
        GrappleAiPointer: 0x800f,
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
    internal static bool TryGet(ushort pointer, out RoomEnemyDefinition definition)
    {
        switch (pointer)
        {
            case MamaPointer:
                definition = Mama;
                return true;
            case BabyPointer:
                definition = Baby;
                return true;
            default:
                definition = default;
                return false;
        }
    }
}
