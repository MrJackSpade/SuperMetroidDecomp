using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Frontend.Intro.Artwork;

/// <summary>
/// The opening narration's full, editable screen colours (#1464): its background sets and sprite sets, each set held as
/// exact colours or the authored relationship that reproduces them.
/// </summary>
public sealed class IntroCinematicPalette
{
    /// <summary>
    /// The colours whose sprite set repeats the neutral ramp ($8C:E4E9-E508); its set may be held as that ramp.
    /// </summary>
    private static readonly ScreenColourGroup NeutralRamp = ScreenColourGroup.IntroNarrationNeutralRamp;

    /// <summary>
    /// The colours whose sprite set cross-fades ($8C:E5A9-E5C8): its first eight visible colours may repeat the sepia
    /// gradient of <see cref="CrossFadeGradientSource"/>.
    /// </summary>
    private static readonly ScreenColourGroup CrossFade = ScreenColourGroup.IntroCrossFadeObjects;

    /// <summary>The discovery scene's background colours ($8C:E42B-E43A), whose sepia gradient the cross-fade repeats.</summary>
    private static readonly ScreenColourGroup CrossFadeGradientSource = ScreenColourGroup.IntroDiscoveryBackground;

    /// <summary>Decoded background sets; empty when the installed colours match the compiled stock paint.</summary>
    internal readonly IntroColourSet[] backgroundSets;

    /// <summary>Decoded sprite sets; empty when the installed colours match the compiled stock paint.</summary>
    internal readonly IntroColourSet[] spriteSets;

    /// <summary>Keeps the opening's selected colours, recognising stock, neutral-cycle and shared cross-fade sets.</summary>
    /// <param name="colours">The selected colours by owner, which the import pipeline decoded.</param>
    internal static IntroCinematicPalette FromColours(ScreenColours colours) => new(Ensure.NotNull(colours));

    /// <summary>Retains exact supplied colors while recognizing stock, neutral-cycle, and shared cross-fade sets.</summary>
    /// <param name="colours">The selected colours by owner.</param>
    private IntroCinematicPalette(ScreenColours colours)
    {
        if (IntroCinematicPaint.Matches(colours))
        {
            backgroundSets = [];
            spriteSets = [];
            return;
        }
        backgroundSets = new IntroColourSet[ColourSets.SetCount];
        for (int set = 0; set < backgroundSets.Length; set++)
            backgroundSets[set] = new IntroColourSet(colours.BackgroundSet(set).ToArray(), IntroColourSetCycle.None, crossFadeSource: null);
        spriteSets = new IntroColourSet[ColourSets.SetCount];
        for (int set = 0; set < spriteSets.Length; set++)
        {
            var cell = new ScreenColourCell(ScreenColourOwner.Sprites, set, 0);
            IntroColourSetCycle cycle = cell == ScreenColourGroups.FirstCell(NeutralRamp)
                ? IntroColourSetCycle.NeutralRamp : IntroColourSetCycle.None;
            IntroColourSet? crossFadeSource = cell == ScreenColourGroups.FirstCell(CrossFade)
                ? SetHolding(CrossFadeGradientSource) : null;
            spriteSets[set] = new IntroColourSet(colours.SpriteSet(set).ToArray(), cycle, crossFadeSource);
        }
    }

    /// <summary>The decoded set that holds <paramref name="group"/>, a whole set.</summary>
    private IntroColourSet SetHolding(ScreenColourGroup group)
    {
        ScreenColourCell first = ScreenColourGroups.FirstCell(group);
        return first.Owner == ScreenColourOwner.Backgrounds ? backgroundSets[first.Set] : spriteSets[first.Set];
    }

    /// <summary>The installed colours by owner.</summary>
    public ScreenColours Colours
    {
        get
        {
            var screen = new ScreenPalette();
            for (int set = 0; set < ColourSets.SetCount; set++)
            for (int color = 0; color < ColourSets.SetColours; color++)
            {
                screen.BackgroundSets.Set(set).Write(color,
                    backgroundSets.Length == 0 ? IntroCinematicPaint.Background(set, color) : backgroundSets[set].Resolve(color));
                screen.SpriteSets.Set(set).Write(color,
                    spriteSets.Length == 0 ? IntroCinematicPaint.Sprite(set, color) : spriteSets[set].Resolve(color));
            }
            return ScreenColours.Of(screen);
        }
    }

    /// <summary>Replaces every colour of the screen with the installed opening palette, corresponding to $8C:E3E9-E5E8; does not advance or reset cinematic fade timers.</summary>
    /// <param name="colours">The screen's colours, every one of which the palette replaces.</param>
    public void LoadTo(ScreenPalette colours) => Colours.LoadTo(colours);
}
