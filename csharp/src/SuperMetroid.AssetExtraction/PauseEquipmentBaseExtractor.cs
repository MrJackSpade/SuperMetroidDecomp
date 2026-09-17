using System.Buffers.Binary;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

/// <summary>Converts the native equipment-page template to semantic atlas references.</summary>
public static class PauseEquipmentBaseExtractor
{
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        byte[] source = RomDataReader.ReadFixedBank(bus, PauseEquipmentBaseDefinitions.Source,
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
