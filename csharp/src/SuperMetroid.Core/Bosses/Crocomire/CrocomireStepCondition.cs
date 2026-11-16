namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>When one of Crocomire's steps moves him (#2163).</summary>
internal enum CrocomireStepCondition
{
    /// <summary>The step always moves him.</summary>
    Always,

    /// <summary>The step moves him unless a mouth hit is waiting for his fight AI to react to it.</summary>
    UnlessHitPending,

    /// <summary>The step moves him only while his body is on screen.</summary>
    WhileOnScreen,
}
