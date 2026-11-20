using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Crocomire.Artwork;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>
/// Crocomire's colours (#1512): whether his body on the background shows its fight colours or its white hurt flash, and the
/// colours his skeleton's death adds to his sprites, the arm's and then the wall spikes'.
/// </summary>
internal sealed class CrocomireColours : IGameplayShadingSource
{
    /// <summary>The shading his wall and the bridge's fragments draw in.</summary>
    internal static PictureShading WallShading { get; } = PictureShading.Shaded(CrocomireShades.Wall);

    /// <summary>The shading his projectiles draw in.</summary>
    internal static PictureShading ProjectileShading { get; } = PictureShading.Shaded(CrocomireShades.Projectiles);

    /// <summary>Whether his body has shown its fight colours; before, it shows the room's.</summary>
    private bool ShowsFightBody { get; set; }

    /// <summary>Whether his body shows the white hurt flash.</summary>
    private bool HurtFlash { get; set; }

    /// <summary>Whether his sprites show his skeleton's arm colours.</summary>
    private bool ShowsSkeletonArm { get; set; }

    /// <summary>Whether his sprites show the wall spikes' colours.</summary>
    private bool ShowsWallSpikes { get; set; }

    /// <summary>Shows his body's fight colours, or the white hurt flash when <paramref name="white"/>.</summary>
    internal void ShowBody(bool white)
    {
        ShowsFightBody |= !white;
        HurtFlash = white;
    }

    /// <summary>Shows his skeleton's arm colours on his sprites.</summary>
    internal void ShowSkeletonArm() => ShowsSkeletonArm = true;

    /// <summary>Shows the wall spikes' colours on his sprites.</summary>
    internal void ShowWallSpikes() => ShowsWallSpikes = true;

    /// <summary>
    /// <paramref name="drawn"/>, a sprite draw's own shading, with the colours his death has shown laid over it: the
    /// skeleton's arm, then the spikes.
    /// </summary>
    internal PictureShading OverSprites(PictureShading drawn)
    {
        Ensure.NotNull(drawn);
        PictureShading shading = drawn;
        if (ShowsSkeletonArm)
            shading = shading.With(CrocomireShades.SkeletonArm);
        if (ShowsWallSpikes)
            shading = shading.With(CrocomireShades.WallSpikes);
        return shading;
    }

    /// <inheritdoc />
    public void ShadeGameplay(GameplayShadingBuilder frame)
    {
        Ensure.NotNull(frame);
        if (HurtFlash)
            frame.Shade(GameplayPictures.BossBody, CrocomireShades.BodyHurt);
        else if (ShowsFightBody)
            frame.Shade(GameplayPictures.BossBody, CrocomireShades.FightBody);
    }
}
