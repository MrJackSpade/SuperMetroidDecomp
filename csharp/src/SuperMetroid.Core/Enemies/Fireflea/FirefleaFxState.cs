namespace SuperMetroid.Core.Enemies.Fireflea;

/// <summary>
/// FX $24's state: the flash clock that <c>$88:B07C</c> initializes and its pre-instruction <c>$88:B0BC</c> steps, and
/// the darkness level the dying Firefleas advance. The cartridge keeps these in WRAM (<c>$7E:1778</c>, <c>$177A</c>,
/// <c>$177E</c>); the room FX state owns them for the session, so a room load does not reset them.
/// </summary>
public sealed class FirefleaFxState
{
    /// <summary><c>FirefleaFlashing_Timer</c>: frames until the next flashing-index step; held while time is frozen.</summary>
    public ushort Timer { get; private set; }

    /// <summary><c>FirefleaFlashing_Index</c>: the current flashing-table element index.</summary>
    public ushort Index { get; private set; }

    /// <summary><c>FirefleaFlashing_DarknessLevel</c>: the level published from the enemy-owned death counter.</summary>
    public ushort Darkness { get; private set; }

    /// <summary>Seeds the initial flash duration, first shade index, and undarkened room state.</summary>
    internal void Initialize()
    {
        Timer = FirefleaFxDefinitions.FlashDuration;
        Index = 0;
        Darkness = 0;
    }

    /// <summary>
    /// Samples enemy-owned darkness, advances the flash clock unless frozen, and writes the shared fixed colour using
    /// native 16-bit shade addition, including the seventh offset value.
    /// </summary>
    internal void Step(FixedColorState fixedColor, bool frozen, ushort darkness)
    {
        Ensure.NotNull(fixedColor);
        Darkness = darkness;
        if (frozen) return;
        ushort timer = unchecked((ushort)(Timer - 1));
        ushort index = Index;
        if (timer == 0)
        {
            timer = FirefleaFxDefinitions.FlashDuration;
            if (unchecked((short)(darkness - FirefleaFxDefinitions.SteadyDarknessThreshold)) < 0)
            {
                index = unchecked((ushort)(index + 1));
                if (index >= FirefleaFxDefinitions.FlashCount) index = 0;
            }
            else index = FirefleaFxDefinitions.SteadyFlashIndex;
            Index = index;
        }
        Timer = timer;

        ushort combined = unchecked((ushort)(
            FirefleaFxDefinitions.FlashingShade(index) +
            FirefleaFxDefinitions.DarknessShade(darkness)));
        byte shade = SnesLittleEndian.HighByte(combined);
        fixedColor.Green = (byte)(shade | FirefleaFxDefinitions.FixedColorGreenBits);
        fixedColor.Blue = (byte)(shade | FirefleaFxDefinitions.FixedColorBlueBits);
        fixedColor.Red = (byte)(shade | FirefleaFxDefinitions.FixedColorRedBits);
    }
}
