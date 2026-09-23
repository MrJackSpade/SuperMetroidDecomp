using System.Buffers.Binary;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyGameOptionsLanguagePalettes()
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        byte[] extracted = SuperMetroid.AssetExtraction.GameOptionsPresentationExtractor.Extract(bus);
        GameOptionsPresentation presentation = GameOptionsPresentation.Load(new MemoryStream(extracted));
        ReadOnlySpan<GameOptionsLanguagePaletteRegion> regions = GameOptionsRomData.LanguagePaletteRegions;
        AssertEqual(4, regions.Length, "native language highlight has four regions");

        // Set_Language_Text_Option_Highlight ($82:EDED) chooses palette zero for
        // the first pair with AltText=0 and the second pair with AltText=1.
        // Check every word, not just the first cell, on both runtime paths.
        for (int language = 0; language < 2; language++)
        {
            bool japanese = language != 0;
            var menu = new GameOptionsMenuState(bus, japaneseText: japanese);
            ReadOnlySpan<byte> vram = menu.CaptureRenderSnapshot().Memory.Vram;
            byte[] editablePage = presentation.CreatePage(GameOptionsPresentationDefinitions.PrimaryPage);
            presentation.ApplyLanguage(editablePage, japanese);

            for (int regionIndex = 0; regionIndex < regions.Length; regionIndex++)
            {
                GameOptionsLanguagePaletteRegion region = regions[regionIndex];
                int expected = ((regionIndex < 2) != japanese)
                    ? GameOptionsRomData.TilePalettes.Selected
                    : GameOptionsRomData.TilePalettes.Unselected;
                for (int offset = region.ByteOffset; offset < region.ByteOffset + region.ByteCount; offset += 2)
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
