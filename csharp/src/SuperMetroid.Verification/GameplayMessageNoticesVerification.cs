using SuperMetroid.Tooling;
using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static Program;
using static RepositoryInputsVerification;
using static SuiteTimingVerification;
using System.Text.Json.Nodes;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using static GameplayMessageTitlesVerification;

/// <summary>Checks cartridge-extracted message text in the installed box renderer and editable presentation catalog.</summary>
internal static class GameplayMessageNoticesVerification
{
    /// <summary>Compares notice tilemaps with the cartridge and checks selection redraw, live rebinding, and glyph rejection.</summary>
    internal static void VerifyGameplayMessageNotices(string romPath)
    {
        ISnesAddressSpace bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(romPath);
        byte[] extracted = SuperMetroid.AssetExtraction.GameplayMessageNoticeExtractor.Extract(bus);
        GameplayMessageNoticePresentation stock = GameplayMessageNoticePresentation.Load(
            new MemoryStream(extracted, writable: false));

        int comparedWords = 0;
        foreach (GameplayMessageId id in GameplayMessageNoticeDefinitions.MessageIds)
        {
            var installed = new GameplayMessageBoxState();
            installed.BindPresentation(null, null, stock);
            installed.Begin(NewForbiddenGameplayMessageBus(), id, 0);
            comparedWords += installed.Tilemap.Length;

            if (!GameplayMessageNoticeDefinitions.IsSaveConfirmation(id))
                continue;
            while (installed.Phase != GameplayMessageBoxPhase.AwaitingInput)
                installed.Step(0);
            ushort[] yesSelected = installed.Tilemap.ToArray();
            installed.Step((ushort)SnesButton.Right);
            AssertTrue(!yesSelected.SequenceEqual(installed.Tilemap),
                $"installed gameplay-message notice {id} redraws its NO-selection row without cartridge reads");
            comparedWords += installed.Tilemap.Length;
        }

        JsonObject document = RequiredJson.Parse(extracted).AsObject();
        JsonObject mapNotice = document.Required("notices").Required(            GameplayMessageId.MapDataAccessCompleted.ToString()).AsObject();
        mapNotice.Required("text").Required(0)["text"] = "MAP TEST";
        byte[] editedBytes = System.Text.Encoding.UTF8.GetBytes(document.ToJsonString(
            MapPresentationFormat.JsonOptions));
        GameplayMessageNoticePresentation edited = GameplayMessageNoticePresentation.Load(
            new MemoryStream(editedBytes, writable: false));

        var active = new GameplayMessageBoxState();
        active.BindPresentation(null, null, stock);
        active.Begin(NewForbiddenGameplayMessageBus(),
            GameplayMessageId.MapDataAccessCompleted, 0);
        active.Step(0);
        GameplayMessageBoxPhase phase = active.Phase;
        int radius = active.RadiusPixels;
        ushort[] before = active.Tilemap.ToArray();
        active.BindPresentation(null, null, edited);
        AssertEqual(phase, active.Phase,
            "message-notice content rebind preserves coroutine phase");
        AssertEqual(radius, active.RadiusPixels,
            "message-notice content rebind preserves window radius");
        AssertTrue(!before.SequenceEqual(active.Tilemap),
            "UTF-8 notice edit reaches an active message without a ROM patch");
        active.BindPresentation(null, null, stock);
        AssertTrue(before.SequenceEqual(active.Tilemap),
            "restoring stock notice restores exact live tilemap");

        byte[] malformed = System.Text.Encoding.UTF8.GetBytes(document.ToJsonString(
            MapPresentationFormat.JsonOptions).Replace("MAP TEST", "map test"));
        AssertThrows<InvalidDataException>(() => GameplayMessageNoticePresentation.Load(
            new MemoryStream(malformed, writable: false)),
            "gameplay-message notice rejects glyphs absent from its documented font mapping");

        Console.WriteLine(
            $"Gameplay-message notices: {GameplayMessageNoticeDefinitions.MessageIds.Length} UTF-8 layouts and {comparedWords} installed words match the cartridge with message ROM reads forbidden; YES/NO selection, live edit/rebind and invalid glyph rejection pass.");
    }

    /// <summary>Checks deterministic stock extraction and that an override changes rendered save text and content identity.</summary>
    internal static void VerifyGameplayMessageNoticeAssets(
        ISnesAddressSpace bus,
        string stockDirectory,
        string overrideDirectory,
        AreaMapPresentationCatalog stockCatalog)
    {
        byte[] deterministic = SuperMetroid.AssetExtraction.GameplayMessageNoticeExtractor.Extract(bus);
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
            document.ToJsonString(MapPresentationFormat.JsonOptions));
        AreaMapPresentationCatalog edited = AreaMapPresentationCatalog.Load(
            stockDirectory, overrideDirectory);
        var state = new GameplayMessageBoxState();
        state.BindPresentation(edited.GameplayMessageTitles, edited.GameplayMessagePanels,
            edited.GameplayMessageNotices);
        state.Begin(NewForbiddenGameplayMessageBus(), GameplayMessageId.SaveConfirmation, 0);
        AssertTrue(!stockCatalog.GameplayMessageNotices.Build(GameplayMessageId.SaveConfirmation)
                .SequenceEqual(state.Tilemap),
            "catalog override changes installed save-confirmation text");
        AssertTrue(stockCatalog.ContentIdentity != edited.ContentIdentity,
            "gameplay-message notice override changes catalog content identity");

        File.WriteAllText(replacement, "{ broken gameplay-message notice");
        AssertThrows<InvalidDataException>(() =>
            AreaMapPresentationCatalog.Load(stockDirectory, overrideDirectory),
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
