using System.Text.Json;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    private static void VerifyEndingExplosionFrameCatalog(ISnesAddressSpace bus)
    {
        // Independent original list operands select every distinct visual frame.
        ushort[] operands = [0xeb15, 0xeb19, 0xeb1d, 0xeb21, 0xeb2b, 0xeb2f, 0xeb33, 0xeb37,
            0xeb5f, 0xeb63, 0xeb3f, 0xeb43, 0xeb47, 0xeb53, 0xeb6b, 0xeb8b];
        // Published asset keys are compatibility evidence, not generated expected values.
        string[] names = ["planet-damage-0", "planet-damage-1", "planet-damage-2", "planet-damage-3",
            "planet-flash-0", "planet-flash-1", "planet-flash-2", "planet-flash-3", "lava-0", "lava-1",
            "glow-0", "glow-1", "glow-2", "starfield", "silhouette", "afterglow"];
        var frames = EndingExplosionSpriteDefinitions.Frames;
        AssertEqual(16, frames.Count, "explosion catalog frame count");
        for (int i = 0; i < operands.Length; i++)
        {
            int address = 0x8b0000 | operands[i];
            ushort pointer = (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);
            int header = 0x8c0000 | pointer;
            int parts = bus.ReadByte(header) | bus.ReadByte(header + 1) << 8;
            AssertEqual(pointer, frames[i].Pointer, $"explosion catalog native pointer {i}");
            AssertEqual(parts, frames[i].StockPartCount, $"explosion catalog native part count {i}");
            AssertEqual(names[i], frames[i].Name, $"explosion catalog asset key {i}");
        }
        AssertTrue(frames.Select(frame => frame.Name).SequenceEqual(names), "explosion catalog enumeration order");
        foreach (int invalid in new[] { int.MinValue, -1, 16, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => _ = frames[invalid], "explosion catalog index bounds");
    }

    private static void VerifyEndingExplosionPrograms(ISnesAddressSpace bus)
    {
        for (int pointer = EndingExplosionInstructionDefinitions.Start;
             pointer < EndingExplosionInstructionDefinitions.End; pointer += sizeof(ushort))
        {
            int address = (int)new SnesAddress(0x8b, (ushort)pointer);
            ushort nativeWord = (ushort)(bus.ReadByte(address) |
                bus.ReadByte(address + 1) << 8);
            AssertEqual(nativeWord, EndingExplosionInstructionDefinitions.ReadWord((ushort)pointer),
                $"ending explosion instruction $8B:{pointer:X4} matches cartridge");
        }
        AssertThrows<InvalidDataException>(() =>
            EndingExplosionInstructionDefinitions.ReadWord(EndingExplosionInstructionDefinitions.End),
            "ending explosion reader rejects an address past its eight lists");
        AssertThrows<InvalidDataException>(() =>
            EndingExplosionInstructionDefinitions.ReadWord(
                unchecked((ushort)(EndingExplosionInstructionDefinitions.Start + 1))),
            "ending explosion reader rejects an unaligned address");

        ushort[] starts = [0xeb0f, 0xeb3d, 0xeb51, 0xeb59, 0xeb69, 0xeb71, 0xeb81, 0xeb89];
        foreach (ushort start in starts)
        {
            var native = new IntroDiscoverySprite(120, 72, 0x0800, start);
            var installed = new IntroDiscoverySprite(120, 72, 0x0800, start);
            for (int frame = 0; frame < 500; frame++)
            {
                // Private opcodes affect the containing ending scene. This actor-level
                // comparison advances their cursor identically without duplicating that scene.
                var nativeCallbacks = new List<ushort>();
                var generatedCallbacks = new List<ushort>();
                native.Step(bus, (opcode, cursor) => { nativeCallbacks.Add(opcode); return cursor; }, pointer =>
                    (ushort)(bus.ReadByte(0x8b0000 | pointer) | bus.ReadByte(0x8b0000 | (pointer + 1)) << 8));
                installed.Step(bus, (opcode, cursor) => { generatedCallbacks.Add(opcode); return cursor; },
                    EndingExplosionInstructionDefinitions.ReadWord);
                AssertTrue(nativeCallbacks.SequenceEqual(generatedCallbacks),
                    $"explosion actor ${start:X4} callback order/timing at frame {frame}");
                AssertEqual(native.InstructionPointer, installed.InstructionPointer,
                    $"explosion actor ${start:X4} cursor at frame {frame}");
                AssertEqual(native.SpriteMapPointer, installed.SpriteMapPointer,
                    $"explosion actor ${start:X4} visual frame at frame {frame}");
                AssertEqual(native.PreInstructionPointer, installed.PreInstructionPointer,
                    $"explosion actor ${start:X4} pre-instruction at frame {frame}");
                AssertEqual(native.GeneralTimer, installed.GeneralTimer,
                    $"explosion actor ${start:X4} timer at frame {frame}");
                AssertEqual(native.IsActive, installed.IsActive,
                    $"explosion actor ${start:X4} lifetime at frame {frame}");
            }
        }

        for (int frame = 0; frame < 10; frame++)
            AssertEqual((ushort)4, (ushort)(bus.ReadByte(0x8ca396 + 22 * frame) | bus.ReadByte(0x8ca397 + 22 * frame) << 8),
                $"explosion frame {frame} has four OAM parts and a 22-byte record");
    }

    private static void VerifyEndingExplosionActorArtwork(GameInstallation installation,
        ISnesAddressSpace bus, EndingObjectArtworkCatalog stock)
    {
        VerifyEndingExplosionPrograms(bus);
        VerifyEndingExplosionFrameCatalog(bus);

        foreach (EndingExplosionSpriteFrameDefinition frame in EndingExplosionSpriteDefinitions.Frames)
        {
            foreach (ushort y in new ushort[] { 0x0048, 0xfff8 })
            {
                bool onScreen =
                    (y & CinematicSpriteDrawDefinitions.OriginYHighByteMask) == 0;
                int source = (int)new SnesAddress(
                    IntroCinematicRomData.Banks.Spritemaps, frame.Pointer);
                var nativeOam = new OamBuffer();
                nativeOam.BeginFrame();
                if (onScreen)
                    DrawImportedSpritemap(bus, nativeOam, source, 120, y, 0x0800);
                else
                    DrawImportedSpritemap(bus, nativeOam, source, 120, y, 0x0800, originIsOnScreen: false);
                nativeOam.FinalizeFrame();
                var installedOam = new OamBuffer();
                installedOam.BeginFrame();
                stock.ExplosionSprites.Draw(frame.Pointer, installedOam,
                    120, y, 0x0800, onScreen);
                installedOam.FinalizeFrame();
                AssertTrue(nativeOam.LowTable.SequenceEqual(installedOam.LowTable) &&
                        nativeOam.HighTable.SequenceEqual(installedOam.HighTable) &&
                        nativeOam.LastFinalizedSpriteCount == installedOam.LastFinalizedSpriteCount,
                    $"ending explosion {frame.Name} at Y=${y:X4} preserves cartridge OAM");
            }
        }
    }

    private static void VerifyEndingExplosionVisualOverride(GameInstallation installation,
        EndingObjectArtworkCatalog stock, EndingObjectSourceReadGuard guard)
    {
        string name = EndingExplosionSpriteFormat.FileName;
        string stockPath = Path.Combine(installation.EndingObjectDirectory, name);
        string overridePath = Path.Combine(installation.EndingObjectOverrideDirectory, name);
        Directory.CreateDirectory(installation.EndingObjectOverrideDirectory);
        EndingExplosionSpriteDocument document =
            JsonSerializer.Deserialize<EndingExplosionSpriteDocument>(
                File.ReadAllBytes(stockPath), MapPresentationFormat.JsonOptions) ??
            throw new InvalidDataException("Stock ending explosion sprite JSON is empty.");
        // The starfield remains visible through the explosion, giving the edit a
        // reliable live-frame assertion instead of merely checking an isolated OAM map.
        document.Frames["starfield"] = document.Frames["starfield"]
            .Select(part => part with { FlipX = !part.FlipX }).ToArray();
        try
        {
            using (var output = File.Create(overridePath))
                EndingExplosionSpritePresentation.Write(output, document);
            EndingObjectArtworkCatalog edited = installation.LoadEndingObjectArt();
            var audio = new CartridgeAudioState();
            var scene = new EndingCreditsState(guard, audio, 0, 0);
            scene.BindObjectArtwork(stock);
            for (int frame = 0; frame < 10000 &&
                scene.Phase != EndingCreditsPhase.FadeInZebesExplosion; frame++)
            {
                scene.Step();
                audio.AdvanceFrame(guard, default);
            }
            AssertEqual(EndingCreditsPhase.FadeInZebesExplosion, scene.Phase,
                "ending explosion edit reaches its actual scene");
            bool visible = false;
            for (int frame = 0; frame < 2400 &&
                scene.Phase != EndingCreditsPhase.PlanetEscapeFast && !visible; frame++)
            {
                scene.BindObjectArtwork(stock);
                LayeredRenderSnapshot before = scene.CaptureRenderSnapshot();
                scene.BindObjectArtwork(edited);
                LayeredRenderSnapshot after = scene.CaptureRenderSnapshot();
                visible = !before.Memory.Oam.SequenceEqual(after.Memory.Oam) &&
                    !SoftwareLayeredSnapshotRenderer.Render(before).AsSpan().SequenceEqual(
                        SoftwareLayeredSnapshotRenderer.Render(after));
                if (!visible)
                {
                    scene.Step();
                    audio.AdvanceFrame(guard, default);
                }
            }
            AssertTrue(visible,
                "edited explosion starfield changes live OAM and visible ending pixels");
            AssertEqual(0, guard.ForbiddenReadAttempts,
                "explosion art rebind never rereads installed bank-$8C sprite maps");
            document.Frames.Remove("starfield");
            AssertThrows<InvalidDataException>(() =>
            {
                using var invalid = new MemoryStream();
                EndingExplosionSpritePresentation.Write(invalid, document);
            }, "ending explosion sprites reject missing named art");
        }
        finally
        {
            File.Delete(overridePath);
        }
    }
}
