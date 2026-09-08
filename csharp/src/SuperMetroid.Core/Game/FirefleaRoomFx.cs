using SuperMetroid.Core.Hardware;
using FirefleaFxData = SuperMetroid.Core.Game.FirefleaFxRomData;

namespace SuperMetroid.Core.Game;

/// <summary>Fixed-color producer for the Fireflea room's otherwise inert HDMA object.</summary>
internal static class FirefleaRoomFx
{
    public static void Initialize(ISnesAddressSpace bus)
    {
        WriteWord(bus, FirefleaFxData.Timer, FirefleaFxData.FlashDuration);
        WriteWord(bus, FirefleaFxData.Index, 0);
        WriteWord(bus, FirefleaFxData.Darkness, 0);
    }

    public static void Step(ISnesAddressSpace bus, bool frozen, ushort darkness)
    {
        // The enemy system owns deaths; the effect only samples that counter. WRAM
        // owns the flashing phase so exact-state saves retain it without a second clock.
        WriteWord(bus, FirefleaFxData.Darkness, darkness);
        if (frozen) return;
        ushort timer = unchecked((ushort)(ReadWord(bus, FirefleaFxData.Timer) - 1));
        ushort index = ReadWord(bus, FirefleaFxData.Index);
        if (timer == 0)
        {
            timer = FirefleaFxData.FlashDuration;
            if (unchecked((short)(darkness - FirefleaFxData.SteadyDarknessThreshold)) < 0)
            {
                index = unchecked((ushort)(index + 1));
                if (index >= FirefleaFxData.FlashCount) index = 0;
            }
            else index = FirefleaFxData.SteadyFlashIndex;
            WriteWord(bus, FirefleaFxData.Index, index);
        }
        WriteWord(bus, FirefleaFxData.Timer, timer);

        // Read the ROM, including its adjacent bytes if the death counter exceeds the
        // six-entry table. The cartridge permits offset twelve; clamping it invents
        // behavior. Preserve the 16-bit addition before taking its high byte.
        ushort combined = unchecked((ushort)(
            ReadWord(bus, FirefleaFxData.FlashingShades + index * 2) +
            ReadWord(bus, FirefleaFxData.DarknessShades + darkness)));
        byte shade = (byte)(combined >> 8);
        bus.WriteByte(PpuFixedColorMirrors.Green, (byte)(shade | 0x80));
        bus.WriteByte(PpuFixedColorMirrors.Blue, (byte)(shade | 0x40));
        bus.WriteByte(PpuFixedColorMirrors.Red, (byte)(shade | 0x20));
    }

    private static ushort ReadWord(ISnesAddressSpace bus, int address) =>
        (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

    private static void WriteWord(ISnesAddressSpace bus, int address, ushort value)
    {
        bus.WriteByte(address, (byte)value);
        bus.WriteByte(address + 1, (byte)(value >> 8));
    }
}
