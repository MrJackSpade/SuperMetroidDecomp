using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Fx.ColourEffects;
using SuperMetroid.Core.Samus.Colours;
using SuperMetroid.Core.World;

namespace SuperMetroid.Core.Runtime;

/// <summary>
/// The colours a door's fades show (#1512): the room being left fades to black, stays dark while the door loads and
/// scrolls, and the room being entered fades in from black. The fade is one fade of the whole display, which every shaded
/// sprite draw, Samus's among them, shows after its own shading; the background's pictures and the escape timer are spared
/// it and fade as far toward their dark shades instead (#2289, #3036), which keep the background colours of the HUD's
/// minimap and highlighted item lit, and between two common rooms its item and minimap backgrounds: wherever the room's
/// own pictures show those colours, its doors among them, they stay lit through the door's scroll as the HUD does. Samus's beams and visor stay lit through the room's fade in (#2093): her beams from
/// the room's load, which the fade in starts with, and her visor, which shows the door's completion green, once the door
/// has opened.
/// </summary>
/// <param name="samusColours">Samus's colours, whose beams' shade the fade in keeps lit.</param>
internal sealed partial class DoorFade(SamusColours samusColours) : IGameplayShadingSource
{
    /// <summary>Samus's colours, whose beams' shade the fade in keeps lit.</summary>
    private readonly SamusColours samusColours = Ensure.NotNull(samusColours);

    /// <summary>Whether the door leads from one common room into another, which keeps more of the HUD lit.</summary>
    private bool betweenCommonRooms;

    /// <summary>Where the fade is.</summary>
    internal DoorFadeStage Stage { get; set; } = DoorFadeStage.None;

    /// <summary>Starts the room being left fading out over <paramref name="fade"/>'s steps.</summary>
    /// <param name="fade">The fade's steps, which the door takes one per update.</param>
    /// <param name="commonRooms">Whether the door leads from one common room into another.</param>
    internal void FadeOut(GradualFadeSteps fade, bool commonRooms)
    {
        betweenCommonRooms = commonRooms;
        Stage = new DoorFadeStage.FadingOut(fade);
    }

    /// <summary>Keeps the faded-out room dark while the door loads and scrolls.</summary>
    internal void Darken() => Stage = new DoorFadeStage.Dark();

    /// <summary>Starts the room being entered fading in over <paramref name="fade"/>'s steps.</summary>
    internal void FadeIn(GradualFadeSteps fade) => Stage = new DoorFadeStage.FadingIn(fade);

    /// <summary>
    /// The door has opened in the room being entered, whose fade in goes on over <paramref name="fade"/>'s steps: Samus's
    /// visor shows the door's completion green, which the fade in keeps lit from now on.
    /// </summary>
    internal void DoorOpened(GradualFadeSteps fade) => Stage = new DoorFadeStage.FadingInDoorOpen(fade);

    /// <summary>Ends the fade: the room entered shows its own colours.</summary>
    internal void End() => Stage = DoorFadeStage.None;

    /// <inheritdoc />
    public void ShadeGameplay(GameplayShadingBuilder frame)
    {
        Ensure.NotNull(frame);
        if (Stage is not DoorFadeStage.Running running)
            return;
        frame.Fade(new GameplayDisplayFade(running.DisplayEffect, GameplayFadeSpares.Backgrounds, running.LitOnSamus(samusColours)));
        // The background's pictures and the escape timer fade between their own colours and their dark shades, which keep
        // their lit colours, as far as the display's fade has come (#2289, #3036).
        PictureShade dark = betweenCommonRooms ? DoorFadeShades.BackgroundDarkBetweenCommonRooms : DoorFadeShades.BackgroundDark;
        foreach (GameplayPictures pictures in GameplayPictureDefinitions.Backgrounds)
            running.Shade(frame, pictures, dark);
        running.Shade(frame, GameplayPictures.EscapeTimer, DoorFadeShades.EscapeTimerDark);
    }
}
