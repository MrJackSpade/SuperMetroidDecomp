namespace SuperMetroid.Core.Enemies.Sparks;

/// <summary>Spark's private bank-$A8 animation instructions.</summary>
internal enum SparkInstruction : ushort
{
    /// <summary><c>Instruction_Spark_SetAsIntangible</c> at $A8:E61D.</summary>
    SetAsIntangible = 0xe61d,

    /// <summary><c>Instruction_Spark_SetAsTangible</c> at $A8:E62A.</summary>
    SetAsTangible = 0xe62a,
}
