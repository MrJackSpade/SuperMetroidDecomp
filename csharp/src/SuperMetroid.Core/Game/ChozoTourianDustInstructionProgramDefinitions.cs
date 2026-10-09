namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled control for Wrecked Ship Chozo footsteps/explosions and Tourian statue descent
/// dust. Their fourteen spritemap operands resolve through extracted presentation art.
/// </summary>
internal abstract class ChozoTourianDustInstructionProgramDefinitions
{
    /// <summary>Wrecked Ship Chozo spike-clearing footsteps at $86:AEC4.</summary>
    internal const ushort Footsteps = 0xaec4;

    /// <summary>Unused Wrecked Ship spike-clearing explosions at $86:AEDC.</summary>
    internal const ushort SpikeClearingExplosions = 0xaedc;

    /// <summary>Tourian entrance-statue descent dust at $86:AF14.</summary>
    internal const ushort TourianDescentDust = 0xaf14;

    /// <summary>Reset/randomize loop at $86:AF18, after the initial loop counter.</summary>
    internal const ushort TourianLoop = TourianDescentDust + 4;

    /// <summary>Number of address/value pairs defining the three compiled mechanics programs.</summary>
    public static int MechanicsWordCount => 31;

    /// <summary>Number of extracted spritemap operands referenced by those programs.</summary>
    public static int PresentationWordCount => 14;

    /// <summary>Gets the native address and value for a mechanics word in program order.</summary>
    /// <param name="index">Zero-based index across the footsteps, explosions, and statue-dust programs.</param>
    /// <returns>The ROM address and compiled value for the selected instruction word.</returns>
    /// <exception cref="IndexOutOfRangeException">The index is outside the compiled mechanics words.</exception>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        if (index < 18)
        {
            bool explosion = index >= 8;
            int local = explosion ? index - 8 : index;
            ushort start = explosion ? SpikeClearingExplosions : Footsteps;
            int frames = explosion ? 6 : 4;
            if (local == 0)
                return new(start, EnemyProjectileCodePointers.Instruction_MoveRandomlyWithinXRadius_YRadius);
            if (local < 3)
                return new((ushort)(start + 2 * local), local == 1 ? (ushort)15 : (ushort)0x030f);
            if (local < 3 + frames)
                return new((ushort)(start + 6 + 4 * (local - 3)), explosion ? (ushort)5 : (ushort)2);
            return new((ushort)(start + 6 + 4 * frames), EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete);
        }

        int dust = index - 18;
        if (dust < 6)
        {
            ushort value = dust switch
            {
                0 => EnemyProjectileCodePointers.Instruction_EnemyProjectile_TimerInY,
                1 => 64,
                2 => TourianStatueRomData.ResetDustPosition,
                3 => EnemyProjectileCodePointers.Instruction_MoveRandomlyWithinXRadius_YRadius,
                4 => 63,
                _ => 3,
            };
            return new((ushort)(TourianDescentDust + 2 * dust), value);
        }
        if (dust < 10)
            return new((ushort)(TourianDescentDust + 12 + 4 * (dust - 6)), 2);
        ushort control = dust switch
        {
            10 => EnemyProjectileCodePointers.Instruction_EnemyProjectile_DecrementTimer_GotoYIfNonZero,
            11 => TourianLoop,
            _ => EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete,
        };
        return new((ushort)(TourianDescentDust + 28 + 2 * (dust - 10)), control);
    }

    /// <summary>Gets the native address of an extracted spritemap operand in program order.</summary>
    /// <param name="index">Zero-based index across the three programs' presentation words.</param>
    /// <returns>The ROM address containing the operand reference.</returns>
    /// <exception cref="IndexOutOfRangeException">The index is outside the compiled presentation words.</exception>
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        if (index < 4) return (ushort)(Footsteps + 8 + 4 * index);
        if (index < 10) return (ushort)(SpikeClearingExplosions + 8 + 4 * (index - 4));
        return (ushort)(TourianDescentDust + 14 + 4 * (index - 10));
    }
    /// <summary>Determines whether this catalog compiles the instruction program for the projectile kind.</summary>
    /// <param name="kind">Projectile kind to check.</param>
    /// <returns><see langword="true"/> for the two Wrecked Ship Chozo footsteps or Tourian statue dust.</returns>
    internal static bool Owns(RoomEnemyProjectileKind kind) => kind is
        RoomEnemyProjectileKind.WreckedShipChozoSpikeFootstep or
        RoomEnemyProjectileKind.WreckedShipChozoSpikeFootstepAlternate or
        RoomEnemyProjectileKind.TourianStatueDescentDust;

    /// <summary>Looks up a compiled mechanics value by its native instruction address.</summary>
    /// <param name="address">ROM address of a word represented by this catalog.</param>
    /// <returns>The compiled value stored at that address.</returns>
    /// <exception cref="InvalidDataException">The address is not part of a compiled mechanics program.</exception>
    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = MechanicsWordCount - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            InstructionMechanicsWord candidate = MechanicsWord(middle);
            if (candidate.Address == address)
                return candidate.Value;
            if (candidate.Address < address)
                low = middle + 1;
            else
                high = middle - 1;
        }
        throw new InvalidDataException(
            $"Chozo/Tourian dust mechanics pointer $86:{address:X4} is not compiled.");
    }
}
