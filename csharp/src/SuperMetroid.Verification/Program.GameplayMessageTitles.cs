using System.Text.Json.Nodes;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Compares installed gameplay-message titles with cartridge tilemaps and verifies live presentation edits without ROM access.</summary>
    /// <param name="romPath">Path to the ROM used to extract stock title presentation and reference tilemap rows.</param>
    private static void VerifyGameplayMessageTitles(string romPath)
    {
        ISnesAddressSpace bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(romPath);
        byte[] extracted = SuperMetroid.AssetExtraction.GameplayMessageTitleExtractor.Extract(bus);
        GameplayMessageTitlePresentation stock = GameplayMessageTitlePresentation.Load(
            new MemoryStream(extracted, writable: false));

        int comparedWords = 0;
        int width = GameplayMessageRomData.Layout.TilemapWidth;
        foreach (GameplayMessageId id in GameplayMessageTitleDefinitions.MessageIds)
        {
            var installed = new GameplayMessageBoxState();
            installed.BindPresentation(stock);
            installed.Begin(new ForbiddenGameplayMessageBus(), id, 0);
            // The title's content row is the cartridge's one-row tilemap named by the
            // message definition table; read it directly as the independent reference.
            int definition = GameplayMessageRomData.Assets.DefinitionTable +
                ((byte)id - 1) * GameplayMessageRomData.Layout.DefinitionBytes;
            int content = GameplayMessageRomData.Assets.BankBase |
                (bus.ReadByte(definition + 4) | bus.ReadByte(definition + 5) << 8);
            ReadOnlySpan<ushort> row = installed.Tilemap.Slice(width, width);
            for (int column = 0; column < width; column++)
                AssertEqual((ushort)(bus.ReadByte(content + column * 2) | bus.ReadByte(content + column * 2 + 1) << 8),
                    row[column], $"installed UTF-8 gameplay title {id} column {column} matches the cartridge tilemap");
            comparedWords += width;
        }

        JsonObject document = JsonNode.Parse(extracted)!.AsObject();
        JsonObject energy = document["titles"]![GameplayMessageId.EnergyTank.ToString()]!.AsObject();
        energy["text"] = "ENERGY TEST";
        byte[] editedBytes = System.Text.Encoding.UTF8.GetBytes(document.ToJsonString(
            MapPresentationFormat.JsonOptions));
        GameplayMessageTitlePresentation edited = GameplayMessageTitlePresentation.Load(
            new MemoryStream(editedBytes, writable: false));

        var active = new GameplayMessageBoxState();
        active.BindPresentation(stock);
        active.Begin(new ForbiddenGameplayMessageBus(), GameplayMessageId.EnergyTank, 0);
        active.Step(0);
        GameplayMessageBoxPhase phase = active.Phase;
        int radius = active.RadiusPixels;
        ushort[] before = active.Tilemap.ToArray();
        active.BindPresentation(edited);
        AssertEqual(phase, active.Phase, "message-title content rebind preserves coroutine phase");
        AssertEqual(radius, active.RadiusPixels, "message-title content rebind preserves window radius");
        AssertTrue(!before.AsSpan().SequenceEqual(active.Tilemap),
            "UTF-8 title edit reaches an already active message without a ROM patch");

        GameplayMessageTitlePresentation restored = GameplayMessageTitlePresentation.Load(
            new MemoryStream(extracted, writable: false));
        active.BindPresentation(restored);
        AssertTrue(before.AsSpan().SequenceEqual(active.Tilemap),
            "restoring stock UTF-8 title restores exact live tilemap");

        byte[] malformed = System.Text.Encoding.UTF8.GetBytes(document.ToJsonString(
            MapPresentationFormat.JsonOptions).Replace("ENERGY TEST", "energy test"));
        AssertThrows<InvalidDataException>(() => GameplayMessageTitlePresentation.Load(
            new MemoryStream(malformed, writable: false)),
            "gameplay-message title rejects glyphs absent from its documented font mapping");

        Console.WriteLine(
            $"Gameplay-message titles: {GameplayMessageTitleDefinitions.MessageIds.Length} UTF-8 titles and {comparedWords} installed words match the cartridge with message ROM reads forbidden; live edit/rebind and invalid glyph rejection pass.");
    }

    /// <summary>Address-space test double that fails immediately if installed gameplay-message code accesses cartridge memory.</summary>
    private sealed class ForbiddenGameplayMessageBus : ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Routes cartridge-byte requests to the rejecting address-space read.</summary>
        /// <param name="address">Cartridge address requested by the caller.</param>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Rejects every cartridge read to ensure title rendering uses installed presentation data.</summary>
        /// <param name="address">Address whose access would indicate a forbidden cartridge dependency.</param>
        public static byte ReadByte(int address) => throw new InvalidOperationException(
            $"Installed gameplay-message title read cartridge address ${address:X6}.");

        /// <summary>Rejects every cartridge write during the installed-title verification.</summary>
        /// <param name="address">Address whose modification would violate the read-only verification contract.</param>
        /// <param name="value">Byte that the caller attempted to write.</param>
        public void WriteByte(int address, byte value) => throw new InvalidOperationException(
            $"Installed gameplay-message title wrote cartridge address ${address:X6}.");
    }
}
