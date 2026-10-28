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
    public string ContentIdentity => SelectedPresentationHash.Create("CrocomireColorCatalog-v1", content =>
    {
        Append(content, Band.FightBody, "fightBody");
        Append(content, Band.InitialWall, "initialWall");
        Append(content, Band.InitialProjectile, "initialProjectile");
        Append(content, Band.SkeletonArm, "skeletonArm");
        Append(content, Band.WallSpikes, "wallSpikes");
    });

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
    /// <summary>Independently supplied differences; every unedited stock ink is calculated.</summary>
    internal readonly Dictionary<CrocomireColorKey, RgbColor> edits = [];

    /// <summary>Retains only colors that differ from their calculated native material paint.</summary>
    internal CrocomireColorCatalog(RgbColor[] fightBody, RgbColor[] initialWall,
        RgbColor[] initialProjectile, RgbColor[] skeletonArm, RgbColor[] wallSpikes)
    {
        RetainEdits(Band.FightBody, fightBody);
        RetainEdits(Band.InitialWall, initialWall);
        RetainEdits(Band.InitialProjectile, initialProjectile);
        RetainEdits(Band.SkeletonArm, skeletonArm);
        RetainEdits(Band.WallSpikes, wallSpikes);
    }

    /// <summary>Keeps each supplied color of one band that differs from its calculated stock paint.</summary>
    private void RetainEdits(Band band, RgbColor[] colors)
    {
        for (int color = 0; color < colors.Length; color++)
            if (colors[color] != Stock(band, color))
                edits.Add(new CrocomireColorKey(band, color), colors[color]);
    }

    /// <summary>Each named material calculates its native paint and transfer overlaps; supplied edits never alias across fields.</summary>
    private static RgbColor Stock(Band band, int color) => band switch
    {
        Band.FightBody => CrocomirePaintDefinitions.FightBody(color),
        Band.InitialWall => CrocomirePaintDefinitions.InitialWall(color),
        Band.InitialProjectile => CrocomirePaintDefinitions.InitialProjectile(color),
        Band.SkeletonArm => CrocomirePaintDefinitions.SkeletonArm(color),
        Band.WallSpikes => CrocomirePaintDefinitions.WallSpikes(color),
        _ => throw new ArgumentOutOfRangeException(nameof(band)),
    };
    /// <summary>Chooses the native transfer length for one Crocomire material role.</summary>
    private static int Count(Band band) => band switch
    {
        Band.FightBody => CrocomireColorDefinitions.FightBodyCount,
        Band.InitialWall => CrocomireColorDefinitions.InitialWallCount,
        Band.InitialProjectile => CrocomireColorDefinitions.InitialProjectileCount,
        Band.SkeletonArm => CrocomireColorDefinitions.SkeletonArmCount,
        Band.WallSpikes => CrocomireColorDefinitions.WallSpikesCount,
        _ => throw new ArgumentOutOfRangeException(nameof(band)),
    };

    /// <summary>Hashes a complete resolved transfer in native color order.</summary>
    private void Append(SelectedPresentationHash content, Band band, string label)
    {
        Span<RgbColor> transfer = stackalloc RgbColor[Count(band)];
        for (int color = 0; color < transfer.Length; color++) transfer[color] = Get(band, color);
        content.AppendColors(label, transfer);
    }
    /// <summary>Installs the native initialization transfers in order: seventeen wall words at CGRAM 160–176, then sixteen projectile words at 208–223.</summary>
    public void ApplyInitial(GameplayColours colours)
    {
        Apply(new BossColours(colours).CrocomireInitialWall, Band.InitialWall);
        Apply(new BossColours(colours).CrocomireInitialProjectile, Band.InitialProjectile);
    }

    /// <summary>Restores the eight selected body/head BG inks from the $A4:B89D role at CGRAM 112–119; the actor separately decides whether a hurt update should show white instead.</summary>
    public void ApplyFightBody(GameplayColours colours) =>
        Apply(new BossColours(colours).CrocomireFightBody, Band.FightBody);

    /// <summary>Installs the sixteen skeleton-arm OBJ inks corresponding to $A4:B8FD at CGRAM 144–159 when the wall-break sequence begins.</summary>
    public void ApplySkeletonArm(GameplayColours colours) =>
        Apply(new BossColours(colours).CrocomireSkeletonArm, Band.SkeletonArm);

    /// <summary>Installs the sixteen wall-spike OBJ inks corresponding to $A4:B91D at CGRAM 176–191 when the rumble script reaches its terminator.</summary>
    public void ApplyWallSpikes(GameplayColours colours) =>
        Apply(new BossColours(colours).CrocomireWallSpikes, Band.WallSpikes);

    /// <summary>Returns an edited color or calculates the corresponding native stock paint.</summary>
    internal RgbColor Get(Band band, int index)
    {
        if ((uint)index >= Count(band)) throw new ArgumentOutOfRangeException(nameof(index));
        return edits.TryGetValue(new CrocomireColorKey(band, index), out RgbColor edited) ? edited : Stock(band, index);
    }

    /// <summary>Writes one complete material band to its selected CGRAM destination.</summary>
    private void Apply(ColourRun destination, Band band)
    {
        for (int color = 0; color < Count(band); color++)
            destination.Write(color, Get(band, color));
    }
}
