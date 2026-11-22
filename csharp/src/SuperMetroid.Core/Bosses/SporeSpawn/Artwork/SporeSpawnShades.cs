using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.SporeSpawn.Artwork;

/// <summary>
/// The shades the import pre-colours Spore Spawn's sheets and his room's pictures in (#1512): his spores' colours, his
/// body's four health bands, and each step of his death fade over his body, the room's level and its background.
/// </summary>
public static partial class SporeSpawnShades
{
    /// <summary>The spores' colours, which he loads as he initializes.</summary>
    public static PictureShade Spores { get; } = new("spore-spawn-spores", 0);

    /// <summary>His body in <paramref name="band"/>'s colours (#2242).</summary>
    public static PictureShade Health(SporeSpawnHealthBand band) => new($"spore-spawn-health-{Ensure.NotNull(band).Key}", 0);

    /// <summary>His body at step <paramref name="step"/> of his death fade.</summary>
    public static PictureShade DeathSprite(SporeSpawnDeathFadeStep step) => new("spore-spawn-death-sprite", step.Step);

    /// <summary>The room's level at step <paramref name="step"/> of his death fade.</summary>
    public static PictureShade DeathLevel(SporeSpawnDeathFadeStep step) => new("spore-spawn-death-level", step.Step);

    /// <summary>The room's background at step <paramref name="step"/> of his death fade.</summary>
    public static PictureShade DeathBackground(SporeSpawnDeathFadeStep step) => new("spore-spawn-death-background", step.Step);
}
