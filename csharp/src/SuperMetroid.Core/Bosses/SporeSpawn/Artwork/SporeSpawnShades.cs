using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.SporeSpawn.Artwork;

/// <summary>
/// The shades the import pre-colours Spore Spawn's sheets and his room's pictures in (#1512): his spores' colours, his
/// body's four health stages, and each step of his death fade over his body, the room's level and its background.
/// </summary>
public static class SporeSpawnShades
{
    /// <summary>The spores' colours, which he loads as he initializes.</summary>
    public static PictureShade Spores { get; } = new("spore-spawn-spores", 0);

    /// <summary>His body in health stage <paramref name="frame"/>, zero through three.</summary>
    public static PictureShade Health(int frame) => new("spore-spawn-health", frame);

    /// <summary>His body at step <paramref name="step"/> of his death fade.</summary>
    public static PictureShade DeathSprite(int step) => new("spore-spawn-death-sprite", step);

    /// <summary>The room's level at step <paramref name="step"/> of his death fade.</summary>
    public static PictureShade DeathLevel(int step) => new("spore-spawn-death-level", step);

    /// <summary>The room's background at step <paramref name="step"/> of his death fade.</summary>
    public static PictureShade DeathBackground(int step) => new("spore-spawn-death-background", step);
}
