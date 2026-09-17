using System.Text.Json.Nodes;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;

internal static partial class Program
{
    private static void VerifyGameplayMessagePanels(string romPath)
    {
        ISnesAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        byte[] extracted = SuperMetroid.AssetExtraction.GameplayMessagePanelExtractor.Extract(bus);
        GameplayMessagePanelPresentation stock = GameplayMessagePanelPresentation.Load(
            new MemoryStream(extracted, writable: false));

        int comparedWords = 0;
        foreach (GameplayMessageId id in GameplayMessagePanelDefinitions.MessageIds)
        {
            var cartridge = new GameplayMessageBoxState();
            cartridge.Begin(bus, id,
                shootBinding: (ushort)SnesButton.B,
                runBinding: (ushort)SnesButton.Y);
            var installed = new GameplayMessageBoxState();
            installed.BindPresentation(null, stock);
            installed.Begin(new ForbiddenGameplayMessageBus(), id,
                shootBinding: (ushort)SnesButton.B,
                runBinding: (ushort)SnesButton.Y);
            AssertTrue(cartridge.Tilemap.SequenceEqual(installed.Tilemap),
                $"installed gameplay-message panel {id} matches cartridge tilemap with remapped controls");
            comparedWords += installed.Tilemap.Length;
        }

        JsonObject document = JsonNode.Parse(extracted)!.AsObject();
        JsonObject missile = document["panels"]![GameplayMessageId.MissileTank.ToString()]!.AsObject();
        missile["title"] = "MISSILE TEST";
        byte[] editedBytes = System.Text.Encoding.UTF8.GetBytes(document.ToJsonString(
            MapPresentationFormat.JsonOptions));
        GameplayMessagePanelPresentation edited = GameplayMessagePanelPresentation.Load(
            new MemoryStream(editedBytes, writable: false));

        var active = new GameplayMessageBoxState();
        active.BindPresentation(null, stock);
        active.Begin(new ForbiddenGameplayMessageBus(), GameplayMessageId.MissileTank,
            shootBinding: (ushort)SnesButton.B);
        active.Step(0);
        GameplayMessageBoxPhase phase = active.Phase;
        int radius = active.RadiusPixels;
        ushort[] before = active.Tilemap.ToArray();
        active.BindPresentation(null, edited);
        AssertEqual(phase, active.Phase,
            "message-panel content rebind preserves coroutine phase");
        AssertEqual(radius, active.RadiusPixels,
            "message-panel content rebind preserves window radius");
        AssertTrue(!before.AsSpan().SequenceEqual(active.Tilemap),
            "UTF-8 panel-title edit reaches an active message without a ROM patch");

        active.BindPresentation(null, stock);
        AssertTrue(before.AsSpan().SequenceEqual(active.Tilemap),
            "restoring stock panel restores exact live tilemap and configured button");

        byte[] malformed = System.Text.Encoding.UTF8.GetBytes(document.ToJsonString(
            MapPresentationFormat.JsonOptions).Replace("MISSILE TEST", "missile test"));
        AssertThrows<InvalidDataException>(() => GameplayMessagePanelPresentation.Load(
            new MemoryStream(malformed, writable: false)),
            "gameplay-message panel rejects glyphs absent from its documented font mapping");

        Console.WriteLine(
            $"Gameplay-message panels: {GameplayMessagePanelDefinitions.MessageIds.Length} UTF-8 titles and {comparedWords} installed words match the cartridge with message ROM reads forbidden; diagrams, remapped controls, live edit/rebind and invalid glyph rejection pass.");
    }

    private static void VerifyGameplayMessagePanelAssets(
        ISnesAddressSpace bus,
        string stockDirectory,
        string overrideDirectory,
        AreaMapPresentationCatalog stockCatalog)
    {
        byte[] deterministic = SuperMetroid.AssetExtraction.GameplayMessagePanelExtractor.Extract(bus);
        AssertTrue(deterministic.AsSpan().SequenceEqual(File.ReadAllBytes(
            Path.Combine(stockDirectory, GameplayMessagePanelDefinitions.FileName))),
            "installed gameplay-message panels are the deterministic cartridge extraction");

        Directory.CreateDirectory(overrideDirectory);
        JsonObject document = JsonNode.Parse(deterministic)!.AsObject();
        document["panels"]![GameplayMessageId.GrappleBeam.ToString()]!["title"] =
            "GRAPPLE TEST";
        string replacement = Path.Combine(
            overrideDirectory, GameplayMessagePanelDefinitions.FileName);
        File.WriteAllText(replacement,
            document.ToJsonString(MapPresentationFormat.JsonOptions));
        AreaMapPresentationCatalog edited = AreaMapPresentationCatalog.Load(
            stockDirectory, overrideDirectory);
        var state = new GameplayMessageBoxState();
        state.BindPresentation(edited.GameplayMessageTitles, edited.GameplayMessagePanels);
        state.Begin(new ForbiddenGameplayMessageBus(), GameplayMessageId.GrappleBeam);
        AssertTrue(!stockCatalog.GameplayMessagePanels.Build(GameplayMessageId.GrappleBeam)
                .AsSpan().SequenceEqual(state.Tilemap),
            "catalog override changes installed large-message title");
        AssertTrue(stockCatalog.ContentIdentity != edited.ContentIdentity,
            "gameplay-message panel override changes catalog content identity");

        File.WriteAllText(replacement, "{ broken gameplay-message panel");
        AssertThrows<InvalidDataException>(() =>
            AreaMapPresentationCatalog.Load(stockDirectory, overrideDirectory),
            "corrupt gameplay-message panel override fails loudly");
        Console.WriteLine(
            "Gameplay-message panel catalog: deterministic stock, override selection/identity and corruption failure pass.");
    }
}
