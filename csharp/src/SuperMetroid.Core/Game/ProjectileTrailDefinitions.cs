using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.Core.Game;

/// <summary>Compiled $90:B5BB/B609 trail-list selection, preserving adjacent-table overreads.</summary>
public static class ProjectileTrailDefinitions
{
    /// <summary>$90:B4C9, InstList_BeamTrail_Empty: immediate trail termination.</summary>
    public const ushort Empty = 0xb4c9;
    /// <summary>$90:B4CB, InstList_LeftBeamTrail_IceBeams_ChargedPowerBeam.</summary>
    public const ushort LeftIce = 0xb4cb;
    /// <summary>$90:B52D, InstList_RightBeamTrail_SomeIceBeams.</summary>
    public const ushort RightIce = 0xb52d;
    /// <summary>$90:B58F, InstList_BeamTrail_WaveBeam, shared by both sides.</summary>
    public const ushort Wave = 0xb58f;
    /// <summary>$90:B5A1, InstList_BeamTrail_SuperMissile, also used by ordinary missiles.</summary>
    public const ushort Missile = 0xb5a1;

    private static ReadOnlySpan<ushort> Left =>
    [
        Empty, Wave, LeftIce, LeftIce, Empty, Empty, LeftIce, LeftIce,
        Empty, Empty, LeftIce, LeftIce, Empty, Empty, Empty, Empty,
        LeftIce, Wave, LeftIce, LeftIce, Empty, Empty, LeftIce, LeftIce,
        Empty, Empty, LeftIce, LeftIce, Empty, Empty, Empty, Empty,
        Missile, Missile, Empty, Empty, LeftIce, LeftIce, LeftIce,
    ];
    private static ReadOnlySpan<ushort> Right =>
    [
        Empty, Empty, Empty, Empty, Empty, Empty, RightIce, RightIce,
        Empty, Empty, Empty, RightIce, Empty, Empty, Empty, Empty,
        Empty, Wave, Empty, RightIce, Empty, Empty, RightIce, RightIce,
        Empty, Empty, Empty, RightIce, Empty, Empty, Empty, Empty,
        Empty, Empty, Empty, Empty, LeftIce, RightIce, LeftIce,
    ];

    public static ushort ReadSelector(ISnesAddressSpace bus, int address)
    {
        int offset = address - SamusProjectileRomData.Trails.LeftInstructionPointers;
        if ((uint)offset < (Left.Length + Right.Length) * sizeof(ushort) && (offset & 1) == 0)
        {
            int index = offset / sizeof(ushort);
            return index < Left.Length ? Left[index] : Right[index - Left.Length];
        }
        // Index overflow belongs to physical memory, not a clamped beam category.
        return RomDataReader.ReadWordFixedBank(bus, address);
    }
}
