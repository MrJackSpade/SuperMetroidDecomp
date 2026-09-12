using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyCompiledSurfaceMotion(SuperMetroidAddressSpace rom)
    {
        ushort Word(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        for (int shape = 0; shape < 32; shape++)
        {
            ushort multiplier = Word(0xa3e931 + shape * 4);
            AssertEqual(multiplier, CrawlerSlopeDefinitions.Multiplier(shape), "Crawler native slope multiplier");
            for (int raw = 0; raw <= ushort.MaxValue; raw++)
            {
                int speed = unchecked((short)raw);
                int magnitude = Math.Abs(speed) * multiplier;
                AssertEqual(speed < 0 ? -magnitude : magnitude,
                    CrawlerSlopeDefinitions.Scale((ushort)raw, shape), "Crawler signed slope displacement");
            }
        }
        for (int speed = 0; speed <= ushort.MaxValue; speed++)
        {
            int address = 0xa3d517 + Math.Min(speed, 15) * 4;
            var actual = YardKickDefinitions.ForHorizontalSpeed((ushort)speed);
            AssertEqual(Word(address), actual.Fraction, "Yard native capped kick fraction");
            AssertEqual(Word(address + 2), actual.Whole, "Yard native capped kick whole");
        }
        AssertThrows<InvalidDataException>(() => CrawlerSlopeDefinitions.Multiplier(32), "Crawler invalid slope");
        Console.WriteLine("Surface motion definitions: 32 native slope multipliers, 2097152 signed displacements and 65536 Yard kick selectors match.");
    }
}
