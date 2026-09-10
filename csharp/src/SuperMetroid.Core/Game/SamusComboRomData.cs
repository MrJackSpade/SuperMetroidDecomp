namespace SuperMetroid.Core.Game;

/// <summary>Native identities and definition tables for bank-$90 special beam attacks.</summary>
public static class SamusComboRomData
{
    /// <summary>CostOfSBAsInPowerBombs at $90:CC21, twelve word costs by beam index.</summary>
    public const int Costs = 0x90cc21;
    /// <summary>Special projectile data pointer table at $93:8413.</summary>
    public const int DataPointers = 0x938413;
    /// <summary>Shinespark echo / Spazer trail data pointer table at $93:8403.</summary>
    public const int EchoDataPointers = 0x938403;
    /// <summary>IcePlasmaSBAProjectileOriginAngles at $90:CD08.</summary>
    public const int OriginAngles = 0x90cd08;
    /// <summary>FireSBA's charged, non-interacting projectile type tag.</summary>
    public const ushort ProjectileTag = 0x8010;
    /// <summary>FireSpazerSBA's auxiliary trail projectile type.</summary>
    public const ushort SpazerTrailType = 0x8024;
    /// <summary>ProjPreInstr_IceSba at $90:CF09.</summary>
    public const ushort Ice = 0xcf09;
    /// <summary>ProjPreInstr_IceSba2 at $90:CF7A.</summary>
    public const ushort IceOutward = 0xcf7a;
    /// <summary>ProjPreInstr_WaveSba at $90:DA08.</summary>
    public const ushort Wave = 0xda08;
    /// <summary>ProjPreInstr_SpazerSba at $90:DB06.</summary>
    public const ushort Spazer = 0xdb06;
    /// <summary>ProjPreInstr_PlasmaSba at $90:D793.</summary>
    public const ushort Plasma = 0xd793;
}
