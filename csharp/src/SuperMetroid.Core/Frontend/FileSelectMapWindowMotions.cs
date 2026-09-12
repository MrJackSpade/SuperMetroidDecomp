using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Frontend;

/// <summary>Application-owned signed 16.16 window motion, in left/right/top/bottom order.</summary>
internal readonly record struct FileSelectMapWindowMotion(ushort Timer, uint Left, uint Right, uint Top, uint Bottom);

/// <summary>Retail NTSC map-transition definitions, checked against the pinned cartridge in verification.</summary>
internal static class FileSelectMapWindowMotions
{
    /// <summary>$81:AA34 and $81:AA94: Crateria window velocities and pre-underflow timer.</summary>
    private static readonly FileSelectMapWindowMotion Crateria = new(51, 0xfffe3c00, 0x00033400, 0xffff0800, 0x00040000);
    /// <summary>$81:AA44 and $81:AA96: Brinstar window velocities and pre-underflow timer.</summary>
    private static readonly FileSelectMapWindowMotion Brinstar = new(53, 0xffff3800, 0x00040000, 0xfffda400, 0x00026800);
    /// <summary>$81:AA54 and $81:AA98: Norfair window velocities and pre-underflow timer.</summary>
    private static readonly FileSelectMapWindowMotion Norfair = new(45, 0xfffdf000, 0x00039400, 0xfffc0000, 0x0001a800);
    /// <summary>$81:AA64 and $81:AA9A: Wrecked Ship window velocities and pre-underflow timer.</summary>
    private static readonly FileSelectMapWindowMotion WreckedShip = new(51, 0xfffc0000, 0x0000f800, 0xfffe7400, 0x00036800);
    /// <summary>$81:AA74 and $81:AA9C: Maridia window velocities and pre-underflow timer.</summary>
    private static readonly FileSelectMapWindowMotion Maridia = new(51, 0xfffc0000, 0x0000f800, 0xfffcec00, 0x0001e000);
    /// <summary>$81:AA84 and $81:AA9E: Tourian window velocities and pre-underflow timer.</summary>
    private static readonly FileSelectMapWindowMotion Tourian = new(34, 0xfffc2000, 0x00037800, 0xfffc0000, 0x00035c00);

    public static FileSelectMapWindowMotion Get(int area) => area switch
    {
        (int)AreaId.Crateria => Crateria,
        (int)AreaId.Brinstar => Brinstar,
        (int)AreaId.Norfair => Norfair,
        (int)AreaId.WreckedShip => WreckedShip,
        (int)AreaId.Maridia => Maridia,
        (int)AreaId.Tourian => Tourian,
        _ => throw new ArgumentOutOfRangeException(nameof(area), "Only the six Zebes areas have world-map transitions.")
    };
}
