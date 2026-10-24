
namespace SuperMetroid.Core.Samus.Grapple;

/// <summary>Cartridge values of <see cref="GrappleBeamDefinitions"/> that only verification reads.</summary>
internal static class GrappleBeamDefinitionsConnectionsConstants
{
    /// <summary>Number of shot directions represented by each four-byte connection table.</summary>
    public const int DirectionCount = GrappleBeamDefinitions.Firing.DirectionCount;
    /// <summary>Bytes in one special-angle record.</summary>
    public const int SpecialAngleRecordByteCount = 10;
    /// <summary>Number of fixed-angle special connection records.</summary>
    public const int SpecialAngleRecordCount = 8;
    /// <summary>$9B:C3C6: the default pose/angle connection records.</summary>
    public const int DefaultTable = 0x9bc3c6;
    /// <summary>$9B:C3EE: the connection records used while moving vertically.</summary>
    public const int MovingVerticallyTable = 0x9bc3ee;
    /// <summary>$9B:C416: the connection records used while crouching.</summary>
    public const int CrouchingTable = 0x9bc416;
    /// <summary>Special-angle records used for fixed, swing, and wall-grab reactions.</summary>
    public const int SpecialAngleTable = 0x9bc43e;

}
