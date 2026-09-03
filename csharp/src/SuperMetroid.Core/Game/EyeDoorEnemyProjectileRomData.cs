namespace SuperMetroid.Core.Game;

/// <summary>Named bank-$86 identities and tables used only by eye-door effects.</summary>
public static class EyeDoorEnemyProjectileRomData
{
    /// <summary>SNES address-space base for bank-$86 enemy-projectile data.</summary>
    public const int BankBase = 0x860000;

    public const ushort ProjectileDefinition = 0xb743;
    public const ushort SweatDefinition = 0xb751;
    public const ushort SmokeDefinition = 0xe517;

    public const ushort ProjectilePreInstruction = 0xb6b9;
    public const ushort SweatPreInstruction = 0xb714;
    public const ushort SmokeInertPreInstruction = 0xe508;

    public const ushort ProjectileImpactInstructionList = 0xb5f3;
    public const ushort SweatImpactInstructionList = 0xb61d;
    public const ushort SmokeInstructionListTable = 0xe42c;
    public const ushort SmokeOffsetTable = 0xe47e;

    public const int PixelsPerRoomBlock = 16;

    /// <summary>
    /// Exact signed position-pair table at <c>$86:B62D</c>. The PLM parameter is an even
    /// byte offset, so indexing it as words reproduces the native absolute indexed read.
    /// </summary>
    public static ReadOnlySpan<short> ProjectileOriginOffsets =>
    [
        -16, 16, -96, -64, -128, -32, -96, 64, -128, 32,
        16, 16, 96, -64, 112, -64, 128, -64, 144, -64,
    ];

    /// <summary>Exact signed X/Y velocity pairs at <c>$86:B6B1</c>.</summary>
    public static ReadOnlySpan<short> SweatVelocities => [-64, 512, 64, 512];
}
