using Microsoft.CodeAnalysis.Operations;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Rooms.Artwork;

namespace SuperMetroid.ResourceAudit.ClosedContracts;

/// <summary>Transfer length constraints apply only after an owned legacy source is selected.</summary>
internal static class ClosedTransferDomainAudit
{
    /// <summary>Checks constant lengths of sky page copies and constant source and length pairs of gunship
    /// transfers, leaving unknown or deliberately out-of-domain queries alone.</summary>
    internal static string? InvalidConstants(IInvocationOperation operation)
    {
        string type = operation.TargetMethod.ContainingType.ToDisplayString();
        int? Constant(string name) => operation.Arguments.Single(argument => argument.Parameter?.Name == name)
            .Value.ConstantValue is { HasValue: true, Value: not null } value ? Convert.ToInt32(value.Value) : null;
        if (type == typeof(RoomSkyTilemapCatalog).FullName && operation.TargetMethod.Name == "Page")
            // The page itself is checked against the sky's pages by its index domain.
            return Constant("tileCount") is int skyCount && skyCount != RoomSkyTilemapFormat.PageTiles
                ? "A scrolling-sky page copy is not one complete page." : null;
        if (operation.TargetMethod.Name != "TryResolve" || type != typeof(GunshipLiftoffArtworkCatalog).FullName) return null;
        if (Constant("sourceAddress") is not int source) return null;
        if (!Enumerable.Range(0, GunshipLiftoffTransferDefinitions.Frames.Length)
                .Any(page => EnemySourceDefinitions.GunshipLiftoffPageSource(page) == source)) return null;
        return Constant("byteCount") is int count && count != GunshipLiftoffTransferDefinitions.ByteCount
            ? $"Owned gunship takeoff source requires {GunshipLiftoffTransferDefinitions.ByteCount} bytes, not {count}." : null;
    }
}
