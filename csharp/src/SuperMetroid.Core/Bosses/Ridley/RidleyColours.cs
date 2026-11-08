using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Enemies.Drawing;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// The colours a Ridley encounter has loaded (#1512): the shades laid over its sprites (Ridley's, the Ceres door's, the
/// container's and the Baby's) and over its room's pictures (the alarm, the warning, the retreat, the floor's zoom and
/// Norfair's reveal), in the order they were loaded.
/// </summary>
internal sealed class RidleyColours : IGameplayShadingSource
{
    /// <summary>The shades laid over the encounter's sprites.</summary>
    internal LaidShades Sprites { get; } = new();

    /// <summary>The shades laid over the room's pictures.</summary>
    internal LaidShades Room { get; } = new();

    /// <summary>
    /// The shading the encounter's sprites draw in: the shades laid over them, with the hurt flash's row over those when
    /// <paramref name="flashing"/>, as the flash's draw takes the common row whatever the encounter loaded into its own.
    /// </summary>
    internal PictureShading SpriteShading(bool flashing)
    {
        PictureShading laid = Sprites.Over(PictureShading.Standard);
        return flashing ? laid.With(EnemyShades.HurtFlash) : laid;
    }

    /// <inheritdoc />
    public void ShadeGameplay(GameplayShadingBuilder frame)
    {
        Ensure.NotNull(frame);
        foreach (PictureShade shade in Room.Laid)
            frame.Shade(GameplayPictures.Room, shade);
    }
}
