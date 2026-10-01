using Microsoft.CodeAnalysis.Operations;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.ResourceAudit;

/// <summary>Transfer length constraints apply only after an owned legacy source is selected.</summary>
internal static class ClosedTransferDomainAudit
{
    internal static string? InvalidConstants(IInvocationOperation operation)
    {
        if (operation.TargetMethod.ContainingType.ToDisplayString() != typeof(GunshipLiftoffArtworkCatalog).FullName ||
            operation.TargetMethod.Name != "TryResolve") return null;
        int? Constant(string name) => operation.Arguments.Single(argument => argument.Parameter?.Name == name)
            .Value.ConstantValue is { HasValue: true, Value: not null } value ? Convert.ToInt32(value.Value) : null;
        if (Constant("sourceAddress") is not int source ||
            !GunshipLiftoffTransferDefinitions.Frames.ToArray().Any(frame => frame.SourceAddress == source)) return null;
        return Constant("byteCount") is int count && count != GunshipLiftoffTransferDefinitions.ByteCount
            ? $"Owned gunship takeoff source requires {GunshipLiftoffTransferDefinitions.ByteCount} bytes, not {count}." : null;
    }
}
