using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;

internal static partial class Program
{
    private static void VerifyGameOverPresentationAssets(
        ISnesAddressSpace bus,
        string stock,
        string overrides,
        AreaMapPresentationCatalog original)
    {
        string stockPath = Path.Combine(stock, GameOverPresentationDefinitions.FileName);
        byte[] extracted = SuperMetroid.AssetExtraction.GameOverPresentationExtractor.Extract(bus);
        AssertTrue(extracted.AsSpan().SequenceEqual(File.ReadAllBytes(stockPath)),
            "installed game-over JSON is the deterministic cartridge extraction");
        _ = GameOverPresentation.Load(new MemoryStream(extracted));
        AssertEqual(60, GameOverBabyAnimationDefinitions.All.Length,
            "compiled game-over Baby instruction count");

        var nativeAudio = new CartridgeAudioState();
        var installedAudio = new CartridgeAudioState();
        var native = new GameOverMenuState(bus, nativeAudio);
        var installed = new GameOverMenuState(new ForbiddenMapBus(), installedAudio, original);
        bool choseNo = false;
        bool released = false;
        int comparedFrames = 0;
        for (int tick = 0; tick < 700 && !native.TitleRequested; tick++)
        {
            AssertEqual(native.Phase, installed.Phase,
                "installed game-over phase matches cartridge path");
            AssertEqual(native.SelectedItem, installed.SelectedItem,
                "installed game-over selection matches cartridge path");
            AssertEqual(native.BabyInstructionPointer, installed.BabyInstructionPointer,
                "compiled Baby sequence retains native pointer identity");
            AssertEqual(native.BabySpritemap, installed.BabySpritemap,
                "compiled Baby sequence selects native frame identity");
            AssertTrue(native.Render().AsSpan().SequenceEqual(installed.Render()),
                $"installed game-over frame {tick} matches cartridge pixels");
            comparedFrames++;

            ushort input = 0;
            if (native.Phase == GameOverMenuPhase.Main && tick >= 500)
            {
                if (!choseNo)
                {
                    input = (ushort)SnesButton.Down;
                    choseNo = true;
                }
                else if (!released)
                {
                    released = true;
                }
                else
                {
                    input = (ushort)SnesButton.A;
                }
            }
            native.Step(input);
            installed.Step(input);
            IReadOnlyList<CartridgeAudioCommand> nativeCommands =
                nativeAudio.AdvanceFrame(bus, default);
            IReadOnlyList<CartridgeAudioCommand> installedCommands =
                installedAudio.AdvanceFrame(bus, default);
            AssertTrue(nativeCommands.SequenceEqual(installedCommands),
                $"compiled game-over audio dispatch matches cartridge frame {tick}");
        }
        AssertTrue(native.TitleRequested && installed.TitleRequested,
            "both game-over paths complete the No fade to title");
        AssertTrue(comparedFrames > 500,
            "game-over parity covers animation, music wait, input and fade");

        var document = JsonSerializer.Deserialize<GameOverPresentationDocument>(
            extracted, MapPresentationFormat.JsonOptions)!;
        document = document with
        {
            BabyAnchor = new(document.BabyAnchor.X + 8, document.BabyAnchor.Y),
            CursorX = document.CursorX + 8,
        };
        MapPresentationCell firstVisible = document.Tilemap.First(cell =>
            cell.TileColumn != (GameOverRomData.BlankTile.CharacterIndex % MapTileAtlasFormat.TileColumns) ||
            cell.TileRow != (GameOverRomData.BlankTile.CharacterIndex / MapTileAtlasFormat.TileColumns));
        int visibleIndex = Array.IndexOf(document.Tilemap, firstVisible);
        document.Tilemap[visibleIndex] = firstVisible with { FlipX = !firstVisible.FlipX };
        document.Sprites[GameOverPresentationDefinitions.CursorFrameName(0)][0] =
            document.Sprites[GameOverPresentationDefinitions.CursorFrameName(0)][0] with
            { FlipY = !document.Sprites[GameOverPresentationDefinitions.CursorFrameName(0)][0].FlipY };
        Directory.CreateDirectory(overrides);
        string replacement = Path.Combine(overrides, GameOverPresentationDefinitions.FileName);
        using (var output = File.Create(replacement))
            GameOverPresentation.Write(output, document);
        AreaMapPresentationCatalog edited = AreaMapPresentationCatalog.Load(stock, overrides);
        AssertTrue(edited.ContentIdentity != original.ContentIdentity,
            "game-over override changes selected catalog identity");
        AssertEqual(document.BabyAnchor, edited.GameOver.BabyAnchor,
            "edited game-over Baby anchor reaches compiled presentation");

        var originalEditAudio = new CartridgeAudioState();
        var editedAudio = new CartridgeAudioState();
        var originalMenu = new GameOverMenuState(
            new ForbiddenMapBus(), originalEditAudio, original);
        var editedMenu = new GameOverMenuState(
            new ForbiddenMapBus(), editedAudio, edited);
        for (int frame = 0; frame < 40; frame++)
        {
            originalMenu.Step(0);
            editedMenu.Step(0);
            _ = originalEditAudio.AdvanceFrame(bus, default);
            _ = editedAudio.AdvanceFrame(bus, default);
        }
        AssertTrue(!originalMenu.Render().AsSpan().SequenceEqual(editedMenu.Render()),
            "game-over tilemap, actor and anchor edits reach the real renderer");

        for (int frame = 0; frame < 23; frame++)
            originalMenu.Step(0);
        ushort pointerBeforeRebind = originalMenu.BabyInstructionPointer;
        GameOverMenuPhase phaseBeforeRebind = originalMenu.Phase;
        byte babyXBeforeRebind = originalMenu.CaptureRenderSnapshot().Memory.Oam[0];
        originalMenu.BindMapPresentation(edited);
        AssertEqual(pointerBeforeRebind, originalMenu.BabyInstructionPointer,
            "game-over content rebind preserves Baby animation phase");
        AssertEqual(phaseBeforeRebind, originalMenu.Phase,
            "game-over content rebind preserves menu phase");
        AssertEqual(unchecked((byte)(babyXBeforeRebind + 8)),
            originalMenu.CaptureRenderSnapshot().Memory.Oam[0],
            "game-over rebind applies edited Baby anchor without resetting live timing");

        File.WriteAllText(replacement, "{ broken game-over JSON");
        AssertThrows<InvalidDataException>(() => AreaMapPresentationCatalog.Load(stock, overrides),
            "corrupt game-over override fails loudly");
        using (var output = File.Create(replacement))
            GameOverPresentation.Write(output, document);
        AssertThrows<InvalidDataException>(() => GameOverPresentation.Write(Stream.Null,
            document with { BabyPalette = 8 }),
            "game-over rejects invalid actor palette");
        AssertThrows<InvalidDataException>(() => GameOverPresentation.Write(Stream.Null,
            document with { Tilemap = document.Tilemap[..^1] }),
            "game-over rejects incomplete tilemap");
        var missingSprite = JsonSerializer.Deserialize<GameOverPresentationDocument>(
            extracted, MapPresentationFormat.JsonOptions)!;
        missingSprite.Sprites.Remove(GameOverPresentationDefinitions.EggFrame);
        AssertThrows<InvalidDataException>(() => GameOverPresentation.Write(Stream.Null, missingSprite),
            "game-over rejects missing named sprite");
        var shortPalette = JsonSerializer.Deserialize<GameOverPresentationDocument>(
            extracted, MapPresentationFormat.JsonOptions)!;
        shortPalette.BabyPalettes[GameOverPresentationDefinitions.BabyPaletteName(
            GameOverBabyPalette.OpenCry)] = [0];
        AssertThrows<InvalidDataException>(() => GameOverPresentation.Write(Stream.Null, shortPalette),
            "game-over rejects malformed Baby palette");

        Console.WriteLine(
            $"Game-over presentation: {comparedFrames} exact ROM-free frames, compiled " +
            "Baby dispatcher parity, visible edits, state-safe rebind and strict failures pass.");
    }
}
