using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Crocomire.Artwork;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>
/// Crocomire's colours (#1512): whether his body on the background shows its fight colours or its white hurt flash, and the
/// colours his skeleton's death adds to his sprites, the arm's and then the wall spikes'.
/// </summary>
internal sealed partial class CrocomireColours : IGameplayShadingSource
{
    /// <summary>The shading his wall and the bridge's fragments draw in.</summary>
    internal static PictureShading WallShading { get; } = PictureShading.Shaded(CrocomireShades.Wall);

    /// <summary>The shading his projectiles draw in.</summary>
    internal static PictureShading ProjectileShading { get; } = PictureShading.Shaded(CrocomireShades.Projectiles);

    /// <summary>What his body shows (#2560): the room's colours until its first update, then his fight colours or the hurt flash.</summary>
    internal CrocomireBodyColours Body { get; set; }

    /// <summary>How far his death's colours have reached his sprites (#2560): none, the wall spikes', then the skeleton's arm's too.</summary>
    internal CrocomireDeathColours DeathColours { get; set; }

    /// <summary>Shows <paramref name="colours"/> on his body: his fight colours or the white hurt flash.</summary>
    internal void ShowBody(CrocomireBodyColours colours) => Body = colours;

    /// <summary>Shows his skeleton's arm colours on his sprites, after the wall spikes'.</summary>
    /// <exception cref="InvalidOperationException">The wall spikes' colours have not been shown, which the skeleton's reveal follows.</exception>
    internal void ShowSkeletonArm() => DeathColours = DeathColours == CrocomireDeathColours.WallSpikes
        ? CrocomireDeathColours.WallSpikesAndSkeletonArm
        : throw new InvalidOperationException($"Crocomire's skeleton arm colours follow the wall spikes', not {DeathColours}.");

    /// <summary>Shows the wall spikes' colours on his sprites.</summary>
    internal void ShowWallSpikes() => DeathColours = CrocomireDeathColours.WallSpikes;

    /// <summary>
    /// <paramref name="drawn"/>, a sprite draw's own shading, with the colours his death has shown laid over it: the
    /// skeleton's arm, then the spikes.
    /// </summary>
    internal PictureShading OverSprites(PictureShading drawn)
    {
        Ensure.NotNull(drawn);
        PictureShading shading = drawn;
        if (DeathColours == CrocomireDeathColours.WallSpikesAndSkeletonArm)
            shading = shading.With(CrocomireShades.SkeletonArm);
        if (DeathColours != CrocomireDeathColours.None)
            shading = shading.With(CrocomireShades.WallSpikes);
        return shading;
    }

    /// <inheritdoc />
    public void ShadeGameplay(GameplayShadingBuilder frame)
    {
        Ensure.NotNull(frame);
        switch (Body)
        {
            case CrocomireBodyColours.HurtFlash:
                frame.Shade(GameplayPictures.BossBody, CrocomireShades.BodyHurt);
                break;
            case CrocomireBodyColours.Fight:
                frame.Shade(GameplayPictures.BossBody, CrocomireShades.FightBody);
                break;
            case CrocomireBodyColours.Room:
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(CrocomireBodyColours)} {Body}.");
        }
    }
}
