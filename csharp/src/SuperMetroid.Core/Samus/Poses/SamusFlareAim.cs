using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Samus.Poses;

/// <summary>
/// Where a pose places a held charge's flare (#2529): at the muzzle of one of the ten aims, or, while turning, at the
/// placement of the aim the turn holds.
/// </summary>
internal abstract record SamusFlareAim
{
    /// <summary>Closes the hierarchy to the placements declared here.</summary>
    private SamusFlareAim()
    {
    }

    /// <summary>The flare at the muzzle of <paramref name="Direction"/>.</summary>
    /// <param name="Direction">The aim whose muzzle the flare is placed at.</param>
    internal sealed record AtMuzzle(SamusProjectileDirection Direction) : SamusFlareAim
    {
        /// <inheritdoc/>
        internal override Offset16<RoomPixels> PlacedBy(Artwork.BeamFlarePlacements placements, bool running) =>
            Ensure.NotNull(placements).AtMuzzle(running, Direction);
    }

    /// <summary>The flare at the turning placement of a turn holding <paramref name="HeldAim"/>.</summary>
    /// <param name="HeldAim">The aim the turn holds, whose turning placement the flare takes.</param>
    internal sealed record WhileTurning(SamusTurningAim HeldAim) : SamusFlareAim
    {
        /// <inheritdoc/>
        /// <remarks>A turning pose's movement is its turn, never running, so its placement is the standing one.</remarks>
        internal override Offset16<RoomPixels> PlacedBy(Artwork.BeamFlarePlacements placements, bool running) =>
            Ensure.NotNull(placements).WhileTurning(HeldAim);
    }

    /// <summary>The beam flare's installed visual offset at this placement.</summary>
    /// <param name="placements">The beam flare's installed placements.</param>
    /// <param name="running">Whether Samus runs; a turning placement is a standing one.</param>
    internal abstract Offset16<RoomPixels> PlacedBy(Artwork.BeamFlarePlacements placements, bool running);
}
