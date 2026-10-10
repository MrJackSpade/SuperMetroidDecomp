using System.Text.Json.Nodes;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;

internal static partial class Program
{
    /// <summary>Checks installed message layouts, selection redraws, live content rebinding, and rejection of invalid notice text.</summary>
    /// <param name="romPath">Retail ROM path used to extract the stock gameplay-message notice document.</param>
    private static void VerifyGameplayMessageNotices(string romPath)
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
            installed.Begin(new ForbiddenGameplayMessageBus(), id, 0);
            comparedWords += installed.Tilemap.Length;

            if (!GameplayMessageNoticeDefinitions.IsSaveConfirmation(id))
                continue;
            while (installed.Phase != GameplayMessageBoxPhase.AwaitingInput)
                installed.Step(0);
            ushort[] yesSelected = installed.Tilemap.ToArray();
            installed.Step((ushort)SnesButton.Right);
            AssertTrue(!yesSelected.AsSpan().SequenceEqual(installed.Tilemap),
                $"installed gameplay-message notice {id} redraws its NO-selection row without cartridge reads");
            comparedWords += installed.Tilemap.Length;
        }

        JsonObject document = JsonNode.Parse(extracted)!.AsObject();
        JsonObject mapNotice = document["notices"]![
            GameplayMessageId.MapDataAccessCompleted.ToString()]!.AsObject();
        mapNotice["text"]![0]!["text"] = "MAP TEST";
        byte[] editedBytes = System.Text.Encoding.UTF8.GetBytes(document.ToJsonString(
            MapPresentationFormat.JsonOptions));
        GameplayMessageNoticePresentation edited = GameplayMessageNoticePresentation.Load(
            new MemoryStream(editedBytes, writable: false));

        var active = new GameplayMessageBoxState();
        active.BindPresentation(null, null, stock);
        active.Begin(new ForbiddenGameplayMessageBus(),
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
        AssertTrue(!before.AsSpan().SequenceEqual(active.Tilemap),
            "UTF-8 notice edit reaches an active message without a ROM patch");
        active.BindPresentation(null, null, stock);
        AssertTrue(before.AsSpan().SequenceEqual(active.Tilemap),
            "restoring stock notice restores exact live tilemap");

        byte[] malformed = System.Text.Encoding.UTF8.GetBytes(document.ToJsonString(
            MapPresentationFormat.JsonOptions).Replace("MAP TEST", "map test"));
        AssertThrows<InvalidDataException>(() => GameplayMessageNoticePresentation.Load(
            new MemoryStream(malformed, writable: false)),
            "gameplay-message notice rejects glyphs absent from its documented font mapping");

        Console.WriteLine(
            $"Gameplay-message notices: {GameplayMessageNoticeDefinitions.MessageIds.Length} UTF-8 layouts and {comparedWords} installed words match the cartridge with message ROM reads forbidden; YES/NO selection, live edit/rebind and invalid glyph rejection pass.");
    }

    /// <summary>Verifies deterministic stock extraction, override selection and identity, and loud failure for corrupt overrides.</summary>
    /// <param name="bus">Cartridge address space used to extract the stock notice data.</param>
    /// <param name="stockDirectory">Directory containing the installed stock presentation files.</param>
    /// <param name="overrideDirectory">Directory in which the test creates a user notice override.</param>
    /// <param name="stockCatalog">Loaded catalog used to compare stock and overridden message content.</param>
    private static void VerifyGameplayMessageNoticeAssets(
        ISnesAddressSpace bus,
        string stockDirectory,
        string overrideDirectory,
        AreaMapPresentationCatalog stockCatalog)
    {
        byte[] deterministic = SuperMetroid.AssetExtraction.GameplayMessageNoticeExtractor.Extract(bus);
        AssertTrue(deterministic.AsSpan().SequenceEqual(File.ReadAllBytes(
            Path.Combine(stockDirectory, GameplayMessageNoticeDefinitions.FileName))),
            "installed gameplay-message notices are the deterministic cartridge extraction");

        Directory.CreateDirectory(overrideDirectory);
        JsonObject document = JsonNode.Parse(deterministic)!.AsObject();
        document["notices"]![GameplayMessageId.SaveConfirmation.ToString()]!["text"]![0]!["text"] =
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
        state.Begin(new ForbiddenGameplayMessageBus(), GameplayMessageId.SaveConfirmation, 0);
        AssertTrue(!stockCatalog.GameplayMessageNotices.Build(GameplayMessageId.SaveConfirmation)
                .AsSpan().SequenceEqual(state.Tilemap),
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
}
