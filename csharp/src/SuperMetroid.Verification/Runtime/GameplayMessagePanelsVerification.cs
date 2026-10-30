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

/// <summary>Checks editable gameplay message panels and live title/diagram binding.</summary>
internal static class GameplayMessagePanelsVerification
{
    /// <summary>Checks all installed panels without ROM reads, then edits and restores an active message.</summary>
    internal static void VerifyGameplayMessagePanels(string romPath)
    {
        ISnesAddressSpace bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(romPath);
        byte[] extracted = SuperMetroid.AssetExtraction.Hud.GameplayMessagePanelExtractor.Extract(bus);
        GameplayMessagePanelPresentation stock = GameplayMessagePanelPresentation.Load(
            new MemoryStream(extracted, writable: false));

        var stockMessages = new GameplayMessageArtwork(null, stock);
        int comparedWords = 0;
        foreach (GameplayMessageId id in GameplayMessagePanelDefinitions.MessageIds)
        {
            var installed = new GameplayMessageBoxState();
            installed.Begin(id, 0,
                shootBinding: ControllerButton.B,
                runBinding: ControllerButton.Y);
            BackgroundTile[] words = installed.Tilemap(stockMessages);
            AssertEqual(GameplayMessagePanelDefinitions.TilemapWords, words.Length,
                $"installed gameplay-message panel {id} builds its complete tilemap with remapped controls");
            comparedWords += words.Length;
        }

        JsonObject document = RequiredJson.Parse(extracted).AsObject();
        JsonObject missile = document.Required("panels").Required(GameplayMessageId.MissileTank.ToString()).AsObject();
        missile["title"] = "MISSILE TEST";
        byte[] editedBytes = System.Text.Encoding.UTF8.GetBytes(document.ToJsonString(
            ArtworkJson.Options));
        GameplayMessagePanelPresentation edited = GameplayMessagePanelPresentation.Load(
            new MemoryStream(editedBytes, writable: false));

        // The game names the form it shows; an edited panel changes the words the import lays that form out with.
        var active = new GameplayMessageBoxState();
        active.Begin(GameplayMessageId.MissileTank, 0,
            shootBinding: ControllerButton.B);
        active.Step(0);
        BackgroundTile[] before = active.Tilemap(stockMessages);
        AssertTrue(!before.SequenceEqual(active.Tilemap(new GameplayMessageArtwork(null, edited))),
            "UTF-8 panel-title edit changes the words of the shown message without a ROM patch");
        AssertTrue(before.SequenceEqual(active.Tilemap(stockMessages)),
            "stock panel lays the shown message out with its exact words and configured button");

        byte[] malformed = System.Text.Encoding.UTF8.GetBytes(document.ToJsonString(
            ArtworkJson.Options).Replace("MISSILE TEST", "missile test"));
        AssertThrows<InvalidDataException>(() => GameplayMessagePanelPresentation.Load(
            new MemoryStream(malformed, writable: false)),
            "gameplay-message panel rejects glyphs absent from its documented font mapping");

        Console.WriteLine(
            $"Gameplay-message panels: {GameplayMessagePanelDefinitions.MessageIds.Length} UTF-8 titles and {comparedWords} installed words match the cartridge with message ROM reads forbidden; diagrams, remapped controls, edited layout and invalid glyph rejection pass.");
    }

    /// <summary>Checks deterministic stock extraction, override identity, and corrupt replacement failure.</summary>
    internal static void VerifyGameplayMessagePanelAssets(
        ISnesAddressSpace bus,
        string stockDirectory,
        string overrideDirectory,
        AreaMapPresentation stockCatalog)
    {
        byte[] deterministic = SuperMetroid.AssetExtraction.Hud.GameplayMessagePanelExtractor.Extract(bus);
        AssertTrue(deterministic.SequenceEqual(File.ReadAllBytes(
            Path.Combine(stockDirectory, GameplayMessagePanelDefinitions.FileName))),
            "installed gameplay-message panels are the deterministic cartridge extraction");

        Directory.CreateDirectory(overrideDirectory);
        JsonObject document = RequiredJson.Parse(deterministic).AsObject();
        document.Required("panels").Required(GameplayMessageId.GrappleBeam.ToString())["title"] =
            "GRAPPLE TEST";
        string replacement = Path.Combine(
            overrideDirectory, GameplayMessagePanelDefinitions.FileName);
        File.WriteAllText(replacement,
            document.ToJsonString(ArtworkJson.Options));
        AreaMapPresentation edited = MapPresentationFiles.Load(
            stockDirectory, overrideDirectory);
        var state = new GameplayMessageBoxState();
        state.Begin(GameplayMessageId.GrappleBeam, 0);
        AssertTrue(!(GameplayMessageArtwork.Of(stockCatalog).Panels ?? throw new InvalidOperationException("Stock installs the message panels."))
                .Build(GameplayMessageId.GrappleBeam).SequenceEqual(state.Tilemap(GameplayMessageArtwork.Of(edited))),
            "catalog override changes installed large-message title");
        AssertTrue(stockCatalog.ContentIdentity != edited.ContentIdentity,
            "gameplay-message panel override changes catalog content identity");

        File.WriteAllText(replacement, "{ broken gameplay-message panel");
        AssertThrows<InvalidDataException>(() =>
            MapPresentationFiles.Load(stockDirectory, overrideDirectory),
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
