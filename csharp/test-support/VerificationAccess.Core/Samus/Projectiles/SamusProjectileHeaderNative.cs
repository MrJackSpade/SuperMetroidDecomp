namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Verification's native view of Samus projectile headers and selectors: the bank-$93 address of each.</summary>
internal static class SamusProjectileHeaderNative
{
    /// <summary>The bank-$93 address of <paramref name="header"/>.</summary>
    internal static ushort Address(SamusProjectileHeader header) => header switch
    {
        SamusProjectileHeader.UnchargedPower => 0x8431,
        SamusProjectileHeader.UnchargedSpazer => 0x8447,
        SamusProjectileHeader.UnchargedSpazerIce => 0x845d,
        SamusProjectileHeader.UnchargedSpazerIceWave => 0x8473,
        SamusProjectileHeader.UnchargedPlasmaIceWave => 0x8489,
        SamusProjectileHeader.UnchargedIce => 0x849f,
        SamusProjectileHeader.UnchargedWave => 0x84b5,
        SamusProjectileHeader.UnchargedPlasma => 0x84cb,
        SamusProjectileHeader.UnchargedIceWave => 0x84e1,
        SamusProjectileHeader.UnchargedSpazerWave => 0x84f7,
        SamusProjectileHeader.UnchargedPlasmaWave => 0x850d,
        SamusProjectileHeader.UnchargedPlasmaIce => 0x8523,
        SamusProjectileHeader.ChargedPower => 0x8539,
        SamusProjectileHeader.ChargedSpazer => 0x854f,
        SamusProjectileHeader.ChargedSpazerIce => 0x8565,
        SamusProjectileHeader.ChargedSpazerIceWave => 0x857b,
        SamusProjectileHeader.ChargedPlasmaIceWave => 0x8591,
        SamusProjectileHeader.ChargedIce => 0x85a7,
        SamusProjectileHeader.ChargedPlasma => 0x85bd,
        SamusProjectileHeader.ChargedWave => 0x85d3,
        SamusProjectileHeader.ChargedIceWave => 0x85e9,
        SamusProjectileHeader.ChargedSpazerWave => 0x85ff,
        SamusProjectileHeader.ChargedPlasmaIce => 0x8615,
        SamusProjectileHeader.ChargedPlasmaWave => 0x862b,
        SamusProjectileHeader.Missile => 0x8641,
        SamusProjectileHeader.SuperMissile => 0x8657,
        SamusProjectileHeader.SuperMissileLink => 0x866d,
        SamusProjectileHeader.PowerBomb => 0x8671,
        SamusProjectileHeader.Bomb => 0x8675,
        SamusProjectileHeader.BeamExplosion => 0x8679,
        SamusProjectileHeader.MissileExplosion => 0x867d,
        SamusProjectileHeader.BombExplosion => 0x8681,
        SamusProjectileHeader.PlasmaSBA => 0x8685,
        SamusProjectileHeader.WaveSBA => 0x8689,
        SamusProjectileHeader.SpazerSBA => 0x868d,
        SamusProjectileHeader.SuperMissileExplosion => 0x8691,
        SamusProjectileHeader.Projectile25 => 0x8695,
        SamusProjectileHeader.SpazerSBATrail => 0x86ab,
        SamusProjectileHeader.ShinesparkEcho => 0x86c1,
        SamusProjectileHeader.Projectile27 => 0x86d7,
        _ => throw new ArgumentOutOfRangeException(nameof(header), header, "Undefined projectile header."),
    };

    /// <summary>The header stored at bank-$93 <paramref name="address"/>, or null when none starts there.</summary>
    internal static SamusProjectileHeader? At(int address) =>
        Enum.GetValues<SamusProjectileHeader>().Where(header => (0x930000 | Address(header)) == address)
            .Select(header => (SamusProjectileHeader?)header).SingleOrDefault();

    /// <summary>The selector tables by bank-$93 address.</summary>
    extension(SamusProjectileSelectionDefinitions)
    {
        /// <summary>
        /// The word at bank-$93 <paramref name="address"/>: a selector entry's header address (zero for none) or a header's
        /// leading damage word; anything else fails.
        /// </summary>
        internal static ushort ReadWord(int address)
        {
            if (address is >= 0x9383c1 and < 0x938431 && (address & 1) == 1)
                return SamusProjectileSelectionDefinitions.Entry(SamusProjectileSelectorTable.UnchargedBeam, (address - 0x9383c1) / 2)
                    is { } header ? Address(header) : (ushort)0;
            return At(address) is { } damaged ? SamusProjectileDamageDefinitions.Read(damaged)
                : throw new InvalidDataException($"Projectile selector word ${address:X6} is outside the compiled cartridge definitions.");
        }
    }

    /// <summary>Header damage by bank-$93 address.</summary>
    extension(SamusProjectileDamageDefinitions)
    {
        /// <summary>$93:8431 ProjectileDataTable_Uncharged_Power, the first beam header.</summary>
        internal static int BeamHeaderStart => 0x938431;
        /// <summary>Each beam header occupies one damage word and ten direction-pointer words.</summary>
        internal static int BeamHeaderStride => 22;

        /// <summary>The damage of the header at bank-$93 <paramref name="address"/>.</summary>
        internal static ushort Read(int address) => At(address) is { } header ? SamusProjectileDamageDefinitions.Read(header)
            : throw new InvalidDataException($"Projectile damage header ${address:X6} is outside the compiled cartridge definitions.");
    }

    /// <summary>Header programs by bank-$93 address.</summary>
    extension(SamusProjectileHeaderDefinitions)
    {
        /// <summary>The program the header at bank-$93 <paramref name="header"/>'s list field <paramref name="direction"/> selects.</summary>
        internal static SamusProjectileProgram? Program(ushort header, int direction) =>
            SamusProjectileHeaderDefinitions.Program(At(0x930000 | header)
                ?? throw new InvalidDataException($"$93:{header:X4} is not a Samus projectile header."), direction);
    }

    /// <summary>The beam selector tables by bank-$93 address.</summary>
    extension(SamusProjectileRomData.Beams)
    {
        /// <summary>$93:83C1: bank-$93 projectile-data pointers for uncharged combinations.</summary>
        internal static int UnchargedDataPointers => 0x9383c1;
        /// <summary>$93:83D9: bank-$93 projectile-data pointers for charged combinations.</summary>
        internal static int ChargedDataPointers => 0x9383d9;
    }

    /// <summary>The non-beam selector tables by bank-$93 address.</summary>
    extension(SamusProjectileRomData.NonBeam)
    {
        /// <summary>$93:83F1: bank-$93 data pointers for missiles, bombs, and special projectiles.</summary>
        internal static int DataPointers => 0x9383f1;
        /// <summary>$93:842B: bank-$93 definitions for Super Missile link projectiles.</summary>
        internal static int SuperMissileLinkDataPointers => 0x93842b;
    }
}
