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
        var colors = PowerBombGlowColorFile.Read(new MemoryStream(PowerBombGlowColorExtractor.Extract(rom)));
        // Near the bottom-left, the last oval still leaves visible corners outside it.
        var explosion = new SamusPowerBombExplosionState { Blast = { PresentationColors = colors } };
        explosion.Arm();
        explosion.Spawn(new(32, 208));
        for (int frame = 0; frame < 200 && explosion.Blast.Phase != PowerBombExplosionPhase.Afterglow; frame++)
            explosion.StepFrame();
        AssertEqual(PowerBombExplosionPhase.Afterglow, explosion.Blast.Phase, "reported blast reaches the fade transition");
        AssertEqual(PowerBombExplosionPhase.ExplosionWhite, explosion.Blast.RenderedPhase, "transition retains the final expanding oval");
        int radius = explosion.Blast.RenderedOvalRadius
            ?? throw new InvalidOperationException("The final white frame showed no authored oval.");
        // The final white oval: centred on the blast at the bottom-left, the white explosion's last oval wide and three
        // quarters of it tall, below the HUD (#1468).
        AssertEqual(PowerBombExplosionDefinitions.WhiteOvalRadii[^1], radius, "native final white profile");
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
