using System.Text.Json;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    private static void VerifyInstalledCeresDoorVisuals(ISnesAddressSpace bus,
        string directory, EnemyTileArtworkCatalog stock)
    {
        CeresDoorVisualCatalog visual = stock.CeresDoorVisual
            ?? throw new InvalidDataException("Installed Ceres-door visuals are missing.");
        var vram = new SnesVram();
        visual.LoadTiles(vram);
        byte[] nativeTiles = RomDataReader.ReadFixedBank(bus,
            CeresDoorVisualRomData.TileSource, CeresDoorVisualRomData.TileByteCount);
        AssertTrue(vram.Bytes.Slice(CeresDoorVisualRomData.TileVramDestination,
                CeresDoorVisualRomData.TileByteCount).SequenceEqual(nativeTiles),
            "installed Ceres-door PNG preserves the complete direct tile DMA");

        var nativeColors = new SnesCgram();
        var compiledColors = new SnesCgram();
        nativeColors.LoadFromBus(bus, CeresDoorVisualRomData.NormalColors,
            CeresDoorVisualRomData.SetupColorCount, CeresDoorVisualRomData.NormalTargetColor);
        visual.LoadNormalColors(compiledColors, CeresDoorVisualRomData.NormalTargetColor);
        AssertTrue(nativeColors.Colors.SequenceEqual(compiledColors.Colors),
            "installed Ceres-door normal RGB5 palette matches the cartridge");
        nativeColors.LoadFromBus(bus, CeresDoorVisualRomData.EscapeColors,
            CeresDoorVisualRomData.SetupColorCount, CeresDoorVisualRomData.ActiveTargetColor);
        visual.LoadEscapeColors(compiledColors, CeresDoorVisualRomData.ActiveTargetColor);
        AssertTrue(nativeColors.Colors.SequenceEqual(compiledColors.Colors),
            "installed Ceres-door escape RGB5 palette matches the cartridge");
        for (int row = 0; row < CeresDoorVisualRomData.AnimationRowCount; row++)
        {
            nativeColors.LoadFromBus(bus,
                CeresDoorVisualRomData.AnimationColors +
                    row * CeresDoorVisualRomData.AnimationRowByteStride,
                CeresDoorVisualRomData.AnimationColorCount,
                CeresDoorVisualRomData.AnimationTargetColor);
            visual.LoadAnimationColors(compiledColors, row);
            AssertTrue(nativeColors.Colors.SequenceEqual(compiledColors.Colors),
                $"installed Ceres-door animated RGB5 row {row} matches the cartridge");
        }
        for (int frame = 0; frame < CeresDoorVisualRomData.Mode7FrameCount; frame++)
        {
            ushort pointer = RomDataReader.ReadWordFixedBank(bus,
                EnemyRomTablePointers.Ceres.DoorTransferPointers + frame * sizeof(ushort));
            int record = 0xa60000 | pointer;
            AssertEqual((byte)0x80, bus.ReadByte(record),
                $"Ceres-door Mode-7 transfer {frame} uses the native low-byte DMA");
            int source = bus.ReadByte(record + 1) |
                bus.ReadByte(record + 2) << 8 | bus.ReadByte(record + 3) << 16;
            AssertEqual(frame == 0 ? CeresDoorVisualRomData.Mode7FirstFrameSource :
                    CeresDoorVisualRomData.Mode7SecondFrameSource,
                source, $"Ceres-door Mode-7 transfer {frame} source");
            AssertEqual(CeresDoorVisualRomData.Mode7FrameByteCount,
                RomDataReader.ReadWordFixedBank(bus, record + 4),
                $"Ceres-door Mode-7 transfer {frame} length");
            AssertEqual(CeresDoorVisualRomData.Mode7DestinationWord,
                RomDataReader.ReadWordFixedBank(bus, record + 6),
                $"Ceres-door Mode-7 transfer {frame} destination");
            AssertEqual((byte)0, bus.ReadByte(record + 8),
                $"Ceres-door Mode-7 transfer {frame} increment mode");
            AssertEqual((byte)0, bus.ReadByte(record + 9),
                $"Ceres-door Mode-7 transfer {frame} terminator");
            byte[] nativeFrame = RomDataReader.ReadFixedBank(bus, source,
                CeresDoorVisualRomData.Mode7FrameByteCount);
            var nativeVram = new SnesVram();
            var installedVram = new SnesVram();
            nativeVram.LoadMode7MapBytes(nativeFrame,
                CeresDoorVisualRomData.Mode7DestinationWord);
            visual.LoadMode7DoorFrame(installedVram, frame);
            AssertTrue(nativeVram.Bytes.SequenceEqual(installedVram.Bytes),
                $"installed Ceres-door Mode-7 frame {frame} matches native transfer");
        }

        string overrides = Path.Combine(directory, "ceres-door-overrides");
        Directory.CreateDirectory(overrides);
        string pngPath = Path.Combine(directory, CeresDoorVisualFormat.TilesFileName);
        byte[] png = File.ReadAllBytes(pngPath);
        IndexedPngImage image = IndexedPng.Read(new MemoryStream(png),
            RoomCharacterAtlasFormat.TileColumns * 8, 8);
        image.Pixels[0] ^= 1;
        using (var output = File.Create(Path.Combine(overrides,
                   CeresDoorVisualFormat.TilesFileName)))
            IndexedPng.Write(output, image.Width, image.Height, image.Pixels,
                image.Palette);
        string colorsPath = Path.Combine(directory, CeresDoorVisualFormat.ColorsFileName);
        CeresDoorVisualDocument document = JsonSerializer.Deserialize<CeresDoorVisualDocument>(
            File.ReadAllBytes(colorsPath),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        document.Normal[0] = document.Normal[0] with
        {
            Red = (document.Normal[0].Red + 1) & 31,
        };
        document.Animation[0][0] = document.Animation[0][0] with
        {
            Blue = (document.Animation[0][0].Blue + 1) & 31,
        };
        document.Mode7DoorFrames[0][0] ^= 1;
        File.WriteAllBytes(Path.Combine(overrides,
            CeresDoorVisualFormat.ColorsFileName),
            CeresDoorVisualCatalog.Write(document));
        CeresDoorVisualCatalog edited = EnemyTileArtworkFiles.Load(directory,
            overrides).CeresDoorVisual!;
        var editedVram = new SnesVram();
        edited.LoadTiles(editedVram);
        AssertEqual((byte)(nativeTiles[0] ^ 0x80),
            editedVram.Bytes[CeresDoorVisualRomData.TileVramDestination],
            "Ceres-door PNG override changes the live first tile pixel");
        var editedCgram = new SnesCgram();
        edited.LoadNormalColors(editedCgram, CeresDoorVisualRomData.NormalTargetColor);
        AssertTrue(editedCgram.Colors[CeresDoorVisualRomData.NormalTargetColor] !=
            compiledColors.Colors[CeresDoorVisualRomData.NormalTargetColor],
            "Ceres-door palette override changes the live normal color");
        edited.LoadAnimationColors(editedCgram, 0);
        var stockAnimationCgram = new SnesCgram();
        visual.LoadAnimationColors(stockAnimationCgram, 0);
        AssertTrue(editedCgram.Colors[CeresDoorVisualRomData.AnimationTargetColor] !=
            stockAnimationCgram.Colors[CeresDoorVisualRomData.AnimationTargetColor],
            "Ceres-door animation override changes the live selected frame");
        var editedMode7 = new SnesVram();
        var stockMode7 = new SnesVram();
        edited.LoadMode7DoorFrame(editedMode7, 0);
        visual.LoadMode7DoorFrame(stockMode7, 0);
        AssertTrue(!editedMode7.Bytes.SequenceEqual(stockMode7.Bytes),
            "Ceres-door Mode-7 frame override changes the live tilemap");
    }
}
