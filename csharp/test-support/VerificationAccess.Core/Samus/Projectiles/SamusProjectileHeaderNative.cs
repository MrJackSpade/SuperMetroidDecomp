using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.Core.Progression;

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
        SamusProjectileHeader.PlasmaSBA => 0x8685,
        SamusProjectileHeader.WaveSBA => 0x8689,
        SamusProjectileHeader.SpazerSBA => 0x868d,
        SamusProjectileHeader.SpazerSBATrail => 0x86ab,
        SamusProjectileHeader.ShinesparkEcho => 0x86c1,
        _ => throw new ArgumentOutOfRangeException(nameof(header), header, "Undefined projectile header."),
    };

    /// <summary>The header stored at bank-$93 <paramref name="address"/>, or null when none starts there.</summary>
    internal static SamusProjectileHeader? At(int address) =>
        Enum.GetValues<SamusProjectileHeader>().Where(header => (0x930000 | Address(header)) == address)
            .Select(header => (SamusProjectileHeader?)header).SingleOrDefault();

    /// <summary>$93:83C1: the uncharged beam selectors, by four-bit combination.</summary>
    private const int UnchargedBeamSelectors = 0x9383c1;
    /// <summary>$93:83D9: the charged beam selectors, by four-bit combination.</summary>
    private const int ChargedBeamSelectors = 0x9383d9;
    /// <summary>$93:83F1: the missile and bomb selectors, by projectile family row.</summary>
    private const int NonBeamSelectors = 0x9383f1;
    /// <summary>$93:8403: the echo and trail selectors, by the echo's combination less two.</summary>
    private const int EchoSelectors = 0x938403;
    /// <summary>The combinations before the first echo selector's: an echo's entry is its combination's index less two.</summary>
    private const int EchoSelectorLeadingCombinations = 2;
    /// <summary>$93:8413: the special-attack selectors, by four-bit combination.</summary>
    private const int SpecialAttackSelectors = 0x938413;
    /// <summary>$93:842F: the Super Missile link's selector, the third of its table.</summary>
    private const int SuperMissileLinkSelector = 0x93842f;

    /// <summary>
    /// Every selection the port makes and the cartridge selector word that makes it: each combination's uncharged and
    /// charged shot (a Spazer and Plasma combination's read past its table, into the next), each missile and bomb family's
    /// row, each echo's entry, each retail special attack with a particle header, and the Super Missile link.
    /// </summary>
    internal static IEnumerable<ProjectileSelectorWord> Selectors()
    {
        foreach (SamusBeamCombination beams in Enum.GetValues<SamusBeamCombination>())
        {
            int row = sizeof(ushort) * beams.TableIndex;
            yield return new(UnchargedBeamSelectors + row, SamusProjectileSelection.BeamShot(beams, charged: false));
            yield return new(ChargedBeamSelectors + row, SamusProjectileSelection.BeamShot(beams, charged: true));
            if (beams.IsRetail && SamusProjectileSelection.SpecialAttack(beams) is { } special)
                yield return new(SpecialAttackSelectors + row, special);
        }
        foreach (SamusProjectileFamily family in (SamusProjectileFamily[])[
            SamusProjectileFamily.Missile, SamusProjectileFamily.SuperMissile, SamusProjectileFamily.PowerBomb, SamusProjectileFamily.Bomb])
            yield return new(NonBeamSelectors + sizeof(ushort) * NonBeamSelectorRow(family), SamusProjectileSelection.NonBeam(family));
        foreach (SamusBeamEcho echo in Enum.GetValues<SamusBeamEcho>())
            yield return new(EchoSelectors + sizeof(ushort) * (SamusProjectileType.EchoOf(echo).BeamCombination.TableIndex - EchoSelectorLeadingCombinations),
                SamusProjectileSelection.Echo(echo));
        yield return new(SuperMissileLinkSelector, SamusProjectileHeader.SuperMissileLink);
    }

    /// <summary>The row of <paramref name="family"/>'s selector in the missile and bomb selector table.</summary>
    private static int NonBeamSelectorRow(SamusProjectileFamily family) => family switch
    {
        SamusProjectileFamily.Missile => 1,
        SamusProjectileFamily.SuperMissile => 2,
        SamusProjectileFamily.PowerBomb => 3,
        SamusProjectileFamily.Bomb => 5,
        SamusProjectileFamily.Beam or SamusProjectileFamily.BeamExplosion or SamusProjectileFamily.MissileExplosion =>
            throw new ArgumentOutOfRangeException(nameof(family), family, "The family has no missile or bomb selector."),
        _ => throw new ArgumentOutOfRangeException(nameof(family), family, "Undefined projectile family."),
    };

    /// <summary>Header damage by bank-$93 address.</summary>
    extension(SamusProjectileDamage)
    {
        /// <summary>$93:8431 ProjectileDataTable_Uncharged_Power, the first beam header.</summary>
        internal static int BeamHeaderStart => 0x938431;
        /// <summary>Each beam header occupies one damage word and ten direction-pointer words.</summary>
        internal static int BeamHeaderStride => 22;

        /// <summary>The damage of the header at bank-$93 <paramref name="address"/>.</summary>
        internal static ushort Read(int address) => At(address) is { } header ? checked((ushort)SamusProjectileDamage.Read(header))
            : throw new InvalidDataException($"Projectile damage header ${address:X6} is outside the compiled cartridge definitions.");
    }

    /// <summary>Header programs by bank-$93 address.</summary>
    extension(SamusProjectileHeaders)
    {
        /// <summary>The program the header at bank-$93 <paramref name="header"/>'s list field <paramref name="field"/> selects.</summary>
        internal static SamusProjectileSegment ProgramAtField(ushort header, int field) =>
            SamusProjectileHeaders.ProgramAtField(At(0x930000 | header)
                ?? throw new InvalidDataException($"$93:{header:X4} is not a Samus projectile header."), field);

        /// <summary>
        /// The program <paramref name="header"/>'s list field <paramref name="field"/> selects: a directional header holds
        /// one field per direction, at its aim index; a header that starts one program whatever its direction holds only
        /// the first.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">The header keeps no such field.</exception>
        internal static SamusProjectileSegment ProgramAtField(SamusProjectileHeader header, int field) =>
            SamusProjectileHeaderDefinitions.DirectionalAnimations.ContainsKey(header)
                ? SamusProjectileHeaders.Animation(header, SamusProjectileDirectionCodec.FromAimIndex(field))
                : field == 0 ? SamusProjectileHeaders.SoleAnimation(header)
                : throw new ArgumentOutOfRangeException(nameof(field), field, $"{header} keeps only its first list field.");
    }
}
