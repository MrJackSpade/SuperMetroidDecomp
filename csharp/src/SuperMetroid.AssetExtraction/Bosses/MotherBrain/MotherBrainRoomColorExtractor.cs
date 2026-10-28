using SuperMetroid.Core;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.CartridgeTables;

namespace SuperMetroid.AssetExtraction.Bosses.MotherBrain;

/// <summary>Extracts only BGR555 payloads selected by the compiled room-flash program.</summary>
internal static class MotherBrainRoomColorExtractor
{
    /// <summary>Locates the palette operand of room-flash frame <paramref name="index"/> in the cartridge's flash list.</summary>
    internal static ushort FlashPaletteOperandAddress(int index) =>
        (uint)index < MotherBrainRoomPaletteProgramDefinitions.PresentationWordCount
            ? checked((ushort)(MotherBrainRoomFlashSourceDefinitions.FlashList +
                index * MotherBrainRoomFlashSourceDefinitions.FlashEntryBytes +
                MotherBrainRoomFlashSourceDefinitions.FlashPaletteOperandOffset))
            : throw new ArgumentOutOfRangeException(nameof(index));

    /// <summary>Imports room-flash rows and selected encounter palettes into an editable RGB5 document.</summary>
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        Ensure.NotNull(bus);
        var flash = new PaletteRgb5[MotherBrainRoomPaletteProgramDefinitions.PresentationWordCount][];
        for (int index = 0; index < flash.Length; index++)
        {
            int pointerAddress = MotherBrainSourceDefinitions.RoomColors.FlashBank |
                FlashPaletteOperandAddress(index);
            ushort pointer = CartridgeImportSource.Require(bus).ReadCartridgeWord(pointerAddress);
            if (pointer == 0)
                throw new InvalidDataException($"Mother Brain room-flash row {index} has no palette pointer.");
            flash[index] = ReadColors(MotherBrainSourceDefinitions.RoomColors.FlashBank | pointer,
                MotherBrainRoomColorDefinitions.SliceColors * 2);
        }
        using var json = new MemoryStream();
        MotherBrainRoomColorFile.Write(json, new MotherBrainRoomColorDocument
        {
            Version = MotherBrainRoomColorFormat.Version,
            Flash = flash,
            FinalRoom = ReadColors(MotherBrainSourceDefinitions.RoomColors.FlashBank |
                MotherBrainRoomFlashSourceDefinitions.FinalPalette,
                MotherBrainRoomColorDefinitions.SliceColors * 2),
            PhaseTwoAttack = ReadColors(MotherBrainSourceDefinitions.RoomColors.PhaseTwoAttackColors,
                MotherBrainRoomColorDefinitions.PhaseTwoColors),
            PhaseTwoRearLeg = ReadColors(MotherBrainSourceDefinitions.RoomColors.PhaseTwoRearLegColors,
                MotherBrainRoomColorDefinitions.PhaseTwoColors),
            InitialGlassShard = ReadColors(MotherBrainSourceDefinitions.RoomColors.InitialGlassShardColors,
                MotherBrainRoomColorDefinitions.InitialColors),
            InitialTubeProjectile = ReadColors(MotherBrainSourceDefinitions.RoomColors.InitialTubeProjectileColors,
                MotherBrainRoomColorDefinitions.InitialColors),
            RecoveryLights = Enumerable.Range(0, MotherBrainRoomColorDefinitions.RecoveryLightsFrames)
                .Select(frame => ReadColors(MotherBrainSourceDefinitions.RoomColors.RecoveryLightsColors(frame),
                    MotherBrainRoomColorDefinitions.RecoveryLightsSliceColors * 2))
                .ToArray(),
        });
        return json.ToArray();

        PaletteRgb5[] ReadColors(int source, int count)
        {
            var colors = new PaletteRgb5[count];
            for (int index = 0; index < count; index++)
            {
                ushort word = CartridgeImportSource.Require(bus).ReadCartridgeWord((source + index * sizeof(ushort)));
                colors[index] = new PaletteRgb5
                {
                    Red = word & 31,
                    Green = word >> 5 & 31,
                    Blue = word >> 10 & 31,
                };
            }
            return colors;
        }
    }
}
