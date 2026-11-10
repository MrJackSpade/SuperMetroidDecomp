using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Ridley.Artwork;
using SuperMetroid.Core.Enemies.Drawing;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// The colours a Ridley encounter shows (#1512, #1544, #1597): over Ridley's own sprites (with the Ceres Baby and its
/// container he carries), his arrival's colours, the Baby's shade, his own colours hidden in black as he arrives, his eyes'
/// and his body's fades in and his health band; over his room's pictures, the Ceres retreat and the getaway floor's zoom,
/// or Norfair's arena hidden as he arrives and its reveal; and over the self-destruct warning's written pictures, its
/// alarm. Each shows the current step of each effect, in the fixed order his encounter reaches them, a later effect over
/// the earlier ones where it colours the same part. The warning's pictures show their own colours and alarm, never the
/// room's (#1544).
/// </summary>
internal sealed class RidleyColours : IGameplayShadingSource
{
    /// <summary>The colours his encounter's arrival shows on the door, the container and the Baby, or null before it.</summary>
    private PictureShade? arrival;

    /// <summary>The Baby's shade, or null before the Ceres Baby shows one.</summary>
    private CeresBabyShade? baby;

    /// <summary>His own colours cleared to black as he arrives, which his fades and health bands then show over, or null before.</summary>
    private PictureShade? hidden;

    /// <summary>The shade his eyes' fade in has reached, or null before it starts.</summary>
    private int? eyeFade;

    /// <summary>The shade his body's fade in has reached, or null before it starts.</summary>
    private int? bodyFade;

    /// <summary>The health band his own colours show, or null before any.</summary>
    private int? healthBand;

    /// <summary>Whether the Ceres retreat has recoloured his room.</summary>
    private bool retreated;

    /// <summary>The step the getaway floor's zoom shows, or null before the getaway.</summary>
    private int? floorZoom;

    /// <summary>Whether Norfair's arena is hidden in black as he arrives.</summary>
    private bool arenaHidden;

    /// <summary>The step Norfair's arena reveal has reached, or null before it starts.</summary>
    private int? arenaReveal;

    /// <summary>The step the self-destruct warning's alarm shows, or null before the warning.</summary>
    private int? alarm;

    /// <summary>The shading his room's pictures show: the retreat and the floor's zoom, or the arena hidden and its reveal.</summary>
    internal PictureShading RoomShading
    {
        get
        {
            PictureShading shading = PictureShading.Standard;
            if (retreated)
                shading = shading.With(RidleyShades.RetreatRoom).With(RidleyShades.RetreatShared);
            if (floorZoom is { } zoom)
                shading = shading.With(RidleyShades.FloorZoom(zoom));
            if (arenaHidden)
                shading = shading.With(RidleyShades.NorfairHidden);
            return arenaReveal is { } reveal ? shading.With(RidleyShades.NorfairReveal(reveal)) : shading;
        }
    }

    /// <summary>The shading the self-destruct warning's written pictures show: its alarm's step.</summary>
    internal PictureShading WarningShading => alarm is { } step ? PictureShading.Shaded(RidleyShades.Alarm(step)) : PictureShading.Standard;

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

    /// <summary>Shows shade <paramref name="shade"/> of his eyes' fade in.</summary>
    internal void ShowEyeFade(int shade) => eyeFade = shade;

    /// <summary>Shows shade <paramref name="shade"/> of his body's fade in.</summary>
    internal void ShowBodyFade(int shade) => bodyFade = shade;

    /// <summary>Shows health band <paramref name="band"/> over his own colours.</summary>
    internal void ShowHealth(int band) => healthBand = band;

    /// <summary>Shows the colours his Ceres retreat gives his room.</summary>
    internal void ShowRetreat() => retreated = true;

    /// <summary>Shows the getaway floor at zoom step <paramref name="step"/>.</summary>
    internal void ShowFloorZoom(int step) => floorZoom = step;

    /// <summary>Shows step <paramref name="step"/> of Norfair's arena reveal.</summary>
    internal void ShowArenaReveal(int step) => arenaReveal = step;

    /// <summary>Shows step <paramref name="step"/> of the self-destruct warning's alarm.</summary>
    internal void ShowAlarm(int step) => alarm = step;

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
            shading = shading.With(RidleyShades.EyeFade(eyes));
        if (bodyFade is { } body)
            shading = shading.With(RidleyShades.BodyFade(body));
        if (healthBand is { } band)
            shading = shading.With(RidleyShades.Health(band));
        return flashing ? shading.With(EnemyShades.HurtFlash) : shading;
    }

    /// <inheritdoc />
    public void ShadeGameplay(GameplayShadingBuilder frame)
    {
        Ensure.NotNull(frame);
        foreach (PictureShade shade in RoomShading.Shades)
            frame.Shade(GameplayPictures.Room, shade);
        foreach (PictureShade shade in WarningShading.Shades)
            frame.Shade(GameplayPictures.WrittenOverRoom, shade);
    }
}
