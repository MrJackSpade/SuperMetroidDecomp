namespace SuperMetroid.Core.Game;

/// <summary>Native identities for the ordinary projectile slots used by crash echoes.</summary>
public static class SamusShinesparkProjectileRomData
{
    /// <summary>$90:D40D writes type $8029 before InitializeShinesparkEchoOrSpazerSba.</summary>
    public const ushort EchoType = 0x8029;

    /// <summary>ProjPreInstr_SpeedEcho at $90:D4D2, the departing radial echo handler.</summary>
    public const ushort EchoPreInstruction = 0xd4d2;
}
