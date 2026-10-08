namespace SuperMetroid.Core.Game;

/// <summary>Named bank-$86 identities and tables used only by eye-door effects.</summary>
public static class EyeDoorEnemyProjectileRomData
{

    /// <summary><c>$86:B743</c>, native <c>EnemyProjectile_EyeDoorProjectile</c>: the eye-door attack actor, whose initial animation later installs aimed movement and whose shot/impact lists terminate it.</summary>
    public const ushort ProjectileDefinition = 0xb743;
    /// <summary><c>$86:B751</c>, native <c>EnemyProjectile_EyeDoorSweat</c>: the falling sweat actor spawned by the door PLM with side-selected launch velocity and a floor-impact animation.</summary>
    public const ushort SweatDefinition = 0xb751;
    /// <summary><c>$86:E517</c>, native <c>EnemyProjectile_MiscDustPLM</c>: the shared PLM dust/explosion definition used for eye-door smoke, with parameter-selected artwork and RNG-offset placement around the spawning block.</summary>
    public const ushort SmokeDefinition = 0xe517;

    /// <summary><c>$86:B6B9</c>, native <c>PreInstruction_EnemyProjectile_EyeDoorProjectile_Moving</c>: moves with block collision, accumulates angle-selected acceleration, and selects the explosion sequence on impact or an opened door bit.</summary>
    public const ushort ProjectilePreInstruction = 0xb6b9;
    /// <summary><c>$86:B714</c>, native <c>PreInstruction_EnemyProjectile_EyeDoorSweat</c>: moves the sweat drop, adds <c>$000C</c> to its 8.8 Y velocity per update, and selects its impact sequence on a downward floor collision.</summary>
    public const ushort SweatPreInstruction = 0xb714;
    /// <summary><c>$86:E508</c>, native <c>RTS_86E508</c>: no-op callback for PLM dust/smoke; also used by the translated eye-door attack and sweat actors to suspend motion while their impact instructions finish.</summary>
    public const ushort SmokeInertPreInstruction = 0xe508;

    /// <summary>Sixteen whole pixels per room block, used to convert the spawning PLM's block coordinate into projectile, sweat, and smoke origins.</summary>
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
    /// <param name="index">Zero-based word index 0..19, with alternating X and Y offsets; the consumer reads this word and the next from its parameter-selected byte offset.</param>
    /// <returns>The signed whole-pixel origin offset.</returns>
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
    /// <param name="index">Zero-based word index 0..3 in the interleaved left-X, left-Y, right-X, right-Y launch table.</param>
    /// <returns>The signed 8.8 velocity word, in 1/256-pixel units per gameplay update.</returns>
    public static short SweatVelocityWord(int index)
    {
        if ((uint)index >= SweatVelocityWordCount) throw new IndexOutOfRangeException();
        return (short)((index & 1) != 0 ? 512 : 64 * (index - 1));
    }
}
