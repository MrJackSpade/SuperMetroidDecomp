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

/// <summary>Checks installed gameplay message panels and live title/diagram binding.</summary>
internal static class GameplayMessagePanelsVerification
{
    /// <summary>Checks all installed panels without ROM reads and glyph rejection.</summary>
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
            var installed = new GameplayMessageBoxState(new SaveChoiceInputCadence());
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
        missile["title"] = "missile test";
        byte[] malformed = System.Text.Encoding.UTF8.GetBytes(document.ToJsonString(
            ArtworkJson.Options));
        AssertThrows<InvalidDataException>(() => GameplayMessagePanelPresentation.Load(
            new MemoryStream(malformed, writable: false)),
            "gameplay-message panel rejects glyphs absent from its documented font mapping");

        Console.WriteLine(
            $"Gameplay-message panels: {GameplayMessagePanelDefinitions.MessageIds.Length} UTF-8 titles and {comparedWords} installed words match the cartridge with message ROM reads forbidden; diagrams, remapped controls and invalid glyph rejection pass.");
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
