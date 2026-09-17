using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;

internal static partial class Program
{
    private static void VerifyFileSelectPresentationAssets(
        ISnesAddressSpace bus,
        string stock,
        string overrides,
        AreaMapPresentationCatalog original)
    {
        string stockPath = Path.Combine(stock, FileSelectPresentationDefinitions.FileName);
        byte[] extracted = SuperMetroid.AssetExtraction.FileSelectPresentationExtractor.Extract(bus);
        AssertTrue(extracted.AsSpan().SequenceEqual(File.ReadAllBytes(stockPath)),
            "installed file-select JSON is the deterministic cartridge extraction");
        _ = FileSelectPresentation.Load(new MemoryStream(extracted));

        var saveRam = new SuperMetroidSaveRam(bus);
        saveRam.SaveSlot(0, new SuperMetroidSaveSnapshot
        {
            Health = 45,
            MaxHealth = 99,
            GameTimeHours = 12,
            GameTimeMinutes = 34,
        });
        saveRam.SelectSlot(0);

        var nativeAudio = new CartridgeAudioState();
        var installedAudio = new CartridgeAudioState();
        var native = new FileSelectMenuState(bus, nativeAudio);
        var installed = new FileSelectMenuState(
            new FileSelectPresentationGuard(bus), installedAudio, original);
        int comparedFrames = 0;
        Until(FileSelectPhase.Main);
        for (int row = 0; row < 3; row++) Press(SnesButton.Down);
        Press(SnesButton.A);
        Until(FileSelectPhase.CopySelectSource);
        Press(SnesButton.A);
        AssertEqual(FileSelectPhase.CopySelectDestination, native.Phase,
            "copy source advances to destination");
        Press(SnesButton.A);
        AssertEqual(FileSelectPhase.CopyConfirm, native.Phase,
            "copy destination advances to confirmation");
        Press(SnesButton.A);
        AssertEqual(FileSelectPhase.CopyCompleted, native.Phase,
            "copy confirmation reaches completion");
        Press(SnesButton.A);
        Until(FileSelectPhase.Main);
        for (int row = 0; row < 4; row++) Press(SnesButton.Down);
        Press(SnesButton.A);
        Until(FileSelectPhase.ClearSelectSlot);
        Press(SnesButton.A);
        AssertEqual(FileSelectPhase.ClearConfirm, native.Phase,
            "clear selection advances to confirmation");
        Press(SnesButton.A);
        AssertEqual(FileSelectPhase.ClearCompleted, native.Phase,
            "clear confirmation reaches completion");
        Press(SnesButton.A);
        Until(FileSelectPhase.Main);
        Press(SnesButton.Down);
        Press(SnesButton.A);
        Until(FileSelectPhase.FadeOutToOptions, maximumFrames: 80);
        for (int frame = 0; frame < 16; frame++) Frame(0);
        AssertEqual(native.SelectedHelmetFrame, installed.SelectedHelmetFrame,
            "installed file-select helmet animation retains native frame");

        var document = JsonSerializer.Deserialize<FileSelectPresentationDocument>(
            extracted, MapPresentationFormat.JsonOptions)!;
        document.MainCursorAnchors[0] = document.MainCursorAnchors[0] with
            { X = document.MainCursorAnchors[0].X + 8 };
        document.HelmetAnchors[0] = document.HelmetAnchors[0] with
            { X = document.HelmetAnchors[0].X + 8 };
        MapPresentationCell pageCell = document.Pages[
            FileSelectPresentationDefinitions.MainWithDataPage][0x2b];
        document.Pages[FileSelectPresentationDefinitions.MainWithDataPage][0x2b] =
            pageCell with { FlipX = !pageCell.FlipX };
        FileSelectPatchCellDocument energyCell = document.Patches[
            FileSelectPresentationDefinitions.EnergyPatch].Cells[0];
        document.Patches[FileSelectPresentationDefinitions.EnergyPatch].Cells[0] =
            energyCell with { Cell = energyCell.Cell with { FlipY = !energyCell.Cell.FlipY } };
        Directory.CreateDirectory(overrides);
        string replacement = Path.Combine(overrides,
            FileSelectPresentationDefinitions.FileName);
        using (var output = File.Create(replacement))
            FileSelectPresentation.Write(output, document);
        AreaMapPresentationCatalog edited = AreaMapPresentationCatalog.Load(stock, overrides);
        AssertTrue(edited.ContentIdentity != original.ContentIdentity,
            "file-select override changes selected catalog identity");

        var stockMenu = new FileSelectMenuState(
            new FileSelectPresentationGuard(bus), mapPresentation: original);
        var editedMenu = new FileSelectMenuState(
            new FileSelectPresentationGuard(bus), mapPresentation: edited);
        for (int frame = 0; frame < 16; frame++)
        {
            stockMenu.Step(0);
            editedMenu.Step(0);
        }
        AssertTrue(!stockMenu.Render().AsSpan().SequenceEqual(editedMenu.Render()),
            "file-select page, field and actor edits reach the real renderer");
        FileSelectPhase phaseBeforeRebind = stockMenu.Phase;
        int selectionBeforeRebind = stockMenu.SelectedItem;
        Rgba32[] pixelsBeforeRebind = stockMenu.Render();
        stockMenu.BindMapPresentation(edited);
        AssertEqual(phaseBeforeRebind, stockMenu.Phase,
            "file-select content rebind preserves phase");
        AssertEqual(selectionBeforeRebind, stockMenu.SelectedItem,
            "file-select content rebind preserves selection");
        AssertTrue(!pixelsBeforeRebind.AsSpan().SequenceEqual(stockMenu.Render()),
            "file-select rebind applies edited content without resetting state");

        File.WriteAllText(replacement, "{ broken file-select JSON");
        AssertThrows<InvalidDataException>(() => AreaMapPresentationCatalog.Load(stock, overrides),
            "corrupt file-select override fails loudly");
        using (var output = File.Create(replacement))
            FileSelectPresentation.Write(output, document);
        AssertThrows<InvalidDataException>(() => FileSelectPresentation.Write(Stream.Null,
            document with { ObjectPalette = 8 }),
            "file-select rejects invalid object palette");
        var missingPage = JsonSerializer.Deserialize<FileSelectPresentationDocument>(
            extracted, MapPresentationFormat.JsonOptions)!;
        missingPage.Pages.Remove(FileSelectPresentationDefinitions.CopyCompletedPage);
        AssertThrows<InvalidDataException>(() => FileSelectPresentation.Write(Stream.Null, missingPage),
            "file-select rejects a missing named page");
        var duplicatePatch = JsonSerializer.Deserialize<FileSelectPresentationDocument>(
            extracted, MapPresentationFormat.JsonOptions)!;
        FileSelectPatchCellDocument duplicate = duplicatePatch.Patches[
            FileSelectPresentationDefinitions.NoDataPatch].Cells[0];
        duplicatePatch.Patches[FileSelectPresentationDefinitions.NoDataPatch] = new()
        {
            Cells = [duplicate, duplicate],
        };
        AssertThrows<InvalidDataException>(() =>
                FileSelectPresentation.Write(Stream.Null, duplicatePatch),
            "file-select rejects duplicate patch coordinates");

        Console.WriteLine(
            $"File-select presentation: {comparedFrames} exact ROM-free frames through " +
            "COPY, CLEAR, helmets, audio and fades; edits, rebind and strict failures pass.");

        void Until(FileSelectPhase expected, int maximumFrames = 100)
        {
            for (int frame = 0; frame < maximumFrames && native.Phase != expected; frame++)
                Frame(0);
            AssertEqual(expected, native.Phase, $"native file select reaches {expected}");
            AssertEqual(expected, installed.Phase, $"installed file select reaches {expected}");
        }

        void Press(SnesButton button)
        {
            Frame((ushort)button);
            Frame(0);
        }

        void Frame(ushort input)
        {
            AssertEqual(native.Phase, installed.Phase,
                $"installed file-select phase matches frame {comparedFrames}");
            AssertEqual(native.SelectedItem, installed.SelectedItem,
                $"installed file-select selection matches frame {comparedFrames}");
            AssertTrue(native.BackgroundTilemap.SequenceEqual(installed.BackgroundTilemap),
                $"installed file-select tilemap matches frame {comparedFrames}");
            AssertTrue(native.Render().AsSpan().SequenceEqual(installed.Render()),
                $"installed file-select pixels match frame {comparedFrames}");
            native.Step(input);
            installed.Step(input);
            IReadOnlyList<CartridgeAudioCommand> nativeCommands =
                nativeAudio.AdvanceFrame(bus, default);
            IReadOnlyList<CartridgeAudioCommand> installedCommands =
                installedAudio.AdvanceFrame(bus, default);
            AssertTrue(nativeCommands.SequenceEqual(installedCommands),
                $"installed file-select audio matches frame {comparedFrames}");
            comparedFrames++;
        }
    }

    private sealed class FileSelectPresentationGuard(ISnesAddressSpace source) : ISnesAddressSpace
    {
        public byte ReadByte(int address)
        {
            if (IsPresentationAddress(address))
                throw new InvalidOperationException(
                    $"Installed file select read presentation ROM at ${address:X6}.");
            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);

        private static bool IsPresentationAddress(int address) =>
            In(address, FileSelectMapRomData.InitialMenuBackground,
                FileSelectMapRomData.TilemapBytes) ||
            In(address, FileSelectMapRomData.EntryPalette, SnesCgram.ByteCount) ||
            In(address, MapTileAtlasFormat.SourceAddress, MapTileAtlasFormat.ByteCount) ||
            In(address, WorldMapArtworkFormat.ForegroundSource,
                WorldMapArtworkFormat.ForegroundBytes) ||
            In(address, WorldMapArtworkFormat.BackgroundSource,
                WorldMapArtworkFormat.BackgroundBytes) ||
            In(address, MapSpriteFormat.SourceAddress, MapSpriteFormat.ByteCount) ||
            address is >= 0x81b40a and < 0x81b800 ||
            In(address,
                MenuPpuState.SpritemapPointerTableAddress + 0x2c * sizeof(ushort),
                (0x4d - 0x2c + 1) * sizeof(ushort));

        private static bool In(int address, int start, int count) =>
            address >= start && address < start + count;
    }
}
