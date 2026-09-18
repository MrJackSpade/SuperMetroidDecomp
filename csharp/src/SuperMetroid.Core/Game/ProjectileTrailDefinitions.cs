namespace SuperMetroid.Core.Game;

/// <summary>Compiled $90:B5BB/B609 trail-list selection, including every reachable adjacent-code observation.</summary>
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

    /// <summary>
    /// $90:B5BB-$90:B688, every aligned word reachable when the native producer indexes
    /// either selector base with the projectile type's complete low six bits. The first
    /// 39 words are the authored left table, the next 39 are the authored right table,
    /// and the final 25 preserve the right selector's bounded observation of adjacent code.
    /// </summary>
    private static ReadOnlySpan<ushort> ReachableSelectors =>
    [
        Empty, Wave, LeftIce, LeftIce, Empty, Empty, LeftIce, LeftIce,
        Empty, Empty, LeftIce, LeftIce, Empty, Empty, Empty, Empty,
        LeftIce, Wave, LeftIce, LeftIce, Empty, Empty, LeftIce, LeftIce,
        Empty, Empty, LeftIce, LeftIce, Empty, Empty, Empty, Empty,
        Missile, Missile, Empty, Empty, LeftIce, LeftIce, LeftIce,
        Empty, Empty, Empty, Empty, Empty, Empty, RightIce, RightIce,
        Empty, Empty, Empty, RightIce, Empty, Empty, Empty, Empty,
        Empty, Wave, Empty, RightIce, Empty, Empty, RightIce, RightIce,
        Empty, Empty, Empty, RightIce, Empty, Empty, Empty, Empty,
        Empty, Empty, Empty, Empty, LeftIce, RightIce, LeftIce,
        0xbd8b, 0x0c18, 0x0089, 0xd00f, 0x2905, 0x003f, 0x0d80, 0x29eb,
        0x000f, 0x03c9, 0xb000, 0x1817, 0x1f69, 0x0a00, 0xf4aa, 0x7e7e,
        0xabab, 0x22a0, 0xb900, 0xd658, 0x07f0, 0x8888, 0xf710, 0x38ab,
        0xa96b,
    ];

    /// <summary>Returns an aligned selector word from the complete native low-six-bit observation window.</summary>
    public static ushort ReadSelector(int address)
    {
        int offset = address - SamusProjectileRomData.Trails.LeftInstructionPointers;
        if ((uint)offset < ReachableSelectors.Length * sizeof(ushort) && (offset & 1) == 0)
            return ReachableSelectors[offset / sizeof(ushort)];

        throw new InvalidDataException(
            $"Projectile trail selector ${address:X6} is outside the bounded low-six-bit observation window.");
    }
}
