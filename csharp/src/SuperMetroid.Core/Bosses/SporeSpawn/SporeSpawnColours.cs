using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.SporeSpawn.Artwork;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>
/// Spore Spawn's colours (#1512): the health stage his body shows and the step his death fade has reached over his body
/// and his room. A defeated room loads the fade's last step as he initializes; the door's fade brings it in.
/// </summary>
internal sealed class SporeSpawnColours : IGameplayShadingSource
{
    /// <summary>The health stage his body shows, or null before any.</summary>
    private int? health;

    /// <summary>The step his death fade has reached, or null before it starts.</summary>
    private int? death;

    /// <summary>The shading his body and the objects he spawns draw in: his spores' colours with his body's stage over them.</summary>
    internal PictureShading Sprites
    {
        get
        {
            PictureShading shading = PictureShading.Shaded(SporeSpawnShades.Spores);
            if (death is { } step)
                return shading.With(SporeSpawnShades.DeathSprite(step));
            return health is { } frame ? shading.With(SporeSpawnShades.Health(frame)) : shading;
        }
    }

    /// <summary>Shows health stage <paramref name="frame"/> on his body.</summary>
    internal void ShowHealth(int frame) => health = frame;

    /// <summary>Shows step <paramref name="step"/> of his death fade over his body and his room.</summary>
    internal void ShowDeath(int step) => death = step;

    /// <inheritdoc />
    public void ShadeGameplay(GameplayShadingBuilder frame)
    {
        Ensure.NotNull(frame);
        if (death is not { } step)
            return;
        frame.Shade(GameplayPictures.Room, SporeSpawnShades.DeathLevel(step));
        frame.Shade(GameplayPictures.Room, SporeSpawnShades.DeathBackground(step));
    }
}
