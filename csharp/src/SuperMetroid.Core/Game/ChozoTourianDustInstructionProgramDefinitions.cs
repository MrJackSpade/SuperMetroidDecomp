namespace SuperMetroid.Core.Game;

internal readonly record struct ChozoTourianDustInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled control for Wrecked Ship Chozo footsteps/explosions and Tourian statue descent
/// dust. Their fourteen spritemap operands remain live cartridge presentation data.
/// </summary>
internal static class ChozoTourianDustInstructionProgramDefinitions
{
    /// <summary>Wrecked Ship Chozo spike-clearing footsteps at $86:AEC4.</summary>
    internal const ushort Footsteps = 0xaec4;

    /// <summary>Unused Wrecked Ship spike-clearing explosions at $86:AEDC.</summary>
    internal const ushort SpikeClearingExplosions = 0xaedc;

    /// <summary>Tourian entrance-statue descent dust at $86:AF14.</summary>
    internal const ushort TourianDescentDust = 0xaf14;

    private static readonly ChozoTourianDustInstructionMechanicsWord[] Words =
    [
        new(Footsteps,
            EnemyProjectileCodePointers.Instruction_MoveRandomlyWithinXRadius_YRadius),
        new(0xaec6, 0x000f),
        new(0xaec8, 0x030f),
        new(0xaeca, 0x0002),
        new(0xaece, 0x0002),
        new(0xaed2, 0x0002),
        new(0xaed6, 0x0002),
        new(0xaeda, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete),
        new(SpikeClearingExplosions,
            EnemyProjectileCodePointers.Instruction_MoveRandomlyWithinXRadius_YRadius),
        new(0xaede, 0x000f),
        new(0xaee0, 0x030f),
        new(0xaee2, 0x0005),
        new(0xaee6, 0x0005),
        new(0xaeea, 0x0005),
        new(0xaeee, 0x0005),
        new(0xaef2, 0x0005),
        new(0xaef6, 0x0005),
        new(0xaefa, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete),
        new(TourianDescentDust,
            EnemyProjectileCodePointers.Instruction_EnemyProjectile_TimerInY),
        new(0xaf16, 0x0040),
        new(0xaf18, TourianStatueRomData.ResetDustPosition),
        new(0xaf1a,
            EnemyProjectileCodePointers.Instruction_MoveRandomlyWithinXRadius_YRadius),
        new(0xaf1c, 0x003f),
        new(0xaf1e, 0x0003),
        new(0xaf20, 0x0002),
        new(0xaf24, 0x0002),
        new(0xaf28, 0x0002),
        new(0xaf2c, 0x0002),
        new(0xaf30,
            EnemyProjectileCodePointers.Instruction_EnemyProjectile_DecrementTimer_GotoYIfNonZero),
        new(0xaf32, 0xaf18),
        new(0xaf34, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete),
    ];

    private static readonly ushort[] PresentationWords =
    [
        0xaecc, 0xaed0, 0xaed4, 0xaed8,
        0xaee4, 0xaee8, 0xaeec, 0xaef0, 0xaef4, 0xaef8,
        0xaf22, 0xaf26, 0xaf2a, 0xaf2e,
    ];

    internal static int MechanicsWordCount => Words.Length;
    internal static int PresentationWordCount => PresentationWords.Length;
    internal static ChozoTourianDustInstructionMechanicsWord MechanicsWord(int index) =>
        Words[index];
    internal static ushort PresentationWordAddress(int index) => PresentationWords[index];

    internal static bool Owns(RoomEnemyProjectileKind kind) => kind is
        RoomEnemyProjectileKind.WreckedShipChozoSpikeFootstep or
        RoomEnemyProjectileKind.WreckedShipChozoSpikeFootstepAlternate or
        RoomEnemyProjectileKind.TourianStatueDescentDust;

    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = Words.Length - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            ChozoTourianDustInstructionMechanicsWord candidate = Words[middle];
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

    internal static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != EnemyProjectileCodePointers.BankBase)
            return false;
        ushort bankAddress = unchecked((ushort)address);
        foreach (ChozoTourianDustInstructionMechanicsWord word in Words)
        {
            if (bankAddress == word.Address ||
                bankAddress == unchecked((ushort)(word.Address + 1)))
                return true;
        }
        return false;
    }
}
