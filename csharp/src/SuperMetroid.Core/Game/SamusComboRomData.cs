namespace SuperMetroid.Core.Game;

/// <summary>Native identities and definition tables for bank-$90 special beam attacks.</summary>
public static class SamusComboRomData
{
    /// <summary>ProjPreInstr_PlasmaSba expansion/contraction step in pixels.</summary>
    public const int PlasmaRadiusStep = 4;
    /// <summary>ProjPreInstr_PlasmaSbaFunc_0 ($90:D7E1) switches phase at this radius.</summary>
    public const int PlasmaOuterRadius = 192;
    /// <summary>ProjPreInstr_PlasmaSbaFunc_1 ($90:D7FA) switches phase below this radius.</summary>
    public const int PlasmaInnerThreshold = 45;
    /// <summary>ProjPreInstr_WaveSba ($90:DA08) oscillator cap, in native signed 8.8 units.</summary>
    public const int WaveSpeedLimit = 2048;
    /// <summary>ProjPreInstr_WaveSba ($90:DA08) per-frame acceleration, in signed 8.8 units.</summary>
    public const int WaveAcceleration = 64;
    /// <summary>$90:DA17 library-one Wave particle removal request.</summary>
    public const ushort WaveRemovalSound = 0x29;
    /// <summary>ProjPreInstr_WaveSba ($90:DA08) library-one oscillator pulse request.</summary>
    public const ushort WavePulseSound = 0x28;
    /// <summary>CostOfSBAsInPowerBombs at $90:CC21, twelve word costs by beam index.</summary>
    public const int Costs = 0x90cc21;
    /// <summary>Special projectile data pointer table at $93:8413.</summary>
    public const int DataPointers = 0x938413;
    /// <summary>Shinespark echo / Spazer trail data pointer table at $93:8403.</summary>
    public const int EchoDataPointers = 0x938403;
    /// <summary>FireSBA's charged, non-interacting projectile type tag.</summary>
    public const ushort ProjectileTag = 0x8010;
    /// <summary>FireSpazerSBA's auxiliary trail projectile type.</summary>
    public const ushort SpazerTrailType = 0x8024;
}
