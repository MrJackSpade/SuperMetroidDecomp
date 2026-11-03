using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Core.Rendering;

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

    /// <summary>The shading the encounter's sprites draw in, or the hurt flash's with Ridley's own rows kept when <paramref name="flashing"/>.</summary>
    internal PictureShading SpriteShading(bool flashing) =>
        Sprites.Over(flashing ? EnemyShades.HitFlashed : PictureShading.Standard);

    /// <inheritdoc />
    public void ShadeGameplay(GameplayShadingBuilder frame)
    {
        Ensure.NotNull(frame);
        foreach (PictureShade shade in Room.Laid)
            frame.Shade(GameplayPictures.Room, shade);
    }
}
