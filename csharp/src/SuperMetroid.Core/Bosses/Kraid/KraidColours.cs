using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Kraid.Artwork;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// Kraid's colours (#1512): the health band, or the hurt flash, his body and sprite parts show, the death colours his body
/// shows with his arm drawn into them, and his eyes' glow. His body on layer 2 and his parts show the import's shade of
/// them; his eyes show the glow's colours over his band's.
/// </summary>
internal sealed class KraidColours : IGameplayShadingSource
{
    /// <summary>The band shown, minus one for the hurt flash, or null while he shows his room's own colours.</summary>
    internal int? Band { get; private set; }

    /// <summary>Whether his body shows his death colours.</summary>
    internal bool ShowsDeathArm { get; private set; }

    /// <summary>His eyes' glow, which his AI reads back.</summary>
    internal KraidEyeGlow EyeGlow { get; } = new();

    /// <summary>The shading his ceiling rocks and spat rocks draw in.</summary>
    internal static PictureShading RocksShading { get; } = PictureShading.Shaded(KraidShades.Rocks);

    /// <summary>The shading his sprite parts show: his band's, or his room's own colours before any.</summary>
    internal PictureShading Parts => Band is { } band ? PictureShading.Shaded(KraidShades.Health(band)) : PictureShading.Standard;

    /// <summary>Shows health band <paramref name="band"/>, or the hurt flash for minus one, with <paramref name="bandEyes"/> as its eyes.</summary>
    internal void ShowBand(int band, ReadOnlySpan<RgbColor> bandEyes)
    {
        Band = band;
        EyeGlow.ShowBand(bandEyes);
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
        if (Band is { } band)
            frame.Shade(GameplayPictures.BossBody, KraidShades.Health(band));
        if (EyeGlow.Effect() is { } eyes)
            frame.Affect(GameplayPictures.BossBody, eyes);
    }
}
