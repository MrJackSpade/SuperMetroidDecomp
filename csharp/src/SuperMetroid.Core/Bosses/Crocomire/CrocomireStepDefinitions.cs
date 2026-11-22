namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>
/// The steps Crocomire's walk cues take (#2163): his advance toward the spike wall, his step back from Samus, and his
/// back-off from the wall, each with and without the random foot dust his heavier steps raise.
/// </summary>
internal static partial class CrocomireStepDefinitions
{
    /// <summary>The pixels one step moves him.</summary>
    private const int StepPixels = 4;

    /// <summary>A step toward the spike wall, held while a mouth hit is pending.</summary>
    internal static CrocomireStep Advance { get; } = new()
    {
        HorizontalPixels = -StepPixels,
        Condition = CrocomireStepCondition.UnlessHitPending,
        RaisesFootDust = false,
    };

    /// <summary>A step toward the spike wall, held while a mouth hit is pending, after raising foot dust.</summary>
    internal static CrocomireStep AdvanceRaisingDust { get; } = new()
    {
        HorizontalPixels = -StepPixels,
        Condition = CrocomireStepCondition.UnlessHitPending,
        RaisesFootDust = true,
    };

    /// <summary>A step back, away from the spike wall, taken only while his body is on screen.</summary>
    internal static CrocomireStep StepBack { get; } = new()
    {
        HorizontalPixels = StepPixels,
        Condition = CrocomireStepCondition.WhileOnScreen,
        RaisesFootDust = false,
    };

    /// <summary>A step back, away from the spike wall, taken only while his body is on screen, after raising foot dust.</summary>
    internal static CrocomireStep StepBackRaisingDust { get; } = new()
    {
        HorizontalPixels = StepPixels,
        Condition = CrocomireStepCondition.WhileOnScreen,
        RaisesFootDust = true,
    };

    /// <summary>A step backing off from the spike wall.</summary>
    internal static CrocomireStep BackOff { get; } = new()
    {
        HorizontalPixels = StepPixels,
        Condition = CrocomireStepCondition.Always,
        RaisesFootDust = false,
    };

    /// <summary>A step backing off from the spike wall, after raising foot dust.</summary>
    internal static CrocomireStep BackOffRaisingDust { get; } = new()
    {
        HorizontalPixels = StepPixels,
        Condition = CrocomireStepCondition.Always,
        RaisesFootDust = true,
    };
}
