using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

/// <summary>Resolves reserve visual data once, validating the compiled native fill-to-shape mapping.</summary>
public static class PauseReserveTankExtractor
{
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        for (int index = 0; index < 16; index++)
            if (RomDataReader.ReadWordFixedBank(bus, PauseReserveTankDefinitions.PartialMaps + index * 2) != PauseReserveTankDefinitions.PartialMap(index))
                throw new InvalidDataException("Unexpected native reserve fill binding.");
        var anchors = new MapLabelPoint[PauseReserveTankDefinitions.AnchorCount];
        int y = unchecked((ushort)(RomDataReader.ReadWordFixedBank(bus, PauseReserveTankDefinitions.YPosition) - 1));
        for (int index = 0; index < anchors.Length; index++)
            anchors[index] = new(RomDataReader.ReadWordFixedBank(bus, PauseReserveTankDefinitions.XPositions + index * 2), y);
        var frames = new Dictionary<string, SpriteVisualPart[]>();
        foreach (var definition in PauseReserveTankDefinitions.Frames()) frames.Add(definition.Name, MenuSpriteExtractor.Read(bus, definition.Id));
        using var output = new MemoryStream();
        PauseReserveTankPresentation.Write(output, new() { Version = PauseReserveTankDefinitions.Version,
            Palette = new SnesObjAttributeWord(PauseReserveTankDefinitions.PaletteBits).PaletteIndex, Anchors = anchors, Frames = frames });
        return output.ToArray();
    }
}
