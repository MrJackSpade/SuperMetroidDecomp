using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;

internal static partial class Program
{
    private static void VerifyGameOptionsPresentationAssets(
        ISnesAddressSpace bus,
        string stock,
        string overrides,
        AreaMapPresentationCatalog original)
    {
        string stockPath = Path.Combine(stock, GameOptionsPresentationDefinitions.FileName);
        byte[] extracted = SuperMetroid.AssetExtraction.GameOptionsPresentationExtractor.Extract(bus);
        AssertTrue(extracted.AsSpan().SequenceEqual(File.ReadAllBytes(stockPath)),
            "installed options JSON is the deterministic cartridge extraction");
        _ = GameOptionsPresentation.Load(new MemoryStream(extracted));

        var nativeAudio = new CartridgeAudioState();
        var installedAudio = new CartridgeAudioState();
        var native = new GameOptionsMenuState(bus, nativeAudio);
        var installed = new GameOptionsMenuState(
            new ForbiddenMapBus(), installedAudio, mapPresentation: original);
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

        var document = JsonSerializer.Deserialize<GameOptionsPresentationDocument>(
            extracted, MapPresentationFormat.JsonOptions)!;
        document.HeadingAnchors[GameOptionsPresentationDefinitions.PrimaryMenu] =
            document.HeadingAnchors[GameOptionsPresentationDefinitions.PrimaryMenu] with
            { X = document.HeadingAnchors[GameOptionsPresentationDefinitions.PrimaryMenu].X + 8 };
        document.CursorAnchors[GameOptionsPresentationDefinitions.PrimaryMenu][0] =
            document.CursorAnchors[GameOptionsPresentationDefinitions.PrimaryMenu][0] with
            { X = document.CursorAnchors[GameOptionsPresentationDefinitions.PrimaryMenu][0].X + 8 };
        MapPresentationCell pageCell = document.Pages[
            GameOptionsPresentationDefinitions.PrimaryPage][0x144];
        document.Pages[GameOptionsPresentationDefinitions.PrimaryPage][0x144] =
            pageCell with { FlipX = !pageCell.FlipX };
        SpriteVisualPart cursorPart = document.Sprites[
            GameOptionsPresentationDefinitions.CursorFrameName(0)][0];
        document.Sprites[GameOptionsPresentationDefinitions.CursorFrameName(0)][0] =
            cursorPart with { FlipY = !cursorPart.FlipY };
        Directory.CreateDirectory(overrides);
        string replacement = Path.Combine(overrides,
            GameOptionsPresentationDefinitions.FileName);
        using (var output = File.Create(replacement))
            GameOptionsPresentation.Write(output, document);
        AreaMapPresentationCatalog edited = AreaMapPresentationCatalog.Load(stock, overrides);
        AssertTrue(edited.ContentIdentity != original.ContentIdentity,
            "options override changes selected catalog identity");

        var stockMenu = new GameOptionsMenuState(new ForbiddenMapBus(),
            mapPresentation: original);
        var editedMenu = new GameOptionsMenuState(new ForbiddenMapBus(),
            mapPresentation: edited);
        for (int frame = 0; frame < 16; frame++)
        {
            stockMenu.Step(0);
            editedMenu.Step(0);
        }
        AssertTrue(!stockMenu.Render().AsSpan().SequenceEqual(editedMenu.Render()),
            "options page, actor and anchor edits reach the real renderer");

        GameOptionsPhase phaseBeforeRebind = stockMenu.Phase;
        int selectedBeforeRebind = stockMenu.SelectedItem;
        byte cursorXBeforeRebind = stockMenu.CaptureRenderSnapshot().Memory.Oam[0];
        stockMenu.BindMapPresentation(edited);
        AssertEqual(phaseBeforeRebind, stockMenu.Phase,
            "options content rebind preserves menu phase");
        AssertEqual(selectedBeforeRebind, stockMenu.SelectedItem,
            "options content rebind preserves selected row");
        AssertEqual(unchecked((byte)(cursorXBeforeRebind + 8)),
            stockMenu.CaptureRenderSnapshot().Memory.Oam[0],
            "options rebind applies edited cursor anchor without resetting state");

        stockMenu.Step((ushort)SnesButton.A);
        AssertEqual(GameOptionsPhase.FadeOutToIntro, stockMenu.Phase,
            "editable options enters intro fade-out");
        AssertTrue(LastSpriteX(stockMenu.CaptureRenderSnapshot()) >= 256,
            "editable options hides cursor during intro fade-out");

        File.WriteAllText(replacement, "{ broken options JSON");
        AssertThrows<InvalidDataException>(() => AreaMapPresentationCatalog.Load(stock, overrides),
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
        var missingPage = JsonSerializer.Deserialize<GameOptionsPresentationDocument>(
            extracted, MapPresentationFormat.JsonOptions)!;
        missingPage.Pages.Remove(GameOptionsPresentationDefinitions.SpecialJapanesePage);
        AssertThrows<InvalidDataException>(() => GameOptionsPresentation.Write(Stream.Null, missingPage),
            "options reject a missing named page");
        var overlappingToggle = JsonSerializer.Deserialize<GameOptionsPresentationDocument>(
            extracted, MapPresentationFormat.JsonOptions)!;
        GameOptionsToggleVisualDocument toggle = overlappingToggle.SpecialToggles[
            GameOptionsPresentationDefinitions.IconCancelToggle];
        overlappingToggle.SpecialToggles[GameOptionsPresentationDefinitions.IconCancelToggle] =
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
            Frame((ushort)button);
            Frame(0);
        }

        void Frame(ushort input)
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
            AssertTrue(native.Render().AsSpan().SequenceEqual(installed.Render()),
                $"installed options frame {comparedFrames} matches cartridge pixels");
            native.Step(input);
            installed.Step(input);
            IReadOnlyList<CartridgeAudioCommand> nativeCommands =
                nativeAudio.AdvanceFrame(bus, default);
            IReadOnlyList<CartridgeAudioCommand> installedCommands =
                installedAudio.AdvanceFrame(bus, default);
            AssertTrue(nativeCommands.SequenceEqual(installedCommands),
                $"installed options audio matches cartridge frame {comparedFrames}");
            comparedFrames++;
        }
    }
}
