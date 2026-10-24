using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Tooling;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Areas.Ceres.Artwork;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Rendering;
using SuperMetroid.TestSupport.CpuBusTestDoubles;

namespace SuperMetroid.Verification.Enemies.Artwork;

/// <summary>Checks installed Ceres door tiles, color rows, and Mode 7 transfer frames.</summary>
internal static class EnemyTileArtworkCeresDoorVerification
{
    /// <summary>Compares native VRAM and CGRAM, then checks independent PNG, palette, and Mode 7 edits.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots, native VRAM words")]
    [CpuBusBoundary("exact bus test: retail ROM oracle for Ceres door tiles, palettes and Mode-7 transfer records")]
    internal static void VerifyInstalledCeresDoorVisuals(ISnesAddressSpace bus,
        string directory, InstalledEnemyArtwork stock)
    {
        CeresDoorVisualCatalog visual = stock.Areas.CeresDoorVisual
            ?? throw new InvalidDataException("Installed Ceres-door visuals are missing.");
        byte[] nativeTiles = RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus),
            CeresDoorVisualRomData.TileSource, CeresDoorVisualRomData.TileByteCount);
        AssertTrue(visual.Characters.Span.SequenceEqual(nativeTiles),
            "installed Ceres-door PNG preserves the complete direct tile DMA");
        // $A6:F739 copies the characters to VRAM byte $E000, word $7000: OBJ character $100.
        AssertEqual(SpriteCharacters.TableByte(VramDmaDestination.FromNative(0x7000)), PlacedUploads.Of(CeresDoorCharacters.Shown).FirstByte,
            "the door's characters show where the cartridge's DMA lands them among the OBJ characters");
        AssertEqual(nativeTiles.Length, PlacedUploads.Of(CeresDoorCharacters.Shown).ByteCount, "the door shows all its characters");

        var nativeColors = new SnesCgram();
        var compiledColors = new GameplayColours(new SpriteColourSets());
        SuperMetroid.AssetExtraction.Cartridge.CartridgePaletteImporter.LoadToCgram(nativeColors, bus, CeresDoorVisualRomData.NormalColors,
            CeresDoorVisualRomData.SetupColorCount, CeresDoorVisualRomDataConstants.NormalTargetColor);
        visual.LoadNormalColors(CeresDoorColourDefinitions.Inactive.In(compiledColors));
        AssertTrue(new CgramPalettes(nativeColors).ImageEquals(new CgramPalettes(compiledColors.AsPaletteColors())),
            "installed Ceres-door normal RGB5 palette matches the cartridge");
        SuperMetroid.AssetExtraction.Cartridge.CartridgePaletteImporter.LoadToCgram(nativeColors, bus, CeresDoorVisualRomData.EscapeColors,
            CeresDoorVisualRomData.SetupColorCount, CeresDoorVisualRomDataConstants.ActiveTargetColor);
        visual.LoadEscapeColors(CeresDoorColourDefinitions.Active.In(compiledColors));
        AssertTrue(new CgramPalettes(nativeColors).ImageEquals(new CgramPalettes(compiledColors.AsPaletteColors())),
            "installed Ceres-door escape RGB5 palette matches the cartridge");
        for (int row = 0; row < CeresDoorVisualRomData.AnimationRowCount; row++)
        {
            SuperMetroid.AssetExtraction.Cartridge.CartridgePaletteImporter.LoadToCgram(nativeColors, bus,
                CeresDoorVisualRomData.AnimationColors +
                    row * CeresDoorVisualRomData.AnimationRowByteStride,
                CeresDoorVisualRomData.AnimationColorCount,
                CeresDoorVisualRomDataConstants.AnimationTargetColor);
            visual.LoadAnimationColors(compiledColors.BackgroundSets, row);
            AssertTrue(new CgramPalettes(nativeColors).ImageEquals(new CgramPalettes(compiledColors.AsPaletteColors())),
                $"installed Ceres-door animated RGB5 row {row} matches the cartridge");
        }
        for (int frame = 0; frame < CeresDoorVisualRomData.Mode7FrameCount; frame++)
        {
            ushort pointer = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus),
                EnemyRomTablePointers.Ceres.DoorTransferPointers + frame * sizeof(ushort));
            int record = 0xa60000 | pointer;
            AssertEqual((byte)0x80, CpuBusRegionRead.Read(bus, record),
                $"Ceres-door Mode-7 transfer {frame} uses the native low-byte DMA");
            int source = (int)new SnesAddress(CpuBusRegionRead.Read(bus, record + 3), SnesLittleEndian.Word(CpuBusRegionRead.Read(bus, record + 1), CpuBusRegionRead.Read(bus, record + 2)));
            AssertEqual(frame == 0 ? CeresDoorVisualRomData.Mode7FirstFrameSource :
                    CeresDoorVisualRomData.Mode7SecondFrameSource,
                source, $"Ceres-door Mode-7 transfer {frame} source");
            AssertEqual(CeresDoorVisualRomData.Mode7FrameByteCount,
                RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), record + 4),
                $"Ceres-door Mode-7 transfer {frame} length");
            AssertEqual(CeresDoorVisualRomData.Mode7MapCell,
                RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), record + 6),
                $"Ceres-door Mode-7 transfer {frame} destination");
            AssertEqual((byte)0, CpuBusRegionRead.Read(bus, record + 8),
                $"Ceres-door Mode-7 transfer {frame} increment mode");
            AssertEqual((byte)0, CpuBusRegionRead.Read(bus, record + 9),
                $"Ceres-door Mode-7 transfer {frame} terminator");
            byte[] nativeFrame = RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus), source,
                CeresDoorVisualRomData.Mode7FrameByteCount);
            AssertTrue(nativeFrame.SequenceEqual(visual.Mode7DoorFrame(frame)),
                $"installed Ceres-door Mode-7 frame {frame} matches native transfer");
        }

        string overrides = Path.Combine(directory, "ceres-door-overrides");
        Directory.CreateDirectory(overrides);
        string pngPath = Path.Combine(directory, CeresDoorVisualFormat.TilesFileName);
        byte[] png = File.ReadAllBytes(pngPath);
        IndexedPngImage image = IndexedPng.Read(new MemoryStream(png),
            new(RoomCharacterAtlasFormat.TileColumns * 8, 8));
        image.Pixels[0] ^= 1;
        using (var output = File.Create(Path.Combine(overrides,
                   CeresDoorVisualFormat.TilesFileName)))
            IndexedPng.Write(output, new(image.Size.X, image.Size.Y), image.Pixels,
                image.Colours);
        string colorsPath = Path.Combine(directory, CeresDoorVisualFormat.ColorsFileName);
        CeresDoorVisualDocument document = RequiredJson.Deserialize<CeresDoorVisualDocument>(
            File.ReadAllBytes(colorsPath),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
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
        CeresDoorVisualCatalog edited = (EnemyTileArtworkFiles.Load(directory,
            overrides).Areas.CeresDoorVisual ?? throw new InvalidOperationException("Verification requires EnemyTileArtworkFiles.Load(directory, overrides).CeresDoorVisual."));
        AssertEqual((byte)(nativeTiles[0] ^ 0x80), edited.Characters.Span[0],
            "Ceres-door PNG override changes the live first tile pixel");
        var editedCgram = new GameplayColours(new SpriteColourSets());
        edited.LoadNormalColors(CeresDoorColourDefinitions.Inactive.In(editedCgram));
        AssertTrue(editedCgram.Colors[CeresDoorVisualRomDataConstants.NormalTargetColor] !=
            compiledColors.Colors[CeresDoorVisualRomDataConstants.NormalTargetColor],
            "Ceres-door palette override changes the live normal color");
        edited.LoadAnimationColors(editedCgram.BackgroundSets, 0);
        var stockAnimationCgram = new GameplayColours(new SpriteColourSets());
        visual.LoadAnimationColors(stockAnimationCgram.BackgroundSets, 0);
        AssertTrue(editedCgram.Colors[CeresDoorVisualRomDataConstants.AnimationTargetColor] !=
            stockAnimationCgram.Colors[CeresDoorVisualRomDataConstants.AnimationTargetColor],
            "Ceres-door animation override changes the live selected frame");
        AssertTrue(!edited.Mode7DoorFrame(0).SequenceEqual(visual.Mode7DoorFrame(0)),
            "Ceres-door Mode-7 frame override changes the live tilemap");
    }
}
