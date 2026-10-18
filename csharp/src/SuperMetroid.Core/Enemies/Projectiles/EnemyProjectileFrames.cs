using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// The presentation boundary of enemy projectile frames: the installed program frame or bank-$8D composition an
/// <see cref="EnemyProjectileFrameId"/> shows, bound to the installed schema by frame name.
/// </summary>
internal static class EnemyProjectileFrames
{
    /// <summary>Each installed program frame's artwork key.</summary>
    private static readonly Lazy<Dictionary<EnemyProjectileFrameId, ushort>> ProgramFrameKeys = new(Bind);

    /// <summary>The bank-$8D composition each composition frame draws.</summary>
    private static readonly Dictionary<EnemyProjectileFrameId, ushort> Compositions = new()
    {
        [EnemyProjectileFrameId.Blank] = EnemyProjectileSpritemapDefinitions.BlankSpritemap,
        [EnemyProjectileFrameId.SkreeDebris] = SkreeMetareeParticleVisualDefinitions.SkreeComposition,
        [EnemyProjectileFrameId.MetareeDebris] = SkreeMetareeParticleVisualDefinitions.MetareeComposition,
    };

    /// <summary>Binds every installed program frame to the identity its name gives.</summary>
    private static Dictionary<EnemyProjectileFrameId, ushort> Bind()
    {
        var keys = new Dictionary<EnemyProjectileFrameId, ushort>();
        foreach (EnemyProjectilePresentationFrameDefinition frame in EnemyProjectilePresentationFrameSets.All)
            keys.Add(Identity(frame.Name), frame.OperandAddress);
        return keys;
    }

    /// <summary>The identity named after installed program frame <paramref name="name"/>.</summary>
    internal static EnemyProjectileFrameId Identity(string name) =>
        Enum.Parse<EnemyProjectileFrameId>(string.Concat(name.Split('_').Select(part => part.Length == 0 ? part : char.ToUpperInvariant(part[0]) + part[1..])));

    /// <summary>Whether <paramref name="frame"/> draws a bank-$8D composition rather than an installed program frame.</summary>
    internal static bool IsComposition(EnemyProjectileFrameId frame) => Compositions.ContainsKey(frame);

    /// <summary>The bank-$8D composition that composition frame <paramref name="frame"/> draws.</summary>
    internal static ushort Composition(EnemyProjectileFrameId frame) => Compositions.TryGetValue(frame, out ushort spritemap)
        ? spritemap
        : throw new ArgumentOutOfRangeException(nameof(frame), frame, "This projectile frame is an installed program frame.");

    /// <summary>The installed artwork key of program frame <paramref name="frame"/>.</summary>
    internal static ushort ProgramFrameKey(EnemyProjectileFrameId frame) => ProgramFrameKeys.Value.TryGetValue(frame, out ushort key)
        ? key
        : throw new ArgumentOutOfRangeException(nameof(frame), frame, "No installed program frame shows this projectile frame.");

    /// <summary>The parts <paramref name="frame"/> draws from installed projectile <paramref name="artwork"/>.</summary>
    internal static ReadOnlyMemory<EnemySpritemapPart> Parts(EnemyProjectileSpritemapCatalog artwork, EnemyProjectileFrameId frame) =>
        Compositions.TryGetValue(frame, out ushort spritemap) ? artwork.Get(spritemap) : artwork.GetProgramFrame(ProgramFrameKey(frame));
}
