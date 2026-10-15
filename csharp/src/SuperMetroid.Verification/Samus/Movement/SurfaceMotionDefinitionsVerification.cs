using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Enemies.Crawlers;
using SuperMetroid.Core.Enemies.Yard;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Samus.Movement;

/// <summary>Uses retail words to verify crawler slope scaling and Yard's capped horizontal kick.</summary>
internal static class SurfaceMotionDefinitionsVerification
{
    /// <summary>Checks 32 slope multipliers, every signed 16-bit speed, and all Yard kick selectors.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for surface motion")]
    internal static void VerifyCompiledSurfaceMotion(SuperMetroidAddressSpace rom)
    {
        for (int shape = 0; shape < 32; shape++)
        {
            ushort multiplier = CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa3e931 + shape * 4));
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
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord(address), actual.Fraction, "Yard native capped kick fraction");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((address + 2)), actual.Whole, "Yard native capped kick whole");
        }
        AssertThrows<InvalidDataException>(() => CrawlerSlopeDefinitions.Multiplier(32), "Crawler invalid slope");
        Console.WriteLine("Surface motion definitions: 32 native slope multipliers, 2097152 signed displacements and 65536 Yard kick selectors match.");
    }
}
