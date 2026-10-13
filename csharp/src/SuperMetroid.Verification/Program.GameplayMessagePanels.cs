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

/// <summary>Checks editable gameplay message panels and live title/diagram binding.</summary>
internal static class GameplayMessagePanelsVerification
{
    /// <summary>Checks all installed panels without ROM reads, then edits and restores an active message.</summary>
    internal static void VerifyGameplayMessagePanels(string romPath)
    {
        ISnesAddressSpace bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(romPath);
        byte[] extracted = SuperMetroid.AssetExtraction.GameplayMessagePanelExtractor.Extract(bus);
        GameplayMessagePanelPresentation stock = GameplayMessagePanelPresentation.Load(
            new MemoryStream(extracted, writable: false));

        int comparedWords = 0;
        foreach (GameplayMessageId id in GameplayMessagePanelDefinitions.MessageIds)
        {
            var installed = new GameplayMessageBoxState();
            installed.BindPresentation(null, stock);
            installed.Begin(NewForbiddenGameplayMessageBus(), id, 0,
                shootBinding: (ushort)SnesButton.B,
                runBinding: (ushort)SnesButton.Y);
            AssertEqual(GameplayMessagePanelDefinitions.TilemapWords, installed.Tilemap.Length,
                $"installed gameplay-message panel {id} builds its complete tilemap with remapped controls");
            comparedWords += installed.Tilemap.Length;
        }

        JsonObject document = RequiredJson.Parse(extracted).AsObject();
        JsonObject missile = document.Required("panels").Required(GameplayMessageId.MissileTank.ToString()).AsObject();
        missile["title"] = "MISSILE TEST";
        byte[] editedBytes = System.Text.Encoding.UTF8.GetBytes(document.ToJsonString(
            MapPresentationFormat.JsonOptions));
        GameplayMessagePanelPresentation edited = GameplayMessagePanelPresentation.Load(
            new MemoryStream(editedBytes, writable: false));

        var active = new GameplayMessageBoxState();
        active.BindPresentation(null, stock);
        active.Begin(NewForbiddenGameplayMessageBus(), GameplayMessageId.MissileTank, 0,
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

    /// <summary>Checks deterministic stock extraction, override identity, and corrupt replacement failure.</summary>
    internal static void VerifyGameplayMessagePanelAssets(
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
        JsonObject document = RequiredJson.Parse(deterministic).AsObject();
        document.Required("panels").Required(GameplayMessageId.GrappleBeam.ToString())["title"] =
            "GRAPPLE TEST";
        string replacement = Path.Combine(
            overrideDirectory, GameplayMessagePanelDefinitions.FileName);
        File.WriteAllText(replacement,
            document.ToJsonString(MapPresentationFormat.JsonOptions));
        AreaMapPresentationCatalog edited = AreaMapPresentationCatalog.Load(
            stockDirectory, overrideDirectory);
        var state = new GameplayMessageBoxState();
        state.BindPresentation(edited.GameplayMessageTitles, edited.GameplayMessagePanels);
        state.Begin(NewForbiddenGameplayMessageBus(), GameplayMessageId.GrappleBeam, 0);
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

    /// <summary>Runs the gameplay message panel comparison with the retail cartridge.</summary>
    [VerificationCommand("--gameplay-message-panels")]
    internal static int GameplayMessagePanelsCommand()
    {
        string messagePanelRom = RepositoryRomPath;
        Suite(nameof(VerifyGameplayMessagePanels), () => VerifyGameplayMessagePanels(messagePanelRom));
        return 0;
    }
}
