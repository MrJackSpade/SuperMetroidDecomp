using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.AssetExtraction.Frontend;
using SuperMetroid.Tooling;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Frontend.GameOver;
using SuperMetroid.Core.Frontend.GameOver.Artwork;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Map.Artwork;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rendering;
using SuperMetroid.AssetExtraction.Frontend.Map;
using SuperMetroid.AssetExtraction.Frontend.GameOver.Artwork;

namespace SuperMetroid.Verification.Frontend.GameOver.Artwork;

/// <summary>Checks installed game-over artwork and Baby animation against cartridge playback.</summary>
internal static class GameOverPresentationVerification
{
    /// <summary>Compares a No-to-title sequence, then checks visible overrides, live rebinding, and malformed assets.</summary>
    internal static void VerifyGameOverPresentationAssets(
        ISnesAddressSpace bus,
        string stock,
        string overrides,
        AreaMapPresentation original)
    {
        string stockPath = Path.Combine(stock, GameOverPresentationDefinitions.FileName);
        byte[] extracted = SuperMetroid.AssetExtraction.Frontend.GameOverPresentationExtractor.Extract(bus);
        AssertTrue(extracted.SequenceEqual(File.ReadAllBytes(stockPath)),
            "installed game-over JSON is the deterministic cartridge extraction");
        _ = GameOverSourcePresentation.Load(new MemoryStream(extracted));
        AssertEqual(60, GameOverBabyAnimationDefinitions.InstructionCount,
            "compiled game-over Baby instruction count");

        var nativeAudio = new AudioQueues();
        var installedAudio = new AudioQueues();
        var native = new GameOverMenuState(nativeAudio, original, MenuSheetFixtures.For(original));
        var installed = new GameOverMenuState(installedAudio, original, MenuSheetFixtures.For(original));
        bool choseNo = false;
        bool released = false;
        int comparedFrames = 0;
        for (int tick = 0; tick < 700 && !native.TitleRequested; tick++)
        {
            AssertEqual(native.Phase, installed.Phase,
                "installed game-over phase matches cartridge path");
            AssertEqual(native.SelectedItem, installed.SelectedItem,
                "installed game-over selection matches cartridge path");
            AssertEqual(native.BabyStep, installed.BabyStep,
                "compiled Baby sequence reaches the same program step");
            AssertEqual(native.BabyFrame, installed.BabyFrame,
                "compiled Baby sequence selects the same pose");
            AssertTrue(native.Render().SequenceEqual(installed.Render()),
                $"installed game-over frame {tick} matches cartridge pixels");
            comparedFrames++;

            ControllerButton input = 0;
            if (native.Phase == GameOverMenuPhase.Main && tick >= 500)
            {
                if (!choseNo)
                {
                    input = ControllerButton.Down;
                    choseNo = true;
                }
                else if (!released)
                {
                    released = true;
                }
                else
                {
                    input = ControllerButton.A;
                }
            }
            native.Step(input);
            installed.Step(input);
            IReadOnlyList<AudioRequest> nativeCommands =
                nativeAudio.AdvanceFrame(default);
            IReadOnlyList<AudioRequest> installedCommands =
                installedAudio.AdvanceFrame(default);
            AssertTrue(nativeCommands.SequenceEqual(installedCommands),
                $"compiled game-over audio dispatch matches cartridge frame {tick}");
        }
        AssertTrue(native.TitleRequested && installed.TitleRequested,
            "both game-over paths complete the No fade to title");
        AssertTrue(comparedFrames > 500,
            "game-over parity covers animation, music wait, input and fade");

        var document = RequiredJson.Deserialize<GameOverPresentationDocument>(
            extracted, ArtworkJson.Options);
        document = document with
        {
            BabyAnchor = new(checked((ushort)(document.BabyAnchor.X + 8)), document.BabyAnchor.Y),
            CursorX = document.CursorX + 8,
        };
        MapPresentationCell firstVisible = document.Tilemap.First(cell =>
            cell.TileColumn != (GameOverPageDefinitions.BlankTile % MapTileAtlasFormat.TileColumns) ||
            cell.TileRow != (GameOverPageDefinitions.BlankTile / MapTileAtlasFormat.TileColumns));
        int visibleIndex = Array.IndexOf(document.Tilemap, firstVisible);
        document.Tilemap[visibleIndex] = firstVisible with { FlipX = !firstVisible.FlipX };
        document.Sprites[GameOverPresentationNames.CursorFrameName(0)][0] =
            document.Sprites[GameOverPresentationNames.CursorFrameName(0)][0] with
            { FlipY = !document.Sprites[GameOverPresentationNames.CursorFrameName(0)][0].FlipY };
        Directory.CreateDirectory(overrides);
        string replacement = Path.Combine(overrides, GameOverPresentationDefinitions.FileName);
        using (var output = File.Create(replacement))
            GameOverSourcePresentation.Write(output, document);
        AreaMapPresentation edited = MapPresentationFiles.Load(stock, overrides);
        AssertTrue(edited.ContentIdentity != original.ContentIdentity,
            "game-over override changes selected catalog identity");
        AssertEqual(document.BabyAnchor, edited.GameOver.BabyAnchor,
            "edited game-over Baby anchor reaches compiled presentation");

        var originalEditAudio = new AudioQueues();
        var editedAudio = new AudioQueues();
        var originalMenu = new GameOverMenuState(
            originalEditAudio, original, MenuSheetFixtures.For(original));
        var editedMenu = new GameOverMenuState(
            editedAudio, edited, MenuSheetFixtures.For(edited));
        for (int frame = 0; frame < 40; frame++)
        {
            originalMenu.Step(0);
            editedMenu.Step(0);
            _ = originalEditAudio.AdvanceFrame(default);
            _ = editedAudio.AdvanceFrame(default);
        }
        AssertTrue(!originalMenu.Render().SequenceEqual(editedMenu.Render()),
            "game-over tilemap, actor and anchor edits reach the real renderer");

        for (int frame = 0; frame < 23; frame++)
            originalMenu.Step(0);
        int? stepBeforeRebind = originalMenu.BabyStep;
        GameOverMenuPhase phaseBeforeRebind = originalMenu.Phase;
        int babyXBeforeRebind = originalMenu.CaptureRenderSnapshot().Paint.SpriteParts()[0].Part.Position.X;
        originalMenu.BindFixtureArtwork(edited);
        AssertEqual(stepBeforeRebind, originalMenu.BabyStep,
            "game-over content rebind preserves Baby animation phase");
        AssertEqual(phaseBeforeRebind, originalMenu.Phase,
            "game-over content rebind preserves menu phase");
        AssertEqual(babyXBeforeRebind + 8,
            (int)originalMenu.CaptureRenderSnapshot().Paint.SpriteParts()[0].Part.Position.X,
            "game-over rebind applies edited Baby anchor without resetting live timing");

        File.WriteAllText(replacement, "{ broken game-over JSON");
        AssertThrows<InvalidDataException>(() => MapPresentationFiles.Load(stock, overrides),
            "corrupt game-over override fails loudly");
        using (var output = File.Create(replacement))
            GameOverSourcePresentation.Write(output, document);
        AssertThrows<InvalidDataException>(() => GameOverSourcePresentation.Write(Stream.Null,
            document with { BabyPalette = 8 }),
            "game-over rejects invalid actor palette");
        AssertThrows<InvalidDataException>(() => GameOverSourcePresentation.Write(Stream.Null,
            document with { Tilemap = document.Tilemap[..^1] }),
            "game-over rejects incomplete tilemap");
        var missingSprite = RequiredJson.Deserialize<GameOverPresentationDocument>(
            extracted, ArtworkJson.Options);
        missingSprite.Sprites.Remove(GameOverPresentationDefinitions.EggFrame);
        AssertThrows<InvalidDataException>(() => GameOverSourcePresentation.Write(Stream.Null, missingSprite),
            "game-over rejects missing named sprite");
        var shortPalette = RequiredJson.Deserialize<GameOverPresentationDocument>(
            extracted, ArtworkJson.Options);
        shortPalette.BabyPalettes[GameOverPresentationNames.BabyPaletteName(
            GameOverBabyPalette.OpenCry)] = [0];
        AssertThrows<InvalidDataException>(() => GameOverSourcePresentation.Write(Stream.Null, shortPalette),
            "game-over rejects malformed Baby palette");

        Console.WriteLine(
            $"Game-over presentation: {comparedFrames} exact ROM-free frames, compiled " +
            "Baby dispatcher parity, visible edits, state-safe rebind and strict failures pass.");
    }
}
