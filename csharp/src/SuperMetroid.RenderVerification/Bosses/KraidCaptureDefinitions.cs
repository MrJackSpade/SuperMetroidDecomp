namespace SuperMetroid.RenderVerification.Bosses;

/// <summary>Retail door and staged missile identities used by the Kraid capture sequence.</summary>
internal static class KraidCaptureDefinitions
{
    /// <summary>Retail incoming door $83:91B6 used by the existing Kraid rise audit.</summary>
    internal const ushort IncomingDoor = 0x91b6;
    /// <summary>Live missile type word used by the established Kraid collision audit.</summary>
    internal const ushort AuditMissileType = 0x8100;
    /// <summary>Nonzero instruction sentinel; this staged projectile only enters collision, not animation.</summary>
    internal const ushort AuditLiveInstruction = 0x9000;
}
