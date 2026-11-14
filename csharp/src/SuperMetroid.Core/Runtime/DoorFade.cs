using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Fx.ColourEffects;
using SuperMetroid.Core.World;

namespace SuperMetroid.Core.Runtime;

/// <summary>
/// The colours a door's fades show (#1512): the room being left fades to black, stays dark while the door loads and
/// scrolls, and the room being entered fades in from black. The fade is one fade of the whole display, which every picture
/// and every shaded sprite draw, Samus's among them, shows after its own shading; the HUD is spared it and takes the
/// import's shade of each step instead, which keeps its minimap and highlighted item lit, and between two common rooms
/// its item and minimap backgrounds.
/// </summary>
internal sealed class DoorFade : IGameplayShadingSource
{
    /// <summary>The steps of the fade running or last run; null before a door's first fade.</summary>
    private GradualFadeSteps? steps;

    /// <summary>Whether the door leads from one common room into another, which keeps more of the HUD lit.</summary>
    private bool betweenCommonRooms;

    /// <summary>
    /// Samus's shades written into the dark display's colours, in the order written: the destination's load writes her
    /// beams', and the door's completed scroll her visor's green. The fade in moves each toward the same colours, its
    /// target holding her beams' and every suit's visor showing that green, so they stay lit.
    /// </summary>
    private readonly List<PictureShade> samusWritten = [];

    /// <summary>Where the fade is.</summary>
    internal DoorFadeStage Stage { get; private set; } = DoorFadeStage.None;

    /// <summary>Starts the room being left fading out over <paramref name="fade"/>'s steps.</summary>
    /// <param name="fade">The fade's steps, which the door takes one per update.</param>
    /// <param name="commonRooms">Whether the door leads from one common room into another.</param>
    internal void FadeOut(GradualFadeSteps fade, bool commonRooms)
    {
        steps = Ensure.NotNull(fade);
        betweenCommonRooms = commonRooms;
        samusWritten.Clear();
        Stage = DoorFadeStage.FadingOut;
    }

    /// <summary>
    /// Writes <paramref name="shade"/>'s colours of Samus's into the dark display's colours, as the destination's load
    /// writes her beams' and the door's completed scroll her visor's.
    /// </summary>
    /// <exception cref="InvalidOperationException">The display is neither dark nor fading in.</exception>
    internal void WriteSamusColours(PictureShade shade)
    {
        if (Stage is not (DoorFadeStage.Dark or DoorFadeStage.FadingIn))
            throw new InvalidOperationException($"A door writes Samus's colours into its dark display, not while {Stage}.");
        samusWritten.Add(shade);
    }

    /// <summary>Keeps the faded-out room dark while the door loads and scrolls.</summary>
    /// <exception cref="InvalidOperationException">No room is fading out.</exception>
    internal void Darken()
    {
        if (Stage != DoorFadeStage.FadingOut)
            throw new InvalidOperationException($"A door darkens once its room has faded out, not while {Stage}.");
        Stage = DoorFadeStage.Dark;
    }

    /// <summary>Starts the room being entered fading in over <paramref name="fade"/>'s steps.</summary>
    /// <exception cref="InvalidOperationException">The room being left has not faded out.</exception>
    internal void FadeIn(GradualFadeSteps fade)
    {
        if (Stage != DoorFadeStage.Dark)
            throw new InvalidOperationException($"A door fades its room in once the room it left is dark, not while {Stage}.");
        steps = Ensure.NotNull(fade);
        Stage = DoorFadeStage.FadingIn;
    }

    /// <summary>Ends the fade: the room entered shows its own colours.</summary>
    internal void End() => Stage = DoorFadeStage.None;

    /// <inheritdoc />
    public void ShadeGameplay(GameplayShadingBuilder frame)
    {
        Ensure.NotNull(frame);
        if (Stage == DoorFadeStage.None)
            return;
        GradualFadeSteps fade = steps ?? throw new InvalidOperationException("A door fade shows once its steps have started.");
        // The HUD's shade is the import's of the shared counter's value at the last step taken.
        int? last = fade.Taken.IsEmpty ? null : fade.Taken[^1];
        frame.Fade(new GameplayDisplayFade(DisplayEffect(fade), GameplayFadeSpares.Hud, [.. samusWritten]));
        if (HudShade(last) is { } hud)
            frame.Shade(GameplayPictures.Hud, hud);
        if (EscapeTimerShade(last) is { } timer)
            frame.Shade(GameplayPictures.EscapeTimer, timer);
    }

    /// <summary>The effect the fade shows over the whole display at its stage, over <paramref name="fade"/>'s steps.</summary>
    /// <exception cref="InvalidOperationException">No fade is running, or the stage is undefined.</exception>
    private ColourEffect DisplayEffect(GradualFadeSteps fade) => Stage switch
    {
        DoorFadeStage.FadingOut => fade.Toward(RgbColor.Black),
        DoorFadeStage.Dark => new ColourFlash(RgbColor.Black),
        DoorFadeStage.FadingIn => fade.From(RgbColor.Black),
        DoorFadeStage.None => throw new InvalidOperationException("A door fade shows no effect while no fade runs."),
        _ => throw new InvalidOperationException($"Undefined door fade stage {Stage}."),
    };

    /// <summary>
    /// The HUD's shade at the fade's stage, <paramref name="last"/> the counter value of the last step taken: none before a
    /// fade out's first step, which shows the HUD's standard colours, and the dark shade before a fade in's first step.
    /// </summary>
    /// <exception cref="InvalidOperationException">No fade is running, or the stage is undefined.</exception>
    private PictureShade? HudShade(int? last) => Stage switch
    {
        DoorFadeStage.FadingOut => last is { } value ? HudFadingOut(value) : null,
        DoorFadeStage.Dark => HudDark(),
        DoorFadeStage.FadingIn => last is { } value ? HudFadingIn(value) : HudDark(),
        DoorFadeStage.None => throw new InvalidOperationException("A door fade shades the HUD while a fade runs."),
        _ => throw new InvalidOperationException($"Undefined door fade stage {Stage}."),
    };

    /// <summary>
    /// The escape timer's shade at the fade's stage: the timer keeps its lit colours through the fade as the HUD does,
    /// faded step for step with it, and a fade out shows its standard colours until its first step, as it shows the HUD's.
    /// </summary>
    /// <exception cref="InvalidOperationException">No fade is running, or the stage is undefined.</exception>
    private PictureShade? EscapeTimerShade(int? last) => Stage switch
    {
        DoorFadeStage.FadingOut => last is { } value ? DoorFadeShades.EscapeTimerFadingOut(value) : null,
        DoorFadeStage.Dark => DoorFadeShades.EscapeTimerFadingOut(DoorTransitionColourDefinitions.HudFadeShades - 1),
        DoorFadeStage.FadingIn => last is { } value
            ? DoorFadeShades.EscapeTimerFadingIn(value)
            : DoorFadeShades.EscapeTimerFadingOut(DoorTransitionColourDefinitions.HudFadeShades - 1),
        DoorFadeStage.None => throw new InvalidOperationException("A door fade shades the escape timer while a fade runs."),
        _ => throw new InvalidOperationException($"Undefined door fade stage {Stage}."),
    };

    /// <summary>The HUD's shade once the room has faded out: the fade out's last step, its faded colours black.</summary>
    private PictureShade HudDark() => HudFadingOut(DoorTransitionColourDefinitions.HudFadeShades - 1);

    /// <summary>The HUD's shade of a fade out of the room whose last step took counter value <paramref name="value"/>.</summary>
    private PictureShade HudFadingOut(int value) => betweenCommonRooms
        ? DoorFadeShades.HudFadingOutBetweenCommonRooms(value) : DoorFadeShades.HudFadingOut(value);

    /// <summary>The HUD's shade of a fade into the room whose last step took counter value <paramref name="value"/>.</summary>
    private PictureShade HudFadingIn(int value) => betweenCommonRooms
        ? DoorFadeShades.HudFadingInBetweenCommonRooms(value) : DoorFadeShades.HudFadingIn(value);
}
