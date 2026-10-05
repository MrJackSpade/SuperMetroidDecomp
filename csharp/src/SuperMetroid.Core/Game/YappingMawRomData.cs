namespace SuperMetroid.Core.Game;

/// <summary>Native definition data for the Yapping Maw's curved attack.</summary>
public static class YappingMawRomData
{
    /// <summary>
    /// $A8:A26A/$A26F, Function_YappingMaw_Neutral: CMP/BMI retains shorter
    /// target lengths and caps longer ones at 64 before computing the link radius.
    /// </summary>
    public const ushort MaximumTargetDistance = 64;
    /// <summary>
    /// $A8:A0A7-A0C6, YappingMawSamusOffsets: eight clockwise directions from up
    /// walk an equal-step Manhattan-radius16 diamond. Each step exchanges eight
    /// pixels between the two components; opposite directions negate both.
    /// Native BeginExtension quantizes/wraps its angle before this selection.
    /// </summary>
    internal static (short X, short Y) HeldSamusOffset(int direction)
    {
        if ((uint)direction >= 8) throw new IndexOutOfRangeException();
        static short Component(int phase) => (short)(8 * (2 - Math.Abs(phase - 4)));
        return (Component((direction + 2) & 7), Component(direction));
    }
}