using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using static SuperMetroid.Verification.Rooms.Doors.DoorDefinitionsVerification;
using static SuperMetroid.Verification.Harness.RetailPresentationFixtureVerification;
using SuperMetroid.Core.Frontend.FileSelect;
using SuperMetroid.Core.Saves;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Frontend.FileSelect;
using SuperMetroid.Verification.Harness;

/// <summary>Checks copy-confirmation arrow OAM and palette timing for all six file-slot pairs.</summary>
internal static class FileCopyArrowVerification
{
    /// <summary>Compares native arrow parts and the eight-tick palette delay.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots")]
    internal static void VerifyFileCopyArrow()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var installation = RepositoryInstallation.Installation;
        var art = InstalledFixtures.Maps;
        // All save changes belong to this private store.
        var saves = new SuperMetroid.Core.Saves.SaveSlotStore(RetailPresentationFixture());
        for (int slot = 0; slot < 3; slot++)
            saves.SaveSlot(slot, new SuperMetroidSaveSnapshot { Health = 99, MaxHealth = 99 });
        foreach (var pair in new[] { new FileCopyArrowVerifyFileCopyArrowCase(0, 2), new FileCopyArrowVerifyFileCopyArrowCase(0, 1), new FileCopyArrowVerifyFileCopyArrowCase(1, 2), new FileCopyArrowVerifyFileCopyArrowCase(2, 0), new FileCopyArrowVerifyFileCopyArrowCase(1, 0), new FileCopyArrowVerifyFileCopyArrowCase(2, 1) }.Select((slots, row) => new VerifyFileCopyArrowCase(slots, row)))
        {
            saves.SelectSlot(0);
            var menu = new FileSelectMenuState(saves, mapPresentation: art, spriteSheets: MenuSheetFixtures.For(art));
            void Press(ControllerButton button) { menu.Step(0); menu.Step(button); }
            void Until(FileSelectPhase phase)
            {
                for (int frame = 0; frame < 60 && menu.Phase != phase; frame++) menu.Step(0);
                AssertEqual(phase, menu.Phase, "Copy arrow fixture reaches menu phase");
            }
            ScreenSprites Capture()
            {
                menu.CaptureRenderSnapshot();
                return menu.screen.Objects;
            }
            Until(FileSelectPhase.Main);
            for (int i = 0; i < 3; i++) Press(ControllerButton.Down);
            Press(ControllerButton.A);
            Until(FileSelectPhase.CopySelectSource);
            for (int i = 0; i < pair.Slots.First; i++) Press(ControllerButton.Down);
            Press(ControllerButton.A);
            int firstDestination = pair.Slots.First == 0 ? 1 : 0;
            if (pair.Slots.Second != firstDestination) Press(ControllerButton.Down);
            int noArrowCount = Capture().LastFinalizedSpriteCount;
            Press(ControllerButton.A);
            AssertEqual(FileSelectPhase.CopyConfirm, menu.Phase, "arrow appears at confirmation");
            int record = 0x82bb0c + pair.Row * 6;
            ushort id = ReadVerificationWord(rom, record);
            ushort originX = ReadVerificationWord(rom, record + 2);
            ushort originY = ReadVerificationWord(rom, record + 4);
            int spritemap = 0x820000 | ReadVerificationWord(rom, 0x82c569 + id * 2);
            int count = ReadVerificationWord(rom, spritemap);
            ScreenSprites actual = Capture();
            AssertEqual(noArrowCount + count, actual.LastFinalizedSpriteCount, "confirmation adds native Copy arrow sprite parts");
            var expected = new OamBuffer();
            expected.BeginFrame();
            for (int part = 0; part < count; part++)
            {
                int address = spritemap + 2 + part * 5;
                expected.AddOnScreenSpritePart(new SnesSpritemapXWord(ReadVerificationWord(rom, address)),
                    new SnesSpritemapYByte(FileCopyArrowRomByte(rom, address + 2)), (SnesObjAttributeWord)ReadVerificationWord(rom, address + 3), new Position16<SpritePixels>(originX, originY));
                AssertEqual(expected.GetEntry(part), actual.GetEntry(noArrowCount + part),
                    $"copy {pair.Slots}: exact arrow position, tile, flip, palette and priority, part {part}");
            }
            expected.FinalizeFrame();
            RgbColor[] initial = menu.screen.Palette.Colors.ToArray();
            for (int frame = 1; frame <= 12; frame++)
            {
                menu.Step(0);
                int rotations = frame < 8 ? 0 : 1 + (frame - 8) / 4;
                for (int color = 0; color < 256; color++)
                {
                    int source = color is >= 145 and <= 151 ? 145 + (color - 145 + rotations) % 7 : color;
                    AssertEqual(initial[source], menu.screen.Palette.Colors[color], "arrow palette rotates only colors 1..7 after eight ticks then every four");
                }
            }
            // Rendering must not advance animation.
            RgbColor[] beforeRender = menu.screen.Palette.Colors.ToArray();
            menu.Render(); menu.CaptureRenderSnapshot();
            AssertTrue(beforeRender.SequenceEqual(menu.screen.Palette.Colors), "render leaves palette timing alone");
            Press(ControllerButton.A);
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
