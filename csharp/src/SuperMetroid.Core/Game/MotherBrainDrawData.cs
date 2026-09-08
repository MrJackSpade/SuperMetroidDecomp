namespace SuperMetroid.Core.Game;

/// <summary>Drawing geometry owned by the bank-$A9 Mother Brain routines.</summary>
public static class MotherBrainDrawData
{
    /// <summary>$A9:938A brain draw gate: origin plus 32 must reach the camera's left edge before writing wrapping OAM.</summary>
    public const int BrainLeftCullMargin = 32;
}
