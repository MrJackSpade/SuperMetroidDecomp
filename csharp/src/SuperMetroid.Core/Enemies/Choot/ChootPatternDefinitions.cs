namespace SuperMetroid.Core.Enemies.Choot;

/// <summary>Named motion selection and loop-origin advance for Choot falling patterns.</summary>
internal static class ChootPatternDefinitions
{
    /// <summary>The selected normal/slow loop-origin advance 30, authored with its drawn path.</summary>
    private const int NormalLoopYDistance = 30;
    /// <summary>Selected wide loop-origin advance 28, authored with its drawn path.</summary>
    private const int WideLoopYDistance = 28;
    /// <summary>Selected very-wide loop-origin advance 32, authored with its drawn path.</summary>
    private const int VeryWideLoopYDistance = 32;

    /// <summary>
    /// Parameter one's high byte selects normal, wide, very wide, slow and very slow motion
    /// and the corresponding loop Y advances. Slow variants
    /// repeat normal-path plateau frames and use the same vertical advance.
    /// The following alias words are outside the five supported selectors.
    /// The advances are not the paths' final offsets (31/29/32 against 30/28/32), so they
    /// stay authored inputs of each drawn trajectory (see residualScalarInputsReview).
    /// </summary>
    internal static readonly ChootPatternDefinition[] Patterns =
    [
        new(ChootFallingPath.Normal, NormalLoopYDistance),
        new(ChootFallingPath.Wide, WideLoopYDistance),
        new(ChootFallingPath.VeryWide, VeryWideLoopYDistance),
        new(ChootFallingPath.Slow, NormalLoopYDistance),
        new(ChootFallingPath.VerySlow, NormalLoopYDistance),
    ];
}
