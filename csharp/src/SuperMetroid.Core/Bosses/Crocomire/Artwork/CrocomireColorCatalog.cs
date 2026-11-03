using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.Crocomire.Artwork;

/// <summary>
/// Editable Crocomire fight, wall, projectile, skeleton-arm, and wall-spike RGB5 colors.
/// Phase transitions and the white hurt-flash decision remain engine-owned.
/// </summary>
public sealed class CrocomireColorCatalog
{
    /// <summary>Five transfer roles whose overlapping stock sources stay independently editable.</summary>
    internal enum Band
    {
        /// <summary>Eight body and head inks restored on nonwhite hurt-flash updates.</summary>
        FightBody,
        /// <summary>Seventeen wall colors loaded during fight initialization.</summary>
        InitialWall,
        /// <summary>Seventeen projectile colors loaded after the wall colours, with independent overlap edits.</summary>
        InitialProjectile,
        /// <summary>Sixteen skeleton-arm colors installed when the wall break begins.</summary>
        SkeletonArm,
        /// <summary>Sixteen wall-spike colors installed at the rumble terminator.</summary>
        WallSpikes,
    }

    /// <summary>Each named material's calculated paint; supplied edits never alias across fields.</summary>
    internal static readonly Dictionary<Band, Func<int, RgbColor>> StockPaint = new()
    {
        [Band.FightBody] = CrocomirePaint.FightBody,
        [Band.InitialWall] = CrocomirePaint.InitialWall,
        [Band.InitialProjectile] = CrocomirePaint.InitialProjectile,
        [Band.SkeletonArm] = CrocomirePaint.SkeletonArm,
        [Band.WallSpikes] = CrocomirePaint.WallSpikes,
    };

    /// <summary>The transfer length of each Crocomire material role.</summary>
    internal static readonly Dictionary<Band, int> Counts = new()
    {
        [Band.FightBody] = CrocomireColorDefinitions.FightBodyCount,
        [Band.InitialWall] = CrocomireColorDefinitions.InitialWallCount,
        [Band.InitialProjectile] = CrocomireColorDefinitions.InitialProjectileCount,
        [Band.SkeletonArm] = CrocomireColorDefinitions.SkeletonArmCount,
        [Band.WallSpikes] = CrocomireColorDefinitions.WallSpikesCount,
    };

    /// <summary>Independently supplied differences; every unedited stock ink is calculated.</summary>
    internal readonly Dictionary<CrocomireColorKey, RgbColor> edits;

    /// <summary>Retains only colors that differ from their calculated material paint.</summary>
    /// <param name="edits">The supplied colours the import found differing from their calculated material paint.</param>
    internal CrocomireColorCatalog(Dictionary<CrocomireColorKey, RgbColor> edits) => this.edits = edits;

    /// <summary>Returns an edited color or calculates the corresponding stock paint.</summary>
    internal RgbColor Get(Band band, int index) => CrocomirePalette.Get(edits, band, index);
}
