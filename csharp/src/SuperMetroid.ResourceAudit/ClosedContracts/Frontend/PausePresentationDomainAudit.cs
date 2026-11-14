using Microsoft.CodeAnalysis.Operations;
using SuperMetroid.Core.Frontend.Pause.Artwork;

namespace SuperMetroid.ResourceAudit.ClosedContracts.Frontend;

/// <summary>
/// Preserves the reviewed pause selector phases. A selector is placed for a named control (#1772), so no constant can
/// select an anchor outside the reviewed controls.
/// </summary>
internal static class PausePresentationDomainAudit
{
    /// <summary>Rejects constant pause selector phases outside the reviewed domain.</summary>
    internal static string? InvalidConstants(IInvocationOperation operation)
    {
        string type = operation.TargetMethod.ContainingType.ToDisplayString(), method = operation.TargetMethod.Name;
        int? Constant(string name) => operation.Arguments.FirstOrDefault(argument => argument.Parameter?.Name == name)
            ?.Value.ConstantValue is { HasValue: true, Value: not null } value ? Convert.ToInt32(value.Value) : null;

        if (type == typeof(PauseSelectorPresentation).FullName &&
            method is "NormalizePhase" or "Duration" or "Draw" && Constant("phase") is < 0)
            return "Constant phase is negative; reviewed pause phases normalize only nonnegative indices.";
        return null;
    }
}
