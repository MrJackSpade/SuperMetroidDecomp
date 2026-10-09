using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

/// <summary>Resolves reserve visual data once, validating the compiled native fill-to-shape mapping.</summary>
public static class PauseReserveTankExtractor
{
    /// <summary>Validates the native reserve fill-to-shape bindings, resolves tank anchors and palette, and serializes every referenced sprite frame.</summary>
    /// <param name="bus">Supported-cartridge address space containing reserve mappings, positions, palette attributes, and spritemaps.</param>
    /// <returns>UTF-8 JSON bytes for reserve-tank presentation data without energy mechanics.</returns>
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        for (int index = 0; index < 16; index++)
            if (RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), PauseReserveTankDefinitions.PartialMaps + index * 2) != PauseReserveTankDefinitions.PartialMap(index))
                throw new InvalidDataException("Unexpected native reserve fill binding.");
        var anchors = new MapLabelPoint[PauseReserveTankDefinitions.AnchorCount];
        int y = unchecked((ushort)(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), PauseReserveTankDefinitions.YPosition) - 1));
        for (int index = 0; index < anchors.Length; index++)
            anchors[index] = new(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), PauseReserveTankDefinitions.XPositions + index * 2), y);
        var frames = new Dictionary<string, SpriteVisualPart[]>();
        foreach (var definition in PauseReserveTankDefinitions.Frames()) frames.Add(definition.Name, MenuSpriteExtractor.Read(bus, definition.Id));
        using var output = new MemoryStream();
        PauseReserveTankPresentation.Write(output, new() { Version = PauseReserveTankDefinitions.Version,
            Palette = new SnesObjAttributeWord(PauseReserveTankDefinitions.PaletteBits).PaletteIndex, Anchors = anchors, Frames = frames });
        return output.ToArray();
    }
}
