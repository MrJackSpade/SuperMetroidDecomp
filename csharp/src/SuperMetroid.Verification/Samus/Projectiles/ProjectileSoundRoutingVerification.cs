using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Samus.Projectiles.SamusProjectileRomDataVerification;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.Core.Progression;
using SuperMetroid.AssetExtraction.Audio;

namespace SuperMetroid.Verification.Samus.Projectiles;

/// <summary>Checks compiled charged and uncharged beam sound selectors against the retail table.</summary>
internal static class ProjectileSoundRoutingVerification
{
    /// <summary>Compares all four-bit beam combinations and rejects selectors outside the table.</summary>
    internal static void VerifyProjectileSoundRoutingDefinitions(
        SuperMetroidAddressSpace retail)
    {
        int observations = 0;
        foreach (bool charged in new[] { false, true })
        {
            int table = charged
                ? SamusProjectileBeamTables.ChargedSounds
                : SamusProjectileBeamTables.UnchargedSounds;
            for (int selector = 0;
                 selector < Enum.GetValues<SamusBeamCombination>().Length;
                 selector++)
            {
                ushort expected = ReadProjectileWord(retail, table + selector * 2);
                AssertEqual(
                    SoundEffectCommands.DecodeOptional(SoundEffectLibrary.Library1, expected),
                    SamusProjectileSoundRoutingDefinitions.Resolve(charged, SamusBeamCombinations.FromTableIndex(selector)),
                    $"{(charged ? "charged" : "uncharged")} beam sound selector {selector:X}");
                observations++;
            }
        }

        AssertEqual(32, observations,
            "projectile sound routing authored and adjacent observations");
        foreach (int invalid in new[] { -1, 16, 17, int.MaxValue })
        {
            AssertThrows<ArgumentOutOfRangeException>(
                () => SamusProjectileSoundRoutingDefinitions.Resolve(false, SamusBeamCombinations.FromTableIndex(invalid)),
                $"projectile sound selector {invalid} fails outside four-bit domain");
        }

        Console.WriteLine(
            "  Projectile sound routing: 24 authored and 8 bounded adjacent-table observations agree.");
    }

    /// <summary>Guards the compiled bank-$90 sound table against accidental production rereads.</summary>
    internal static MutableCartridgeReadGuard NewProjectileSoundRoutingForbiddenBus(ISnesAddressSpace source)
    {
        var guard = new MutableCartridgeReadGuard(source);
        guard.Forbid(address => address is >= 0x90c28f and <= 0x90c2c6,
            address => $"Production reread compiled projectile sound-routing byte ${address:X6}.");
        return guard;
    }
}
