namespace SuperMetroid.ResourceAudit;

/// <summary>One missing or invalid compiled PLM program record with its root owner and bank-84 offset.</summary>
internal sealed record PlmProgramFinding(string Code, string Owner, ushort Address, string Detail);
