using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Ridley.Artwork;
using SuperMetroid.Core.Enemies.Drawing;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// The colours a Ridley encounter shows (#1512, #1544, #1597): over Ridley's own sprites (with the Ceres Baby and its
/// container he carries), his arrival's colours, the Baby's shade, his own colours hidden in black as he arrives, his eyes'
/// and his body's fades in and his health band; over his room's pictures, the Ceres retreat, or Norfair's arena hidden as
/// he arrives and its reveal. Each shows the current step of each effect, in the fixed order his encounter reaches them, a
/// later effect over the earlier ones where it colours the same part. The getaway floor's zoom and the self-destruct
/// warning's alarm are the Ceres escape's (<see cref="CeresEscape"/>, #2228).
/// </summary>
internal sealed partial class RidleyColours : IGameplayShadingSource
{
    /// <summary>The colours his encounter's arrival shows on the door, the container and the Baby, or null before it.</summary>
    private PictureShade? arrival;

    /// <summary>The Baby's shade, or null before the Ceres Baby shows one.</summary>
    private CeresBabyShade? baby;

    /// <summary>His own colours cleared to black as he arrives, which his fades and health bands then show over, or null before.</summary>
    private PictureShade? hidden;

    /// <summary>The image his eyes' fade in has reached, or null before it starts.</summary>
    private RidleyFadeImage? eyeFade;

    /// <summary>The image his body's fade in has reached, or null before it starts.</summary>
    private RidleyFadeImage? bodyFade;

    /// <summary>The health colouring his own colours show, or null before any.</summary>
    private RidleyHealthShade? healthShade;

    /// <summary>Whether the Ceres retreat has recoloured his room.</summary>
    private bool retreated;

    /// <summary>Whether Norfair's arena is hidden in black as he arrives.</summary>
    private bool arenaHidden;

    /// <summary>The image Norfair's arena reveal has reached, or null before it starts.</summary>
    private RidleyFadeImage? arenaReveal;

    /// <summary>The shading his room's pictures show: the Ceres retreat, or the arena hidden and its reveal.</summary>
    internal PictureShading RoomShading
    {
        get
        {
            PictureShading shading = PictureShading.Standard;
            if (retreated)
                shading = shading.With(RidleyShades.RetreatRoom).With(RidleyShades.RetreatShared);
            if (arenaHidden)
                shading = shading.With(RidleyShades.NorfairHidden);
            return arenaReveal is { } reveal ? shading.With(RidleyShades.Fade(reveal)) : shading;
        }
    }

    /// <summary>Shows Ceres Ridley's arrival: the door's, container's and Baby's colours, and his own colours hidden in black.</summary>
    internal void ShowCeresArrival()
    {
        arrival = RidleyShades.CeresStart;
        hidden = RidleyShades.CeresHidden;
    }

    /// <summary>
    /// Shows Norfair Ridley's arrival: his initialization's colours, his own colours hidden in black, and his arena hidden
    /// in black for its reveal.
    /// </summary>
    internal void ShowNorfairArrival()
    {
        arrival = RidleyShades.NorfairStart;
        hidden = RidleyShades.NorfairHidden;
        arenaHidden = true;
    }

    /// <summary>Shows the Ceres Baby and its container in <paramref name="shade"/>.</summary>
    internal void ShowBaby(CeresBabyShade shade) => baby = shade;

    /// <summary>Shows image <paramref name="image"/> of his eyes' fade in.</summary>
    /// <exception cref="ArgumentException">The image is not one of the eye fade's.</exception>
    internal void ShowEyeFade(RidleyFadeImage image) => eyeFade = image.Of(RidleyFadeDefinitions.EyeFade);

    /// <summary>Shows image <paramref name="image"/> of his body's fade in.</summary>
    /// <exception cref="ArgumentException">The image is not one of the body fade's.</exception>
    internal void ShowBodyFade(RidleyFadeImage image) => bodyFade = image.Of(RidleyFadeDefinitions.BodyFade);

    /// <summary>Shows health colouring <paramref name="shade"/> over his own colours.</summary>
    internal void ShowHealth(RidleyHealthShade shade) => healthShade = Ensure.NotNull(shade);

    /// <summary>Shows the colours his Ceres retreat gives his room.</summary>
    internal void ShowRetreat() => retreated = true;

    /// <summary>Shows image <paramref name="image"/> of Norfair's arena reveal.</summary>
    /// <exception cref="ArgumentException">The image is not one of the reveal's.</exception>
    internal void ShowArenaReveal(RidleyFadeImage image) => arenaReveal = image.Of(RidleyFadeDefinitions.ArenaReveal);

    /// <summary>
    /// The shading the encounter's sprites draw in: his arrival's colours, the Baby's shade, his own colours hidden, his
    /// fades in and his health band, with the hurt flash over those when <paramref name="flashing"/>, which shows whatever
    /// lies under it.
    /// </summary>
    internal PictureShading SpriteShading(bool flashing)
    {
        PictureShading shading = PictureShading.Standard;
        if (arrival is { } colours)
            shading = shading.With(colours);
        if (baby is { } shade)
            shading = shading.With(RidleyShades.Baby(shade));
        if (hidden is { } black)
            shading = shading.With(black);
        if (eyeFade is { } eyes)
            shading = shading.With(RidleyShades.Fade(eyes));
        if (bodyFade is { } body)
            shading = shading.With(RidleyShades.Fade(body));
        if (healthShade is { } health)
            shading = shading.With(RidleyShades.Health(health));
        return flashing ? shading.With(EnemyShades.HurtFlash) : shading;
    }

    /// <inheritdoc />
    public void ShadeGameplay(GameplayShadingBuilder frame)
    {
        Ensure.NotNull(frame);
        foreach (PictureShade shade in RoomShading.Shades)
            frame.Shade(GameplayPictures.Room, shade);
    }
}
