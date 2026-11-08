using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Enemies.Drawing;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// The colours a Ridley encounter has loaded (#1512, #1544): the shades laid over Ridley's own sprites (with the Ceres
/// Baby and its container he carries) and over his room's pictures (the alarm, the warning, the retreat, the floor's zoom and
/// Norfair's reveal), in the order they were loaded.
/// </summary>
internal sealed class RidleyColours : IGameplayShadingSource
{
    /// <summary>The shades laid over the encounter's sprites.</summary>
    internal LaidShades Sprites { get; } = new();

    /// <summary>The shades laid over the room's pictures.</summary>
    internal LaidShades Room { get; } = new();

    /// <summary>
    /// The shading the encounter's sprites draw in: the shades laid over them, with the hurt flash over those when
    /// <paramref name="flashing"/>, which shows whatever the encounter laid.
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
