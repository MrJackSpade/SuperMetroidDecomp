namespace SuperMetroid.Core.Hardware;

/// <summary>
/// Immutable PPU window register values. No projectile, HDMA or other
/// mutable gameplay owner is retained, so captures can outlive their producer.
/// TM/TS and TMW/TSW are separate admission registers, not window membership.
/// </summary>
/// <param name="Window12Selection">W12SEL ($2123): the BG1 and BG2 window selections.</param>
/// <param name="Window34Selection">W34SEL ($2124): the BG3 and BG4 window selections.</param>
/// <param name="ObjectColorSelection">WOBJSEL ($2125): the OBJ and colour-window selections.</param>
/// <param name="FirstLeft">WH0 ($2126), inclusive first-window left screen column.</param>
/// <param name="FirstRight">WH1 ($2127), inclusive first-window right screen column.</param>
/// <param name="SecondLeft">WH2 ($2128), inclusive second-window left screen column.</param>
/// <param name="SecondRight">WH3 ($2129), inclusive second-window right screen column.</param>
/// <param name="BackgroundLogic">WBGLOG ($212A) value, 0 through <see cref="MaximumLogic"/>: four two-bit BG operations.</param>
/// <param name="ObjectColorLogic">WOBJLOG ($212B) value, 0 through <see cref="MaximumLogic"/>: the OBJ and colour two-bit operations, with its unused high bits retained.</param>
public readonly record struct SnesWindowRegisters(
    SnesWindowSelectionPair Window12Selection, SnesWindowSelectionPair Window34Selection,
    SnesWindowSelectionPair ObjectColorSelection,
    int FirstLeft, int FirstRight, int SecondLeft, int SecondRight,
    int BackgroundLogic, int ObjectColorLogic)
{
    /// <summary>The rightmost screen column a window edge names.</summary>
    public const int MaximumColumn = SnesPpuLayout.ScreenWidthPixels - 1;

    /// <summary>The largest logic register value: four two-bit operations.</summary>
    public const int MaximumLogic = byte.MaxValue;

    /// <summary>The bits of one layer's window operation.</summary>
    private const int LogicBits = 2;

    /// <summary>The bits of one operation once shifted to the low end.</summary>
    private const int LogicMask = (1 << LogicBits) - 1;

    /// <summary>Inclusive first-window left screen column.</summary>
    public int FirstLeft
    {
        get => firstLeft;
        init => firstLeft = Ensure.BetweenInclusive(0, MaximumColumn, value);
    }

    /// <summary>Validated store of <see cref="FirstLeft"/>.</summary>
    private readonly int firstLeft = Ensure.BetweenInclusive(0, MaximumColumn, FirstLeft);

    /// <summary>Inclusive first-window right screen column.</summary>
    public int FirstRight
    {
        get => firstRight;
        init => firstRight = Ensure.BetweenInclusive(0, MaximumColumn, value);
    }

    /// <summary>Validated store of <see cref="FirstRight"/>.</summary>
    private readonly int firstRight = Ensure.BetweenInclusive(0, MaximumColumn, FirstRight);

    /// <summary>Inclusive second-window left screen column.</summary>
    public int SecondLeft
    {
        get => secondLeft;
        init => secondLeft = Ensure.BetweenInclusive(0, MaximumColumn, value);
    }

    /// <summary>Validated store of <see cref="SecondLeft"/>.</summary>
    private readonly int secondLeft = Ensure.BetweenInclusive(0, MaximumColumn, SecondLeft);

    /// <summary>Inclusive second-window right screen column.</summary>
    public int SecondRight
    {
        get => secondRight;
        init => secondRight = Ensure.BetweenInclusive(0, MaximumColumn, value);
    }

    /// <summary>Validated store of <see cref="SecondRight"/>.</summary>
    private readonly int secondRight = Ensure.BetweenInclusive(0, MaximumColumn, SecondRight);

    /// <summary>The four two-bit BG operations.</summary>
    public int BackgroundLogic
    {
        get => backgroundLogic;
        init => backgroundLogic = Ensure.BetweenInclusive(0, MaximumLogic, value);
    }

    /// <summary>Validated store of <see cref="BackgroundLogic"/>.</summary>
    private readonly int backgroundLogic = Ensure.BetweenInclusive(0, MaximumLogic, BackgroundLogic);

    /// <summary>The OBJ and colour two-bit operations, with any unused high bits.</summary>
    public int ObjectColorLogic
    {
        get => objectColorLogic;
        init => objectColorLogic = Ensure.BetweenInclusive(0, MaximumLogic, value);
    }

    /// <summary>Validated store of <see cref="ObjectColorLogic"/>.</summary>
    private readonly int objectColorLogic = Ensure.BetweenInclusive(0, MaximumLogic, ObjectColorLogic);

    /// <summary>
    /// Evaluates the target's selected and combined windows at one screen pixel.
    /// Window endpoints are inclusive; inverted windows and two-window logic are
    /// applied before any main/subscreen layer admission or color-math policy.
    /// </summary>
    /// <param name="target">The background, OBJ, or color-window register field to evaluate.</param>
    /// <param name="x">The horizontal screen column, from 0 through 255.</param>
    /// <returns>Whether the pixel belongs to the resulting window; false when both windows are disabled.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The target is not a defined PPU window target.</exception>
    public bool Contains(SnesWindowTarget target, int x)
    {
        int index = (int)target;
        SnesWindowSelectionPair pair = target switch
        {
            SnesWindowTarget.Bg1 or SnesWindowTarget.Bg2 => Window12Selection,
            SnesWindowTarget.Bg3 or SnesWindowTarget.Bg4 => Window34Selection,
            SnesWindowTarget.Obj or SnesWindowTarget.ColorMath => ObjectColorSelection,
            _ => throw new ArgumentOutOfRangeException(nameof(target)),
        };
        bool upper = (index & 1) != 0;
        int logic = target <= SnesWindowTarget.Bg4
            ? BackgroundLogic >> (index * LogicBits)
            : ObjectColorLogic >> ((index - (int)SnesWindowTarget.Obj) * LogicBits);
        return SnesWindowMask.Contains(pair.Layer(upper),
            (SnesWindowLogic)(logic & LogicMask), x, FirstLeft, FirstRight, SecondLeft, SecondRight);
    }

    /// <summary>Returns the independently window-masked layers for TMW or TSW.</summary>
    /// <param name="x">The horizontal screen column, from 0 through 255.</param>
    /// <param name="windowEnabledLayers">The layers TMW or TSW subjects to their windows.</param>
    public SnesMainScreenLayers MaskedLayers(int x, SnesMainScreenLayers windowEnabledLayers)
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
