namespace SuperMetroid.Core.Game;

/// <summary>The five mutually exclusive enemy-death animation variants.</summary>
internal enum EnemyDeathAnimation : ushort
{
    SmallExplosion = 0,
    KilledBySamusContact = 1,
    NormalExplosion = 2,
    MiniKraidExplosion = 3,
    BigExplosion = 4,
}

/// <summary>Compiled instruction selectors shared by generic and family-specific enemy deaths.</summary>
internal static class EnemyDeathExplosionDefinitions
{
    /// <summary>
    /// <c>InstListPointers_EnemyDeathExplosion</c> at <c>$86:EFD5-$86:EFDE</c>,
    /// indexed by the bounded death-animation variant.
    /// </summary>
    private static readonly ushort[] InstructionPointers =
    [
        EnemyDeathInstructionProgramDefinitions.SmallExplosion,
        EnemyDeathInstructionProgramDefinitions.KilledBySamusContact,
        EnemyDeathInstructionProgramDefinitions.NormalExplosion,
        EnemyDeathInstructionProgramDefinitions.MiniKraidExplosion,
        EnemyDeathInstructionProgramDefinitions.BigExplosion,
    ];

    /// <summary>$86:ECA3 blank-map wait used after a drop is collected or expires.</summary>
    internal const ushort NoDropTailInstruction =
        EnemyDeathInstructionProgramDefinitions.RespawnTail;

    /// <summary>Returns the bank-$86 instruction list for one authored death variant.</summary>
    internal static ushort InstructionPointer(ushort animation)
    {
        if (animation >= InstructionPointers.Length)
        {
            throw new ArgumentOutOfRangeException(
                nameof(animation), animation,
                "Enemy death animation must be zero through four.");
        }

        return InstructionPointers[animation];
    }
}
