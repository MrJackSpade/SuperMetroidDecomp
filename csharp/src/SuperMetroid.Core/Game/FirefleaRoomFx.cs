using SuperMetroid.Core.Hardware;
using FirefleaFxData = SuperMetroid.Core.Game.FirefleaFxRomData;

namespace SuperMetroid.Core.Game;

/// <summary>Fixed-color producer for the Fireflea room's otherwise inert HDMA object.</summary>
internal static class FirefleaRoomFx
{
    /// <summary>Sets the native six-update flash timer and clears the saved phase and darkness level.</summary>
    /// <param name="bus">The mapped address space used to initialize the Fireflea effect's WRAM words.</param>
    public static void Initialize(ISnesAddressSpace bus)
    {
        WriteWord(bus, FirefleaFxData.Timer, FirefleaFxDefinitions.FlashDuration);
        WriteWord(bus, FirefleaFxData.Index, 0);
        WriteWord(bus, FirefleaFxData.Darkness, 0);
    }

    /// <summary>Samples darkness and, unless frozen, advances the flashing phase and writes the fixed-color registers.</summary>
    /// <param name="bus">The address space used for effect state writes and PPU fixed-color mirrors.</param>
    /// <param name="memory">Live WRAM used to read the effect's timer and flashing index.</param>
    /// <param name="frozen">Whether to hold the timer and phase; the supplied darkness value is still recorded.</param>
    /// <param name="darkness">The current enemy-death darkness level, which selects cycling or steady flashing.</param>
    public static void Step(ISnesAddressSpace bus, ISnesMutableMemory memory, bool frozen, ushort darkness)
    {
        // The enemy system owns deaths; the effect only samples that counter. WRAM
        // owns the flashing phase so exact-state saves retain it without a second clock.
        WriteWord(bus, FirefleaFxData.Darkness, darkness);
        if (frozen) return;
        ushort timer = unchecked((ushort)(SnesWorkRam.ReadWord(memory, FirefleaFxData.Timer) - 1));
        ushort index = SnesWorkRam.ReadWord(memory, FirefleaFxData.Index);
        if (timer == 0)
        {
            timer = FirefleaFxDefinitions.FlashDuration;
            if (unchecked((short)(darkness - FirefleaFxDefinitions.SteadyDarknessThreshold)) < 0)
            {
                index = unchecked((ushort)(index + 1));
                if (index >= FirefleaFxDefinitions.FlashCount) index = 0;
            }
            else index = FirefleaFxDefinitions.SteadyFlashIndex;
            WriteWord(bus, FirefleaFxData.Index, index);
        }
        WriteWord(bus, FirefleaFxData.Timer, timer);

        // The cartridge permits darkness offset twelve, where it observes the adjacent
        // opcode word rather than clamping to the six authored shade records. That exact
        // seventh value is part of the compiled domain. Preserve the 16-bit addition
        // before taking its high byte.
        ushort combined = unchecked((ushort)(
            FirefleaFxDefinitions.FlashingShade(index) +
            FirefleaFxDefinitions.DarknessShade(darkness)));
        byte shade = (byte)(combined >> 8);
        bus.WriteByte(PpuFixedColorMirrors.Green, (byte)(shade | 0x80));
        bus.WriteByte(PpuFixedColorMirrors.Blue, (byte)(shade | 0x40));
        bus.WriteByte(PpuFixedColorMirrors.Red, (byte)(shade | 0x20));
    }

    /// <summary>Writes a 16-bit value to consecutive SNES addresses in little-endian byte order.</summary>
    /// <param name="bus">The address space receiving the two byte writes.</param>
    /// <param name="address">The destination address of the low byte.</param>
    /// <param name="value">The word to split into low and high bytes.</param>
    private static void WriteWord(ISnesAddressSpace bus, int address, ushort value)
    {
        bus.WriteByte(address, (byte)value);
        bus.WriteByte(address + 1, (byte)(value >> 8));
    }
}
