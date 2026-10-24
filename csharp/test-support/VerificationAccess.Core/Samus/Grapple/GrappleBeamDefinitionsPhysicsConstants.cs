
namespace SuperMetroid.Core.Samus.Grapple;

/// <summary>Cartridge values of <see cref="GrappleBeamDefinitions.Physics"/> that only verification reads.</summary>
internal static class GrappleBeamDefinitionsPhysicsConstants
{
    /// <summary><c>$A0:B3C3</c>, signed sine values indexed by <c>SnesAngle</c>.</summary>
    public const int SignedSineTable = 0xa0b3c3;

    /// <summary>Provides the signed sine table used when checking grapple physics.</summary>
    extension(GrappleBeamDefinitions.Physics)
    {
        /// <inheritdoc cref="GrappleBeamDefinitionsPhysicsConstants.SignedSineTable"/>
        internal static int SignedSineTable => GrappleBeamDefinitionsPhysicsConstants.SignedSineTable;
    }
}
