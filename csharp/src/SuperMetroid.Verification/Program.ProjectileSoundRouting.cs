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
                 selector < SamusProjectileSoundRoutingDefinitions.SelectorCount;
                 selector++)
            {
                ushort expected = ReadProjectileWord(retail, table + selector * 2);
                AssertEqual(
                    expected,
                    SamusProjectileSoundRoutingDefinitions.Resolve(charged, selector),
                    $"{(charged ? "charged" : "uncharged")} beam sound selector {selector:X}");
                observations++;
            }
        }

        AssertEqual(32, observations,
            "projectile sound routing authored and adjacent observations");
        foreach (int invalid in new[] { -1, 16, 17, int.MaxValue })
        {
            AssertThrows<ArgumentOutOfRangeException>(
                () => SamusProjectileSoundRoutingDefinitions.Resolve(false, invalid),
                $"projectile sound selector {invalid} fails outside four-bit domain");
        }

        Console.WriteLine(
            "  Projectile sound routing: 24 authored and 8 bounded adjacent-table observations agree.");
    }

    private sealed class ProjectileSoundRoutingForbiddenBus(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        public byte ReadByte(int address)
        {
            if (address is >= 0x90c28f and <= 0x90c2c6)
            {
                throw new InvalidOperationException(
                    $"Production reread compiled projectile sound-routing byte ${address:X6}.");
            }

            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
