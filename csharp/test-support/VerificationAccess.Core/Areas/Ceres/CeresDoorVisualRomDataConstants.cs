namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>Cartridge values and helpers of <see cref="CeresDoorVisualRomData"/> that only verification reads.</summary>
internal static class CeresDoorVisualRomDataConstants
{
    /// <summary>First CGRAM color index receiving the normal setup palette.</summary>
    public const int NormalTargetColor = 0x142 / 2;
    /// <summary>First CGRAM color index receiving the active variant's setup palette.</summary>
    public const int ActiveTargetColor = 0x1e2 / 2;
    /// <summary>First CGRAM color index receiving the animated six-color span.</summary>
    public const int AnimationTargetColor = 0x52 / 2;
}
