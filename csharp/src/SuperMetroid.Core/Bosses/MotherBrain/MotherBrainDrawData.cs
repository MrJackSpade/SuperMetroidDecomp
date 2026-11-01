namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Drawing geometry owned by the Mother Brain routines.</summary>
public static class MotherBrainDrawData
{
    /// <summary>The brain draw gate: origin plus 32 must reach the camera's left edge before writing wrapping sprites.</summary>
    public const int BrainLeftCullMargin = 32;
}
