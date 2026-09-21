namespace SuperMetroid.Core.Game;

/// <summary>Compiled instruction selectors for Mother Brain's body-relative death explosions.</summary>
internal static class MotherBrainDeathExplosionDefinitions
{
    /// <summary>
    /// Three instruction-list pointers at <c>$86:C929-$86:C92E</c>: small explosion,
    /// smoke, and big explosion, selected by projectile parameter zero through two.
    /// </summary>
    private static readonly ushort[] InstructionLists =
    [
        EnemyProjectileInstructionMechanicsDefinitions.MotherBrainSmallDeathExplosionInitial,
        EnemyProjectileInstructionMechanicsDefinitions.MotherBrainDeathSmokeInitial,
        EnemyProjectileInstructionMechanicsDefinitions.MotherBrainBigDeathExplosionInitial,
    ];

    /// <summary>Returns the native animation-program identity for one death explosion.</summary>
    internal static ushort InstructionList(ushort parameter)
    {
        if (parameter >= InstructionLists.Length)
        {
            throw new ArgumentOutOfRangeException(
                nameof(parameter), parameter, "Mother Brain death explosion parameter must be zero through two.");
        }

        return InstructionLists[parameter];
    }
}
