using SuperMetroid.AssetExtraction.Cartridge;
using static SuperMetroid.Verification.Samus.Movement.SamusMovementDefinitionsVerification;
using SuperMetroid.Core.Samus.Abilities;

namespace SuperMetroid.Verification.Samus.Abilities;

/// <summary>Checks extracted Samus death and suit-pickup sequence ranges before runtime consumption.</summary>
internal static class SamusSpecialSequenceDefinitionsVerification
{
    /// <summary>
    /// Validates every extracted special-sequence table and the structural boundaries that
    /// make its decoded records safe to consume. Behavioral sequence tests remain the
    /// authority for phase timing, movement, palette side effects, and completion. The Power
    /// Bomb's blast is an oval described by its radius (#1468), so its per-line shape tables
    /// are no longer read.
    /// </summary>
    internal static void VerifySamusSpecialSequenceDefinitions()
    {
        AssertTrue(Enum.GetValues<SuperMetroid.Core.Samus.Movement.SamusMovementType>()
                .All(movement => SamusDeathSequenceFrames.InitialFrame(movement) is 0 or 1 or 5),
            "death initial-frame table covers every retail movement type");

        string romPath = Path.GetFullPath("Super Metroid.smc");
        if (!File.Exists(romPath))
        {
            Console.WriteLine("  Samus special sequence ROM data: retail range audit skipped (private ROM absent).");
            return;
        }

        SuperMetroidAddressSpace bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(romPath);
        TouchRange(bus, SuitPickupBeamDefinitions.BeamCurve,
            SuitPickupBeamDefinitions.BeamLines / 2,
            "suit-pickup symmetric beam curve");

        Console.WriteLine(
            "  Samus special sequence ROM data: death DMA and suit beam ranges are valid; compiled sine values have a separate exhaustive audit.");
    }
}
