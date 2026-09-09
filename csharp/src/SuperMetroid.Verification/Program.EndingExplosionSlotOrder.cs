using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    private static void VerifyEndingExplosionSlotOrder()
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom("Super Metroid.smc");
        var audio = new CartridgeAudioState();
        var ending = new EndingCreditsState(bus, audio, 0, 0);
        for (int frame = 0; frame < 20000 && ending.Phase != EndingCreditsPhase.PlanetEscapeFast; frame++)
        {
            ending.Step();
            audio.AdvanceFrame(bus, default);
        }
        AssertEqual(EndingCreditsPhase.PlanetEscapeFast, ending.Phase, "native flyaway handoff reached");
        for (int frame = 0; frame < 100; frame++) ending.Step();
        var definition = EndingCreditsRomData.Sprites.ExplosionAfterglow;
        var afterglow = new IntroDiscoverySprite(definition.X, definition.Y,
            definition.Attributes.Raw, definition.InstructionPointer);
        afterglow.Step(bus);
        var expected = new OamBuffer();
        expected.BeginFrame();
        afterglow.Draw(bus, expected);
        expected.FinalizeFrame();
        // F2FA installs the afterglow at byte slot 6 (index 3); the starfields occupy
        // byte slots 2 and 0. The native descending draw must give the complete
        // afterglow first access to OAM, ahead of both 53-entry starfields.
        AssertEqual(37, expected.LastFinalizedSpriteCount, "retail afterglow component count");
        var actual = ending.CaptureRenderSnapshot().Memory;
        AssertTrue(actual.Oam[..(37 * 4)].SequenceEqual(expected.LowTable[..(37 * 4)]),
            "native slot order preserves all 37 afterglow pieces before starfield OAM overflow");
        var snapshot = ending.CaptureRenderSnapshot();
        var background = snapshot.Layers.ToArray().OfType<Mode7RenderLayer>().Single();
        AssertTrue(Enumerable.Range(0, actual.ModeledSpriteCount).All(index =>
            (actual.Oam[index * 4 + 3] & 0x30) == 0), "live flyaway actors use native priority zero");
        var fixedWhite = snapshot.Layers.ToArray().OfType<FixedColorAddRenderLayer>().Single();
        RenderLayer[] order = [fixedWhite, new ObjRenderLayer(FixedColor: fixedWhite), background];
        var nativePixels = SoftwareLayeredSnapshotRenderer.Render(new(actual, order,
            snapshot.ObjectSelection, snapshot.Brightness));
        var actualPixels = ending.Render();
        Directory.CreateDirectory("csharp/test-temp/ending-507");
        PngWriter.WriteRgba("csharp/test-temp/ending-507/actual.png", 256, 224, actualPixels);
        PngWriter.WriteRgba("csharp/test-temp/ending-507/native-priority.png", 256, 224, nativePixels);
        AssertTrue(actualPixels.AsSpan().SequenceEqual(nativePixels),
            "live getaway ship occludes priority-zero afterglow pixels");
    }
}
