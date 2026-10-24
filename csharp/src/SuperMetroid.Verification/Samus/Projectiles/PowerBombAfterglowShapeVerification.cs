using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Samus.Projectiles;
using SuperMetroid.Verification.Harness;

/// <summary>Checks that Power Bomb afterglow retains the final oval window through its fade.</summary>
internal static class PowerBombAfterglowShapeVerification
{
    /// <summary>Compares each afterglow scanline's captured window with the native shape profile.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for power bomb afterglow shape")]
    internal static void VerifyPowerBombAfterglowShape()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom("Super Metroid.smc");
        var colors = PowerBombFixedColorCatalog.Load(new MemoryStream(PowerBombFixedColorExtractor.Extract(rom)));
        // Near the bottom-left, the last oval still leaves visible corners outside it.
        var explosion = new SamusPowerBombExplosionState { PresentationColors = colors };
        explosion.Arm();
        explosion.Spawn(new(32, 208));
        for (int frame = 0; frame < 200 && explosion.Phase != PowerBombExplosionPhase.Afterglow; frame++)
            explosion.StepFrame();
        AssertEqual(PowerBombExplosionPhase.Afterglow, explosion.Phase, "reported blast reaches the fade transition");
        AssertEqual(PowerBombExplosionPhase.ExplosionWhite, explosion.RenderedPhase, "transition retains the final expanding oval");
        PowerBombShapeCursor finalShape = explosion.RenderedShape
            ?? throw new InvalidOperationException("The final white frame consumed no shape.");
        AssertEqual(new PowerBombShapeCursor(PowerBombShapeStream.White, 16), finalShape, "native final white profile");
        int finalShapeAddress = PowerBombShapeReferenceData.PreScaledWhite + finalShape.Index * 192;
        AssertEqual(0x889e46, finalShapeAddress, "the final white profile's bank-$88 table");
        var last = GameplayEffectCapture.CapturePowerBombColorMath(PowerBombRenderSnapshot.Capture(explosion), new Position16<RoomPixels>(0, 0));
        int checkedFrames = 0;
        while (explosion.IsActive)
        {
            var actual = GameplayEffectCapture.CapturePowerBombColorMath(PowerBombRenderSnapshot.Capture(explosion), new Position16<RoomPixels>(0, 0));
            for (int y = 32; y < 224; y++)
            {
                int row = Math.Abs(y - 208);
                int halfWidth = row < 192 ? rom.ReadCartridgeByte(finalShapeAddress + row) : 0;
                var expected = halfWidth == 0 ? ColorAddWindow.Empty : new ColorAddWindow(
                    (byte)Math.Max(0, 32 - halfWidth), (byte)Math.Min(255, 32 + halfWidth), 0, 0, 0);
                AssertEqual(expected.Left, (actual ?? throw new InvalidOperationException("Verification requires actual.")).Windows[y].Left, "afterglow retains native oval left edge");
                AssertEqual(expected.Right, actual.Windows[y].Right, "afterglow retains native oval right edge");
                AssertEqual((last ?? throw new InvalidOperationException("Verification requires last.")).Windows[y].Left, actual.Windows[y].Left, "no rectangular transition at fade entry");
                AssertEqual(last.Windows[y].Right, actual.Windows[y].Right, "fade never widens the final oval");
            }
            checkedFrames++;
            AssertTrue(checkedFrames < 200, "afterglow cleanup is bounded");
            explosion.StepFrame();
        }
        AssertTrue(checkedFrames > 1, "confirms both final expansion and fading frames");
        // The final white frame primes 32 afterglow steps; every afterglow call consumes one,
        // and the call that reaches zero ends the explosion instead of fading once more.
        AssertEqual(32, checkedFrames, "afterglow consumes exactly its 32 primed steps before cleanup");
        Console.WriteLine($"Power Bomb afterglow: {checkedFrames} frames retain the native oval in their captured windows.");
    }

    /// <summary>Runs the focused final-oval and afterglow transition check.</summary>
    [VerificationCommand("--power-bomb-afterglow-shape")]
    internal static int PowerBombAfterglowShapeCommand()
    {
        Suite(nameof(VerifyPowerBombAfterglowShape), () => VerifyPowerBombAfterglowShape());
        return 0;
    }
}
