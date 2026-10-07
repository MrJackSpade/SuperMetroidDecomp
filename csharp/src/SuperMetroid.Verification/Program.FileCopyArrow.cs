using System.Reflection;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Desktop;

internal static partial class Program
{
    private static void VerifyFileCopyArrow()
    {
        var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var installation = runtimeFixtureInstallation.Value;
        var art = installation.LoadMaps();
        // All SRAM changes belong to this private imported address space.
        var saves = new SuperMetroidSaveRam(rom, RetailPresentationFixture());
        for (int slot = 0; slot < 3; slot++)
            saves.SaveSlot(slot, new SuperMetroidSaveSnapshot { Health = 99, MaxHealth = 99 });
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var fields = (FieldInfo[])typeof(DebuggerObjectGraphSerializer).GetMethod("GetSerializableFields",
            BindingFlags.NonPublic | BindingFlags.Static)!.Invoke(null, [typeof(FileSelectMenuState)])!;
        foreach (int missing in new[] { 1, 2 })
        {
            FieldInfo[] legacy = DebuggerStateFieldMigrations.SelectSerializedFields(typeof(FileSelectMenuState), fields, fields.Length - missing);
            AssertTrue(!legacy.Any(field => field.Name == "copyArrowPaletteTimer"), "legacy state omits new arrow timer");
            AssertEqual(missing == 1, legacy.Any(field => field.Name == "currentPresentationPage"), "older presentation migration remains compatible");
        }
        foreach (var pair in new[] { (0, 2), (0, 1), (1, 2), (2, 0), (1, 0), (2, 1) }.Select((slots, row) => (slots, row)))
        {
            saves.SelectSlot(0);
            var menu = new FileSelectMenuState(rom, mapPresentation: art);
            void Press(SnesButton button) { menu.Step(0); menu.Step((ushort)button); }
            void Until(FileSelectPhase phase)
            {
                for (int frame = 0; frame < 60 && menu.Phase != phase; frame++) menu.Step(0);
                AssertEqual(phase, menu.Phase, "Copy arrow fixture reaches menu phase");
            }
            OamBuffer Capture()
            {
                menu.CaptureRenderSnapshot();
                return (OamBuffer)typeof(FileSelectMenuState).GetField("oam", flags)!.GetValue(menu)!;
            }
            Until(FileSelectPhase.Main);
            for (int i = 0; i < 3; i++) Press(SnesButton.Down);
            Press(SnesButton.A);
            Until(FileSelectPhase.CopySelectSource);
            for (int i = 0; i < pair.slots.Item1; i++) Press(SnesButton.Down);
            Press(SnesButton.A);
            int firstDestination = pair.slots.Item1 == 0 ? 1 : 0;
            if (pair.slots.Item2 != firstDestination) Press(SnesButton.Down);
            int noArrowCount = Capture().LastFinalizedSpriteCount;
            Press(SnesButton.A);
            AssertEqual(FileSelectPhase.CopyConfirm, menu.Phase, "arrow appears at confirmation");
            int record = 0x82bb0c + pair.row * 6;
            ushort id = ReadVerificationWord(rom, record);
            ushort originX = ReadVerificationWord(rom, record + 2);
            ushort originY = ReadVerificationWord(rom, record + 4);
            int spritemap = 0x820000 | ReadVerificationWord(rom, 0x82c569 + id * 2);
            int count = ReadVerificationWord(rom, spritemap);
            OamBuffer actual = Capture();
            AssertEqual(noArrowCount + count, actual.LastFinalizedSpriteCount, "confirmation adds native Copy arrow sprite parts");
            var expected = new OamBuffer();
            expected.BeginFrame();
            for (int part = 0; part < count; part++)
            {
                int address = spritemap + 2 + part * 5;
                expected.AddOnScreenSpritePart((SnesSpritemapXWord)ReadVerificationWord(rom, address),
                    rom.ReadByte(address + 2), (SnesObjAttributeWord)ReadVerificationWord(rom, address + 3), originX, originY);
                AssertEqual(expected.GetEntry(part), actual.GetEntry(noArrowCount + part),
                    $"copy {pair.slots}: exact arrow position, tile, flip, palette and priority, part {part}");
            }
            var ppu = (MenuPpuState)typeof(FileSelectMenuState).GetField("ppu", flags)!.GetValue(menu)!;
            expected.FinalizeFrame();
            Rgba32[] arrowPixels = SnesObjRenderer.Render(expected, ppu.Vram, ppu.Cgram, obsel: MenuRenderDefinitions.ObjectSelection);
            Rgba32[] menuPixels = menu.Render();
            int visiblePixels = 0;
            for (int pixel = 0; pixel < arrowPixels.Length; pixel++)
                if (arrowPixels[pixel].A != 0)
                {
                    visiblePixels++;
                    AssertEqual(arrowPixels[pixel], menuPixels[pixel], "native arrow artwork is visible in the final menu pixels");
                }
            AssertTrue(visiblePixels > 0, "Copy arrow has nontransparent installed artwork");
            ushort[] initial = ppu.Cgram.Colors.ToArray();
            for (int frame = 1; frame <= 12; frame++)
            {
                menu.Step(0);
                int rotations = frame < 8 ? 0 : 1 + (frame - 8) / 4;
                for (int color = 0; color < 256; color++)
                {
                    int source = color is >= 145 and <= 151 ? 145 + (color - 145 + rotations) % 7 : color;
                    AssertEqual(initial[source], ppu.Cgram.Colors[color], "arrow palette rotates only colors 1..7 after eight ticks then every four");
                }
            }
            // Rendering must not advance animation.
            ushort[] beforeRender = ppu.Cgram.Colors.ToArray();
            menu.Render(); menu.CaptureRenderSnapshot();
            AssertTrue(beforeRender.AsSpan().SequenceEqual(ppu.Cgram.Colors), "render leaves palette timing alone");
            Press(SnesButton.A);
            AssertEqual(FileSelectPhase.CopyCompleted, menu.Phase, "copy completes");
            AssertTrue(Capture().LastFinalizedSpriteCount < noArrowCount + count, "completed page removes Copy arrow");
            typeof(FileSelectMenuState).GetField("copyArrowPaletteTimer", flags)!.SetValue(menu, 0);
            DebuggerStateFieldMigrations.InitializeMissingFields(menu, fields.Length - 1);
            AssertEqual(8, (int)typeof(FileSelectMenuState).GetField("copyArrowPaletteTimer", flags)!.GetValue(menu)!,
                "old debugger states initialize the arrow timer");
        }
        Console.WriteLine("Copy arrow: all six native slot pairs match OAM; confirmation-only visibility and palette timing pass.");
    }
}
