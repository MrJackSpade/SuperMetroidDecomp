namespace SuperMetroid.Core.Hardware;

/// <summary>
/// Immutable literal PPU window register values. No projectile, HDMA or other
/// mutable gameplay owner is retained, so captures can outlive their producer.
/// TM/TS and TMW/TSW are separate admission registers, not window membership.
/// </summary>
/// <param name="Window12Selection">W12SEL ($2123), BG1/BG2 selection nibbles.</param>
/// <param name="Window34Selection">W34SEL ($2124), BG3/BG4 selection nibbles.</param>
/// <param name="ObjectColorSelection">WOBJSEL ($2125), OBJ/color selection nibbles.</param>
/// <param name="FirstLeft">WH0 ($2126), inclusive first-window left edge.</param>
/// <param name="FirstRight">WH1 ($2127), inclusive first-window right edge.</param>
/// <param name="SecondLeft">WH2 ($2128), inclusive second-window left edge.</param>
/// <param name="SecondRight">WH3 ($2129), inclusive second-window right edge.</param>
/// <param name="BackgroundLogic">WBGLOG ($212A), four two-bit BG operations.</param>
/// <param name="ObjectColorLogic">WOBJLOG ($212B), OBJ/color two-bit operations.</param>
public readonly record struct SnesWindowRegisters(
    byte Window12Selection, byte Window34Selection, byte ObjectColorSelection,
    byte FirstLeft, byte FirstRight, byte SecondLeft, byte SecondRight,
    byte BackgroundLogic, byte ObjectColorLogic)
{
    public bool Contains(SnesWindowTarget target, byte x)
    {
        int index = (int)target;
        byte selection = target switch
        {
            SnesWindowTarget.Bg1 or SnesWindowTarget.Bg2 => Window12Selection,
            SnesWindowTarget.Bg3 or SnesWindowTarget.Bg4 => Window34Selection,
            SnesWindowTarget.Obj or SnesWindowTarget.ColorMath => ObjectColorSelection,
            _ => throw new ArgumentOutOfRangeException(nameof(target)),
        };
        int selectionShift = (index & 1) * 4;
        int logic = target <= SnesWindowTarget.Bg4
            ? BackgroundLogic >> (index * 2)
            : ObjectColorLogic >> ((index - (int)SnesWindowTarget.Obj) * 2);
        return SnesWindowMask.Contains((SnesWindowSelection)((selection >> selectionShift) & 15),
            (SnesWindowLogic)(logic & 3), x, FirstLeft, FirstRight, SecondLeft, SecondRight);
    }

    /// <summary>Returns the independently window-masked layers for TMW or TSW.</summary>
    public SnesMainScreenLayers MaskedLayers(byte x, SnesMainScreenLayers windowEnabledLayers)
    {
        SnesMainScreenLayers masked = SnesMainScreenLayers.None;
        for (int index = (int)SnesWindowTarget.Bg1; index <= (int)SnesWindowTarget.Obj; index++)
        {
            var layer = (SnesMainScreenLayers)(1 << index);
            if ((windowEnabledLayers & layer) != 0 && Contains((SnesWindowTarget)index, x))
                masked |= layer;
        }
        return masked;
    }
}
