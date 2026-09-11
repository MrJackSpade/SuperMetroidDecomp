namespace SuperMetroid.Core.Game;

/// <summary>Native definition data for the Yapping Maw's curved attack.</summary>
public static class YappingMawRomData
{
    /// <summary>
    /// $A8:A26A/$A26F, Function_YappingMaw_Neutral: CMP/BMI retains shorter
    /// target lengths and caps longer ones at 64 before computing the link radius.
    /// </summary>
    public const ushort MaximumTargetDistance = 64;
}
