using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    private static void VerifyEndingCloudPlacement()
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom("Super Metroid.smc");
        var audio = new CartridgeAudioState();
        var ending = new EndingCreditsState(bus, audio, 0, 0);
        for (int frame = 0; frame < 20000 && ending.Phase != EndingCreditsPhase.FadeInEscapeSceneB; frame++)
        {
            ending.Step();
            audio.AdvanceFrame(bus, default);
        }
        AssertEqual(EndingCreditsPhase.FadeInEscapeSceneB, ending.Phase, "cloud scene setup reached");
        // $8B:D731 spawns EED3/EED9/EEDF/EEE5 with parameter zero.
        // F0B2/F0E1/F0E9/F0F1 therefore all use X=128 and these four Y origins.
        short[] nativeY = [-96, -32, 288, 352];
        EndingSpriteRole[] roles = [EndingSpriteRole.CloudTopA, EndingSpriteRole.CloudTopB,
            EndingSpriteRole.CloudBottomA, EndingSpriteRole.CloudBottomB];
        EndingSpriteDefinition[] definitions = [EndingCreditsRomData.Sprites.EscapeBCloudTopA,
            EndingCreditsRomData.Sprites.EscapeBCloudTopB, EndingCreditsRomData.Sprites.EscapeBCloudBottomA,
            EndingCreditsRomData.Sprites.EscapeBCloudBottomB];
        var actors = definitions.Select((definition, index) => new EndingSprite(
            new IntroDiscoverySprite(128, unchecked((ushort)nativeY[index]), definition.Attributes.Raw,
                definition.InstructionPointer), roles[index])).ToArray();
        for (int frame = 0; frame < 20; frame++)
        {
            ending.Step();
            foreach (var actor in actors)
            {
                EndingCloudMotion.Step(actor, 64);
                actor.Sprite.Step(bus);
            }
        }
        var oam = new OamBuffer();
        oam.BeginFrame();
        foreach (var actor in actors) actor.Sprite.Draw(bus, oam);
        oam.FinalizeFrame();
        var actual = ending.CaptureRenderSnapshot();
        byte[] oamBytes = [.. oam.LowTable.ToArray(), .. oam.HighTable.ToArray()];
        var expectedMemory = new PpuMemorySnapshot(actual.Memory.Vram, actual.Memory.Cgram,
            oamBytes, oam.LastFinalizedSpriteCount);
        var expected = SoftwareLayeredSnapshotRenderer.Render(new(expectedMemory, actual.Layers,
            actual.ObjectSelection, actual.Brightness));
        var pixels = ending.Render();
        Directory.CreateDirectory("csharp/test-temp/ending-505");
        PngWriter.WriteRgba("csharp/test-temp/ending-505/actual.png", 256, 224, pixels);
        PngWriter.WriteRgba("csharp/test-temp/ending-505/native-placement.png", 256, 224, expected);
        AssertTrue(pixels.AsSpan().SequenceEqual(expected), "scene B cloud edges match native spawn positions");
    }
}
