using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static SuiteTimingVerification;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;

/// <summary>Checks that Power Bomb afterglow retains the final oval window through its fade.</summary>
internal static class PowerBombAfterglowShapeVerification
{
    /// <summary>Compares each afterglow scanline with the native shape profile and verifies pixels outside the oval remain unaffected.</summary>
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
            explosion.StepFrame(rom);
        AssertEqual(PowerBombExplosionPhase.Afterglow, explosion.Phase, "reported blast reaches the fade transition");
        AssertEqual(PowerBombExplosionPhase.ExplosionWhite, explosion.RenderedPhase, "transition retains the final expanding oval");
        ushort finalShape = explosion.RenderedShapeDefinitionPointer;
        AssertEqual((ushort)0x9e46, finalShape, "native final white profile");
        var last = SnesGameplayFrameRenderer.CapturePowerBombColorMath(rom, explosion, new Position16<RoomPixels>(0, 0));
        int checkedFrames = 0;
        while (explosion.IsActive)
        {
            var actual = SnesGameplayFrameRenderer.CapturePowerBombColorMath(rom, explosion, new Position16<RoomPixels>(0, 0));
            var pixels = Enumerable.Repeat(new Rgba32(0, 0, 0), 256 * 224).ToArray();
            SnesGameplayFrameRenderer.ApplyPowerBombColorMath(pixels, rom, explosion, new Position16<RoomPixels>(0, 0));
            for (int y = 32; y < 224; y++)
            {
                int row = Math.Abs(y - 208);
                int halfWidth = row < 192 ? rom.ReadCartridgeByte(0x880000 | (finalShape + row)) : 0;
                var expected = halfWidth == 0 ? ColorAddWindow.Empty : new ColorAddWindow(
                    (byte)Math.Max(0, 32 - halfWidth), (byte)Math.Min(255, 32 + halfWidth), 0, 0, 0);
                AssertEqual(expected.Left, (actual ?? throw new InvalidOperationException("Verification requires actual.")).Windows[y].Left, "afterglow retains native oval left edge");
                AssertEqual(expected.Right, actual.Windows[y].Right, "afterglow retains native oval right edge");
                AssertEqual((last ?? throw new InvalidOperationException("Verification requires last.")).Windows[y].Left, actual.Windows[y].Left, "no rectangular transition at fade entry");
                AssertEqual(last.Windows[y].Right, actual.Windows[y].Right, "fade never widens the final oval");
                for (int x = 0; x < 256; x++)
                    if (x < expected.Left || x > expected.Right)
                        AssertEqual(new Rgba32(0, 0, 0), pixels[y * 256 + x], "outside-oval pixels remain unaffected during fade");
            }
            checkedFrames++;
            AssertTrue(checkedFrames < 200, "afterglow cleanup is bounded");
            explosion.StepFrame(rom);
        }
        AssertTrue(checkedFrames > 1, "confirms both final expansion and fading frames");
        Console.WriteLine($"Power Bomb afterglow: {checkedFrames} frames retain the native oval, including captured windows and rendered corner pixels.");
    }

    /// <summary>Runs the focused final-oval and afterglow transition check.</summary>
    [VerificationCommand("--power-bomb-afterglow-shape")]
    internal static int PowerBombAfterglowShapeCommand()
    {
        Suite(nameof(VerifyPowerBombAfterglowShape), () => VerifyPowerBombAfterglowShape());
        return 0;
    }
}
