using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// The colours Mother Brain's encounter shows (#1512, #1544, #1597), each owner's its own: her own sprites (her brain, her
/// neck and her attacks), her body layer, the Baby, her glass shards and tube projectiles, and her room. Each shows the
/// current step of each effect her fight has reached, in the fixed order the fight reaches them: her fake death's fade,
/// her ascent's attack colours, her beam's colours, the Baby's drain to grey, her revival, her health and her death's
/// fade over her body; her room's grey and then its lights over her room; the Baby's arrival and then its fade over the
/// Baby. A later effect shows over the earlier ones where it colours the same part of her.
/// </summary>
internal sealed class MotherBrainColours : IGameplayShadingSource
{
    /// <summary>
    /// The image each fade layer last showed (<see cref="MotherBrainFadeLayer"/>): her brain's fake-death fade or its
    /// revival, the Baby's drain of her body, her revival, her death's fade, her corpse's fade, her room's lights
    /// recovering and the Baby's fade; a layer no fade has reached yet shows none.
    /// </summary>
    private readonly Dictionary<MotherBrainFadeLayer, MotherBrainFadeImage> fades = [];

    /// <summary>Whether her ascent has shown her phase-two attack and rear-leg colours.</summary>
    private bool phaseTwo;

    /// <summary>Her beam's colours over her whole body: a rainbow phase or the colours it restores, or null before any.</summary>
    private PictureShade? beam;

    /// <summary>The health band her whole body shows, or null before her revival enables it.</summary>
    private MotherBrainHealthBand? healthBand;

    /// <summary>The shading her detached brain kept as her death detached it, or null while it is attached.</summary>
    private PictureShading? detachedBrain;

    /// <summary>Her room's flash step, or its steady grey once the flash stops, or null before the fake death.</summary>
    private PictureShade? roomGrey;

    /// <summary>Whether her room's lights have gone out as the Baby dies; their recovery shows over it.</summary>
    private bool lightsOut;

    /// <summary>Whether the Baby has arrived in its own colours.</summary>
    private bool babyArrived;

    /// <summary>The shading her glass shards and tube projectiles draw in: the colours her room's entry gives them.</summary>
    internal static PictureShading GlassAndTubes { get; } = PictureShading.Shaded(MotherBrainShades.RoomEntry);

    /// <summary>
    /// The shading her own sprites draw in: her fake death's fade and her ascent's attack colours, under her whole body's
    /// colouring; once her death detaches her brain, the colours it kept, under its corpse fade.
    /// </summary>
    internal PictureShading OwnShading
    {
        get
        {
            if (detachedBrain is { } kept)
                return Shown(MotherBrainFadeLayer.Corpse) is { } image ? kept.With(MotherBrainShades.Fade(image)) : kept;
            PictureShading shading = PictureShading.Standard;
            if (Shown(MotherBrainFadeLayer.Brain) is { } fade)
                shading = shading.With(MotherBrainShades.Fade(fade));
            if (phaseTwo)
                shading = shading.With(MotherBrainShades.PhaseTwo);
            return OverBody(shading);
        }
    }

    /// <summary>The shading her body layer shows: her whole body's colouring.</summary>
    private PictureShading BodyLayerShading => OverBody(PictureShading.Standard);

    /// <summary>The shading her fight lays over her room's pictures: its grey, then its lights.</summary>
    private PictureShading RoomShading
    {
        get
        {
            PictureShading shading = PictureShading.Standard;
            if (roomGrey is { } grey)
                shading = shading.With(grey);
            if (Shown(MotherBrainFadeLayer.RoomLights) is { } recovering)
                return shading.With(MotherBrainShades.Fade(recovering));
            return lightsOut ? shading.With(MotherBrainShades.LightsOut) : shading;
        }
    }

    /// <summary>The shading the Baby draws in: its arrival's colours, then its fade.</summary>
    internal PictureShading BabyShading
    {
        get
        {
            PictureShading shading = babyArrived ? PictureShading.Shaded(MotherBrainShades.BabyArrives) : PictureShading.Standard;
            return Shown(MotherBrainFadeLayer.Baby) is { } image ? shading.With(MotherBrainShades.Fade(image)) : shading;
        }
    }

    /// <summary>
    /// Shows <paramref name="image"/> on its fade's layer (<see cref="MotherBrainFade.Layer"/>), in place of the image that
    /// layer showed before.
    /// </summary>
    internal void ShowFade(MotherBrainFadeImage image) => fades[Ensure.NotNull(image.Fade).Layer] = image;

    /// <summary>The image <paramref name="layer"/> last showed, or null before any of its fades reached it.</summary>
    private MotherBrainFadeImage? Shown(MotherBrainFadeLayer layer) => fades.TryGetValue(layer, out MotherBrainFadeImage image) ? image : null;

    /// <summary>Shows her phase-two attack and rear-leg colours, which her ascent loads.</summary>
    internal void ShowPhaseTwo() => phaseTwo = true;

    /// <summary>Shows rainbow image <paramref name="image"/> over her whole body.</summary>
    internal void ShowRainbow(MotherBrainRainbowImage image) => beam = MotherBrainShades.Rainbow(image);

    /// <summary>Shows the colours her beam restores over her whole body as it ends.</summary>
    internal void ShowRestored() => beam = MotherBrainShades.Restored;

    /// <summary>Shows <paramref name="band"/>'s colours over her whole body.</summary>
    internal void ShowHealth(MotherBrainHealthBand band) => healthBand = Ensure.NotNull(band);

    /// <summary>Detaches her brain as she dies: it keeps the colours it shows, and only the corpse's fade changes them from here.</summary>
    internal void DetachBrain() => detachedBrain ??= OwnShading;

    /// <summary>Shows image <paramref name="image"/> of her room's flash.</summary>
    internal void ShowRoomFlash(MotherBrainRoomFlashImage image) => roomGrey = MotherBrainShades.Flash(image);

    /// <summary>Shows her room's steady grey after the flash stops.</summary>
    internal void ShowRoomGrey() => roomGrey = MotherBrainShades.FinalRoom;

    /// <summary>Shows her room's lights gone out as the Baby dies.</summary>
    internal void ShowLightsOut() => lightsOut = true;

    /// <summary>Shows the Baby in the colours it arrives in.</summary>
    internal void ShowBabyArrived() => babyArrived = true;

    /// <summary>
    /// <paramref name="under"/> with her whole body's colouring over it: her beam's colours, the drain over the rainbow it
    /// ran out on, her revival over the drain's grey, her health and her death's fade, each where it has started.
    /// </summary>
    private PictureShading OverBody(PictureShading under)
    {
        PictureShading shading = under;
        if (beam is { } colours)
            shading = shading.With(colours);
        if (Shown(MotherBrainFadeLayer.BodyDrain) is { } drained)
            shading = shading.With(MotherBrainShades.Fade(drained));
        if (Shown(MotherBrainFadeLayer.BodyRevival) is { } revived)
            shading = shading.With(MotherBrainShades.Fade(revived));
        if (healthBand is { } band)
            shading = shading.With(MotherBrainShades.Health(band));
        return Shown(MotherBrainFadeLayer.BodyDeath) is { } dying ? shading.With(MotherBrainShades.Fade(dying)) : shading;
    }

    /// <inheritdoc />
    public void ShadeGameplay(GameplayShadingBuilder frame)
    {
        Ensure.NotNull(frame);
        foreach (PictureShade shade in BodyLayerShading.Shades)
            frame.Shade(GameplayPictures.BossBody, shade);
        foreach (PictureShade shade in RoomShading.Shades)
            frame.Shade(GameplayPictures.Room, shade);
    }
}
