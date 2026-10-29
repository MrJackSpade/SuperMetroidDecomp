using Microsoft.CodeAnalysis.Operations;
using SuperMetroid.Core.Samus.Artwork;

namespace SuperMetroid.ResourceAudit.ClosedContracts.Samus;

/// <summary>Rejects a provably invalid constant pose in a body frame-picture lookup.</summary>
internal static class SamusBodyTransferDomainAudit
{
    /// <summary>Returns a finding only when a constant pose is provably outside its admitted domain.</summary>
    internal static string? InvalidConstants(IInvocationOperation operation)
    {
        if (operation.TargetMethod.ContainingType.ToDisplayString() != typeof(InstalledSamusBodyArtwork).FullName) return null;
        int? Constant(string name) => operation.Arguments.FirstOrDefault(argument => argument.Parameter?.Name == name)?
            .Value.ConstantValue is { HasValue: true, Value: not null } value ? Convert.ToInt32(value.Value) : null;
        if (operation.TargetMethod.Name == "Frame" && Constant("pose") is int pose && pose >= InstalledSamusBodyArtwork.PoseCount)
            return "Known Samus pose has no admitted body frame list.";
        return null;
    }
}
