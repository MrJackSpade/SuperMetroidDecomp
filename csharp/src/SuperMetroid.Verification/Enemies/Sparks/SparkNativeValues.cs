namespace SuperMetroid.Verification.Enemies.Sparks;

/// <summary>The native Spark selector tables the Spark checks read. Only verification reads them.</summary>
internal static class SparkNativeValues
{
    /// <summary>$A8:E682, the three-entry initial instruction-list table indexed by the masked population parameter.</summary>
    internal const int InstructionListTable = 0xa8e682;

    /// <summary>$A8:E688, the three-entry initial function table indexed by the same selector.</summary>
    internal const int FunctionTable = 0xa8e688;
}
