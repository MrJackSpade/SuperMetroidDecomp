using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// The colours Mother Brain's encounter shows (#1512, #1544), each owner's its own: her own sprites (her brain, her neck
/// and her attacks), her body layer, the Baby, her glass shards and tube projectiles, and the effects her fight lays over
/// her room. No owner draws in another's colours: the original game kept several of them in one colour row, so a load
/// for one showed on the others, and copied her brain's colours into another row as she died; the port keeps neither.
/// </summary>
internal sealed class MotherBrainColours : IGameplayShadingSource
{
    /// <summary>Whether her death has detached her brain, which from then on no longer shows her body's colour loads.</summary>
    private bool brainDetached;

    /// <summary>The shades laid over her own sprites: her brain, her neck and her attacks.</summary>
    internal LaidShades Own { get; } = new();

    /// <summary>The shades laid over the Baby.</summary>
    internal LaidShades Baby { get; } = new();

    /// <summary>The shades laid over her body layer.</summary>
    internal LaidShades BodyLayer { get; } = new();

    /// <summary>The shades her fight lays over her room's pictures: its flash, its steady grey and its lights.</summary>
    internal LaidShades Room { get; } = new();

    /// <summary>The shading her glass shards and tube projectiles draw in: the colours her room's entry gives them.</summary>
    internal static PictureShading GlassAndTubes { get; } = PictureShading.Shaded(MotherBrainShades.RoomEntry);

    /// <summary>The shading her own sprites draw in.</summary>
    internal PictureShading OwnShading => Own.Over(PictureShading.Standard);

    /// <summary>The shading the Baby draws in.</summary>
    internal PictureShading BabyShading => Baby.Over(PictureShading.Standard);

    /// <summary>Lays a colour load of her whole body over her body layer and her own sprites; once her brain is detached, over her body layer alone.</summary>
    internal void LayOverBodyAndBrain(PictureShade shade)
    {
        BodyLayer.Lay(shade);
        if (!brainDetached)
            Own.Lay(shade);
    }

    /// <summary>Detaches her brain as she dies: it keeps the colours it shows, and only the corpse's fade changes them from here.</summary>
    internal void DetachBrain() => brainDetached = true;

    /// <summary>Lays a step of the corpse's fade over her detached brain.</summary>
    /// <exception cref="InvalidOperationException">Her brain has not been detached.</exception>
    internal void LayOverCorpse(PictureShade shade)
    {
        if (!brainDetached)
            throw new InvalidOperationException("Mother Brain's corpse fades after her death detaches her brain.");
        Own.Lay(shade);
    }

    /// <inheritdoc />
    public void ShadeGameplay(GameplayShadingBuilder frame)
    {
        Ensure.NotNull(frame);
        foreach (PictureShade shade in BodyLayer.Laid)
            frame.Shade(GameplayPictures.BossBody, shade);
        foreach (PictureShade shade in Room.Laid)
            frame.Shade(GameplayPictures.Room, shade);
    }
}
