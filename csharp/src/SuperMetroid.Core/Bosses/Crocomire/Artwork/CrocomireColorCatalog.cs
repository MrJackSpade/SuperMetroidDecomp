using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Artwork;

namespace SuperMetroid.Core.Bosses.Crocomire.Artwork;

/// <summary>
/// Editable Crocomire fight, wall, projectile, skeleton-arm, and wall-spike RGB5 colors.
/// Phase transitions and the white hurt-flash decision remain engine-owned.
/// </summary>
public sealed class CrocomireColorCatalog
{
    /// <summary>Canonical selected colors preserve the five native transfer labels/order.</summary>
    public string ContentIdentity => CrocomirePalette.ContentIdentity(this);

    /// <summary>Five native transfer roles whose overlapping stock sources stay independently editable.</summary>
    internal enum Band
    {
        /// <summary>Eight body and head inks restored on nonwhite hurt-flash updates.</summary>
        FightBody,
        /// <summary>Seventeen wall colors loaded during fight initialization.</summary>
        InitialWall,
        /// <summary>Seventeen projectile colors loaded after the wall row, with independent overlap edits.</summary>
        InitialProjectile,
        /// <summary>Sixteen skeleton-arm colors installed when the wall break begins.</summary>
        SkeletonArm,
        /// <summary>Sixteen wall-spike colors installed at the rumble terminator.</summary>
        WallSpikes,
    }

    /// <summary>The five transfers in native order, each with the label its content identity hashes it under.</summary>
    internal static readonly CrocomireTransferLabel[] IdentityOrder =
    [
        new(Band.FightBody, "fightBody"),
        new(Band.InitialWall, "initialWall"),
        new(Band.InitialProjectile, "initialProjectile"),
        new(Band.SkeletonArm, "skeletonArm"),
        new(Band.WallSpikes, "wallSpikes"),
    ];

    /// <summary>Each named material's calculated native paint; supplied edits never alias across fields.</summary>
    internal static readonly Dictionary<Band, Func<int, RgbColor>> StockPaint = new()
    {
        [Band.FightBody] = CrocomirePaint.FightBody,
        [Band.InitialWall] = CrocomirePaint.InitialWall,
        [Band.InitialProjectile] = CrocomirePaint.InitialProjectile,
        [Band.SkeletonArm] = CrocomirePaint.SkeletonArm,
        [Band.WallSpikes] = CrocomirePaint.WallSpikes,
    };

    /// <summary>The native transfer length of each Crocomire material role.</summary>
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

    /// <summary>Retains only colors that differ from their calculated native material paint.</summary>
    internal CrocomireColorCatalog(RgbColor[] fightBody, RgbColor[] initialWall,
        RgbColor[] initialProjectile, RgbColor[] skeletonArm, RgbColor[] wallSpikes) =>
        edits = CrocomirePalette.Edits(new()
        {
            [Band.FightBody] = fightBody,
            [Band.InitialWall] = initialWall,
            [Band.InitialProjectile] = initialProjectile,
            [Band.SkeletonArm] = skeletonArm,
            [Band.WallSpikes] = wallSpikes,
        });

    /// <summary>Installs the native initialization transfers in order: seventeen wall words at CGRAM 160–176, then sixteen projectile words at 208–223.</summary>
    public void ApplyInitial(GameplayColours colours) => CrocomirePalette.ApplyInitial(this, new BossColours(colours));

    /// <summary>Restores the eight selected body/head BG inks from the $A4:B89D role at CGRAM 112–119; the actor separately decides whether a hurt update should show white instead.</summary>
    public void ApplyFightBody(GameplayColours colours) =>
        CrocomirePalette.Apply(this, new BossColours(colours).CrocomireFightBody, Band.FightBody);

    /// <summary>Installs the sixteen skeleton-arm OBJ inks corresponding to $A4:B8FD at CGRAM 144–159 when the wall-break sequence begins.</summary>
    public void ApplySkeletonArm(GameplayColours colours) =>
        CrocomirePalette.Apply(this, new BossColours(colours).CrocomireSkeletonArm, Band.SkeletonArm);

    /// <summary>Installs the sixteen wall-spike OBJ inks corresponding to $A4:B91D at CGRAM 176–191 when the rumble script reaches its terminator.</summary>
    public void ApplyWallSpikes(GameplayColours colours) =>
        CrocomirePalette.Apply(this, new BossColours(colours).CrocomireWallSpikes, Band.WallSpikes);

    /// <summary>Returns an edited color or calculates the corresponding native stock paint.</summary>
    internal RgbColor Get(Band band, int index) => CrocomirePalette.Get(edits, band, index);
}
