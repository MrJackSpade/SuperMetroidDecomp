namespace SuperMetroid.Core.Game;

/// <summary>Native layout of Hibashi's activity-frame instruction routines.</summary>
internal static class HibashiInstructionLayout
{
    /// <summary>
    /// Byte stride of the 22 <see cref="HibashiInstruction"/> activity-frame routines at
    /// $A6:8E13-$8FBD. Frame zero is $1A bytes long because it also sets the initial X radius;
    /// every later routine follows at this stride.
    /// </summary>
    private const int ActivityFrameStride = 0x14;

    /// <summary>
    /// Index of the Y-offset/radius pair an activity-frame routine selects. Each routine selects
    /// the pair matching its position, so the index derives from the executed routine address
    /// (frame zero's six extra bytes fall below one stride) rather than another 22-arm mapping.
    /// </summary>
    internal static int ActivityFrameIndex(HibashiInstruction instruction) =>
        ((ushort)instruction - (ushort)HibashiInstruction.ActivityFrame0) / ActivityFrameStride;
}
