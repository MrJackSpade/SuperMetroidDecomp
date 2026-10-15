using SuperMetroid.Tooling;
using static SuperMetroid.Verification.Harness.RepositoryInputsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using System.Text.Json.Nodes;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Hud.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
namespace SuperMetroid.Verification.Runtime;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Assets;

/// <summary>Checks extracted message-title text against native tilemaps and live presentation rebinding.</summary>
internal static class GameplayMessageTitlesVerification
{
    /// <summary>Compares installed title rows to cartridge words, then exercises a live UTF-8 edit and restore.</summary>
    internal static void VerifyGameplayMessageTitles(string romPath)
    {
        ISnesAddressSpace bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(romPath);
        byte[] extracted = SuperMetroid.AssetExtraction.Hud.GameplayMessageTitleExtractor.Extract(bus);
        GameplayMessageTitlePresentation stock = GameplayMessageTitlePresentation.Load(
            new MemoryStream(extracted, writable: false));

        int comparedWords = 0;
        int width = GameplayMessageRomData.Layout.TilemapWidth;
        foreach (GameplayMessageId id in GameplayMessageTitleDefinitions.MessageIds)
        {
            var installed = new GameplayMessageBoxState();
            installed.BindPresentation(stock);
            installed.Begin(NewForbiddenGameplayMessageBus(), id, 0);
            // The title's content row is the cartridge's one-row tilemap named by the
            // message definition table; read it directly as the independent reference.
            int definition = GameplayMessageRomData.Assets.DefinitionTable +
                ((byte)id - 1) * GameplayMessageRomData.Layout.DefinitionBytes;
            int content = GameplayMessageRomData.Assets.BankBase |
                (SnesLittleEndian.Word(GameplayMessageTitlesRomByte(bus, definition + 4), GameplayMessageTitlesRomByte(bus, definition + 5)));
            ReadOnlySpan<ushort> row = installed.Tilemap.Slice(width, width);
            for (int column = 0; column < width; column++)
                AssertEqual((ushort)(SnesLittleEndian.Word(GameplayMessageTitlesRomByte(bus, content + column * 2), GameplayMessageTitlesRomByte(bus, content + column * 2 + 1))),
                    row[column], $"installed UTF-8 gameplay title {id} column {column} matches the cartridge tilemap");
            comparedWords += width;
        }

        JsonObject document = RequiredJson.Parse(extracted).AsObject();
        JsonObject energy = document.Required("titles").Required(GameplayMessageId.EnergyTank.ToString()).AsObject();
        energy["text"] = "ENERGY TEST";
        byte[] editedBytes = System.Text.Encoding.UTF8.GetBytes(document.ToJsonString(
            ArtworkJson.Options));
        GameplayMessageTitlePresentation edited = GameplayMessageTitlePresentation.Load(
            new MemoryStream(editedBytes, writable: false));

        var active = new GameplayMessageBoxState();
        active.BindPresentation(stock);
        active.Begin(NewForbiddenGameplayMessageBus(), GameplayMessageId.EnergyTank, 0);
        active.Step(0);
        GameplayMessageBoxPhase phase = active.Phase;
        int radius = active.RadiusPixels;
        ushort[] before = active.Tilemap.ToArray();
        active.BindPresentation(edited);
        AssertEqual(phase, active.Phase, "message-title content rebind preserves coroutine phase");
        AssertEqual(radius, active.RadiusPixels, "message-title content rebind preserves window radius");
        AssertTrue(!before.SequenceEqual(active.Tilemap),
            "UTF-8 title edit reaches an already active message without a ROM patch");

        GameplayMessageTitlePresentation restored = GameplayMessageTitlePresentation.Load(
            new MemoryStream(extracted, writable: false));
        active.BindPresentation(restored);
        AssertTrue(before.SequenceEqual(active.Tilemap),
            "restoring stock UTF-8 title restores exact live tilemap");

        byte[] malformed = System.Text.Encoding.UTF8.GetBytes(document.ToJsonString(
            ArtworkJson.Options).Replace("ENERGY TEST", "energy test"));
        AssertThrows<InvalidDataException>(() => GameplayMessageTitlePresentation.Load(
            new MemoryStream(malformed, writable: false)),
            "gameplay-message title rejects glyphs absent from its documented font mapping");

        Console.WriteLine(
            $"Gameplay-message titles: {GameplayMessageTitleDefinitions.MessageIds.Length} UTF-8 titles and {comparedWords} installed words match the cartridge with message ROM reads forbidden; live edit/rebind and invalid glyph rejection pass.");
    }

    /// <summary>Reads a title-table byte solely for the independent cartridge comparison.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for gameplay message titles")]
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
