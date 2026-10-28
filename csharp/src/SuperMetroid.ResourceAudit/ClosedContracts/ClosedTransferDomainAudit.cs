using Microsoft.CodeAnalysis.Operations;
using SuperMetroid.Core.Rooms.Artwork;

namespace SuperMetroid.ResourceAudit.ClosedContracts;

/// <summary>Transfer length constraints apply only after an owned legacy source is selected.</summary>
internal static class ClosedTransferDomainAudit
{
    /// <summary>Checks constant lengths of sky page copies, leaving unknown or deliberately out-of-domain queries alone.</summary>
    internal static string? InvalidConstants(IInvocationOperation operation)
    {
        string type = operation.TargetMethod.ContainingType.ToDisplayString();
        int? Constant(string name) => operation.Arguments.Single(argument => argument.Parameter?.Name == name)
            .Value.ConstantValue is { HasValue: true, Value: not null } value ? Convert.ToInt32(value.Value) : null;
        if (type != typeof(RoomSkyTilemaps).FullName || operation.TargetMethod.Name != "Page") return null;
        // The page itself is checked against the sky's pages by its index domain.
        return Constant("tileCount") is int skyCount && skyCount != RoomSkyTilemapFormat.PageTiles
            ? "A scrolling-sky page copy is not one complete page." : null;
    }
}
