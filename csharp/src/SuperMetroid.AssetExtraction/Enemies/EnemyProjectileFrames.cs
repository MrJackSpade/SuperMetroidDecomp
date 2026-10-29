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
        EnemyProjectileSpritemapDefinitions.FrameNames.Select(Identity).Append(EnemyProjectileFrameId.Blank).ToHashSet();

    /// <summary>The identity named after installed program frame <paramref name="name"/>.</summary>
    internal static EnemyProjectileFrameId Identity(string name) =>
        Enum.Parse<EnemyProjectileFrameId>(string.Concat(name.Split('_').Select(part => part.Length == 0 ? part : char.ToUpperInvariant(part[0]) + part[1..])));

    /// <summary>The parts <paramref name="frame"/> draws from installed projectile <paramref name="artwork"/>.</summary>
    internal static ReadOnlyMemory<SpritePart> Parts(EnemyProjectileSpritemapCatalog artwork, EnemyProjectileFrameId frame) =>
        Compositions.Contains(frame) ? artwork.Get(frame) : artwork.GetProgramFrame(frame);
}
