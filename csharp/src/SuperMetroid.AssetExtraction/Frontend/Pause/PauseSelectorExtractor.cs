using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.Core;
using SuperMetroid.Core.Spatial;
using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Frontend.Pause.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Frontend.Pause.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Pause;

/// <summary>Separates selector artwork/timing/positions from the fixed low-byte category dispatcher.</summary>
public static class PauseSelectorExtractor
{
    /// <summary>The visual animation role of each equipment category; suit/miscellaneous items and boots share Equipment.</summary>
    private static readonly IReadOnlyDictionary<PauseEquipmentCategory, string> Groups = new Dictionary<PauseEquipmentCategory, string>
    {
        [PauseEquipmentCategory.Reserves] = "Reserve",
        [PauseEquipmentCategory.Beams] = "Beam",
        [PauseEquipmentCategory.Suits] = "Equipment",
        [PauseEquipmentCategory.Boots] = "Equipment",
    };

    /// <summary>Validates native selector category bindings, resolves anchors and sprite frames, and serializes its bounded animation phases and palette.</summary>
    /// <param name="bus">Supported-cartridge address space containing selector pointers, positions, spritemaps, animation bytes, and palette attributes.</param>
    /// <returns>UTF-8 JSON bytes for selector presentation, excluding the fixed category dispatcher.</returns>
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        if (RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), PauseMenuSourceDefinitions.PauseSelectorVariant) != PauseMenuSourceDefinitions.PauseSelectorCategoryVariable)
            throw new InvalidDataException("Unexpected native selector category binding.");
        var bases = new SnesAddress(PauseMenuSourceDefinitions.PauseSelectorBank.Bank, RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), PauseMenuSourceDefinitions.PauseSelectorBases));
        var columns = PauseMenuSourceDefinitions.SelectorColumns;
        for (int table = 0; table < columns.Length; table++)
            if (RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), bases.AddWithinBank(table * 2)) != PauseMenuSourceDefinitions.SelectorSpritemaps[columns[table][0].Category])
                throw new InvalidDataException("Unexpected native selector sprite binding.");
        var anchors = new Dictionary<string, Position16<SpritePixels>>();
        for (int table = 0; table < columns.Length; table++)
        {
            var position = new SnesAddress(PauseMenuSourceDefinitions.PauseSelectorBank.Bank, RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), PauseMenuSourceDefinitions.PauseSelectorPositionPointers.AddWithinBank(table * 2)));
            for (int item = 0; item < columns[table].Length; item++)
                anchors.Add(columns[table][item].Anchor, new(unchecked((ushort)(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), position.AddWithinBank(item * 4)) - 1)),
                    unchecked((ushort)(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), position.AddWithinBank(item * 4 + 2)) - 1))));
        }
        var frames = new Dictionary<string, SpriteVisualPart[]>();
        // Suits and boots share the Equipment frames, so only the first three categories own a group.
        foreach (PauseEquipmentCategory category in new[] { PauseEquipmentCategory.Reserves, PauseEquipmentCategory.Beams, PauseEquipmentCategory.Suits })
            frames.Add(Ensure.Listed(Groups, category), MenuSpriteExtractor.Read(bus, PauseMenuSourceDefinitions.SelectorSpritemaps[category]));
        int animation = (int)new SnesAddress(PauseMenuSourceDefinitions.PauseSelectorBank.Bank, RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), PauseMenuSourceDefinitions.PauseSelectorAnimation));
        var phases = new List<PauseSelectorPhase>();
        for (int i = 0; ; i++)
        {
            int duration = bus.ReadCartridgeByte(animation + i * 3);
            if (duration == byte.MaxValue) break;
            if (i >= PauseSelectorFormat.MaximumPhases || bus.ReadCartridgeByte(animation + i * 3 + 2) != 0)
                throw new InvalidDataException("Unexpected native selector frame offset or missing terminator.");
            phases.Add(new() { DurationTicks = duration, Reserve = "Reserve", Beam = "Beam", Equipment = "Equipment" });
        }
        var palette = new SnesObjAttributeWord(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), PauseMenuSourceDefinitions.PauseSelectorPalette));
        if (palette.Raw != palette.PaletteBits) throw new InvalidDataException("Invalid native selector palette word.");
        using var output = new MemoryStream();
        PauseSelectorSourcePresentation.Write(output, new() { Version = PauseSelectorFormat.Version,
            InitialDurationTicks = bus.ReadCartridgeByte((int)PauseMenuSourceDefinitions.PauseSelectorInitialTimer), Palette = palette.PaletteIndex,
            Anchors = anchors, Frames = frames, Animation = phases.ToArray() });
        return output.ToArray();
    }
}
