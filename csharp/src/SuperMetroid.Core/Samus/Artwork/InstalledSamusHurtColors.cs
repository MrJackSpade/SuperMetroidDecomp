using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Editable RGB5 artwork for the hurt flash and cinematic restoration.</summary>
public sealed class InstalledSamusHurtColors
{
    /// <summary>Optional independent edit to the hurt palette's transparent-slot word.</summary>
    internal readonly RgbColor? hurtZero;
    /// <summary>Optional independent edit to the intro palette's transparent-slot word.</summary>
    internal readonly RgbColor? introZero;
    /// <summary>Intro shade levels that differ from the calculated native levels.</summary>
    internal readonly Dictionary<int, int> introLevels;
    /// <summary>Intro colors whose full RGB5 words cannot be derived from their shade levels.</summary>
    internal readonly Dictionary<ColourCell, RgbColor> introOverrides;
    /// <summary>Hurt colors that differ from the corresponding intro-derived native flash shade.</summary>
    internal readonly Dictionary<ColourCell, RgbColor> hurtOverrides;

    /// <summary>Retains only independent palette edits and derives unchanged native shade relationships.</summary>
    internal InstalledSamusHurtColors(RgbColor[] hurt, RgbColor[] intro)
    {
        hurtZero = hurt[0] == SamusHurtColorDefinitions.HurtTransparentWord ? null : hurt[0];
        introZero = intro[0] == SamusHurtColorDefinitions.IntroTransparentWord ? null : intro[0];
        introLevels = Enumerable.Range(1, 15)
            .Where(index => (intro[index].Red) != SamusHurtColorLevels.DefaultIntroLevel(index))
            .ToDictionary(index => index, index => intro[index].Red);
        introOverrides = Enumerable.Range(1, 15)
            .Where(index => intro[index] != SamusHurtColorLevels.IntroFromLevel(IntroLevel(index)))
            .ToDictionary(index => new ColourCell(0, index), index => intro[index]);
        hurtOverrides = Enumerable.Range(1, 15)
            .Where(index => hurt[index] != SamusHurtColorLevels.HurtFromIntro(intro[index]))
            .ToDictionary(index => new ColourCell(0, index), index => hurt[index]);
    }

    /// <summary>Returns one display color; counter, phase, and sound rules remain in code.</summary>
    /// <param name="variant">Flash or cinematic restoration palette; this method does not select the palette from gameplay state.</param>
    /// <param name="index">Zero-based OBJ palette color index, from zero through 15; zero retains the independently editable transparent-slot payload.</param>
    /// <returns>A packed SNES color word with red in bits 0..4, green in bits 5..9, and blue in bits 10..14.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="variant"/> is undefined or <paramref name="index"/> is outside the sixteen-color palette.</exception>
    public RgbColor Resolve(SamusHurtColorVariant variant, int index)
    {
        if (variant is not (SamusHurtColorVariant.Hurt or SamusHurtColorVariant.Intro))
            throw new ArgumentOutOfRangeException(nameof(variant));
        if ((uint)index >= SamusHurtColorFormat.ColorsPerPalette)
            throw new ArgumentOutOfRangeException(nameof(index));
        if (variant == SamusHurtColorVariant.Intro) return Intro(index);
        return index == 0 ? hurtZero ?? SamusHurtColorDefinitions.HurtTransparentWord : hurtOverrides.TryGetValue(new ColourCell(0, index), out RgbColor value)
            ? value : SamusHurtColorLevels.HurtFromIntro(Intro(index));
    }

    /// <summary>Gets an independently edited intro shade level or its native default.</summary>
    private int IntroLevel(int index) => introLevels.ContainsKey(index) ? introLevels[index] : SamusHurtColorLevels.DefaultIntroLevel(index);
    /// <summary>Gets an edited intro color or derives it from the selected shade level.</summary>
    private RgbColor Intro(int index) => index == 0 ? introZero ?? SamusHurtColorDefinitions.IntroTransparentWord : introOverrides.TryGetValue(new ColourCell(0, index), out RgbColor value)
        ? value : SamusHurtColorLevels.IntroFromLevel(IntroLevel(index));
}
