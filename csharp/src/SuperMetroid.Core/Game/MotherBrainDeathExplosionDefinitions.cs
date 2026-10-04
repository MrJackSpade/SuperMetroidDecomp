namespace SuperMetroid.Core.Game;

/// <summary>Compiled instruction selectors for Mother Brain's body-relative death explosions.</summary>
internal static class MotherBrainDeathExplosionDefinitions
{
    /// <summary>
    /// Three instruction-list identities at <c>$86:C929-$86:C92E</c>: small explosion,
    /// smoke, and big explosion, selected by projectile parameter zero through two.
    /// </summary>
    internal static ushort InstructionList(ushort parameter) => parameter switch
    {
        0 => EnemyProjectileInstructionMechanicsDefinitions.MotherBrainSmallDeathExplosionInitial,
        1 => EnemyProjectileInstructionMechanicsDefinitions.MotherBrainDeathSmokeInitial,
        2 => EnemyProjectileInstructionMechanicsDefinitions.MotherBrainBigDeathExplosionInitial,
        _ => throw new ArgumentOutOfRangeException(
            nameof(parameter), parameter, "Mother Brain death explosion parameter must be zero through two."),
    };
}
