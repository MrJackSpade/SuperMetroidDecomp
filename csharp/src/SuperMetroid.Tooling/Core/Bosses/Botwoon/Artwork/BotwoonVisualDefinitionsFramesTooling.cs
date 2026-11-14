using SuperMetroid.Core.Spatial;
namespace SuperMetroid.Core.Bosses.Botwoon.Artwork;

/// <summary>Frame lists and counts of <see cref="BotwoonVisualDefinitions"/> that only development tools read.</summary>
internal static class BotwoonVisualDefinitionsFramesTooling
{
    /// <summary>The members, read as members of <see cref="BotwoonVisualDefinitions"/>.</summary>
    extension(BotwoonVisualDefinitions)
    {
        /// <summary>
        /// Every projectile frame in installed-artwork order: body links by physical facing and phase,
        /// then the tails, the hidden segment and the spit phases.
        /// </summary>
        internal static IReadOnlyList<BotwoonProjectileFrame> ProjectileFrames =>
        [
            .. BotwoonVisualToolingDefinitions.PhysicalOrder.SelectMany(facing => Enumerable.Range(0, BotwoonAnimations.BodyPhaseCount)
                .Select(phase => BotwoonProjectileFrame.Body(facing, phase))),
            .. BotwoonVisualDefinitions.TailOrder.Select(BotwoonProjectileFrame.Tail),
            BotwoonProjectileFrame.Hidden,
            .. Enumerable.Range(0, BotwoonAnimations.SpitPhaseCount)
                .Select(BotwoonProjectileFrame.Spit),
        ];

        /// <summary>The tail facings in the cartridge's tail program order, from up counterclockwise to up-right.</summary>
        private static CompassDirection[] TailOrder =>
        [
            CompassDirection.Up, CompassDirection.UpLeft, CompassDirection.Left, CompassDirection.DownLeft,
            CompassDirection.Down, CompassDirection.DownRight, CompassDirection.Right, CompassDirection.UpRight,
        ];
    }
}
