using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

/// <summary>Separates selector artwork/timing/positions from the fixed low-byte category dispatcher.</summary>
public static class PauseSelectorExtractor
{
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        if (RomDataReader.ReadWordFixedBank(bus, PauseSelectorDefinitions.VariantPointer) != PauseSelectorDefinitions.CategoryVariable)
            throw new InvalidDataException("Unexpected native selector category binding.");
        int bases = PauseSelectorDefinitions.Bank | RomDataReader.ReadWordFixedBank(bus, PauseSelectorDefinitions.BasePointer);
        for (int category = 0; category < 4; category++)
            if (RomDataReader.ReadWordFixedBank(bus, bases + category * 2) != PauseSelectorDefinitions.NativeSpriteId(category))
                throw new InvalidDataException("Unexpected native selector sprite binding.");
        var anchors = new Dictionary<string, MapLabelPoint>();
        foreach (var anchor in PauseSelectorDefinitions.Anchors())
        {
            int position = PauseSelectorDefinitions.Bank | RomDataReader.ReadWordFixedBank(bus, PauseSelectorDefinitions.PositionPointers + anchor.Category * 2);
            anchors.Add(anchor.Name, new(unchecked((ushort)(RomDataReader.ReadWordFixedBank(bus, position + anchor.Item * 4) - 1)),
                unchecked((ushort)(RomDataReader.ReadWordFixedBank(bus, position + anchor.Item * 4 + 2) - 1))));
        }
        var frames = new Dictionary<string, SpriteVisualPart[]>();
        for (int category = 0; category < 3; category++)
            frames.Add(PauseSelectorDefinitions.Group(category), MenuSpriteExtractor.Read(bus, PauseSelectorDefinitions.NativeSpriteId(category)));
        int animation = PauseSelectorDefinitions.Bank | RomDataReader.ReadWordFixedBank(bus, PauseSelectorDefinitions.AnimationPointer);
        var phases = new List<PauseSelectorPhase>();
        for (int i = 0; ; i++)
        {
            int duration = bus.ReadByte(animation + i * 3);
            if (duration == byte.MaxValue) break;
            if (i >= PauseSelectorDefinitions.MaximumPhases || bus.ReadByte(animation + i * 3 + 2) != 0)
                throw new InvalidDataException("Unexpected native selector frame offset or missing terminator.");
            phases.Add(new() { DurationTicks = duration, Reserve = "Reserve", Beam = "Beam", Equipment = "Equipment" });
        }
        var palette = new SnesObjAttributeWord(RomDataReader.ReadWordFixedBank(bus, PauseSelectorDefinitions.PaletteSource));
        if (palette.Raw != palette.PaletteBits) throw new InvalidDataException("Invalid native selector palette word.");
        using var output = new MemoryStream();
        PauseSelectorPresentation.Write(output, new() { Version = PauseSelectorDefinitions.Version,
            InitialDurationTicks = bus.ReadByte(PauseSelectorDefinitions.InitialTimer), Palette = palette.PaletteIndex,
            Anchors = anchors, Frames = frames, Animation = phases.ToArray() });
        return output.ToArray();
    }
}
