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
    /// <summary>Her brain's image of the fake death's fade to grey or of its revival, or null before the fake death.</summary>
    private MotherBrainFadeImage? fakeDeathFade;

    /// <summary>Whether her ascent has shown her phase-two attack and rear-leg colours.</summary>
    private bool phaseTwo;

    /// <summary>Her beam's colours over her whole body: a rainbow phase or the colours it restores, or null before any.</summary>
    private PictureShade? beam;

    /// <summary>The image her drain to grey by the Baby has reached, or null before it starts.</summary>
    private MotherBrainFadeImage? drain;

    /// <summary>The image her revival from grey has reached, or null before it starts.</summary>
    private MotherBrainFadeImage? revival;

    /// <summary>The health band her whole body shows, or null before her revival enables it.</summary>
    private MotherBrainHealthBand? healthBand;

    /// <summary>The image her death's fade of her body has reached, or null before it starts.</summary>
    private MotherBrainFadeImage? deathFade;

    /// <summary>The shading her detached brain kept as her death detached it, or null while it is attached.</summary>
    private PictureShading? detachedBrain;

    /// <summary>The image her detached brain's corpse fade has reached, or null before it starts.</summary>
    private MotherBrainFadeImage? corpseFade;

    /// <summary>Her room's flash step, or its steady grey once the flash stops, or null before the fake death.</summary>
    private PictureShade? roomGrey;

    /// <summary>Her room's lights going out as the Baby dies, or their recovery step, or null before.</summary>
    private PictureShade? roomLights;

    /// <summary>Whether the Baby has arrived in its own colours.</summary>
    private bool babyArrived;

    /// <summary>The image of the Baby's fade to black it shows, or null before it starts.</summary>
    private MotherBrainFadeImage? babyFade;

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
                return corpseFade is { } image ? kept.With(MotherBrainShades.Fade(image)) : kept;
            PictureShading shading = PictureShading.Standard;
            if (fakeDeathFade is { } fade)
                shading = shading.With(MotherBrainShades.Fade(fade));
            if (phaseTwo)
                shading = shading.With(MotherBrainShades.PhaseTwo);
            return OverBody(shading);
        }
    }

    /// <summary>The shading her body layer shows: her whole body's colouring.</summary>
    internal PictureShading BodyLayerShading => OverBody(PictureShading.Standard);

    /// <summary>The shading her fight lays over her room's pictures: its grey, then its lights.</summary>
    internal PictureShading RoomShading
    {
        get
        {
            PictureShading shading = PictureShading.Standard;
            if (roomGrey is { } grey)
                shading = shading.With(grey);
            return roomLights is { } lights ? shading.With(lights) : shading;
        }
    }

    /// <summary>The shading the Baby draws in: its arrival's colours, then its fade.</summary>
    internal PictureShading BabyShading
    {
        get
        {
            PictureShading shading = babyArrived ? PictureShading.Shaded(MotherBrainShades.BabyArrives) : PictureShading.Standard;
            return babyFade is { } image ? shading.With(MotherBrainShades.Fade(image)) : shading;
        }
    }

    /// <summary>Shows <paramref name="image"/> of the fake death's fade of her brain to grey.</summary>
    internal void ShowFakeDeathToGrey(MotherBrainFadeImage image) => fakeDeathFade = image.Of(MotherBrainFadeDefinitions.FakeDeathToGrey);

    /// <summary>Shows <paramref name="image"/> of her brain's revival from the fake death's grey.</summary>
    internal void ShowFakeDeathFromGrey(MotherBrainFadeImage image) => fakeDeathFade = image.Of(MotherBrainFadeDefinitions.FakeDeathFromGrey);

    /// <summary>Shows her phase-two attack and rear-leg colours, which her ascent loads.</summary>
    internal void ShowPhaseTwo() => phaseTwo = true;

    /// <summary>Shows rainbow image <paramref name="image"/> over her whole body.</summary>
    internal void ShowRainbow(MotherBrainRainbowImage image) => beam = MotherBrainShades.Rainbow(image);

    /// <summary>Shows the colours her beam restores over her whole body as it ends.</summary>
    internal void ShowRestored() => beam = MotherBrainShades.Restored;

    /// <summary>Shows <paramref name="image"/> of the Baby's drain of her whole body to grey.</summary>
    internal void ShowDrain(MotherBrainFadeImage image) => drain = image.Of(MotherBrainFadeDefinitions.DrainToGrey);

    /// <summary>Shows <paramref name="image"/> of her whole body's revival from grey.</summary>
    internal void ShowRevival(MotherBrainFadeImage image) => revival = image.Of(MotherBrainFadeDefinitions.RevivalFromGrey);

    /// <summary>Shows <paramref name="band"/>'s colours over her whole body.</summary>
    internal void ShowHealth(MotherBrainHealthBand band) => healthBand = Ensure.NotNull(band);

    /// <summary>Shows <paramref name="image"/> of her death's fade over her whole body.</summary>
    internal void ShowDeathFade(MotherBrainFadeImage image) => deathFade = image.Of(MotherBrainFadeDefinitions.DeathBody);

    /// <summary>Detaches her brain as she dies: it keeps the colours it shows, and only the corpse's fade changes them from here.</summary>
    internal void DetachBrain() => detachedBrain ??= OwnShading;

    /// <summary>Shows <paramref name="image"/> of the corpse's fade over her detached brain.</summary>
    /// <exception cref="InvalidOperationException">Her brain has not been detached.</exception>
    internal void ShowCorpseFade(MotherBrainFadeImage image)
    {
        if (detachedBrain is null)
            throw new InvalidOperationException("Mother Brain's corpse fades after her death detaches her brain.");
        corpseFade = image.Of(MotherBrainFadeDefinitions.CorpseFade);
    }

    /// <summary>Shows image <paramref name="image"/> of her room's flash.</summary>
    internal void ShowRoomFlash(MotherBrainRoomFlashImage image) => roomGrey = MotherBrainShades.Flash(image);

    /// <summary>Shows her room's steady grey after the flash stops.</summary>
    internal void ShowRoomGrey() => roomGrey = MotherBrainShades.FinalRoom;

    /// <summary>Shows her room's lights gone out as the Baby dies.</summary>
    internal void ShowLightsOut() => roomLights = MotherBrainShades.LightsOut;

    /// <summary>Shows image <paramref name="lights"/> of her room's lights recovering.</summary>
    internal void ShowLightsRecovering(MotherBrainRecoveryLights lights) => roomLights = MotherBrainShades.RecoveryLights(lights);

    /// <summary>Shows the Baby in the colours it arrives in.</summary>
    internal void ShowBabyArrived() => babyArrived = true;

    /// <summary>Shows <paramref name="image"/> of the Baby's fade to black.</summary>
    internal void ShowBabyFade(MotherBrainFadeImage image) => babyFade = image.Of(MotherBrainFadeDefinitions.BabyFade);

    /// <summary>
    /// <paramref name="under"/> with her whole body's colouring over it: her beam's colours, the drain over the rainbow it
    /// ran out on, her revival over the drain's grey, her health and her death's fade, each where it has started.
    /// </summary>
    private PictureShading OverBody(PictureShading under)
    {
        PictureShading shading = under;
        if (beam is { } colours)
            shading = shading.With(colours);
        if (drain is { } drained)
            shading = shading.With(MotherBrainShades.Fade(drained));
        if (revival is { } revived)
            shading = shading.With(MotherBrainShades.Fade(revived));
        if (healthBand is { } band)
            shading = shading.With(MotherBrainShades.Health(band));
        return deathFade is { } dying ? shading.With(MotherBrainShades.Fade(dying)) : shading;
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
