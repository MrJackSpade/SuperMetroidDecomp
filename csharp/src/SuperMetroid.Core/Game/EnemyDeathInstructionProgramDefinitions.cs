namespace SuperMetroid.Core.Game;

/// <summary>One compiled generic enemy-death mechanics word at its bank-$86 address.</summary>
internal readonly record struct EnemyDeathInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled control for the five generic enemy-death animations and their shared blank
/// respawn tail. The thirty-one spritemap operands remain live cartridge presentation data.
/// </summary>
internal static class EnemyDeathInstructionProgramDefinitions
{
    /// <summary><c>InstList_EnemyProjectile_Pickup_HandleRespawningEnemy</c> at $86:ECA3.</summary>
    internal const ushort RespawnTail = 0xeca3;

    /// <summary>Big-explosion death program at $86:ECAB.</summary>
    internal const ushort BigExplosion = 0xecab;

    /// <summary>Mini-Kraid death program at $86:ECC5.</summary>
    internal const ushort MiniKraidExplosion = 0xecc5;

    /// <summary>Normal-explosion death program at $86:ED4B.</summary>
    internal const ushort NormalExplosion = 0xed4b;

    /// <summary>Small-explosion death program at $86:ED69.</summary>
    internal const ushort SmallExplosion = 0xed69;

    /// <summary>Samus-contact death program at $86:EDFF.</summary>
    internal const ushort KilledBySamusContact = 0xedff;

    private static readonly EnemyDeathInstructionMechanicsWord[] Words =
    [
        new(RespawnTail, 0x0040),
        new(0xeca7,
            EnemyProjectileCodePointers.Instruction_EnemyProjectile_Pickup_HandleRespawningEnemy),
        new(0xeca9, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete),

        new(BigExplosion, EnemyProjectileCodePointers.Instruction_EnemyProjectile_TimerInY),
        new(0xecad, 0x0005),
        new(0xecaf,
            EnemyProjectileCodePointers.Instruction_EnemyProj_EnemyDeathExpl_SpawnSpriteObjectInY_10),
        new(0xecb1, 0x0003),
        new(0xecb3,
            EnemyProjectileCodePointers.Instruction_EnemyProj_EnemyDeathExpl_SpawnSpriteObjectInY_10),
        new(0xecb5, 0x000c),
        new(0xecb7, 0x0008),
        new(0xecbb,
            EnemyProjectileCodePointers.Instruction_EnemyProj_EDeathExplo_QueueSmallExplosionSoundFX),
        new(0xecbd,
            EnemyProjectileCodePointers.Instruction_EnemyProjectile_DecrementTimer_GotoYIfNonZero),
        new(0xecbf, 0xecaf),
        new(0xecc1,
            EnemyProjectileCodePointers.Instruction_EnemyProjectile_EnemyDeathExplosion_BecomePickup),
        new(0xecc3, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete),

        new(MiniKraidExplosion,
            EnemyProjectileCodePointers.Instruction_EnemyProjectile_TimerInY),
        new(0xecc7, 0x0010),
        new(0xecc9,
            EnemyProjectileCodePointers.Instruction_EnemyProj_EnemyDeathExpl_SpawnSpriteObjectInY_20),
        new(0xeccb, 0x0003),
        new(0xeccd,
            EnemyProjectileCodePointers.Instruction_EnemyProj_EnemyDeathExpl_SpawnSpriteObjectInY_20),
        new(0xeccf, 0x000c),
        new(0xecd1,
            EnemyProjectileCodePointers.Instruction_EnemyProj_EnemyDeathExpl_SpawnSpriteObjectInY_20),
        new(0xecd3, 0x0015),
        new(0xecd5, 0x0008),
        new(0xecd9,
            EnemyProjectileCodePointers.Instruction_EnemyProj_EDeathExplo_QueueSmallExplosionSoundFX),
        new(0xecdb,
            EnemyProjectileCodePointers.Instruction_EnemyProjectile_DecrementTimer_GotoYIfNonZero),
        new(0xecdd, 0xecc9),
        new(0xecdf,
            EnemyProjectileCodePointers.Instruction_EnemyProjectile_EnemyDeathExplosion_BecomePickup),
        new(0xece1, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete),

        new(NormalExplosion, 0x0005),
        new(0xed4f, 0x0005),
        new(0xed53, 0x0005),
        new(0xed57,
            EnemyProjectileCodePointers.Instruction_EnemyProj_EnemyDeathExpl_QueueEnemyKilledSoundFX),
        new(0xed59, 0x0005),
        new(0xed5d, 0x0005),
        new(0xed61, 0x0005),
        new(0xed65,
            EnemyProjectileCodePointers.Instruction_EnemyProjectile_EnemyDeathExplosion_BecomePickup),
        new(0xed67, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete),

        new(SmallExplosion, 0x0004),
        new(0xed6d, 0x0006),
        new(0xed71, 0x0005),
        new(0xed75,
            EnemyProjectileCodePointers.Instruction_EnemyProj_EnemyDeathExpl_QueueEnemyKilledSoundFX),
        new(0xed77, 0x0005),
        new(0xed7b, 0x0005),
        new(0xed7f, 0x0006),
        new(0xed83,
            EnemyProjectileCodePointers.Instruction_EnemyProjectile_EnemyDeathExplosion_BecomePickup),
        new(0xed85, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete),

        new(KilledBySamusContact, 0x0002),
        new(0xee03, 0x0002),
        new(0xee07, 0x0002),
        new(0xee0b, 0x0002),
        new(0xee0f, 0x0002),
        new(0xee13, 0x0002),
        new(0xee17, 0x0002),
        new(0xee1b,
            EnemyProjectileCodePointers.Instruction_EnemyProj_EDeathExplo_QueueContactKilledSoundFX),
        new(0xee1d, 0x0002),
        new(0xee21, 0x0002),
        new(0xee25, 0x0002),
        new(0xee29, 0x0002),
        new(0xee2d, 0x0002),
        new(0xee31, 0x0002),
        new(0xee35, 0x0002),
        new(0xee39, 0x0002),
        new(0xee3d, 0x0002),
        new(0xee41,
            EnemyProjectileCodePointers.Instruction_EnemyProjectile_EnemyDeathExplosion_BecomePickup),
        new(0xee43, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete),
    ];

    private static readonly ushort[] PresentationWords =
    [
        0xeca5, 0xecb9, 0xecd7,
        0xed4d, 0xed51, 0xed55, 0xed5b, 0xed5f, 0xed63,
        0xed6b, 0xed6f, 0xed73, 0xed79, 0xed7d, 0xed81,
        0xee01, 0xee05, 0xee09, 0xee0d, 0xee11, 0xee15, 0xee19, 0xee1f,
        0xee23, 0xee27, 0xee2b, 0xee2f, 0xee33, 0xee37, 0xee3b, 0xee3f,
    ];

    internal static int MechanicsWordCount => Words.Length;
    internal static int PresentationWordCount => PresentationWords.Length;
    internal static EnemyDeathInstructionMechanicsWord MechanicsWord(int index) => Words[index];
    internal static ushort PresentationWordAddress(int index) => PresentationWords[index];

    internal static bool Owns(RoomEnemyProjectileKind kind, ushort address) =>
        (kind == RoomEnemyProjectileKind.EnemyDeathExplosion ||
         // A collected/expired pickup jumps to the same native respawn tail; it
         // must not acquire ownership of the explosion programs that follow it.
         (kind == RoomEnemyProjectileKind.EnemyDeathPickup &&
          address >= RespawnTail && address < BigExplosion)) &&
        FindWord(address) >= 0;

    internal static ushort ReadMechanicsWord(ushort address)
    {
        int index = FindWord(address);
        if (index >= 0)
            return Words[index].Value;

        throw new InvalidDataException(
            $"Generic enemy-death mechanics pointer $86:{address:X4} is not compiled.");
    }

    internal static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != EnemyProjectileCodePointers.BankBase)
            return false;

        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < Words.Length; index++)
        {
            ushort wordAddress = Words[index].Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }

        return false;
    }

    private static int FindWord(ushort address)
    {
        int low = 0;
        int high = Words.Length - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            EnemyDeathInstructionMechanicsWord candidate = Words[middle];
            if (candidate.Address == address)
                return middle;
            if (candidate.Address < address)
                low = middle + 1;
            else
                high = middle - 1;
        }

        return -1;
    }
}
