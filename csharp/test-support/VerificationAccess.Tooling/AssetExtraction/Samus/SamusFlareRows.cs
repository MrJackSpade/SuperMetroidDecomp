using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>Reads a flare origin table's rows back as flare aims, which only checks and record decoding do (#1595).</summary>
internal static class SamusFlareRows
{
    /// <summary>The flare aim a flare origin table's <paramref name="row"/> holds the placement of.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The row holds neither a muzzle nor a turning placement.</exception>
    internal static SamusFlareAim AimAtRow(int row) =>
        row is >= 0 and < SamusProjectileDirectionCodec.Count ? SamusFlareAim.AtMuzzle(SamusProjectileDirectionCodec.FromAimIndex(row))
        : row >= SamusProjectileDirectionCodec.Count && row - SamusProjectileDirectionCodec.Count < SamusProjectileDirectionCodec.TurningPlacementOrder.Count
            ? SamusFlareAim.WhileTurning(SamusProjectileDirectionCodec.TurningPlacementOrder[row - SamusProjectileDirectionCodec.Count])
            : throw new ArgumentOutOfRangeException(nameof(row), row, "The flare origin tables hold ten directions and three turning placements.");
}
