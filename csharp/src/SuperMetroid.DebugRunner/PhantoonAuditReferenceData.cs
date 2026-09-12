/// <summary>Independent native instruction anchors for Phantoon diagnostic expectations.</summary>
internal static class PhantoonAuditReferenceData
{
    /// <summary>$A7:CE2B: InitAI_PhantoonBody's initial function-timer LDA immediate; NTSC 120 frames, PAL 96.</summary>
    internal const int InitialFlameDelayInstruction = 0xa7ce2b;
    /// <summary>$86:9885: PhantoonDestroyableFlameInit_Type2_Enraged clockwise LDA immediate; NTSC +2, PAL +3.</summary>
    internal const int ClockwiseRageSpeedInstruction = 0x869885;
    /// <summary>$86:988D: PhantoonDestroyableFlameInit_Type2_Enraged counterclockwise LDA immediate; NTSC -2, PAL -3.</summary>
    internal const int CounterclockwiseRageSpeedInstruction = 0x86988d;
}
