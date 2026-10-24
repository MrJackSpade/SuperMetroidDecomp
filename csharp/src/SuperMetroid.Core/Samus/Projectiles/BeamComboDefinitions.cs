using SuperMetroid.Core.Progression;
namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Native identities and definition tables for bank-$90 special beam attacks.</summary>
public static class BeamComboDefinitions
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
    /// <summary>FireSBA's charged, non-interacting projectile type tag.</summary>
    public const ushort ProjectileTag = SamusProjectileTypeWord.LiveMarker | SamusProjectileTypeWord.ChargedBeamMarker;
    /// <summary>FireSpazerSBA's auxiliary trail projectile type.</summary>
    public const ushort SpazerTrailType = SamusProjectileTypeWord.LiveMarker | 0x24;
}
