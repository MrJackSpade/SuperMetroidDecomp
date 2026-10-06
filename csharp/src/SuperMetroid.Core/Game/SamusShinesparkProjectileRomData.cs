using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

/// <summary>Native identities for the ordinary projectile slots used by crash echoes.</summary>
public static class SamusShinesparkProjectileRomData
{
    /// <summary>$90:D40D writes type $8029 before InitializeShinesparkEchoOrSpazerSba.</summary>
    public const ushort EchoType = 0x8029;

    /// <summary>ProjPreInstr_SpeedEcho at $90:D4D2, the departing radial echo handler.</summary>
    public const ushort EchoPreInstruction = 0xd4d2;
    /// <summary>$90:D4C6..D4D1: horizontal, vertical and diagonal crash poses select an axis and its opposite.</summary>
    internal static (SnesAngle First, SnesAngle Second) DepartureAngles(byte pose)
    {
        SnesAngle first = pose switch
        {
            SamusPoseIds.ShinesparkHorizontalRightPose or SamusPoseIds.ShinesparkHorizontalLeftPose => SnesAngle.Zero,
            SamusPoseIds.ShinesparkVerticalRightPose or SamusPoseIds.ShinesparkVerticalLeftPose => SnesAngle.QuarterTurn,
            SamusPoseIds.ShinesparkDiagonalRightPose => SnesAngle.NormalizeRaw(-SnesAngle.QuarterTurn.RawValue / 2),
            SamusPoseIds.ShinesparkDiagonalLeftPose => SnesAngle.NormalizeRaw(SnesAngle.QuarterTurn.RawValue / 2),
            _ => throw new InvalidOperationException($"Shinespark crash finish requires pose $C9-$CE, not ${pose:X2}."),
        };
        return (first, first.AddRaw(SnesAngle.HalfTurn.RawValue));
    }
}
