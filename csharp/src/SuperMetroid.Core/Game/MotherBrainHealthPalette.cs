using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

/// <summary>Ports $AD:E3D5's strict health comparisons and three palette copies.</summary>
public static class MotherBrainHealthPalette
{
    public static void Apply(ISnesAddressSpace bus, SnesCgram cgram, ushort health,
        MotherBrainHealthPalettePresentation? presentation = null)
    {
        int index = health >= MotherBrainHealthPaletteRomData.FirstThreshold ? 0 :
            health >= MotherBrainHealthPaletteRomData.SecondThreshold ? 1 :
            health >= MotherBrainHealthPaletteRomData.FinalThreshold ? 2 : 3;
        if (presentation is not null)
        {
            presentation.Apply(cgram, index);
            return;
        }
        IImportCartridgeSource cartridge = bus as IImportCartridgeSource ??
            throw new InvalidOperationException(
                "Uncompiled Mother Brain health palettes require a cartridge import source.");
        int body = ReadPointer(MotherBrainHealthPaletteRomData.BrainTable);
        int leg = ReadPointer(MotherBrainHealthPaletteRomData.BackLegTable);
        cgram.LoadFromBus(bus, body, MotherBrainRainbowPaletteRomData.ColorCount, MotherBrainRainbowPaletteRomData.BodyColor);
        cgram.LoadFromBus(bus, body, MotherBrainRainbowPaletteRomData.ColorCount, MotherBrainRainbowPaletteRomData.BrainColor);
        cgram.LoadFromBus(bus, leg, MotherBrainRainbowPaletteRomData.ColorCount, MotherBrainRainbowPaletteRomData.SecondaryColor);

        int ReadPointer(int table)
        {
            int address = table + index * 2;
            return MotherBrainRainbowPaletteRomData.SourceBank |
                cartridge.ReadCartridgeByte(address) |
                cartridge.ReadCartridgeByte(address + 1) << 8;
        }
    }
}
