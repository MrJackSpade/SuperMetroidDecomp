namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>Where verification's native colour oracle finds the Ceres door's colours: the first colour each setup and glow image replaces.</summary>
internal static class CeresDoorVisualConstants
{
    /// <summary>First CGRAM color index receiving the normal setup palette.</summary>
    public const int NormalTargetColor = 0x142 / 2;
    /// <summary>First CGRAM color index receiving the active variant's setup palette.</summary>
    public const int ActiveTargetColor = 0x1e2 / 2;
    /// <summary>First CGRAM color index receiving the animated six-color span.</summary>
    public const int AnimationTargetColor = 0x52 / 2;
}
