using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.AssetExtraction;

/// <summary>Extracts only BGR555 payloads selected by the compiled room-flash program.</summary>
internal static class MotherBrainRoomColorExtractor
{
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        var flash = new PaletteRgb5[MotherBrainRoomPaletteProgramDefinitions.PresentationWordCount][];
        for (int index = 0; index < flash.Length; index++)
        {
            int pointerAddress = MotherBrainRoomColorRomData.SourceBank |
                MotherBrainRoomPaletteProgramDefinitions.PresentationWordAddress(index);
            ushort pointer = ReadWord(pointerAddress);
            if (pointer == 0)
                throw new InvalidDataException($"Mother Brain room-flash row {index} has no palette pointer.");
            flash[index] = ReadColors(MotherBrainRoomColorRomData.SourceBank | pointer,
                MotherBrainRoomColorRomData.SliceColors * 2);
        }
        using var json = new MemoryStream();
        MotherBrainRoomColorPresentation.Write(json, new MotherBrainRoomColorDocument
        {
            Version = MotherBrainRoomColorFormat.Version,
            Flash = flash,
            FinalRoom = ReadColors(MotherBrainRoomColorRomData.SourceBank |
                MotherBrainRoomPaletteProgramDefinitions.FinalPalette,
                MotherBrainRoomColorRomData.SliceColors * 2),
            PhaseTwoAttack = ReadColors(MotherBrainRoomColorRomData.PhaseTwoAttackSource,
                MotherBrainRoomColorRomData.PhaseTwoColors),
            PhaseTwoRearLeg = ReadColors(MotherBrainRoomColorRomData.PhaseTwoRearLegSource,
                MotherBrainRoomColorRomData.PhaseTwoColors),
        });
        return json.ToArray();

        PaletteRgb5[] ReadColors(int source, int count)
        {
            var colors = new PaletteRgb5[count];
            for (int index = 0; index < count; index++)
            {
                ushort word = ReadWord(source + index * sizeof(ushort));
                colors[index] = new PaletteRgb5
                {
                    Red = word & 31,
                    Green = word >> 5 & 31,
                    Blue = word >> 10 & 31,
                };
            }
            return colors;
        }

        ushort ReadWord(int address) => (ushort)(bus.ReadByte(address) |
            bus.ReadByte(address + 1) << 8);
    }
}
