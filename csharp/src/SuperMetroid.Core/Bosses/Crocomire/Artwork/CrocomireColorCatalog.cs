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
    internal readonly Dictionary<CrocomireColorKey, Bgr555> edits = [];

    /// <summary>Retains only colors that differ from their calculated native material paint.</summary>
    internal CrocomireColorCatalog(Bgr555[] fightBody, Bgr555[] initialWall,
        Bgr555[] initialProjectile, Bgr555[] skeletonArm, Bgr555[] wallSpikes)
    {
        Bgr555[][] supplied = [fightBody, initialWall, initialProjectile, skeletonArm, wallSpikes];
        for (int band = 0; band < supplied.Length; band++)
            for (int color = 0; color < supplied[band].Length; color++)
                if (supplied[band][color] != Stock((Band)band, color))
                    edits.Add(new CrocomireColorKey((Band)band, color), supplied[band][color]);
    }

    /// <summary>Each named material calculates its native paint and transfer overlaps; supplied edits never alias across fields.</summary>
    private static Bgr555 Stock(Band band, int color) => band switch
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
        Band.FightBody => CrocomirePaletteRomData.FightBodyCount,
        Band.InitialWall => CrocomirePaletteRomData.InitialWallCount,
        Band.InitialProjectile => CrocomirePaletteRomData.InitialProjectileCount,
        Band.SkeletonArm => CrocomirePaletteRomData.SkeletonArmCount,
        Band.WallSpikes => CrocomirePaletteRomData.WallSpikesCount,
        _ => throw new ArgumentOutOfRangeException(nameof(band)),
    };

    /// <summary>Hashes a complete resolved transfer in native color order.</summary>
    private void Append(SelectedPresentationHash content, Band band, string label)
    {
        Span<Bgr555> transfer = stackalloc Bgr555[Count(band)];
        for (int color = 0; color < transfer.Length; color++) transfer[color] = Get(band, color);
        content.AppendColors(label, transfer);
    }
    /// <summary>Installs the native initialization transfers in order: seventeen wall words at CGRAM 160–176, then seventeen projectile words at 208–224, retaining each source overlap word.</summary>
    public void ApplyInitial(GameplayColours cgram)
    {
        Apply(new BossColours(cgram).CrocomireInitialWall, Band.InitialWall);
        Apply(new BossColours(cgram).CrocomireInitialProjectile, Band.InitialProjectile);
    }

    /// <summary>Restores the eight selected body/head BG inks from the $A4:B89D role at CGRAM 112–119; the actor separately decides whether a hurt update should show white instead.</summary>
    public void ApplyFightBody(GameplayColours cgram) =>
        Apply(new BossColours(cgram).CrocomireFightBody, Band.FightBody);

    /// <summary>Installs the sixteen skeleton-arm OBJ inks corresponding to $A4:B8FD at CGRAM 144–159 when the wall-break sequence begins.</summary>
    public void ApplySkeletonArm(GameplayColours cgram) =>
        Apply(new BossColours(cgram).CrocomireSkeletonArm, Band.SkeletonArm);

    /// <summary>Installs the sixteen wall-spike OBJ inks corresponding to $A4:B91D at CGRAM 176–191 when the rumble script reaches its terminator.</summary>
    public void ApplyWallSpikes(GameplayColours cgram) =>
        Apply(new BossColours(cgram).CrocomireWallSpikes, Band.WallSpikes);

    /// <summary>Returns an edited color or calculates the corresponding native stock paint.</summary>
    internal Bgr555 Get(Band band, int index)
    {
        if ((uint)index >= Count(band)) throw new ArgumentOutOfRangeException(nameof(index));
        return edits.TryGetValue(new CrocomireColorKey(band, index), out Bgr555 edited) ? edited : Stock(band, index);
    }

    /// <summary>Writes one complete material band to its selected CGRAM destination.</summary>
    private void Apply(ColourRun destination, Band band)
    {
        for (int color = 0; color < Count(band); color++)
            destination.Write(color, Get(band, color));
    }
}
