using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>
/// Verification's native view of the enemy-projectile artwork: the bank-$8D spritemap each composition is stored at and
/// the bank-$86 operand each program frame is stored at, both read through the import's tables.
/// </summary>
internal static class EnemyProjectileSpritemapNative
{
    /// <summary>The compositions' native locations, read as members of <see cref="EnemyProjectileSpritemapDefinitions"/>.</summary>
    extension(EnemyProjectileSpritemapDefinitions)
    {
        /// <summary>The empty bank-$8D projectile spritemap, which draws nothing.</summary>
        internal static ushort BlankSpritemap => EnemyProjectileCompositionDefinitions.BlankSpritemap;

        /// <summary>
        /// Each composition's bank-$8D spritemap and installed name, in schema order: the projectile compositions, then the
        /// Ceres elevator arrival's pictures (#2425).
        /// </summary>
        internal static SpritemapFrameDefinition[] Frames => [.. EnemyProjectileSpritemapDefinitions.FrameNames.Select(name =>
            new SpritemapFrameDefinition(EnemyProjectileCompositionSources.Spritemap(EnemyProjectileFrames.Identity(name)), name)),
            .. EnemyProjectileSpritemapDefinitions.ElevatorArrivalFrameNames.Select(entry =>
                new SpritemapFrameDefinition(EnemyProjectileCompositionSources.ElevatorArrivalSpritemap(entry.Key), entry.Value))];
    }

    /// <summary>The composition frame stored at bank-$8D spritemap <paramref name="spritemap"/>.</summary>
    internal static EnemyProjectileFrameId FrameAt(ushort spritemap) => spritemap == EnemyProjectileCompositionDefinitions.BlankSpritemap
        ? EnemyProjectileFrameId.Blank
        : EnemyProjectileSpritemapDefinitions.FrameNames.Select(EnemyProjectileFrames.Identity)
            .Single(frame => EnemyProjectileCompositionSources.Spritemap(frame) == spritemap);

    /// <summary>The Ceres elevator arrival's pose whose picture is stored at bank-$8D spritemap <paramref name="spritemap"/>, if any.</summary>
    private static SuperMetroid.Core.Areas.Ceres.CeresElevatorArrivalPose? ElevatorArrivalPoseAt(ushort spritemap) =>
        EnemyProjectileSpritemapDefinitions.ElevatorArrivalFrameNames.Keys
            .Where(pose => EnemyProjectileCompositionSources.ElevatorArrivalSpritemap(pose) == spritemap)
            .Select(pose => (SuperMetroid.Core.Areas.Ceres.CeresElevatorArrivalPose?)pose).SingleOrDefault();

    /// <summary>Native lookups of an installed catalog.</summary>
    extension(EnemyProjectileSpritemapCatalog catalog)
    {
        /// <summary>The parts of the composition stored at bank-$8D spritemap <paramref name="spritemap"/>.</summary>
        internal ReadOnlyMemory<SpritePart> Get(ushort spritemap) => ElevatorArrivalPoseAt(spritemap) is { } pose
            ? catalog.GetElevatorArrival(pose)
            : catalog.Get(FrameAt(spritemap));
    }

    /// <summary>Native locations of an installed program frame.</summary>
    extension(EnemyProjectilePresentationFrameDefinition frame)
    {
        /// <summary>The bank-$86 operand the program frame is stored at.</summary>
        internal ushort OperandAddress => EnemyProjectileProgramFrameSources.Operand(frame.Frame);
    }
}
