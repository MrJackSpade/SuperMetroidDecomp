using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

/// <summary>Fixed-color producer for the Fireflea room's otherwise inert HDMA object.</summary>
internal static class FirefleaRoomFx
{
    /// <summary>Initialize.</summary>
    public static void Initialize(ISnesMemoryBus bus)
    {
        var fx = new FirefleaFxMemory(bus);
        fx.Timer = FirefleaFxDefinitions.FlashDuration;
        fx.Index = 0;
        fx.Darkness = 0;
    }

    /// <summary>Step.</summary>
    public static void Step(ISnesMemoryBus bus, bool frozen, ushort darkness)
    {
        // The enemy system owns deaths; the effect only samples that counter. WRAM
        // owns the flashing phase so exact-state saves retain it without a second clock.
        var fx = new FirefleaFxMemory(bus);
        fx.Darkness = darkness;
        if (frozen) return;
        ushort timer = unchecked((ushort)(fx.Timer - 1));
        ushort index = fx.Index;
        if (timer == 0)
        {
            timer = FirefleaFxDefinitions.FlashDuration;
            if (unchecked((short)(darkness - FirefleaFxDefinitions.SteadyDarknessThreshold)) < 0)
            {
                index = unchecked((ushort)(index + 1));
                if (index >= FirefleaFxDefinitions.FlashCount) index = 0;
            }
            else index = FirefleaFxDefinitions.SteadyFlashIndex;
            fx.Index = index;
        }
        fx.Timer = timer;

        // The cartridge permits darkness offset twelve, where it observes the adjacent
        // opcode word rather than clamping to the six authored shade records. That exact
        // seventh value is part of the compiled domain. Preserve the 16-bit addition
        // before taking its high byte.
        ushort combined = unchecked((ushort)(
            FirefleaFxDefinitions.FlashingShade(index) +
            FirefleaFxDefinitions.DarknessShade(darkness)));
        byte shade = SnesLittleEndian.HighByte(combined);
        var fixedColor = new FixedColorMemory(bus);
        fixedColor.Green = (byte)(shade | 0x80);
        fixedColor.Blue = (byte)(shade | 0x40);
        fixedColor.Red = (byte)(shade | 0x20);
    }
}
