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
        Suite(nameof(VerifyEndingExplosionCalculatedParts), () => VerifyEndingExplosionCalculatedParts(bus));
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
            AssertEqual(pointer, EndingExplosionSpriteDefinitions.Pointer((EndingExplosionSpriteDefinitions.Pose)i),
                $"explosion shared pose pointer {i}");
            AssertEqual(parts, frames[i].StockPartCount, $"explosion catalog native part count {i}");
            AssertEqual(names[i], frames[i].Name, $"explosion catalog asset key {i}");
        }
        AssertTrue(frames.Select(frame => frame.Name).SequenceEqual(names), "explosion catalog enumeration order");
        foreach (int invalid in new[] { int.MinValue, -1, 16, int.MaxValue })
        {
            AssertThrows<ArgumentOutOfRangeException>(() => _ = frames[invalid], "explosion catalog index bounds");
            AssertThrows<ArgumentOutOfRangeException>(() => EndingExplosionSpriteDefinitions.Pointer(
                (EndingExplosionSpriteDefinitions.Pose)invalid), "explosion pose pointer bounds");
        }
    }

    private static void VerifyEndingExplosionCalculatedParts(ISnesAddressSpace bus)
    {
        static SpriteComposition Calculate(ushort pointer, SpriteComposition supplied) =>
            EndingExplosionGridParts.CalculateIfMatching(pointer, EndingExplosionQuadrantParts.CalculateIfMatching(pointer, EndingExplosionStarfieldParts.CalculateIfMatching(pointer, EndingExplosionAfterglowParts.CalculateIfMatching(pointer, EndingExplosionSilhouetteParts.CalculateIfMatching(pointer, supplied)))));
        for (int pose = 0; pose < 16; pose++)
        {
            ushort pointer = pose < 10 ? (ushort)(0xa396 + pose * 22) : pose == 10 ? (ushort)0xa472 : pose == 11 ? (ushort)0xa4b0 : pose == 12 ? (ushort)0xa516 : pose == 13 ? (ushort)0xa28b : pose == 14 ? (ushort)0xa5e2 : (ushort)0xa57c;
            int count = bus.ReadByte(0x8c0000 | pointer) | bus.ReadByte(0x8c0000 | (pointer + 1)) << 8;
            var visual = new SpriteVisualPart[count];
            for (int index = 0; index < count; index++)
            {
                int address = 0x8c0000 + pointer + 2 + index * 5;
                var x = new SnesSpritemapXWord((ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8));
                var attributes = new SnesObjAttributeWord((ushort)(bus.ReadByte(address + 3) | bus.ReadByte(address + 4) << 8));
                visual[index] = new SpriteVisualPart
                {
                    OffsetX = x.SignedOffset, OffsetY = unchecked((sbyte)bus.ReadByte(address + 2)),
                    TileColumn = attributes.TileNumber % 16, TileRow = attributes.TileNumber / 16,
                    Size = x.IsLarge ? 16 : 8, Priority = attributes.Priority, Palette = null,
                    FlipX = attributes.FlipHorizontally, FlipY = attributes.FlipVertically,
                };
            }
            SpriteComposition original = IntroCinematicSpriteCompiler.Compile(visual, "native grid");
            if (pose == 14)
            {
                for (int index = 0; index < visual.Length; index++)
                    AssertEqual(visual[index].TileRow * 16 + visual[index].TileColumn,
                        EndingExplosionAfterglowParts.StockTile(index), "original afterglow ordered tile runs");
                foreach (int invalid in new[] { int.MinValue, -1, 37, int.MaxValue })
                    AssertThrows<ArgumentOutOfRangeException>(() => EndingExplosionAfterglowParts.StockTile(invalid), "afterglow tile sequence bounds");
            }
            SpriteComposition calculated = Calculate(pointer, original);
            AssertTrue(!ReferenceEquals(original, calculated), "original grid uses calculated parts");
            string Identity(SpriteComposition composition) => SelectedPresentationHash.Create("grid", composition.AppendIdentity);
            AssertEqual(Identity(original), Identity(calculated), "calculated grid preserves every compiled visual field and order");
            foreach (ushort y in new ushort[] { 72, 0xfff8 })
            {
                var native = new OamBuffer();
                var generated = new OamBuffer();
                native.BeginFrame(); generated.BeginFrame();
                DrawImportedSpritemap(bus, native, 0x8c0000 | pointer, 120, y, 0x0800, originIsOnScreen: y == 72);
                if (y == 72) calculated.DrawOnScreen(generated, 120, y, 0x0800);
                else calculated.DrawOffScreen(generated, 120, y, 0x0800);
                native.FinalizeFrame(); generated.FinalizeFrame();
                AssertTrue(native.LowTable.SequenceEqual(generated.LowTable) && native.HighTable.SequenceEqual(generated.HighTable),
                    "calculated grid preserves native OAM and clipping");
            }
            SpriteVisualPart first = visual[0];
            if (pose is >= 10 and <= 12)
            {
                var role = (EndingExplosionSpriteDefinitions.Pose)pose;
                int basisCount = pose == 10 ? 3 : 5;
                for (int index = 0; index < basisCount; index++)
                    AssertEqual(original.Part(index), EndingExplosionQuadrantParts.StockBasis(role, index), "original packed quadrant basis");
                foreach (int invalid in new[] { int.MinValue, -1, basisCount, int.MaxValue })
                    AssertThrows<ArgumentOutOfRangeException>(() => EndingExplosionQuadrantParts.StockBasis(role, invalid), "quadrant basis index bounds");
                SpriteComposition editedBasis = IntroCinematicSpriteCompiler.Compile(
                    visual.Select(part => part with { TileRow = part.TileRow + 1, Palette = 3 }).ToArray(), "edited symmetric quadrants");
                SpriteComposition calculatedBasis = Calculate(pointer, editedBasis);
                AssertTrue(!ReferenceEquals(editedBasis, calculatedBasis), "symmetric edits retain calculated quadrants");
                AssertEqual(Identity(editedBasis), Identity(calculatedBasis), "edited quadrant inputs preserve every field");
            }
            foreach (SpriteVisualPart edited in new[]
            {
                first with { OffsetX = first.OffsetX + 1 }, first with { OffsetY = first.OffsetY + 1 },
                first with { TileColumn = first.TileColumn == 14 ? 13 : first.TileColumn + 1 }, first with { TileRow = first.TileRow + 1 },
                first with { Size = first.Size == 8 ? 16 : 8 }, first with { Priority = 1 }, first with { Palette = 3 },
                first with { FlipX = !first.FlipX }, first with { FlipY = !first.FlipY },
                first with { OffsetX = 255 }, first with { OffsetY = 127 },
            })
            {
                visual[0] = edited;
                SpriteComposition supplied = IntroCinematicSpriteCompiler.Compile(visual, "edited grid");
                AssertTrue(pose >= 13 ? Identity(supplied) == Identity(Calculate(pointer, supplied)) : ReferenceEquals(supplied, Calculate(pointer, supplied)),
                    "independent edited field keeps supplied composition");
            }
            visual[0] = first;
            (visual[0], visual[1]) = (visual[1], visual[0]);
            SpriteComposition reordered = IntroCinematicSpriteCompiler.Compile(visual, "reordered grid");
            AssertTrue(pose >= 13 ? Identity(reordered) == Identity(Calculate(pointer, reordered)) : ReferenceEquals(reordered, Calculate(pointer, reordered)), "edited part order is preserved");
            SpriteComposition shortened = IntroCinematicSpriteCompiler.Compile(visual[..^1], "shortened grid");
            AssertTrue(pose >= 13 ? Identity(shortened) == Identity(Calculate(pointer, shortened)) : ReferenceEquals(shortened, Calculate(pointer, shortened)), "edited part count is preserved");
        }
        foreach (int invalid in new[] { int.MinValue, -1, 0, 9, 13, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => EndingExplosionQuadrantParts.StockBasis(
                (EndingExplosionSpriteDefinitions.Pose)invalid, 0), "quadrant basis role bounds");
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
                native.Step((EndingSpriteInstruction opcode, ushort cursor) => { nativeCallbacks.Add((ushort)opcode); return cursor; }, pointer =>
                    (ushort)(bus.ReadByte(0x8b0000 | pointer) | bus.ReadByte(0x8b0000 | (pointer + 1)) << 8));
                installed.Step((EndingSpriteInstruction opcode, ushort cursor) => { generatedCallbacks.Add((ushort)opcode); return cursor; },
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

    private static void VerifyEndingExplosionActorArtwork(
        ISnesAddressSpace bus, EndingObjectArtworkCatalog stock)
    {
        Suite(nameof(VerifyEndingExplosionPrograms), () => VerifyEndingExplosionPrograms(bus));
        Suite(nameof(VerifyEndingExplosionFrameCatalog), () => VerifyEndingExplosionFrameCatalog(bus));

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
            var scene = CreateRetailEndingFixture(guard, audio, 0, 0);
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
