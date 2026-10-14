namespace SuperMetroid.Core.Enemies.Fireflea;

/// <summary>Fixed-color producer for the Fireflea room's otherwise inert HDMA object.</summary>
internal static class FirefleaRoomFx
{
    /// <summary>Seeds WRAM with the initial flash duration, first shade index, and undarkened room state.</summary>
    public static void Initialize(ISnesMemoryBus bus)
    {
        var fx = new FirefleaFxMemory(bus);
        fx.Timer = FirefleaFxDefinitions.FlashDuration;
        fx.Index = 0;
        fx.Darkness = 0;
    }

    /// <summary>Samples enemy-owned darkness, advances the WRAM flash clock unless frozen, and writes fixed color using native 16-bit shade addition, including the seventh offset value.</summary>
    public static void Step(ISnesMemoryBus bus, bool frozen, ushort darkness)
    {
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
