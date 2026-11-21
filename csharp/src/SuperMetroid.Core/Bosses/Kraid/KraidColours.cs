using System.Collections.Immutable;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Kraid.Artwork;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// Kraid's colours (#1512): the health band, or the hurt flash, his body and sprite parts show, the death colours his body
/// shows with his arm drawn into them, and his eyes' glow. His body on the background and his parts show the import's shade of
/// them; his eyes show the glow's colours over his band's.
/// </summary>
internal sealed class KraidColours : IGameplayShadingSource
{
    /// <summary>
    /// The shade his body and parts show: a health band's or the hurt flash's (#2297), or null while he shows his room's own
    /// colours.
    /// </summary>
    private PictureShade? Shown { get; set; }

    /// <summary>Whether his body shows his death colours.</summary>
    private bool ShowsDeathArm { get; set; }

    /// <summary>The colours his band's shade shows his eyes in, which his eyes' glow replaces.</summary>
    private RgbColor[] shadedEyes = [];

    /// <summary>His eyes' glow, which his AI reads back: a compatibility value his colours own (#1552).</summary>
    private KraidEyeGlow EyeGlow { get; set; } = KraidEyeGlow.Unlit;

    /// <summary>The shading his ceiling rocks and spat rocks draw in.</summary>
    internal static PictureShading RocksShading { get; } = PictureShading.Shaded(KraidShades.Rocks);

    /// <summary>The shading his sprite parts show: his band's, or his room's own colours before any.</summary>
    internal PictureShading Parts => Shown is { } shade ? PictureShading.Shaded(shade) : PictureShading.Standard;

    /// <summary>
    /// Shows health band <paramref name="band"/> with <paramref name="bandEyes"/> as its eyes; its shade shows the eyes in
    /// <paramref name="shaded"/>, which the eyes' colours are shown in place of.
    /// </summary>
    internal void ShowBand(KraidHealthBand band, ReadOnlySpan<RgbColor> bandEyes, ReadOnlySpan<RgbColor> shaded) =>
        Show(KraidShades.Health(band), bandEyes, shaded);

    /// <summary>
    /// Shows his hurt flash with <paramref name="flashEyes"/> as its eyes; its shade shows the eyes in
    /// <paramref name="shaded"/>, which the eyes' colours are shown in place of.
    /// </summary>
    internal void ShowHurtFlash(ReadOnlySpan<RgbColor> flashEyes, ReadOnlySpan<RgbColor> shaded) =>
        Show(KraidShades.HurtFlash, flashEyes, shaded);

    /// <summary>Shows <paramref name="shade"/> with <paramref name="eyes"/> in his eyes, which its shade shows in <paramref name="shaded"/>.</summary>
    private void Show(PictureShade shade, ReadOnlySpan<RgbColor> eyes, ReadOnlySpan<RgbColor> shaded)
    {
        Shown = shade;
        shadedEyes = Ensure.LengthEqual(shaded, KraidEyeGlow.Eyes).ToArray();
        ShowBandEyes(eyes);
    }

    /// <summary>Shows <paramref name="bandEyes"/>, his health band's eye colours, in his eyes at once.</summary>
    internal void ShowBandEyes(ReadOnlySpan<RgbColor> bandEyes) => EyeGlow = KraidEyeGlow.Showing(bandEyes);

    /// <summary>Steps his eyes' glow one level.</summary>
    /// <returns>Whether the glow has peaked, which ends it.</returns>
    internal bool GlowEyes()
    {
        EyeGlow = EyeGlow.Glowed();
        return EyeGlow.HasPeaked;
    }

    /// <summary>Steps his eyes' fade one level toward <paramref name="bandEyes"/>, his health band's eye colours.</summary>
    /// <returns>Whether his eyes already showed the band's, which ends the fade.</returns>
    internal bool UnglowEyes(ReadOnlySpan<RgbColor> bandEyes)
    {
        bool showedBand = EyeGlow.ShowsBand(bandEyes);
        EyeGlow = EyeGlow.Unglowed(bandEyes);
        return showedBand;
    }

    /// <summary>Shows his death colours on his body.</summary>
    internal void ShowDeathArm() => ShowsDeathArm = true;

    /// <inheritdoc />
    public void ShadeGameplay(GameplayShadingBuilder frame)
    {
        Ensure.NotNull(frame);
        if (ShowsDeathArm)
        {
            frame.Shade(GameplayPictures.BossBody, KraidShades.DeathArm);
            return;
        }
        if (Shown is not { } shade)
            return;
        frame.Shade(GameplayPictures.BossBody, shade);
        // His body's shades show his eyes in one set of colours; the glow's colours are shown in their place.
        ImmutableArray<RgbColor> eyes = EyeGlow.Shown;
        if (!eyes.SequenceEqual(shadedEyes))
            frame.Affect(GameplayPictures.BossBody, new ColourSubstitution(EyeGlow.ShownInPlaceOf(shadedEyes)));
    }
}
