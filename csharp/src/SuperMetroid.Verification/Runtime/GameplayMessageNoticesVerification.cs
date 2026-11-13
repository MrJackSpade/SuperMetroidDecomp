using SuperMetroid.Tooling;
using static SuperMetroid.Verification.Harness.RepositoryInputsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using System.Text.Json.Nodes;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Hud;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction;
namespace SuperMetroid.Verification.Runtime;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Rooms;
using SuperMetroid.AssetExtraction.Hud;

/// <summary>Checks cartridge-extracted message text in the installed box renderer and presentation catalog.</summary>
internal static class GameplayMessageNoticesVerification
{
    /// <summary>Lays out every installed notice and checks selection redraw and glyph rejection.</summary>
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
            var installed = new GameplayMessageBoxState(new SaveChoiceInputCadence());
            installed.Begin(id, 0);
            comparedWords += installed.Tilemap(stockMessages).Length;

            if (!GameplayMessageNotices.IsSaveConfirmation(id))
                continue;
            while (installed.Phase != GameplayMessageBoxPhase.AwaitingInput)
                installed.Step(0);
            BackgroundTile[] yesSelected = installed.Tilemap(stockMessages);
            // The confirmation waits before each controller read; press on the read update.
            for (int wait = 0; wait < SaveChoiceInputCadence.UpdatesBeforeFirstRead; wait++)
                installed.Step(0);
            installed.Step(ControllerButton.Right);
            BackgroundTile[] noSelected = installed.Tilemap(stockMessages);
            AssertTrue(!yesSelected.SequenceEqual(noSelected),
                $"installed gameplay-message notice {id} redraws its NO-selection row without cartridge reads");
            comparedWords += noSelected.Length;
        }

        JsonObject document = RequiredJson.Parse(extracted).AsObject();
        JsonObject mapNotice = document.Required("notices").Required(            GameplayMessageId.MapDataAccessCompleted.ToString()).AsObject();
        mapNotice.Required("text").Required(0)["text"] = "map test";
        byte[] malformed = System.Text.Encoding.UTF8.GetBytes(document.ToJsonString(
            ArtworkJson.Options));
        AssertThrows<InvalidDataException>(() => GameplayMessageNoticePresentation.Load(
            new MemoryStream(malformed, writable: false)),
            "gameplay-message notice rejects glyphs absent from its documented font mapping");

        Console.WriteLine(
            $"Gameplay-message notices: {GameplayMessageNoticeDefinitions.MessageIds.Length} UTF-8 layouts and {comparedWords} installed words match the cartridge with message ROM reads forbidden; YES/NO selection and invalid glyph rejection pass.");
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
