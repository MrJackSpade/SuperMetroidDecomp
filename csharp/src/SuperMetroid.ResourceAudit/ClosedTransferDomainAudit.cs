using Microsoft.CodeAnalysis.Operations;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.ResourceAudit;

/// <summary>Transfer length constraints apply only after an owned legacy source is selected.</summary>
internal static class ClosedTransferDomainAudit
{
    /// <summary>Finds invalid transfer lengths only for sources owned by the bounded catalogs.</summary>
    /// <param name="operation">Semantic invocation of a provider lookup.</param>
    /// <returns>A diagnostic message when an owned source has an invalid length; otherwise null.</returns>
    internal static string? InvalidConstants(IInvocationOperation operation)
    {
        if (operation.TargetMethod.Name != "TryResolve") return null;
        string type = operation.TargetMethod.ContainingType.ToDisplayString();
        if (type != typeof(GunshipLiftoffArtworkCatalog).FullName && type != typeof(RoomSkyTilemapCatalog).FullName) return null;
        int? Constant(string name) => operation.Arguments.Single(argument => argument.Parameter?.Name == name)
            .Value.ConstantValue is { HasValue: true, Value: not null } value ? Convert.ToInt32(value.Value) : null;
        if (Constant("sourceAddress") is not int source) return null;
        if (type == typeof(RoomSkyTilemapCatalog).FullName)
        {
            int offset = source - RoomSkyTilemapFormat.FirstSourceAddress;
            if ((uint)offset >= RoomSkyTilemapFormat.TotalByteCount) return null; // Valid false query.
            if ((source & 1) != 0) return "Owned scrolling-sky source must be even-addressed.";
            if (Constant("byteCount") is not int skyCount) return null;
            bool page = offset % RoomSkyTilemapFormat.PageByteCount == 0 && skyCount == RoomSkyTilemapFormat.PageByteCount;
            bool row = skyCount == SuperMetroid.Core.Game.RoomFxRomData.ScrollingSky.TilemapRowByteCount;
            return (!page && !row) || (long)offset + skyCount > RoomSkyTilemapFormat.TotalByteCount
                ? "Owned scrolling-sky source/length is neither a complete aligned page nor a complete even-addressed row." : null;
        }
        if (!GunshipLiftoffTransferDefinitions.Frames.ToArray().Any(frame => frame.SourceAddress == source)) return null;
        return Constant("byteCount") is int count && count != GunshipLiftoffTransferDefinitions.ByteCount
            ? $"Owned gunship takeoff source requires {GunshipLiftoffTransferDefinitions.ByteCount} bytes, not {count}." : null;
    }
}
