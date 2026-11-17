using System.Collections.Immutable;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Fx.ColourEffects;
using SuperMetroid.Core.Samus.Colours;
using SuperMetroid.Core.World;

namespace SuperMetroid.Core.Runtime;

/// <summary>
/// The colours a door's fades show (#1512): the room being left fades to black, stays dark while the door loads and
/// scrolls, and the room being entered fades in from black. The fade is one fade of the whole display, which every picture
/// and every shaded sprite draw, Samus's among them, shows after its own shading; the HUD and the escape timer are spared
/// it and fade as far toward their dark shades instead (#2289), which keep the HUD's minimap and highlighted item lit, and
/// between two common rooms its item and minimap backgrounds. Samus's beams and visor stay lit through the room's fade in (#2093): her beams from
/// the room's load, which the fade in starts with, and her visor, which shows the door's completion green, once the door
/// has opened.
/// </summary>
/// <param name="samusColours">Samus's colours, whose beams' shade the fade in keeps lit.</param>
internal sealed class DoorFade(SamusColours samusColours) : IGameplayShadingSource
{
    /// <summary>Samus's colours, whose beams' shade the fade in keeps lit.</summary>
    private readonly SamusColours samusColours = Ensure.NotNull(samusColours);

    /// <summary>The steps of the fade running or last run; null before a door's first fade.</summary>
    private GradualFadeSteps? steps;

    /// <summary>Whether the door leads from one common room into another, which keeps more of the HUD lit.</summary>
    private bool betweenCommonRooms;

    /// <summary>Whether the door has opened in the room being entered, which keeps Samus's visor lit through the fade in.</summary>
    private bool doorOpened;

    /// <summary>Where the fade is.</summary>
    internal DoorFadeStage Stage { get; private set; } = DoorFadeStage.None;

    /// <summary>Starts the room being left fading out over <paramref name="fade"/>'s steps.</summary>
    /// <param name="fade">The fade's steps, which the door takes one per update.</param>
    /// <param name="commonRooms">Whether the door leads from one common room into another.</param>
    internal void FadeOut(GradualFadeSteps fade, bool commonRooms)
    {
        steps = Ensure.NotNull(fade);
        betweenCommonRooms = commonRooms;
        doorOpened = false;
        Stage = DoorFadeStage.FadingOut;
    }

    /// <summary>
    /// The door has opened in the room being entered: Samus's visor shows the door's completion green, which the fade in
    /// keeps lit from now on.
    /// </summary>
    /// <exception cref="InvalidOperationException">The room being entered is not fading in.</exception>
    internal void DoorOpened()
    {
        if (Stage != DoorFadeStage.FadingIn)
            throw new InvalidOperationException($"A door opens while its room fades in, not while {Stage}.");
        doorOpened = true;
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
        frame.Fade(new GameplayDisplayFade(DisplayEffect(fade), GameplayFadeSpares.Hud, LitOnSamus()));
        // The HUD and the escape timer fade between their own colours and their dark shades, which keep their lit colours,
        // as far as the display's fade has come (#2289).
        Shade(frame, GameplayPictures.Hud, fade, betweenCommonRooms ? DoorFadeShades.HudDarkBetweenCommonRooms : DoorFadeShades.HudDark);
        Shade(frame, GameplayPictures.EscapeTimer, fade, DoorFadeShades.EscapeTimerDark);
    }

    /// <summary>
    /// Shades <paramref name="pictures"/> at the fade's stage toward or from <paramref name="dark"/>: a fade out from their
    /// own colours toward it, which shows their own colours until its first moving step, the dark shade while the room is
    /// dark, and a fade in from it to their own colours.
    /// </summary>
    /// <exception cref="InvalidOperationException">No fade is running, or the stage is undefined.</exception>
    private void Shade(GameplayShadingBuilder frame, GameplayPictures pictures, GradualFadeSteps fade, PictureShade dark)
    {
        switch (Stage)
        {
            case DoorFadeStage.FadingOut:
                frame.Shade(pictures, fade.Between(PictureShading.Standard, PictureShading.Shaded(dark)));
                return;
            case DoorFadeStage.Dark:
                frame.Shade(pictures, dark);
                return;
            case DoorFadeStage.FadingIn:
                frame.Shade(pictures, fade.Between(PictureShading.Shaded(dark), PictureShading.Standard));
                return;
            case DoorFadeStage.None:
                throw new InvalidOperationException("A door fade shades its pictures while a fade runs.");
            default:
                throw new InvalidOperationException($"Undefined door fade stage {Stage}.");
        }
    }

    /// <summary>
    /// Samus's shades the fade keeps lit at its stage: none while the room being left fades out or stays dark; her beams'
    /// through the fade in, which the room's load starts, and her visor's door-completion green after them once the door
    /// has opened.
    /// </summary>
    private ImmutableArray<PictureShade> LitOnSamus() => Stage != DoorFadeStage.FadingIn ? []
        : doorOpened ? [samusColours.Beams, SamusShades.DoorCompletionVisor]
        : [samusColours.Beams];

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
}
