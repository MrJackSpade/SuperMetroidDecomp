using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Bosses.Crocomire.Artwork;

/// <summary>
/// Crocomire's installed fight, wall, projectile, skeleton-arm, and wall-spike RGB5 colors, each transfer kept as installed.
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
        /// <summary>Sixteen projectile colors loaded after the wall colours, kept as installed where it overlaps them.</summary>
        InitialProjectile,
        /// <summary>Sixteen skeleton-arm colors installed when the wall break begins.</summary>
        SkeletonArm,
        /// <summary>Sixteen wall-spike colors installed at the rumble terminator.</summary>
        WallSpikes,
    }

    /// <summary>The colours each transfer holds.</summary>
    internal static readonly Dictionary<Band, int> Counts = new()
    {
        [Band.FightBody] = CrocomireColorDefinitions.FightBodyCount,
        [Band.InitialWall] = CrocomireColorDefinitions.InitialWallCount,
        [Band.InitialProjectile] = CrocomireColorDefinitions.InitialProjectileCount,
        [Band.SkeletonArm] = CrocomireColorDefinitions.SkeletonArmCount,
        [Band.WallSpikes] = CrocomireColorDefinitions.WallSpikesCount,
    };

    /// <summary>Each transfer's installed colours.</summary>
    private readonly Dictionary<Band, RgbColor[]> transfers;

    /// <summary>Keeps every transfer's complete installed colours.</summary>
    /// <param name="transfers">Each of the five transfers' colours, as installed.</param>
    /// <exception cref="InvalidDataException">A transfer is missing or holds another number of colours.</exception>
    internal CrocomireColorCatalog(Dictionary<Band, RgbColor[]> transfers)
    {
        foreach ((Band band, int count) in Counts)
            if (!transfers.TryGetValue(band, out RgbColor[]? colours) || colours.Length != count)
                throw new InvalidDataException($"Crocomire {band} requires {count} installed colors.");
        this.transfers = transfers.ToDictionary(pair => pair.Key, pair => (RgbColor[])pair.Value.Clone());
    }

    /// <summary>The installed colour <paramref name="index"/> of transfer <paramref name="band"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The index is outside the transfer.</exception>
    internal RgbColor Get(Band band, int index)
    {
        RgbColor[] colours = transfers[band];
        return (uint)index < (uint)colours.Length ? colours[index] : throw new ArgumentOutOfRangeException(nameof(index));
    }
}
