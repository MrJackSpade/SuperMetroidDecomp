using Microsoft.CodeAnalysis.Operations;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Rooms.Artwork;

namespace SuperMetroid.ResourceAudit.ClosedContracts;

/// <summary>Transfer length constraints apply only after an owned legacy source is selected.</summary>
internal static class ClosedTransferDomainAudit
{
    /// <summary>Checks constant source and length pairs for owned sky and gunship
    /// transfers, leaving unknown or deliberately out-of-domain queries alone.</summary>
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
            if (Constant("byteCount") is not int skyCount) return null;
            bool page = offset % RoomSkyTilemapFormat.PageByteCount == 0 && skyCount == RoomSkyTilemapFormat.PageByteCount;
            return !page || (long)offset + skyCount > RoomSkyTilemapFormat.TotalByteCount
                ? "Owned scrolling-sky source/length is not a complete aligned page." : null;
        }
        if (!Enumerable.Range(0, GunshipLiftoffTransferDefinitions.Frames.Length)
                .Any(page => EnemySourceDefinitions.GunshipLiftoffPageSource(page) == source)) return null;
        return Constant("byteCount") is int count && count != GunshipLiftoffTransferDefinitions.ByteCount
            ? $"Owned gunship takeoff source requires {GunshipLiftoffTransferDefinitions.ByteCount} bytes, not {count}." : null;
    }
}
