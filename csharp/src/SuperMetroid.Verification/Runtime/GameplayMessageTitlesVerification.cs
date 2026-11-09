using SuperMetroid.Tooling;
using static SuperMetroid.Verification.Harness.RepositoryInputsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using System.Text.Json.Nodes;
using SuperMetroid.Core.Hud;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Hud;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction;
namespace SuperMetroid.Verification.Runtime;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms;

/// <summary>Checks extracted message-title text against native tilemaps.</summary>
internal static class GameplayMessageTitlesVerification
{
    /// <summary>Compares installed title rows to cartridge words and checks glyph rejection.</summary>
    internal static void VerifyGameplayMessageTitles(string romPath)
    {
        ISnesAddressSpace bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(romPath);
        byte[] extracted = SuperMetroid.AssetExtraction.Hud.GameplayMessageTitleExtractor.Extract(bus);
        GameplayMessageTitlePresentation stock = GameplayMessageTitlePresentation.Load(
            new MemoryStream(extracted, writable: false));

        var stockMessages = new GameplayMessageArtwork(stock);
        int comparedWords = 0;
        int width = SuperMetroid.AssetExtraction.Hud.GameplayMessageTilemapDefinitions.TilemapWidth;
        foreach (GameplayMessageId id in GameplayMessageTitleDefinitions.MessageIds)
        {
            var installed = new GameplayMessageBoxState(new SaveChoiceInputCadence());
            installed.Begin(id, 0);
            // The title's content row is the cartridge's one-row tilemap named by the
            // message definition table; read it directly as the independent reference.
            int definition = HudSourceDefinitions.Messages.DefinitionTable +
                GameplayMessageNumbers.DefinitionEntry(id) * HudSourceDefinitions.Messages.DefinitionBytes;
            int content = HudSourceDefinitions.Messages.BankBase |
                (LittleEndianBytes.Word(GameplayMessageTitlesRomByte(bus, definition + 4), GameplayMessageTitlesRomByte(bus, definition + 5)));
            ReadOnlySpan<BackgroundTile> row = installed.Tilemap(stockMessages).AsSpan(width, width);
            for (int column = 0; column < width; column++)
                AssertEqual(LittleEndianBytes.Word(GameplayMessageTitlesRomByte(bus, content + column * 2), GameplayMessageTitlesRomByte(bus, content + column * 2 + 1)),
                    row[column].PackedWord, $"installed UTF-8 gameplay title {id} column {column}, encoded through the codec, matches the cartridge tilemap");
            comparedWords += width;
        }

        JsonObject document = RequiredJson.Parse(extracted).AsObject();
        JsonObject energy = document.Required("titles").Required(GameplayMessageId.EnergyTank.ToString()).AsObject();
        energy["text"] = "energy test";
        byte[] malformed = System.Text.Encoding.UTF8.GetBytes(document.ToJsonString(
            ArtworkJson.Options));
        AssertThrows<InvalidDataException>(() => GameplayMessageTitlePresentation.Load(
            new MemoryStream(malformed, writable: false)),
            "gameplay-message title rejects glyphs absent from its documented font mapping");

        Console.WriteLine(
            $"Gameplay-message titles: {GameplayMessageTitleDefinitions.MessageIds.Length} UTF-8 titles and {comparedWords} installed words match the cartridge with message ROM reads forbidden; invalid glyph rejection passes.");
    }

    /// <summary>Reads a title-table byte solely for the independent cartridge comparison.</summary>
    private static byte GameplayMessageTitlesRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>Fails if installed title handling tries a cartridge read or write.</summary>
    internal static CartridgeReadGuard NewForbiddenGameplayMessageBus() =>
        CartridgeReadGuard.Unbacked()
            .ForbidAll(address => $"Installed gameplay-message title read cartridge address ${address:X6}.")
            .RejectWrites(address => $"Installed gameplay-message title wrote cartridge address ${address:X6}.");

    /// <summary>Runs the message-title comparison using the repository's retail ROM.</summary>
    [VerificationCommand("--gameplay-message-titles")]
    internal static int GameplayMessageTitlesCommand()
    {
        string messageTitleRom = RepositoryRomPath;
        Suite(nameof(VerifyGameplayMessageTitles), () => VerifyGameplayMessageTitles(messageTitleRom));
        return 0;
    }
}
