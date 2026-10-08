using System.Buffers.Binary;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyGameOptionsToggleGeometry()
    {
        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(bus.Rom)),
            "options toggle oracle revision");
        using var extracted = new MemoryStream(GameOptionsPresentationExtractor.Extract(bus));
        GameOptionsPresentation presentation = GameOptionsPresentation.Load(extracted);
        for (int setting = 0; setting < 2; setting++)
        {
            GameOptionsToggleLayout layout = GameOptionsRomData.SpecialToggles.Layout(setting);
            AssertEqual(setting == 0 ? GameOptionsRomData.SpecialToggles.IconCancel :
                GameOptionsRomData.SpecialToggles.Moonwalk, layout, "named toggle layout alias");
            int[] actual = [layout.EnabledTop, layout.EnabledBottom, layout.DisabledTop, layout.DisabledBottom];
            for (int enabled = 0; enabled < 2; enabled++)
            {
                int[] countOperands = enabled == 0
                    ? [0x82f0d3, 0x82f0e2, 0x82f0f1, 0x82f0ff]
                    : [0x82f114, 0x82f123, 0x82f132, 0x82f140];
                byte[] page = presentation.CreatePage(GameOptionsPresentationDefinitions.SpecialEnglishPage);
                presentation.ApplySpecialToggle(page, setting == 0
                    ? GameOptionsPresentationDefinitions.IconCancelToggle
                    : GameOptionsPresentationDefinitions.MoonwalkToggle, enabled != 0);
                for (int box = 0; box < 4; box++)
                {
                    int address = 0x82f149 + setting * 4 + box / 2 * 8 + box % 2 * 2;
                    int offset = ReadVerificationWord(bus, address);
                    AssertEqual(offset, actual[box], "native toggle box offset");
                    int operand = countOperands[box];
                    AssertEqual((byte)0xa0, bus.ReadByte(operand - 1), "toggle LDY count opcode");
                    AssertEqual((byte)0xa9, bus.ReadByte(operand + 2), "toggle LDA palette opcode");
                    int count = ReadVerificationWord(bus, operand);
                    AssertEqual(count, GameOptionsRomData.SpecialToggles.PaletteRegionByteCount,
                        "native toggle box width");
                    int expectedPalette = ReadVerificationWord(bus, operand + 3) >> 10;
                    for (int cell = offset; cell < offset + count; cell += 2)
                        AssertEqual(expectedPalette,
                            (BinaryPrimitives.ReadUInt16LittleEndian(page.AsSpan(cell, 2)) >> 10) & 7,
                            $"toggle {setting}/{enabled} box {box} cell {cell:X4}");
                }
            }
        }
        foreach (int setting in new[] { int.MinValue, -1, 2, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => GameOptionsRomData.SpecialToggles.Layout(setting),
                $"invalid toggle layout {setting}");
        Console.WriteLine("Options toggle geometry: all eight native boxes, widths and both highlight states pass through extraction and presentation.");
    }

    private static void VerifyGameOptionsLanguagePalettes()
    {
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(bus.Rom)),
            "options language oracle revision");
        byte[] extracted = SuperMetroid.AssetExtraction.GameOptionsPresentationExtractor.Extract(bus);
        GameOptionsPresentation presentation = GameOptionsPresentation.Load(new MemoryStream(extracted));
        AreaMapPresentationCatalog installed = RetailPresentationFixture();
        AssertEqual(4, GameOptionsRomData.LanguagePaletteRegionCount, "native language highlight has four regions");
        foreach (int index in new[] { int.MinValue, -1, 4, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => GameOptionsRomData.LanguagePaletteRegion(index),
                $"unsupported language region {index}");

        // Set_Language_Text_Option_Highlight ($82:EDED) chooses palette zero for
        // the first pair with AltText=0 and the second pair with AltText=1.
        // Check every word, not just the first cell, on both runtime paths.
        for (int language = 0; language < 2; language++)
        {
            bool japanese = language != 0;
            var menu = new GameOptionsMenuState(bus, japaneseText: japanese, mapPresentation: installed);
            ReadOnlySpan<byte> vram = menu.CaptureRenderSnapshot().Memory.Vram;
            byte[] editablePage = presentation.CreatePage(GameOptionsPresentationDefinitions.PrimaryPage);
            presentation.ApplyLanguage(editablePage, japanese);

            for (int regionIndex = 0; regionIndex < 4; regionIndex++)
            {
                // Read each native LDX-offset / LDY-count / LDA-palette call directly.
                int instruction = (japanese ? 0x82ee24 : 0x82edf2) + regionIndex * 12;
                AssertEqual((byte)0xa2, bus.ReadByte(instruction), "language region LDX immediate");
                AssertEqual((byte)0xa0, bus.ReadByte(instruction + 3), "language region LDY immediate");
                AssertEqual((byte)0xa9, bus.ReadByte(instruction + 6), "language region LDA immediate");
                int originalOffset = ReadVerificationWord(bus, instruction + 1);
                int originalCount = ReadVerificationWord(bus, instruction + 4);
                int expected = ReadVerificationWord(bus, instruction + 7) >> 10;
                GameOptionsLanguagePaletteRegion region = GameOptionsRomData.LanguagePaletteRegion(regionIndex);
                AssertEqual(originalOffset, region.ByteOffset, "language region offset");
                AssertEqual(originalCount, region.ByteCount, "language region byte count");
                AssertEqual(expected == 0, region.HighlightWhenJapanese == japanese, "language selection polarity");
                for (int offset = originalOffset; offset < originalOffset + originalCount; offset += 2)
                {
                    int runtimePalette = PaletteAt(vram, MenuPpuState.Bg1TilemapWord * 2 + offset);
                    int editablePalette = PaletteAt(editablePage, offset);
                    AssertEqual(expected, runtimePalette,
                        $"ROM options language={language}, region={regionIndex}, offset={offset:X4}");
                    AssertEqual(expected, editablePalette,
                        $"editable options language={language}, region={regionIndex}, offset={offset:X4}");
                }
            }
        }
        Console.WriteLine("Options language: both language states and all four ROM regions match in runtime and editable paths.");

        static int PaletteAt(ReadOnlySpan<byte> tilemap, int offset) =>
            (BinaryPrimitives.ReadUInt16LittleEndian(tilemap.Slice(offset, 2)) >> 10) & 7;
    }
}
