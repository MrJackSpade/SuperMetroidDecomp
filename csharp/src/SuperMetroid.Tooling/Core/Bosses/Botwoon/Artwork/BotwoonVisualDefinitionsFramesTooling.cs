using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Core.Enemies.Projectiles;

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
            .. BotwoonVisualDefinitions.PhysicalOrder.SelectMany(facing => Enumerable.Range(0, BotwoonAnimations.BodyPhaseCount)
                .Select(phase => new BotwoonProjectileFrame(BotwoonProjectileFrameKind.Body, facing, phase))),
            .. BotwoonVisualDefinitions.TailOrder.Select(facing => new BotwoonProjectileFrame(BotwoonProjectileFrameKind.Tail, facing, 0)),
            new BotwoonProjectileFrame(BotwoonProjectileFrameKind.Hidden, BotwoonFacing.Up, 0),
            .. Enumerable.Range(0, BotwoonAnimations.SpitPhaseCount)
                .Select(phase => new BotwoonProjectileFrame(BotwoonProjectileFrameKind.Spit, BotwoonFacing.Up, phase)),
        ];

        /// <summary>The tail facings in the cartridge's tail program order, from up counterclockwise to up-right.</summary>
        private static BotwoonFacing[] TailOrder =>
        [
            BotwoonFacing.Up, BotwoonFacing.UpLeft, BotwoonFacing.Left, BotwoonFacing.DownLeft,
            BotwoonFacing.Down, BotwoonFacing.DownRight, BotwoonFacing.Right, BotwoonFacing.UpRight,
        ];
    }
}
