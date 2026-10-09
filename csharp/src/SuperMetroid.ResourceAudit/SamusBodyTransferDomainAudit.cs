using Microsoft.CodeAnalysis.Operations;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.ResourceAudit;

/// <summary>Rejects provably invalid constants without inventing per-set bounds for native cross-group selection.</summary>
internal static class SamusBodyTransferDomainAudit
{
    /// <summary>Returns an error when known Samus body transfer selectors exceed their reviewed domain.</summary>
    /// <param name="operation">The Samus body artwork invocation to inspect.</param>
    /// <returns>An explanatory error, or <see langword="null"/> when no invalid constant is proven.</returns>
    internal static string? InvalidConstants(IInvocationOperation operation)
    {
        if (operation.TargetMethod.ContainingType.ToDisplayString() != typeof(SamusBodyArtworkCatalog).FullName) return null;
        int? Constant(string name) => operation.Arguments.FirstOrDefault(argument => argument.Parameter?.Name == name)?
            .Value.ConstantValue is { HasValue: true, Value: not null } value ? Convert.ToInt32(value.Value) : null;
        if (operation.TargetMethod.Name == "Frame" && Constant("pose") is int pose && pose >= SamusBodyArtworkCatalog.PoseCount)
            return "Known Samus pose has no admitted body frame list.";
        if (operation.TargetMethod.Name == "DefinitionAddress" && Constant("set") is int set)
        {
            int count = Constant("upperHalf") == 0 ? SamusBodyArtworkCatalog.BottomSetCount : SamusBodyArtworkCatalog.TopSetCount;
            if (set >= count) return "Known Samus definition set is outside the selected half's domain.";
        }
        if (operation.TargetMethod.Name == "DefinitionAt" && Constant("nativeAddress") is int address &&
            (address < (SamusBodyDefinitionLayout.BankBase | SamusBodyDefinitionLayout.MinimumSetOffset) ||
             address >= (SamusBodyDefinitionLayout.BankBase | SamusBodyDefinitionLayout.EndOffset)))
            return "Known Samus definition address is outside admitted bank-$92 record storage.";
        return null;
    }
}
