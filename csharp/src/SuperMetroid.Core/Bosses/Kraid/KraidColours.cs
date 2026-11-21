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
    /// What his body shows (#2297, #2560): his room's own colours, a health band's or the hurt flash's shade, or his death
    /// colours, beside the shade his parts keep showing.
    /// </summary>
    private BodyShown Shown { get; set; } = new RoomColours();

    /// <summary>The colours his band's shade shows his eyes in, which his eyes' glow replaces.</summary>
    private RgbColor[] shadedEyes = [];

    /// <summary>His eyes' glow, which his AI reads back: a compatibility value his colours own (#1552).</summary>
    private KraidEyeGlow EyeGlow { get; set; } = KraidEyeGlow.Unlit;

    /// <summary>The shading his ceiling rocks and spat rocks draw in.</summary>
    internal static PictureShading RocksShading { get; } = PictureShading.Shaded(KraidShades.Rocks);

    /// <summary>The shading his sprite parts show: his band's, or his room's own colours before any.</summary>
    internal PictureShading Parts => Shown.PartsShade is { } shade ? PictureShading.Shaded(shade) : PictureShading.Standard;

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
        Shown = Shown is DeathColours ? new DeathColours(shade) : new BandShade(shade);
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
    internal void ShowDeathArm() => Shown = new DeathColours(Shown.PartsShade);

    /// <inheritdoc />
    public void ShadeGameplay(GameplayShadingBuilder frame)
    {
        Ensure.NotNull(frame);
        if (Shown is DeathColours)
        {
            frame.Shade(GameplayPictures.BossBody, KraidShades.DeathArm);
            return;
        }
        if (Shown is not BandShade { PartsShade: { } shade })
            return;
        frame.Shade(GameplayPictures.BossBody, shade);
        // His body's shades show his eyes in one set of colours; the glow's colours are shown in their place.
        ImmutableArray<RgbColor> eyes = EyeGlow.Shown;
        if (!eyes.SequenceEqual(shadedEyes))
            frame.Affect(GameplayPictures.BossBody, new ColourSubstitution([.. shadedEyes], [.. eyes]));
    }

    /// <summary>What his body shows (#2560), each case with the shade his parts show.</summary>
    /// <param name="PartsShade">The shade his parts show: the last band's or hurt flash's, or null before any.</param>
    private abstract record BodyShown(PictureShade? PartsShade);

    /// <summary>His room's own colours, before any band or hurt flash.</summary>
    private sealed record RoomColours() : BodyShown((PictureShade?)null);

    /// <summary>A health band's or the hurt flash's shade, on his body and his parts.</summary>
    /// <param name="Shade">The shade.</param>
    private sealed record BandShade(PictureShade Shade) : BodyShown(Shade);

    /// <summary>His death colours on his body; his parts keep the last shade shown.</summary>
    /// <param name="PartsShade">The shade his parts show.</param>
    private sealed record DeathColours(PictureShade? PartsShade) : BodyShown(PartsShade);
}
