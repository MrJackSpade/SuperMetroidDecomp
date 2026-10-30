using SuperMetroid.Tooling;
using static SuperMetroid.Verification.Harness.RepositoryInputsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using System.Text.Json.Nodes;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Hud.Artwork;
namespace SuperMetroid.Verification.Runtime;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms;
using SuperMetroid.AssetExtraction.Frontend.Map;
using SuperMetroid.AssetExtraction.Hud;

/// <summary>Checks cartridge-extracted message text in the installed box renderer and editable presentation catalog.</summary>
internal static class GameplayMessageNoticesVerification
{
    /// <summary>Compares notice tilemaps with the cartridge and checks selection redraw, live rebinding, and glyph rejection.</summary>
    internal static void VerifyGameplayMessageNotices(string romPath)
    {
        ISnesAddressSpace bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(romPath);
        byte[] extracted = SuperMetroid.AssetExtraction.Hud.GameplayMessageNoticeExtractor.Extract(bus);
        GameplayMessageNoticePresentation stock = GameplayMessageNoticePresentation.Load(
            new MemoryStream(extracted, writable: false));

        var stockMessages = new GameplayMessageArtwork(null, null, stock);
        int comparedWords = 0;
        foreach (GameplayMessageId id in GameplayMessageNoticeDefinitions.MessageIds)
        {
            var installed = new GameplayMessageBoxState();
            installed.Begin(id, 0);
            comparedWords += installed.Tilemap(stockMessages).Length;

            if (!GameplayMessageNotices.IsSaveConfirmation(id))
                continue;
            while (installed.Phase != GameplayMessageBoxPhase.AwaitingInput)
                installed.Step(0);
            BackgroundTile[] yesSelected = installed.Tilemap(stockMessages);
            // $85:84BA's two lag waits precede each ReadControllerInput; press on the read frame.
            for (int lag = 0; lag < GameplayMessageBoxDefinitions.Timing.SaveSelectionReadLagFrames; lag++)
                installed.Step(0);
            installed.Step(ControllerButton.Right);
            BackgroundTile[] noSelected = installed.Tilemap(stockMessages);
            AssertTrue(!yesSelected.SequenceEqual(noSelected),
                $"installed gameplay-message notice {id} redraws its NO-selection row without cartridge reads");
            comparedWords += noSelected.Length;
        }

        JsonObject document = RequiredJson.Parse(extracted).AsObject();
        JsonObject mapNotice = document.Required("notices").Required(            GameplayMessageId.MapDataAccessCompleted.ToString()).AsObject();
        mapNotice.Required("text").Required(0)["text"] = "MAP TEST";
        byte[] editedBytes = System.Text.Encoding.UTF8.GetBytes(document.ToJsonString(
            ArtworkJson.Options));
        GameplayMessageNoticePresentation edited = GameplayMessageNoticePresentation.Load(
            new MemoryStream(editedBytes, writable: false));

        // The game names the form it shows; an edited notice changes the words the import lays that form out with.
        var active = new GameplayMessageBoxState();
        active.Begin(GameplayMessageId.MapDataAccessCompleted, 0);
        active.Step(0);
        BackgroundTile[] before = active.Tilemap(stockMessages);
        AssertTrue(!before.SequenceEqual(active.Tilemap(new GameplayMessageArtwork(null, null, edited))),
            "UTF-8 notice edit changes the words of the shown message without a ROM patch");
        AssertTrue(before.SequenceEqual(active.Tilemap(stockMessages)),
            "stock notice lays the shown message out with its exact words");

        byte[] malformed = System.Text.Encoding.UTF8.GetBytes(document.ToJsonString(
            ArtworkJson.Options).Replace("MAP TEST", "map test"));
        AssertThrows<InvalidDataException>(() => GameplayMessageNoticePresentation.Load(
            new MemoryStream(malformed, writable: false)),
            "gameplay-message notice rejects glyphs absent from its documented font mapping");

        Console.WriteLine(
            $"Gameplay-message notices: {GameplayMessageNoticeDefinitions.MessageIds.Length} UTF-8 layouts and {comparedWords} installed words match the cartridge with message ROM reads forbidden; YES/NO selection, edited layout and invalid glyph rejection pass.");
    }

    /// <summary>Checks deterministic stock extraction and that an override changes rendered save text and content identity.</summary>
    internal static void VerifyGameplayMessageNoticeAssets(
        ISnesAddressSpace bus,
        string stockDirectory,
        string overrideDirectory,
        AreaMapPresentation stockCatalog)
    {
        byte[] deterministic = SuperMetroid.AssetExtraction.Hud.GameplayMessageNoticeExtractor.Extract(bus);
        AssertTrue(deterministic.SequenceEqual(File.ReadAllBytes(
            Path.Combine(stockDirectory, GameplayMessageNoticeDefinitions.FileName))),
            "installed gameplay-message notices are the deterministic cartridge extraction");

        Directory.CreateDirectory(overrideDirectory);
        JsonObject document = RequiredJson.Parse(deterministic).AsObject();
        document.Required("notices").Required(GameplayMessageId.SaveConfirmation.ToString()).Required("text").Required(0)["text"] =
            "SAVE TEST?";
        string replacement = Path.Combine(
            overrideDirectory, GameplayMessageNoticeDefinitions.FileName);
        File.WriteAllText(replacement,
            document.ToJsonString(ArtworkJson.Options));
        AreaMapPresentation edited = MapPresentationFiles.Load(
            stockDirectory, overrideDirectory);
        var state = new GameplayMessageBoxState();
        state.Begin(GameplayMessageId.SaveConfirmation, 0);
        AssertTrue(!(GameplayMessageArtwork.Of(stockCatalog).Notices ?? throw new InvalidOperationException("Stock installs the message notices."))
                .Build(GameplayMessageId.SaveConfirmation).SequenceEqual(state.Tilemap(GameplayMessageArtwork.Of(edited))),
            "catalog override changes installed save-confirmation text");
        AssertTrue(stockCatalog.ContentIdentity != edited.ContentIdentity,
            "gameplay-message notice override changes catalog content identity");

        File.WriteAllText(replacement, "{ broken gameplay-message notice");
        AssertThrows<InvalidDataException>(() =>
            MapPresentationFiles.Load(stockDirectory, overrideDirectory),
            "corrupt gameplay-message notice override fails loudly");
        Console.WriteLine(
            "Gameplay-message notice catalog: deterministic stock, override selection/identity and corruption failure pass.");
    }

    /// <summary>Runs message-notice extraction and rendering checks against the repository ROM.</summary>
    [VerificationCommand("--gameplay-message-notices")]
    internal static int GameplayMessageNoticesCommand()
    {
        string messageNoticeRom = RepositoryRomPath;
        Suite(nameof(VerifyGameplayMessageNotices), () => VerifyGameplayMessageNotices(messageNoticeRom));
        return 0;
    }
}
