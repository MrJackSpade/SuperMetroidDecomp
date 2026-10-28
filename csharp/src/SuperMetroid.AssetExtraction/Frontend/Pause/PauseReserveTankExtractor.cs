using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Spatial;
using SuperMetroid.Core.Frontend.Pause.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.CartridgeTables;

namespace SuperMetroid.AssetExtraction.Frontend.Pause;

/// <summary>Resolves reserve visual data once, validating the compiled native fill-to-shape mapping.</summary>
public static class PauseReserveTankExtractor
{
    /// <summary>Validates the native reserve fill-to-shape bindings, resolves tank anchors and palette, and serializes every referenced sprite frame.</summary>
    /// <param name="bus">Supported-cartridge address space containing reserve mappings, positions, palette attributes, and spritemaps.</param>
    /// <returns>UTF-8 JSON bytes for reserve-tank presentation data without energy mechanics.</returns>
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        for (int index = 0; index < PauseMenuSourceDefinitions.ReserveTankPartialFillCount; index++)
            if (RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), PauseMenuSourceDefinitions.ReserveTankPartialFills + index * 2) != (ushort)PauseReserveTankParts.PartialMap(index))
                throw new InvalidDataException("Unexpected native reserve fill binding.");
        var anchors = new Position16<SpritePixels>[PauseReserveTankDefinitions.AnchorCount];
        int y = unchecked((ushort)(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), PauseMenuSourceDefinitions.ReserveTankYPosition) - 1));
        for (int index = 0; index < anchors.Length; index++)
            anchors[index] = new(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), PauseMenuSourceDefinitions.ReserveTankXPositions + index * 2), checked((ushort)y));
        var frames = new Dictionary<string, SpriteVisualPart[]>();
        foreach (var definition in PauseReserveTankParts.Frames()) frames.Add(definition.Name, MenuSpriteExtractor.Read(bus, (ushort)definition.Id));
        using var output = new MemoryStream();
        PauseReserveTankPresentation.Write(output, new() { Version = PauseReserveTankDefinitions.Version,
            Palette = PauseReserveTankDefinitions.Palette, Anchors = anchors, Frames = frames });
        return output.ToArray();
    }
}
