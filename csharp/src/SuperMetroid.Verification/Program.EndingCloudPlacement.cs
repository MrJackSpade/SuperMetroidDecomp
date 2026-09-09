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
        int checkedFrames = 0;
        for (int frame = 0; frame < 200; frame++)
        {
            ending.Step();
            if (ending.Phase is not (EndingCreditsPhase.FadeInEscapeSceneB or
                EndingCreditsPhase.EscapeSceneB or EndingCreditsPhase.FadeOutEscapeSceneB))
                break;
            for (int index = 0; index < actors.Length; index++)
            {
                var actor = actors[index];
                // F455/F478 select their moving callbacks on the first call. The
                // next call starts +/-1 Y per frame, even after zoom exceeds 176.
                // Do not reuse EndingCloudMotion as the expected-motion oracle.
                if (frame != 0)
                    actor.Sprite.YPosition = unchecked((ushort)(actor.Sprite.YPosition + (index < 2 ? 1 : -1)));
                actor.Sprite.Step(bus);
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
            if (frame == 19 || !pixels.AsSpan().SequenceEqual(expected))
            {
                Directory.CreateDirectory("csharp/test-temp/ending-505");
                PngWriter.WriteRgba("csharp/test-temp/ending-505/actual.png", 256, 224, pixels);
                PngWriter.WriteRgba("csharp/test-temp/ending-505/native-placement.png", 256, 224, expected);
            }
            AssertTrue(pixels.AsSpan().SequenceEqual(expected), $"scene B cloud positions/motion frame {frame} ({ending.Phase})");
            checkedFrames++;
        }
        AssertEqual(EndingCreditsPhase.FadeInZebesExplosion, ending.Phase, "complete scene B checked through fade-out");
        Console.WriteLine($"Ending scene B: {checkedFrames} full frames match independently stepped native cloud origins/motion; graphics source is shared, not a full cartridge pixel oracle.");
    }
}
