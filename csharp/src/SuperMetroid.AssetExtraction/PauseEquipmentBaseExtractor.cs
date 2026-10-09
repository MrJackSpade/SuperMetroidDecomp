using System.Buffers.Binary;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

/// <summary>Converts the native equipment-page template to semantic atlas references.</summary>
public static class PauseEquipmentBaseExtractor
{
    /// <summary>Converts the fixed native equipment-page tilemap into semantic map/interface atlas cells and serializes the versioned template.</summary>
    /// <param name="bus">Supported-cartridge address space containing the equipment-page base tilemap.</param>
    /// <returns>UTF-8 JSON bytes for the complete equipment-page base grid.</returns>
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        byte[] source = RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus), PauseEquipmentBaseDefinitions.Source,
            PauseEquipmentBaseDefinitions.Cells * sizeof(ushort));
        var cells = new PauseBackdropCell[PauseEquipmentBaseDefinitions.Cells];
        for (int index = 0; index < cells.Length; index++)
            cells[index] = PauseTileGrid.FromWord(BinaryPrimitives.ReadUInt16LittleEndian(source.AsSpan(index * 2)),
                $"EquipmentBase.{index}");
        using var output = new MemoryStream();
        PauseEquipmentBasePresentation.Write(output, new() { Version = PauseEquipmentBaseDefinitions.Version, Cells = cells });
        return output.ToArray();
    }
}
