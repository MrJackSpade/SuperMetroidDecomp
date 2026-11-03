using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Rendering;
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

    /// <summary>Where the fade is.</summary>
    internal DoorFadeStage Stage { get; private set; } = DoorFadeStage.None;

    /// <summary>Starts the room being left fading out over <paramref name="fade"/>'s steps.</summary>
    /// <param name="fade">The fade's steps, which the door takes one per update.</param>
    /// <param name="commonRooms">Whether the door leads from one common room into another.</param>
    internal void FadeOut(GradualFadeSteps fade, bool commonRooms)
    {
        steps = Ensure.NotNull(fade);
        betweenCommonRooms = commonRooms;
        Stage = DoorFadeStage.FadingOut;
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
        (ColourEffect effect, PictureShade? hud) = Stage switch
        {
            DoorFadeStage.FadingOut => ((ColourEffect)fade.Toward(RgbColor.Black), last is { } value ? (PictureShade?)HudFadingOut(value) : null),
            DoorFadeStage.Dark => (new ColourFlash(RgbColor.Black), (PictureShade?)HudDark()),
            DoorFadeStage.FadingIn => (fade.From(RgbColor.Black), last is { } value ? (PictureShade?)HudFadingIn(value) : HudDark()),
            _ => throw new InvalidOperationException($"Undefined door fade stage {Stage}."),
        };
        frame.Fade(new GameplayDisplayFade(effect, SparesHud: true, SparesSamus: false));
        if (hud is { } shade)
            frame.Shade(GameplayPictures.Hud, shade);
    }

    /// <summary>The HUD's shade once the room has faded out: the fade out's last step, its faded colours black.</summary>
    private PictureShade HudDark() => HudFadingOut(DoorTransitionColourDefinitions.HudFadeShades - 1);

    /// <summary>The HUD's shade of a fade out of the room whose last step took counter value <paramref name="value"/>.</summary>
    private PictureShade HudFadingOut(int value) => betweenCommonRooms
        ? DoorFadeShades.HudFadingOutBetweenCommonRooms(value) : DoorFadeShades.HudFadingOut(value);

    /// <summary>The HUD's shade of a fade into the room whose last step took counter value <paramref name="value"/>.</summary>
    private PictureShade HudFadingIn(int value) => betweenCommonRooms
        ? DoorFadeShades.HudFadingInBetweenCommonRooms(value) : DoorFadeShades.HudFadingIn(value);
}
