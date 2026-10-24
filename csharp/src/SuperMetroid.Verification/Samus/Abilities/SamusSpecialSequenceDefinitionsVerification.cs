using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Samus.Movement.SamusMovementRomDataVerification;
using SuperMetroid.Core.Samus.Abilities;

namespace SuperMetroid.Verification.Samus.Abilities;

/// <summary>Checks extracted Samus death, suit-pickup, and Power Bomb sequence ranges before runtime consumption.</summary>
internal static class SamusSpecialSequenceDefinitionsVerification
{
    /// <summary>
    /// Validates every extracted special-sequence table and the structural boundaries that
    /// make its decoded records safe to consume. Behavioral sequence tests remain the
    /// authority for phase timing, movement, palette side effects, and completion.
    /// </summary>
    internal static void VerifySamusSpecialSequenceDefinitions()
    {
        var initialDeathFrames =
            SamusDeathSequenceDefinitions.InitialFramesByMovementType;
        AssertEqual(28, initialDeathFrames.Length,
            "death initial-frame table covers every retail movement type");

        AssertEqual(
            PowerBombExplosionDefinitions.FirstYellowShape,
            PowerBombExplosionDefinitions.WhiteShapeEnd,
            "Power Bomb white shapes end where yellow shapes begin");
        AssertEqual(0,
            (PowerBombExplosionDefinitions.YellowShapeEnd -
                PowerBombExplosionDefinitions.FirstYellowShape) %
                    PowerBombExplosionDefinitions.ShapeStride,
            "yellow Power Bomb shapes occupy whole records");
        AssertEqual(0,
            (PowerBombExplosionDefinitions.WhiteShapeEnd -
                PowerBombExplosionDefinitions.FirstWhiteShape) %
                    PowerBombExplosionDefinitions.ShapeStride,
            "white Power Bomb shapes occupy whole records");

        string romPath = Path.GetFullPath("Super Metroid.smc");
        if (!File.Exists(romPath))
        {
            Console.WriteLine("  Samus special sequence ROM data: retail range audit skipped (private ROM absent).");
            return;
        }

        SuperMetroidAddressSpace bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(romPath);
        TouchRange(bus, SuitPickupBeamDefinitions.BeamCurve,
            SuitPickupBeamDefinitions.WindowScanlineCount / 2,
            "suit-pickup symmetric beam curve");
        TouchRange(bus,
            0x880000 | PowerBombExplosionDefinitions.FirstWhiteShape,
            PowerBombExplosionDefinitions.WhiteShapeEnd -
                PowerBombExplosionDefinitions.FirstWhiteShape,
            "Power Bomb white shape stream");
        TouchRange(bus,
            0x880000 | PowerBombExplosionDefinitions.FirstYellowShape,
            PowerBombExplosionDefinitions.YellowShapeEnd -
                PowerBombExplosionDefinitions.FirstYellowShape,
            "Power Bomb yellow shape stream");

        Console.WriteLine(
            "  Samus special sequence ROM data: death DMA, suit beam and Power Bomb shape ranges are valid; compiled sine values have a separate exhaustive audit.");
    }
}
