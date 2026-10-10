using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

/// <summary>Native identities for the ordinary projectile slots used by crash echoes.</summary>
public static class SamusShinesparkProjectileRomData
{
    /// <summary>$90:D40D writes type $8029 before InitializeShinesparkEchoOrSpazerSba.</summary>
    public const ushort EchoType = 0x8029;
    /// <summary>
    /// $90:D4C6..D4D1, indexed by (pose - $C9) * 2: the crash's two echo angle indices. The six
    /// crash poses select an axis and its opposite.
    /// </summary>
    /// <remarks>
    /// The routine indexes with whatever pose is installed when the crash finishes. A drained
    /// controller ($91:E4F8) can replace the crash pose with $E8/$E9 while the crash keeps its
    /// movement handler, and those indices read on into the code of $90:D4D2: the bytes at
    /// $90:D504/$D505 and $90:D506/$D507.
    /// </remarks>
    internal static (SnesAngle First, SnesAngle Second) DepartureAngles(SamusPoseId pose)
    {
        (byte first, byte second) = pose switch
        {
            SamusPoseId.ShinesparkHorizontalRightPose => ((byte)0x00, (byte)0x80),
            SamusPoseId.ShinesparkHorizontalLeftPose => ((byte)0x00, (byte)0x80),
            SamusPoseId.ShinesparkVerticalRightPose => ((byte)0x40, (byte)0xc0),
            SamusPoseId.ShinesparkVerticalLeftPose => ((byte)0x40, (byte)0xc0),
            SamusPoseId.ShinesparkDiagonalRightPose => ((byte)0xe0, (byte)0x60),
            SamusPoseId.ShinesparkDiagonalLeftPose => ((byte)0x20, (byte)0xa0),
            SamusPoseId.DrainedCrouchingRightPose => ((byte)0x16, (byte)0x9d),
            SamusPoseId.DrainedCrouchingLeftPose => ((byte)0xb6, (byte)0x0a),
            _ => throw new InvalidOperationException(
                $"Shinespark crash finish has no modeled echo angles for pose ${(int)pose:X2}."),
        };
        return (SnesAngle.FromTableIndex(first), SnesAngle.FromTableIndex(second));
    }
}
