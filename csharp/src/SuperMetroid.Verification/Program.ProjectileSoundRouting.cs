using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyProjectileSoundRoutingDefinitions(
        SuperMetroidAddressSpace retail)
    {
        int observations = 0;
        foreach (bool charged in new[] { false, true })
        {
            int table = charged
                ? SamusProjectileRomData.Beams.ChargedSounds
                : SamusProjectileRomData.Beams.UnchargedSounds;
            for (int selector = 0;
                 selector < Enum.GetValues<SamusBeamCombination>().Length;
                 selector++)
            {
                ushort expected = ReadProjectileWord(retail, table + selector * 2);
                AssertEqual(
                    expected,
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

    private static MutableCartridgeReadGuard NewProjectileSoundRoutingForbiddenBus(ISnesAddressSpace source)
    {
        var guard = new MutableCartridgeReadGuard(source);
        guard.Forbid(address => address is >= 0x90c28f and <= 0x90c2c6,
            address => $"Production reread compiled projectile sound-routing byte ${address:X6}.");
        return guard;
    }
}
