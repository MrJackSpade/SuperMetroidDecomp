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
    /// <summary>$86:ECA3 blank-map wait used after a drop is collected or expires.</summary>
    internal const ushort NoDropTailInstruction =
        EnemyDeathInstructionProgramDefinitions.RespawnTail;

    /// <summary>Dispatches $86:EFD5-$86:EFDE by the mutually exclusive enemy-death animation variant.</summary>
    internal static ushort InstructionPointer(ushort animation)
    {
        return (EnemyDeathAnimation)animation switch
        {
            EnemyDeathAnimation.SmallExplosion => EnemyDeathInstructionProgramDefinitions.SmallExplosion,
            EnemyDeathAnimation.KilledBySamusContact => EnemyDeathInstructionProgramDefinitions.KilledBySamusContact,
            EnemyDeathAnimation.NormalExplosion => EnemyDeathInstructionProgramDefinitions.NormalExplosion,
            EnemyDeathAnimation.MiniKraidExplosion => EnemyDeathInstructionProgramDefinitions.MiniKraidExplosion,
            EnemyDeathAnimation.BigExplosion => EnemyDeathInstructionProgramDefinitions.BigExplosion,
            _ => throw new ArgumentOutOfRangeException(
                nameof(animation), animation,
                "Enemy death animation must be zero through four."),
        };
    }
}
