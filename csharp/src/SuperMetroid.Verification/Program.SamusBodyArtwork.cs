using System.Text.Json.Nodes;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Checks every extracted selector and split character transfer against retail.</summary>
    private static void VerifySamusBodyArtwork(ISnesAddressSpace bus, GameInstallation installation)
    {
        SamusBodyArtworkCatalog stock = installation.LoadSamusBodyArt();
        int definitions = 0;
        for (int pose = 0; pose < SamusBodyArtworkCatalog.PoseCount; pose++)
        {
            int address = 0x92D94E + pose * 2;
            ushort native = (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);
            AssertEqual(native, stock.PosePointers[pose], $"Samus body pose pointer {pose:X2}");
            int graphicsAddress = SamusMovementRomData.Poses.Definitions +
                pose * SamusMovementRomData.Poses.DefinitionByteCount + 4;
            AssertEqual(unchecked((sbyte)bus.ReadByte(graphicsAddress)),
                stock.GraphicsYOffset((byte)pose), $"Samus pose {pose:X2} visual Y origin");
        }
        for (int index = 0; index < SamusBodyArtworkCatalog.FrameCount; index++)
        {
            int address = SamusBodyArtworkCatalog.FirstFrameAddress + index * 4;
            SamusBodyFrameSelection frame = stock.Frames[index];
            AssertEqual(bus.ReadByte(address), frame.TopSet, $"Samus frame {index} upper set");
            AssertEqual(bus.ReadByte(address + 1), frame.TopPosition, $"Samus frame {index} upper position");
            AssertEqual(bus.ReadByte(address + 2), frame.BottomSet, $"Samus frame {index} lower set");
            AssertEqual(bus.ReadByte(address + 3), frame.BottomPosition, $"Samus frame {index} lower position");
        }
        for (int half = 0; half < 2; half++)
        {
            bool upper = half == 0;
            int setCount = upper ? SamusBodyArtworkCatalog.TopSetCount : SamusBodyArtworkCatalog.BottomSetCount;
            int pointerTable = upper ? 0x92D91E : 0x92D938;
            for (int set = 0; set < setCount; set++)
            {
                ushort nativePointer = (ushort)(bus.ReadByte(pointerTable + set * 2) |
                    bus.ReadByte(pointerTable + set * 2 + 1) << 8);
                AssertEqual(nativePointer,
                    upper ? stock.TopSetPointers[set] : stock.BottomSetPointers[set],
                    $"Samus {(upper ? "top" : "bottom")} set {set} pointer");
                IReadOnlyList<SamusBodyTileDefinition> entries = upper ? stock.TopSet(set) : stock.BottomSet(set);
                for (int position = 0; position < entries.Count; position++)
                {
                    SamusBodyTileDefinition entry = entries[position];
                    int definitionAddress = stock.DefinitionAddress(upper, (byte)set, (byte)position);
                    int source = bus.ReadByte(definitionAddress) |
                        bus.ReadByte(definitionAddress + 1) << 8 |
                        bus.ReadByte(definitionAddress + 2) << 16;
                    ushort first = (ushort)(bus.ReadByte(definitionAddress + 3) |
                        bus.ReadByte(definitionAddress + 4) << 8);
                    ushort second = (ushort)(bus.ReadByte(definitionAddress + 5) |
                        bus.ReadByte(definitionAddress + 6) << 8);
                    AssertEqual(source, entry.SourceAddress, $"Samus definition {definitionAddress:X6} source");
                    AssertEqual(first, entry.FirstSize, $"Samus definition {definitionAddress:X6} first size");
                    AssertEqual(second, entry.SecondSize, $"Samus definition {definitionAddress:X6} second size");
                    for (int i = 0; i < entry.Planar.Length; i++)
                        AssertEqual(bus.ReadByte(source + i), entry.Planar.Span[i],
                            $"Samus definition {definitionAddress:X6} tile byte {i:X3}");
                    definitions++;
                }
            }
        }
        AssertEqual(435, definitions, "complete Samus body DMA definition count");

        SamusSpritemapArtworkCatalog sprites = stock.Spritemaps;
        for (int pose = 0; pose < SamusBodyArtworkCatalog.PoseCount; pose++)
        {
            int topAddress = SamusSpritemapArtworkCatalog.TopBaseAddress + pose * 2;
            int bottomAddress = SamusSpritemapArtworkCatalog.BottomBaseAddress + pose * 2;
            AssertEqual((ushort)(bus.ReadByte(topAddress) | bus.ReadByte(topAddress + 1) << 8),
                sprites.TopBase((byte)pose), $"Samus pose {pose:X2} top OAM base");
            AssertEqual((ushort)(bus.ReadByte(bottomAddress) | bus.ReadByte(bottomAddress + 1) << 8),
                sprites.BottomBase((byte)pose), $"Samus pose {pose:X2} bottom OAM base");
        }
        var spritemapGuard = new FrontendCartridgeReadGuard(bus);
        int nonzeroSpritemaps = 0;
        for (int index = 0; index < SamusSpritemapArtworkCatalog.PointerCount; index++)
        {
            int tableAddress = SamusSpritemapArtworkCatalog.PointerTableAddress + index * 2;
            ushort pointer = (ushort)(bus.ReadByte(tableAddress) | bus.ReadByte(tableAddress + 1) << 8);
            AssertEqual(pointer, sprites.Pointers[index], $"Samus OAM pointer index {index}");
            if (!sprites.TryGet((ushort)index, out SamusSpritemapDefinition? record))
            {
                AssertEqual((ushort)0, pointer, $"Samus OAM index {index} mutable-memory pointer");
                continue;
            }
            nonzeroSpritemaps++;
            AssertEqual(pointer, record!.Pointer, $"Samus OAM index {index} record identity");
            int address = 0x920000 | pointer;
            AssertEqual((ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8),
                (ushort)record.Parts.Length, $"Samus OAM record ${pointer:X4} part count");
            for (int part = 0; part < record.Parts.Length; part++)
            {
                int at = address + 2 + part * 5;
                AssertEqual((ushort)(bus.ReadByte(at) | bus.ReadByte(at + 1) << 8),
                    record.Parts[part].X, $"Samus OAM ${pointer:X4} part {part} X/size");
                AssertEqual(bus.ReadByte(at + 2), record.Parts[part].Y,
                    $"Samus OAM ${pointer:X4} part {part} Y");
                AssertEqual((ushort)(bus.ReadByte(at + 3) | bus.ReadByte(at + 4) << 8),
                    record.Parts[part].Attributes,
                    $"Samus OAM ${pointer:X4} part {part} attributes");
            }
            var nativeOam = new OamBuffer();
            var installedOam = new OamBuffer();
            nativeOam.BeginFrame();
            installedOam.BeginFrame();
            nativeOam.AddSamusSpritemap(bus, (ushort)index, 127, 131);
            installedOam.AddSamusSpritemap(spritemapGuard, (ushort)index, 127, 131, sprites);
            AssertTrue(nativeOam.LowTable.SequenceEqual(installedOam.LowTable) &&
                nativeOam.HighTable.SequenceEqual(installedOam.HighTable) &&
                nativeOam.NextByteOffset == installedOam.NextByteOffset,
                $"Samus OAM index {index} installed/native staging parity");
        }
        AssertEqual(1913, nonzeroSpritemaps, "all nonzero Samus OAM pointer entries");

        // The installed selector and both VRAM halves must take the production path
        // without even reading one cartridge byte. This covers every authored pose's
        // initial frame, including all flashback/normal-gameplay pose identities.
        var guardedBus = new FrontendCartridgeReadGuard(bus);
        for (int pose = 0; pose < SamusBodyArtworkCatalog.PoseCount; pose++)
        {
            var nativeTransfer = new SamusTileTransferState();
            var installedTransfer = new SamusTileTransferState();
            installedTransfer.BindArtwork(stock);
            nativeTransfer.SelectForPoseFrame(bus, (byte)pose, 0);
            installedTransfer.SelectForPoseFrame(guardedBus, (byte)pose, 0);
            AssertEqual(nativeTransfer.TopDefinitionAddress, installedTransfer.TopDefinitionAddress,
                $"Samus pose {pose:X2} upper transfer pointer");
            AssertEqual(nativeTransfer.BottomDefinitionAddress, installedTransfer.BottomDefinitionAddress,
                $"Samus pose {pose:X2} lower transfer pointer");
            var nativeVram = new SnesVram();
            var installedVram = new SnesVram();
            nativeTransfer.TransferToVram(bus, nativeVram);
            installedTransfer.TransferToVram(guardedBus, installedVram);
            AssertTrue(nativeVram.Bytes.SequenceEqual(installedVram.Bytes),
                $"Samus pose {pose:X2} complete split VRAM transfer parity");
        }

        // An override changes pixels and selector data without changing animation timing.
        Directory.CreateDirectory(installation.SamusBodyOverrideDirectory);
        string stockPng = Path.Combine(installation.SamusBodyDirectory, "top-00.png");
        string overridePng = Path.Combine(installation.SamusBodyOverrideDirectory, "top-00.png");
        byte[] original = File.ReadAllBytes(stockPng);
        IndexedPngImage image = IndexedPng.Read(new MemoryStream(original, false), 64,
            stock.TopSet(0).Count * 16);
        byte[] pixels = (byte[])image.Pixels.Clone();
        pixels[0] = (byte)((pixels[0] + 1) & 15);
        using (var output = File.Create(overridePng))
            IndexedPng.Write(output, image.Width, image.Height, pixels, image.Palette);
        string selectedManifest = Path.Combine(installation.SamusBodyOverrideDirectory,
            SamusBodyArtworkFiles.ManifestFileName);
        JsonNode document = JsonNode.Parse(File.ReadAllText(Path.Combine(
            installation.SamusBodyDirectory, SamusBodyArtworkFiles.ManifestFileName)))!;
        byte originalPosition = (byte)document["frames"]![0]!["topPosition"]!.GetValue<int>();
        document["frames"]![0]!["topPosition"] = originalPosition == 0 ? 1 : 0;
        sbyte originalYOffset = (sbyte)document["graphicsYOffsets"]![1]!.GetValue<int>();
        document["graphicsYOffsets"]![1] = originalYOffset + 1;
        ushort poseOnePointer = stock.Spritemaps.Pointers[stock.Spritemaps.TopBase(0x01)];
        JsonNode poseOneRecord = document["spritemaps"]!.AsArray().First(entry =>
            entry!["pointer"]!.GetValue<int>() == poseOnePointer)!;
        int originalPartX = poseOneRecord["parts"]![0]!["x"]!.GetValue<int>();
        poseOneRecord["parts"]![0]!["x"] = originalPartX + 1;
        File.WriteAllText(selectedManifest, document.ToJsonString());
        SamusBodyArtworkCatalog replacement = installation.LoadSamusBodyArt();
        AssertTrue(!replacement.TopSet(0)[0].Planar.Span.SequenceEqual(stock.TopSet(0)[0].Planar.Span),
            "Samus body PNG override changes compiled tile bytes");
        AssertTrue(replacement.Frames[0].TopPosition != stock.Frames[0].TopPosition,
            "Samus body JSON override changes a visual frame selector");
        var stockSpritemapOam = new OamBuffer();
        var editedSpritemapOam = new OamBuffer();
        stockSpritemapOam.BeginFrame();
        editedSpritemapOam.BeginFrame();
        ushort poseOneIndex = stock.Spritemaps.TopBase(0x01);
        stockSpritemapOam.AddSamusSpritemap(guardedBus, poseOneIndex, 128, 128,
            stock.Spritemaps);
        editedSpritemapOam.AddSamusSpritemap(guardedBus, poseOneIndex, 128, 128,
            replacement.Spritemaps);
        AssertEqual(unchecked((byte)(stockSpritemapOam.LowTable[0] + 1)),
            editedSpritemapOam.LowTable[0],
            "edited Samus spritemap part changes production OAM X by one pixel");
        var editedSamus = new SamusState { Pose = 0x01 };
        editedSamus.TileTransfers.BindArtwork(replacement);
        AssertEqual((sbyte)(originalYOffset + 1),
            editedSamus.ReadGraphicsYOffset(guardedBus),
            "edited Samus visual Y offset is used without a cartridge read");
        var stockSamus = new SamusState { Pose = 0x01, XPosition = 128, YPosition = 128 };
        stockSamus.TileTransfers.BindArtwork(stock);
        editedSamus.XPosition = stockSamus.XPosition;
        editedSamus.YPosition = stockSamus.YPosition;
        var stockOam = new OamBuffer();
        var editedOam = new OamBuffer();
        stockOam.BeginFrame();
        editedOam.BeginFrame();
        stockSamus.Draw(guardedBus, stockOam, layer1X: 0, layer1Y: 0);
        editedSamus.Draw(guardedBus, editedOam, layer1X: 0, layer1Y: 0);
        AssertEqual(unchecked((ushort)(stockSamus.SpritemapYPosition - 1)),
            editedSamus.SpritemapYPosition,
            "edited pose graphics Y offset shifts the actual Samus OAM origin one pixel");
        AssertEqual(unchecked((byte)originalYOffset),
            SamusPoseProjectileOriginDefinitions.ReadYOffset(0x01),
            "editing Samus art never changes the compiled projectile collision correction");
        var originalSelector = new SamusTileTransferState();
        var editedSelector = new SamusTileTransferState();
        originalSelector.BindArtwork(stock);
        editedSelector.BindArtwork(replacement);
        originalSelector.SelectForPoseFrame(guardedBus, pose: 0x01, animationFrame: 0);
        editedSelector.SelectForPoseFrame(guardedBus, pose: 0x01, animationFrame: 0);
        AssertTrue(originalSelector.TopDefinitionAddress != editedSelector.TopDefinitionAddress,
            "edited Samus body JSON changes the production pose-$01 frame selector");
        var stockTransfer = new SamusTileTransferState();
        var editedTransfer = new SamusTileTransferState();
        stockTransfer.BindArtwork(stock);
        editedTransfer.BindArtwork(replacement);
        stockTransfer.SelectForPoseFrame(guardedBus, pose: 0x09, animationFrame: 0);
        editedTransfer.SelectForPoseFrame(guardedBus, pose: 0x09, animationFrame: 0);
        var stockPixels = new SnesVram();
        var editedPixels = new SnesVram();
        stockTransfer.TransferToVram(guardedBus, stockPixels);
        editedTransfer.TransferToVram(guardedBus, editedPixels);
        AssertTrue(!stockPixels.Bytes.SequenceEqual(editedPixels.Bytes),
            "edited Samus body PNG changes the production VRAM transfer for pose $09");
        document["topPointers"]![0] = 0x8000;
        File.WriteAllText(selectedManifest, document.ToJsonString());
        AssertThrows<InvalidDataException>(() => installation.LoadSamusBodyArt(),
            "Samus body override cannot change native set-pointer identity");
        document["topPointers"]![0] = stock.TopSetPointers[0];
        File.WriteAllText(selectedManifest, document.ToJsonString());
        Console.WriteLine("Samus body art: 253 poses, 1143 frames, 435 split DMAs, 1913 nonzero OAM indices match retail; PNG/JSON overrides load.");
    }
}
