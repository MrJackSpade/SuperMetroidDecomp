using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

/// <summary>Separates selector artwork/timing/positions from the fixed low-byte category dispatcher.</summary>
public static class PauseSelectorExtractor
{
    /// <summary>Validates native selector category bindings, resolves anchors and sprite frames, and serializes its bounded animation phases and palette.</summary>
    /// <param name="bus">Supported-cartridge address space containing selector pointers, positions, spritemaps, animation bytes, and palette attributes.</param>
    /// <returns>UTF-8 JSON bytes for selector presentation, excluding the fixed category dispatcher.</returns>
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        if (RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), PauseSelectorDefinitions.VariantPointer) != PauseSelectorDefinitions.CategoryVariable)
            throw new InvalidDataException("Unexpected native selector category binding.");
        int bases = PauseSelectorDefinitions.Bank | RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), PauseSelectorDefinitions.BasePointer);
        for (int category = 0; category < 4; category++)
            if (RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), bases + category * 2) != PauseSelectorDefinitions.NativeSpriteId(category))
                throw new InvalidDataException("Unexpected native selector sprite binding.");
        var anchors = new Dictionary<string, MapLabelPoint>();
        foreach (var anchor in PauseSelectorDefinitions.Anchors())
        {
            int position = PauseSelectorDefinitions.Bank | RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), PauseSelectorDefinitions.PositionPointers + anchor.Category * 2);
            anchors.Add(anchor.Name, new(unchecked((ushort)(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), position + anchor.Item * 4) - 1)),
                unchecked((ushort)(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), position + anchor.Item * 4 + 2) - 1))));
        }
        var frames = new Dictionary<string, SpriteVisualPart[]>();
        for (int category = 0; category < 3; category++)
            frames.Add(PauseSelectorDefinitions.Group(category), MenuSpriteExtractor.Read(bus, PauseSelectorDefinitions.NativeSpriteId(category)));
        int animation = PauseSelectorDefinitions.Bank | RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), PauseSelectorDefinitions.AnimationPointer);
        var phases = new List<PauseSelectorPhase>();
        for (int i = 0; ; i++)
        {
            int duration = bus.ReadCartridgeByte(animation + i * 3);
            if (duration == byte.MaxValue) break;
            if (i >= PauseSelectorDefinitions.MaximumPhases || bus.ReadCartridgeByte(animation + i * 3 + 2) != 0)
                throw new InvalidDataException("Unexpected native selector frame offset or missing terminator.");
            phases.Add(new() { DurationTicks = duration, Reserve = "Reserve", Beam = "Beam", Equipment = "Equipment" });
        }
        var palette = new SnesObjAttributeWord(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), PauseSelectorDefinitions.PaletteSource));
        if (palette.Raw != palette.PaletteBits) throw new InvalidDataException("Invalid native selector palette word.");
        using var output = new MemoryStream();
        PauseSelectorPresentation.Write(output, new() { Version = PauseSelectorDefinitions.Version,
            InitialDurationTicks = bus.ReadCartridgeByte(PauseSelectorDefinitions.InitialTimer), Palette = palette.PaletteIndex,
            Anchors = anchors, Frames = frames, Animation = phases.ToArray() });
        return output.ToArray();
    }
}
