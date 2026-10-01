using Microsoft.CodeAnalysis.Operations;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.ResourceAudit;

/// <summary>Relational constraints where selecting a known stream narrows its valid array index.</summary>
internal static class SpecializedColorDomainAudit
{
    internal static string? InvalidConstants(IInvocationOperation operation)
    {
        if (operation.TargetMethod.ContainingType.ToDisplayString() != typeof(PowerBombFixedColorCatalog).FullName ||
            operation.TargetMethod.Name != "Resolve") return null;
        var sequence = operation.Arguments.Single(argument => argument.Parameter?.Name == "sequence").Value.ConstantValue;
        var index = operation.Arguments.Single(argument => argument.Parameter?.Name == "index").Value.ConstantValue;
        if (sequence is not { HasValue: true, Value: not null } ||
            index is not { HasValue: true, Value: not null }) return null;
        var kind = (PowerBombFixedColorSequence)Convert.ToInt32(sequence.Value);
        // Invalid enum members are already reported by the independent domain
        // check; never let the audit's own Count call throw for a bad consumer.
        if (!Enum.IsDefined(kind)) return null;
        int count = PowerBombFixedColorFormat.Count(kind);
        return (uint)Convert.ToInt32(index.Value) < count ? null :
            $"Constant index={index.Value} is outside the selected {kind} color stream (0..{count - 1}).";
    }
}
