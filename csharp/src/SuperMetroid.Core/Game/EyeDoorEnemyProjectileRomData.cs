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

    public const int PixelsPerRoomBlock = 16;

    /// <summary>$86:B65B InitAI_EnemyProjectile_EyeDoorProjectile.Xpositions: ten interleaved X/Y origin pairs.</summary>
    public const int ProjectileOriginWordCount = 20;
    /// <summary>$86:B6B1 InitAI_EnemyProjectile_EyeDoorSweat.Xvelocity: two interleaved X/Y launch pairs.</summary>
    public const int SweatVelocityWordCount = 4;

    /// <summary>
    /// $86:B65B..B682: left/right door origins are one block sideways and down.
    /// Unused left patterns alternate six/eight blocks sideways and four/two
    /// blocks above or below. Unused right patterns advance one block at a time,
    /// four blocks above. Word addressing preserves the existing overlapping-pair domain.
    /// </summary>
    public static short ProjectileOriginWord(int index)
    {
        if ((uint)index >= ProjectileOriginWordCount) throw new IndexOutOfRangeException();
        int pair = index / 2;
        bool yAxis = (index & 1) != 0;
        if (pair is 0 or 5)
            return (short)(yAxis ? PixelsPerRoomBlock : pair == 0 ? -PixelsPerRoomBlock : PixelsPerRoomBlock);
        if (pair < 5)
            return (short)(yAxis ? (pair <= 2 ? -1 : 1) * ((pair & 1) != 0 ? 4 : 2) * PixelsPerRoomBlock
                : -((pair & 1) != 0 ? 6 : 8) * PixelsPerRoomBlock);
        return (short)(yAxis ? -4 * PixelsPerRoomBlock : pair * PixelsPerRoomBlock);
    }

    /// <summary>$86:B6B1: horizontal launch is minus/plus one-quarter pixel; both vertical launches are two pixels per frame.</summary>
    public static short SweatVelocityWord(int index)
    {
        if ((uint)index >= SweatVelocityWordCount) throw new IndexOutOfRangeException();
        return (short)((index & 1) != 0 ? 512 : 64 * (index - 1));
    }
}