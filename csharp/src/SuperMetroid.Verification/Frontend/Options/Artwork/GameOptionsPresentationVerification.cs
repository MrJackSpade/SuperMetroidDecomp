using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Tooling;
using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Input;
using static SuperMetroid.Verification.Frontend.Options.GameOptionsCursorPhasesVerification;
using SuperMetroid.Core.Frontend.Options;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Frontend.Options.Artwork;
using SuperMetroid.Core.Map.Artwork;
using SuperMetroid.Core.Rendering;
using SuperMetroid.AssetExtraction.Frontend.Map;

namespace SuperMetroid.Verification.Frontend.Options.Artwork;

/// <summary>Checks installed game-options art against cartridge rendering and editable override behavior.</summary>
internal static class GameOptionsPresentationVerification
{
    /// <summary>Compares menu frames, audio, and controller changes, then checks edited anchors and state-safe rebinding.</summary>
    internal static void VerifyGameOptionsPresentationAssets(
        ISnesAddressSpace bus,
        string stock,
        string overrides,
        AreaMapPresentationCatalog original)
    {
        string stockPath = Path.Combine(stock, GameOptionsPresentationDefinitions.FileName);
        byte[] extracted = SuperMetroid.AssetExtraction.Frontend.GameOptionsPresentationExtractor.Extract(bus);
        AssertTrue(extracted.SequenceEqual(File.ReadAllBytes(stockPath)),
            "installed options JSON is the deterministic cartridge extraction");
        _ = GameOptionsPresentation.Load(new MemoryStream(extracted));

        var nativeAudio = new AudioQueues();
        var installedAudio = new AudioQueues();
        var native = new GameOptionsMenuState(nativeAudio,
            mapPresentation: original, spriteSheets: MenuSheetFixtures.For(original));
        var installed = new GameOptionsMenuState(
            installedAudio, mapPresentation: original, spriteSheets: MenuSheetFixtures.For(original));
        int comparedFrames = 0;

        Until(GameOptionsPhase.Main);
        Press(SnesButton.Down);
        Press(SnesButton.A); // Switch language and rewrite the primary highlights.
        for (int row = 0; row < 3; row++)
            Press(SnesButton.Down);
        Press(SnesButton.A);
        Until(GameOptionsPhase.ControllerSettings);
        Press(SnesButton.R); // Reassign Shoot and swap the displaced action label.
        for (int row = 0; row < 7; row++)
        {
            Press(SnesButton.Down);
            Until(GameOptionsPhase.ControllerSettings);
        }
        Press(SnesButton.A);
        Until(GameOptionsPhase.Main);
        for (int row = 0; row < 4; row++)
            Press(SnesButton.Down);
        Press(SnesButton.A);
        Until(GameOptionsPhase.SpecialSettings);
        Press(SnesButton.A);
        Press(SnesButton.Down);
        Press(SnesButton.Right);
        Press(SnesButton.Down);
        Press(SnesButton.A);
        Until(GameOptionsPhase.Main);

        AssertTrue(native.JapaneseText && installed.JapaneseText,
            "installed options path retains language behavior");
        AssertEqual(native.ControllerBindings, installed.ControllerBindings,
            "installed options path retains controller swaps");
        AssertTrue(native.IconCancelEnabled && installed.IconCancelEnabled &&
            native.MoonwalkEnabled && installed.MoonwalkEnabled,
            "installed options path retains special toggles");

        var document = RequiredJson.Deserialize<GameOptionsPresentationDocument>(
            extracted, ArtworkJson.Options);
        document.HeadingAnchors[GameOptionsPresentationDefinitions.MenuName(GameOptionsPage.Primary)] =
            document.HeadingAnchors[GameOptionsPresentationDefinitions.MenuName(GameOptionsPage.Primary)] with
            { X = (ushort)(document.HeadingAnchors[GameOptionsPresentationDefinitions.MenuName(GameOptionsPage.Primary)].X + 8) };
        document.CursorAnchors[GameOptionsPresentationDefinitions.MenuName(GameOptionsPage.Primary)][0] =
            document.CursorAnchors[GameOptionsPresentationDefinitions.MenuName(GameOptionsPage.Primary)][0] with
            { X = (ushort)(document.CursorAnchors[GameOptionsPresentationDefinitions.MenuName(GameOptionsPage.Primary)][0].X + 8) };
        MapPresentationCell pageCell = document.Pages[
            GameOptionsPresentationDefinitions.TilemapName(GameOptionsTilemap.Primary)][0x144];
        document.Pages[GameOptionsPresentationDefinitions.TilemapName(GameOptionsTilemap.Primary)][0x144] =
            pageCell with { FlipX = !pageCell.FlipX };
        SpriteVisualPart cursorPart = document.Sprites[
            GameOptionsPresentationDefinitions.CursorSpriteName(0)][0];
        document.Sprites[GameOptionsPresentationDefinitions.CursorSpriteName(0)][0] =
            cursorPart with { FlipY = !cursorPart.FlipY };
        Directory.CreateDirectory(overrides);
        string replacement = Path.Combine(overrides,
            GameOptionsPresentationDefinitions.FileName);
        using (var output = File.Create(replacement))
            GameOptionsPresentation.Write(output, document);
        AreaMapPresentationCatalog edited = MapPresentationFiles.Load(stock, overrides);
        AssertTrue(edited.ContentIdentity != original.ContentIdentity,
            "options override changes selected catalog identity");

        var stockMenu = new GameOptionsMenuState(mapPresentation: original, spriteSheets: MenuSheetFixtures.For(original));
        var editedMenu = new GameOptionsMenuState(mapPresentation: edited, spriteSheets: MenuSheetFixtures.For(edited));
        // Native indexes zero and one, fifteen fade-in levels, then the interactive index.
        for (int frame = 0; frame < 18; frame++)
        {
            stockMenu.Step(0);
            editedMenu.Step(0);
        }
        AssertTrue(!stockMenu.Render().SequenceEqual(editedMenu.Render()),
            "options page, actor and anchor edits reach the real renderer");

        GameOptionsPhase phaseBeforeRebind = stockMenu.Phase;
        int selectedBeforeRebind = stockMenu.SelectedItem;
        int cursorXBeforeRebind = stockMenu.CaptureRenderSnapshot().Paint.SpriteParts()[0].Part.Position.X;
        stockMenu.BindFixtureArtwork(edited);
        AssertEqual(phaseBeforeRebind, stockMenu.Phase,
            "options content rebind preserves menu phase");
        AssertEqual(selectedBeforeRebind, stockMenu.SelectedItem,
            "options content rebind preserves selected row");
        AssertEqual(cursorXBeforeRebind + 8,
            (int)stockMenu.CaptureRenderSnapshot().Paint.SpriteParts()[0].Part.Position.X,
            "options rebind applies edited cursor anchor without resetting state");

        stockMenu.Step(SnesButton.A);
        AssertEqual(GameOptionsPhase.FadeOutToIntro, stockMenu.Phase,
            "editable options enters intro fade-out");
        AssertTrue(LastSpriteX(stockMenu.CaptureRenderSnapshot()) >= 256,
            "editable options hides cursor during intro fade-out");

        File.WriteAllText(replacement, "{ broken options JSON");
        AssertThrows<InvalidDataException>(() => MapPresentationFiles.Load(stock, overrides),
            "corrupt options override fails loudly");
        using (var output = File.Create(replacement))
            GameOptionsPresentation.Write(output, document);
        AssertThrows<InvalidDataException>(() => GameOptionsPresentation.Write(Stream.Null,
            document with { CursorPalette = 8 }),
            "options reject invalid cursor palette");
        AssertThrows<InvalidDataException>(() => GameOptionsPresentation.Write(Stream.Null,
            document with
            {
                ControllerLabelAnchors = document.ControllerLabelAnchors[..^1],
            }), "options reject incomplete controller-label anchors");
        var missingPage = RequiredJson.Deserialize<GameOptionsPresentationDocument>(
            extracted, ArtworkJson.Options);
        missingPage.Pages.Remove(GameOptionsPresentationDefinitions.TilemapName(GameOptionsTilemap.SpecialJapanese));
        AssertThrows<InvalidDataException>(() => GameOptionsPresentation.Write(Stream.Null, missingPage),
            "options reject a missing named page");
        var overlappingToggle = RequiredJson.Deserialize<GameOptionsPresentationDocument>(
            extracted, ArtworkJson.Options);
        GameOptionsToggleVisualDocument toggle = overlappingToggle.SpecialToggles[
            GameOptionsPresentationDefinitions.ToggleName(GameOptionsToggle.IconCancel)];
        overlappingToggle.SpecialToggles[GameOptionsPresentationDefinitions.ToggleName(GameOptionsToggle.IconCancel)] =
            toggle with { DisabledCells = [toggle.EnabledCells[0], .. toggle.DisabledCells] };
        AssertThrows<InvalidDataException>(() =>
                GameOptionsPresentation.Write(Stream.Null, overlappingToggle),
            "options reject overlapping toggle highlight cells");

        Console.WriteLine(
            $"Options presentation: {comparedFrames} exact ROM-free frames, dynamic labels, " +
            "sounds, visible edits, state-safe rebind and strict failures pass.");

        void Until(GameOptionsPhase expected)
        {
            for (int frame = 0; frame < 120 && native.Phase != expected; frame++)
                Frame(0);
            AssertEqual(expected, native.Phase, $"native options reaches {expected}");
            AssertEqual(expected, installed.Phase, $"installed options reaches {expected}");
        }

        void Press(SnesButton button)
        {
            Frame(button);
            Frame(0);
        }

        void Frame(SnesButton input)
        {
            AssertEqual(native.Phase, installed.Phase,
                $"installed options phase matches cartridge frame {comparedFrames}");
            AssertEqual(native.SelectedItem, installed.SelectedItem,
                $"installed options selection matches cartridge frame {comparedFrames}");
            if (native.Phase is GameOptionsPhase.DissolveOut or GameOptionsPhase.DissolveIn)
            {
                AssertTrue(LastSpriteX(native.CaptureRenderSnapshot()) >= 256,
                    $"ROM options hides cursor in {native.Phase}");
                AssertTrue(LastSpriteX(installed.CaptureRenderSnapshot()) >= 256,
                    $"editable options hides cursor in {installed.Phase}");
            }
            AssertTrue(native.Render().SequenceEqual(installed.Render()),
                $"installed options frame {comparedFrames} matches cartridge pixels");
            native.Step(input);
            installed.Step(input);
            IReadOnlyList<AudioRequest> nativeCommands =
                nativeAudio.AdvanceFrame(default);
            IReadOnlyList<AudioRequest> installedCommands =
                installedAudio.AdvanceFrame(default);
            AssertTrue(nativeCommands.SequenceEqual(installedCommands),
                $"installed options audio matches cartridge frame {comparedFrames}");
            comparedFrames++;
        }
    }
}
