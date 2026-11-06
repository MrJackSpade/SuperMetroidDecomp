namespace SuperMetroid.Core.Enemies.Choot;

/// <summary>How far each Choot falling pattern's loop origin advances.</summary>
internal static class ChootPatternDefinitions
{
    /// <summary>The normal and slow loop-origin advance, authored with its drawn path.</summary>
    private const int NormalLoopYDistance = 30;
    /// <summary>The wide loop-origin advance, authored with its drawn path.</summary>
    private const int WideLoopYDistance = 28;
    /// <summary>The very-wide loop-origin advance, authored with its drawn path.</summary>
    private const int VeryWideLoopYDistance = 32;

    /// <summary>
    /// The vertical distance, in pixels, each falling path's loop origin advances per loop. Slow variants repeat
    /// normal-path plateau frames and use the same vertical advance. The advances are not the paths' final offsets
    /// (31/29/32 against 30/28/32), so they stay authored inputs of each drawn trajectory (see
    /// residualScalarInputsReview).
    /// </summary>
    internal static readonly Dictionary<ChootFallingPath, int> LoopYDistances = new()
    {
        [ChootFallingPath.Normal] = NormalLoopYDistance,
        [ChootFallingPath.Wide] = WideLoopYDistance,
        [ChootFallingPath.VeryWide] = VeryWideLoopYDistance,
        [ChootFallingPath.Slow] = NormalLoopYDistance,
        [ChootFallingPath.VerySlow] = NormalLoopYDistance,
    };
}
