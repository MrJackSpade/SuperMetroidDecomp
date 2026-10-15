using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using System.Reflection;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using static SuperMetroid.Verification.Rooms.Doors.DoorDefinitionsVerification;
using static SuperMetroid.Verification.Harness.RetailPresentationFixtureVerification;
using SuperMetroid.Core.Frontend.FileSelect;
using SuperMetroid.Core.Frontend.Menus;
using SuperMetroid.Core.Saves;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Frontend.FileSelect;
using SuperMetroid.Verification.Harness;

/// <summary>Checks copy-confirmation arrow OAM and palette timing for all six file-slot pairs.</summary>
internal static class FileCopyArrowVerification
{
    /// <summary>Compares native arrow parts, visible pixels, and the eight-tick palette delay.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots")]
    internal static void VerifyFileCopyArrow()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var installation = RepositoryInstallation.Installation;
        var art = InstalledFixtures.Maps;
        // All SRAM changes belong to this private imported address space.
        var saves = new SuperMetroidSaveRam(rom, RetailPresentationFixture());
        for (int slot = 0; slot < 3; slot++)
            saves.SaveSlot(slot, new SuperMetroidSaveSnapshot { Health = 99, MaxHealth = 99 });
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
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
                return menu.oam;
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
                    FileCopyArrowRomByte(rom, address + 2), (SnesObjAttributeWord)ReadVerificationWord(rom, address + 3), new Position16<OamPixels>(originX, originY));
                AssertEqual(expected.GetEntry(part), actual.GetEntry(noArrowCount + part),
                    $"copy {pair.slots}: exact arrow position, tile, flip, palette and priority, part {part}");
            }
            var ppu = menu.ppu;
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
            Bgr555[] initial = ppu.Cgram.Colors.ToArray();
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
            Bgr555[] beforeRender = ppu.Cgram.Colors.ToArray();
            menu.Render(); menu.CaptureRenderSnapshot();
            AssertTrue(beforeRender.SequenceEqual(ppu.Cgram.Colors), "render leaves palette timing alone");
            Press(SnesButton.A);
            AssertEqual(FileSelectPhase.CopyCompleted, menu.Phase, "copy completes");
            AssertTrue(Capture().LastFinalizedSpriteCount < noArrowCount + count, "completed page removes Copy arrow");
        }
        Console.WriteLine("Copy arrow: all six native slot pairs match OAM; confirmation-only visibility and palette timing pass.");
    }

    /// <summary>Reads native arrow spritemap bytes for the independent OAM reference.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for file copy arrow")]
    private static byte FileCopyArrowRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>Runs the retail copy-confirmation arrow check.</summary>
    [VerificationCommand("--file-copy-arrow")]
    internal static int FileCopyArrowCommand()
    {
        Suite(nameof(VerifyFileCopyArrow), () => VerifyFileCopyArrow());
        return 0;
    }
}
