using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Samus.Projectiles;
using SuperMetroid.Verification.Harness;

/// <summary>Checks that Power Bomb afterglow retains the final oval window through its fade.</summary>
internal static class PowerBombAfterglowShapeVerification
{
    /// <summary>Checks the final expanding frame and every afterglow frame keep the final white oval.</summary>
    internal static void VerifyPowerBombAfterglowShape()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom("Super Metroid.smc");
        var colors = PowerBombFixedColorFile.Read(new MemoryStream(PowerBombFixedColorExtractor.Extract(rom)));
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
        // The final white oval: centred on the blast at the bottom-left, the stream's last radius wide and three
        // quarters of it tall, below the HUD (#1468).
        int radius = finalShape.HorizontalRadius();
        AssertEqual(255, radius, "the final white oval reaches the widest radius");
        ScreenWindow finalOval = ScreenWindow.Of(WindowOval.Ellipse(Offset16<ScreenPixels>.Of(32, 208), new Extent16<ScreenPixels>(checked((ushort)radius),
            checked((ushort)(radius * PowerBombExplosionDefinitions.OvalHeightParts / PowerBombExplosionDefinitions.OvalWidthParts)))), DisplayScreen.HudHeight);
        int checkedFrames = 0;
        while (explosion.IsActive)
        {
            var actual = GameplayEffectCapture.CapturePowerBombGlow(PowerBombRenderSnapshot.Capture(explosion), new Position16<RoomPixels>(0, 0))
                ?? throw new InvalidOperationException("An active Power Bomb captures its window.");
            AssertEqual(finalOval, actual.Window, "the afterglow keeps the final white oval: no rectangle at fade entry, never wider");
            checkedFrames++;
            AssertTrue(checkedFrames < 200, "afterglow cleanup is bounded");
            explosion.StepFrame();
        }
        AssertTrue(checkedFrames > 1, "confirms both final expansion and fading frames");
        // The final white frame primes 32 afterglow steps; every afterglow call consumes one,
        // and the call that reaches zero ends the explosion instead of fading once more.
        AssertEqual(32, checkedFrames, "afterglow consumes exactly its 32 primed steps before cleanup");
        Console.WriteLine($"Power Bomb afterglow: {checkedFrames} frames retain the final oval in their captured windows.");
    }

    /// <summary>Runs the focused final-oval and afterglow transition check.</summary>
    [VerificationCommand("--power-bomb-afterglow-shape")]
    internal static int PowerBombAfterglowShapeCommand()
    {
        Suite(nameof(VerifyPowerBombAfterglowShape), () => VerifyPowerBombAfterglowShape());
        return 0;
    }
}
