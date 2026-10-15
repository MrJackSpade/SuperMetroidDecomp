
namespace SuperMetroid.Core.Samus.Movement;

/// <summary>Cartridge values of <see cref="SquareSlopeDefinitions"/> that only verification reads.</summary>
internal static class SquareSlopeDefinitionsConstants
{
    /// <summary>$A0:C435 SquareSlopeDefinitions_BankA0: solidity and quadrant identity.</summary>
    public const int EnemyReferenceAddress = 0xa0c435;
    /// <summary>$86:8729 SquareSlopeDefinitions_Bank86: identical enemy-projectile copy.</summary>
    public const int ProjectileReferenceAddress = 0x868729;

    /// <summary>Provides native enemy and projectile square-slope reference addresses for collision checks.</summary>
    extension(SquareSlopeDefinitions)
    {
        /// <inheritdoc cref="SquareSlopeDefinitionsConstants.EnemyReferenceAddress"/>
        internal static int EnemyReferenceAddress => SquareSlopeDefinitionsConstants.EnemyReferenceAddress;
        /// <inheritdoc cref="SquareSlopeDefinitionsConstants.ProjectileReferenceAddress"/>
        internal static int ProjectileReferenceAddress => SquareSlopeDefinitionsConstants.ProjectileReferenceAddress;
    }
}
