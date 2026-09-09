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
        VerifyEndingTakeoffColorMath(checkWrapping: true);
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

    private static void VerifyEndingTakeoffColorMath(bool checkWrapping)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom("Super Metroid.smc");
        var audio = new CartridgeAudioState();
        var ending = new EndingCreditsState(bus, audio, 0, 0);
        for (int frame = 0; frame < 20000 && ending.Phase != EndingCreditsPhase.EscapeSceneA; frame++)
        {
            ending.Step();
            audio.AdvanceFrame(bus, default);
        }
        AssertEqual(EndingCreditsPhase.EscapeSceneA, ending.Phase, "takeoff color-math scene reached");
        for (int frame = 0; frame < 90; frame++) ending.Step();
        var snapshot = ending.CaptureRenderSnapshot();
        var memory = new SoftwarePpuSnapshotMemory(snapshot.Memory);
        var registers = snapshot.Layers.ToArray().OfType<Mode7RenderLayer>().First().Registers;
        var background = SnesMode7Renderer.RenderViewport(memory.Vram, memory.Cgram,
            registers.MatrixA, registers.MatrixB, registers.MatrixC, registers.MatrixD,
            registers.CenterX, registers.CenterY, registers.HorizontalOffset, registers.VerticalOffset,
            wrapOutsideMap: registers.WrapOutsideMap);
        // SetupPpu_5_Mode7 ($8B:8293) writes M7SEL=0, not the $80
        // transparent-overflow mode. Keep this oracle independent of the renderer's
        // overflow policy: the native tile lookup masks BOTH coordinates to ten bits.
        var wrappedBackground = new Rgba32[background.Length];
        int cx = (registers.CenterX << 19) >> 19, cy = (registers.CenterY << 19) >> 19;
        int h = ((registers.HorizontalOffset << 19) >> 19) - cx;
        int v = ((registers.VerticalOffset << 19) >> 19) - cy;
        h = (h & 8192) != 0 ? h | ~1023 : h & 1023;
        v = (v & 8192) != 0 ? v | ~1023 : v & 1023;
        for (int y = 0; y < 224; y++)
        for (int x = 0; x < 256; x++)
        {
            int sx = ((((registers.MatrixA * h) & ~63) + ((registers.MatrixB * (y + 1)) & ~63)
                + ((registers.MatrixB * v) & ~63) + (cx << 8) + registers.MatrixA * x) >> 8) & 1023;
            int sy = ((((registers.MatrixC * h) & ~63) + ((registers.MatrixD * (y + 1)) & ~63)
                + ((registers.MatrixD * v) & ~63) + (cy << 8) + registers.MatrixC * x) >> 8) & 1023;
            int tile = snapshot.Memory.Vram[((sy >> 3) * 128 + (sx >> 3)) * 2];
            int color = snapshot.Memory.Vram[(tile * 64 + (sy & 7) * 8 + (sx & 7)) * 2 + 1];
            if (color != 0) wrappedBackground[y * 256 + x] = memory.Cgram.GetRgba(color);
        }
        if (checkWrapping) background = wrappedBackground;
        var objects = SnesObjRenderer.RenderResolved(memory.Oam, memory.Vram, memory.Cgram, snapshot.ObjectSelection);
        var expected = SnesLayerCompositor.CreateBackdrop(memory.Cgram, 256 * 224);
        // D6xx: TM=BG1|OBJ, TS=BG1, CGWSEL=2, CGADSUB=BG1|OBJ.
        // All four cloud actors use palette five, which is eligible for OBJ math.
        // Compute main selection first, then add the original BG1 subscreen once.
        for (int i = 0; i < expected.Length; i++)
        {
            bool objWins = objects.Pixels[i].A != 0 && (objects.Priorities[i] != 0 || background[i].A == 0);
            Rgba32 main = objWins ? objects.Pixels[i] : background[i].A != 0 ? background[i] : expected[i];
            if (background[i].A != 0)
                main = new Rgba32(Add(main.R, background[i].R), Add(main.G, background[i].G), Add(main.B, background[i].B));
            expected[i] = main;
        }
        var actual = ending.Render();
        Directory.CreateDirectory("csharp/test-temp/ending-505");
        PngWriter.WriteRgba("csharp/test-temp/ending-505/takeoff-actual.png", 256, 224, actual);
        PngWriter.WriteRgba(checkWrapping ? "csharp/test-temp/ending-505/takeoff-native-wrap-and-math.png"
            : "csharp/test-temp/ending-505/takeoff-native-math.png", 256, 224, expected);
        if (checkWrapping)
        {
            // Isolate overflow from the independently missing main/subscreen math.
            var projected = SoftwareLayeredSnapshotRenderer.Render(new(snapshot.Memory,
                new RenderLayer[] { new Mode7RenderLayer(registers) }, snapshot.ObjectSelection, 15));
            var wrapped = SnesLayerCompositor.CreateBackdrop(memory.Cgram, 256 * 224);
            SnesLayerCompositor.Composite(wrapped, wrappedBackground);
            int differences = projected.Zip(wrapped).Count(pair => pair.First != pair.Second);
            Console.WriteLine($"Takeoff native wrapping: {differences} background pixels differ in the captured frame.");
            AssertTrue(projected.AsSpan().SequenceEqual(wrapped), "takeoff M7SEL=0 wraps rotated background instead of exposing square map edges");
            return;
        }
        AssertTrue(actual.AsSpan().SequenceEqual(expected), "takeoff applies native BG1 subscreen to eligible main pixels");
        static byte Add(byte a, byte b)
        {
            int value = Math.Min(31, (a >> 3) + (b >> 3));
            return (byte)((value << 3) | (value >> 2));
        }
    }
}
