namespace SuperMetroid.ResourceAudit.PlmPrograms;

/// <summary>Isolated bank-$84 fixture addresses for missing branches and unknown opcodes.</summary>
internal static class PlmAuditFixtureIds
{
    /// <summary>Constructed program root outside the reviewed production records.</summary>
    internal const ushort Start = 0xf000;
    /// <summary>Branch destination intentionally absent from constructed fixture storage.</summary>
    internal const ushort AbsentTarget = 0xf100;
    /// <summary>Unsupported opcode used to confirm the audit rejects unknown instruction widths.</summary>
    internal const ushort UnknownOpcode = 0xffff;
}
