using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Tooling;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using static SuperMetroid.Verification.Harness.RetailPresentationFixtureVerification;
using SuperMetroid.Core.Frontend.FileSelect;
using SuperMetroid.Core.Saves;
using SuperMetroid.Core.Frontend.FileSelect.Artwork;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Map.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Map;

namespace SuperMetroid.Verification.Frontend.FileSelect.Artwork;

/// <summary>Checks extracted file-select presentation parity, editable overrides, and strict document validation.</summary>
internal static class FileSelectPresentationVerification
{
    /// <summary>Compares native and installed COPY/CLEAR flows frame by frame, then verifies live art replacement and malformed-input failures.</summary>
    internal static void VerifyFileSelectPresentationAssets(
        ISnesAddressSpace bus,
        string stock,
        string overrides,
        AreaMapPresentationCatalog original)
    {
        string stockPath = Path.Combine(stock, FileSelectPresentationDefinitions.FileName);
        byte[] extracted = SuperMetroid.AssetExtraction.Frontend.FileSelectPresentationExtractor.Extract(bus);
        AssertTrue(extracted.SequenceEqual(File.ReadAllBytes(stockPath)),
            "installed file-select JSON is the deterministic cartridge extraction");
        _ = FileSelectPresentation.Load(new MemoryStream(extracted));

        var saveRam = new SuperMetroid.Core.Saves.SaveSlotStore(RetailPresentationFixture());
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
        var native = new FileSelectMenuState(saveRam, nativeAudio, original, MenuSheetFixtures.For(original));
        var installed = new FileSelectMenuState(saveRam, installedAudio, original, MenuSheetFixtures.For(original));
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

        var document = RequiredJson.Deserialize<FileSelectPresentationDocument>(
            extracted, ArtworkJson.Options);
        document.MainCursorAnchors[0] = document.MainCursorAnchors[0] with
            { X = (ushort)(document.MainCursorAnchors[0].X + 8) };
        document.HelmetAnchors[0] = document.HelmetAnchors[0] with
            { X = (ushort)(document.HelmetAnchors[0].X + 8) };
        MapPresentationCell pageCell = document.Pages[
            FileSelectPresentationDefinitions.PageName(FileSelectPage.MainWithData)][0x2b];
        document.Pages[FileSelectPresentationDefinitions.PageName(FileSelectPage.MainWithData)][0x2b] =
            pageCell with { FlipX = !pageCell.FlipX };
        FileSelectPatchCellDocument energyCell = document.Patches[
            FileSelectPresentationDefinitions.PatchName(FileSelectPatch.Energy)].Cells[0];
        document.Patches[FileSelectPresentationDefinitions.PatchName(FileSelectPatch.Energy)].Cells[0] =
            energyCell with { Cell = energyCell.Cell with { FlipY = !energyCell.Cell.FlipY } };
        Directory.CreateDirectory(overrides);
        string replacement = Path.Combine(overrides,
            FileSelectPresentationDefinitions.FileName);
        using (var output = File.Create(replacement))
            FileSelectPresentation.Write(output, document);
        AreaMapPresentationCatalog edited = MapPresentationFiles.Load(stock, overrides);
        AssertTrue(edited.ContentIdentity != original.ContentIdentity,
            "file-select override changes selected catalog identity");

        var stockMenu = new FileSelectMenuState(saveRam, mapPresentation: original, spriteSheets: MenuSheetFixtures.For(original));
        var editedMenu = new FileSelectMenuState(saveRam, mapPresentation: edited, spriteSheets: MenuSheetFixtures.For(edited));
        for (int frame = 0; frame < 16; frame++)
        {
            stockMenu.Step(0);
            editedMenu.Step(0);
        }
        AssertTrue(!stockMenu.Render().SequenceEqual(editedMenu.Render()),
            "file-select page, field and actor edits reach the real renderer");
        FileSelectPhase phaseBeforeRebind = stockMenu.Phase;
        FileSelectItem selectionBeforeRebind = stockMenu.SelectedItem;
        Rgba32[] pixelsBeforeRebind = stockMenu.Render().ToArray();
        stockMenu.BindFixtureArtwork(edited);
        AssertEqual(phaseBeforeRebind, stockMenu.Phase,
            "file-select content rebind preserves phase");
        AssertEqual(selectionBeforeRebind, stockMenu.SelectedItem,
            "file-select content rebind preserves selection");
        AssertTrue(!pixelsBeforeRebind.SequenceEqual(stockMenu.Render()),
            "file-select rebind applies edited content without resetting state");

        File.WriteAllText(replacement, "{ broken file-select JSON");
        AssertThrows<InvalidDataException>(() => MapPresentationFiles.Load(stock, overrides),
            "corrupt file-select override fails loudly");
        using (var output = File.Create(replacement))
            FileSelectPresentation.Write(output, document);
        AssertThrows<InvalidDataException>(() => FileSelectPresentation.Write(Stream.Null,
            document with { ObjectPalette = 8 }),
            "file-select rejects invalid object palette");
        var missingPage = RequiredJson.Deserialize<FileSelectPresentationDocument>(
            extracted, ArtworkJson.Options);
        missingPage.Pages.Remove(FileSelectPresentationDefinitions.PageName(FileSelectPage.CopyCompleted));
        AssertThrows<InvalidDataException>(() => FileSelectPresentation.Write(Stream.Null, missingPage),
            "file-select rejects a missing named page");
        var duplicatePatch = RequiredJson.Deserialize<FileSelectPresentationDocument>(
            extracted, ArtworkJson.Options);
        FileSelectPatchCellDocument duplicate = duplicatePatch.Patches[
            FileSelectPresentationDefinitions.PatchName(FileSelectPatch.NoData)].Cells[0];
        duplicatePatch.Patches[FileSelectPresentationDefinitions.PatchName(FileSelectPatch.NoData)] = new()
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
            Frame(button);
            Frame(0);
        }

        void Frame(SnesButton input)
        {
            AssertEqual(native.Phase, installed.Phase,
                $"installed file-select phase matches frame {comparedFrames}");
            AssertEqual(native.SelectedItem, installed.SelectedItem,
                $"installed file-select selection matches frame {comparedFrames}");
            AssertTrue(native.BackgroundTilemap.SequenceEqual(installed.BackgroundTilemap),
                $"installed file-select tilemap matches frame {comparedFrames}");
            AssertTrue(native.Render().SequenceEqual(installed.Render()),
                $"installed file-select pixels match frame {comparedFrames}");
            native.Step(input);
            installed.Step(input);
            IReadOnlyList<AudioRequest> nativeCommands =
                nativeAudio.AdvanceFrame(default);
            IReadOnlyList<AudioRequest> installedCommands =
                installedAudio.AdvanceFrame(default);
            AssertTrue(nativeCommands.SequenceEqual(installedCommands),
                $"installed file-select audio matches frame {comparedFrames}");
            comparedFrames++;
        }
    }


}
