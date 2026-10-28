
namespace SuperMetroid.Core.Samus.Movement;

/// <summary>Cartridge values of <see cref="SquareSlopes"/> that only verification reads.</summary>
internal static class SquareSlopesConstants
{
    /// <summary>$A0:C435 SquareSlopeDefinitions_BankA0: solidity and quadrant identity.</summary>
    public const int EnemyReferenceAddress = 0xa0c435;
    /// <summary>$86:8729 SquareSlopeDefinitions_Bank86: identical enemy-projectile copy.</summary>
    public const int ProjectileReferenceAddress = 0x868729;

    /// <summary>Provides native enemy and projectile square-slope reference addresses for collision checks.</summary>
    extension(SquareSlopes)
    {
        /// <inheritdoc cref="SquareSlopesConstants.EnemyReferenceAddress"/>
        internal static int EnemyReferenceAddress => SquareSlopesConstants.EnemyReferenceAddress;
        /// <inheritdoc cref="SquareSlopesConstants.ProjectileReferenceAddress"/>
        internal static int ProjectileReferenceAddress => SquareSlopesConstants.ProjectileReferenceAddress;
    }
}
