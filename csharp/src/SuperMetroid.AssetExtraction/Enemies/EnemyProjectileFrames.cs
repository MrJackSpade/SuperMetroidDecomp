using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// The import pipeline's view of enemy projectile frames (#1463): the installed program frame or bank-$8D composition
/// an <see cref="EnemyProjectileFrameId"/> shows, bound to the installed schema by frame name, which the spawned-object
/// sheet composer composes each frame's picture from. Gameplay draws those pictures by the frame's identity.
/// </summary>
internal static class EnemyProjectileFrames
{
    /// <summary>The frames that draw an installed bank-$8D composition, including the blank one, rather than a program frame.</summary>
    internal static IReadOnlySet<EnemyProjectileFrameId> Compositions { get; } =
        EnemyProjectileSpritemapDefinitions.FrameNames.Select(Identity).ToHashSet();

    /// <summary>The frame whose document name is installed frame <paramref name="name"/> (#2370), never its identity's name.</summary>
    internal static EnemyProjectileFrameId Identity(string name) => EnemyProjectileFrameSheetKeys.Named(name);

    /// <summary>The parts <paramref name="frame"/> draws from installed projectile <paramref name="artwork"/>.</summary>
    internal static ReadOnlyMemory<SpritePart> Parts(EnemyProjectileSpritemapCatalog artwork, EnemyProjectileFrameId frame) =>
        Compositions.Contains(frame) ? artwork.Get(frame) : artwork.GetProgramFrame(frame);
}
