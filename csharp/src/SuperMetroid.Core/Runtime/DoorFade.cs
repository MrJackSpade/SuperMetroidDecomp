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

    /// <summary>Where the fade is, each stage carrying only what it shows with (#2560).</summary>
    private Fade shown = NoFade.Instance;

    /// <summary>Where the fade is.</summary>
    private DoorFadeStage Stage => shown.Stage;

    /// <summary>Starts the room being left fading out over <paramref name="fade"/>'s steps.</summary>
    /// <param name="fade">The fade's steps, which the door takes one per update.</param>
    /// <param name="commonRooms">Whether the door leads from one common room into another.</param>
    internal void FadeOut(GradualFadeSteps fade, bool commonRooms) =>
        shown = new FadingOut(Ensure.NotNull(fade), commonRooms);

    /// <summary>
    /// The door has opened in the room being entered: Samus's visor shows the door's completion green, which the fade in
    /// keeps lit from now on.
    /// </summary>
    /// <exception cref="InvalidOperationException">The room being entered is not fading in.</exception>
    internal void DoorOpened() => shown = shown is FadingIn fadingIn
        ? fadingIn with { DoorOpened = true }
        : throw new InvalidOperationException($"A door opens while its room fades in, not while {Stage}.");

    /// <summary>Keeps the faded-out room dark while the door loads and scrolls.</summary>
    /// <exception cref="InvalidOperationException">No room is fading out.</exception>
    internal void Darken() => shown = shown is FadingOut fadingOut
        ? new Dark(fadingOut.BetweenCommonRooms)
        : throw new InvalidOperationException($"A door darkens once its room has faded out, not while {Stage}.");

    /// <summary>Starts the room being entered fading in over <paramref name="fade"/>'s steps.</summary>
    /// <exception cref="InvalidOperationException">The room being left has not faded out.</exception>
    internal void FadeIn(GradualFadeSteps fade) => shown = shown is Dark dark
        ? new FadingIn(Ensure.NotNull(fade), dark.BetweenCommonRooms, DoorOpened: false)
        : throw new InvalidOperationException($"A door fades its room in once the room it left is dark, not while {Stage}.");

    /// <summary>Ends the fade: the room entered shows its own colours.</summary>
    internal void End() => shown = NoFade.Instance;

    /// <inheritdoc />
    public void ShadeGameplay(GameplayShadingBuilder frame)
    {
        Ensure.NotNull(frame);
        if (shown is not Running running)
            return;
        frame.Fade(new GameplayDisplayFade(running.DisplayEffect(), GameplayFadeSpares.Hud, running.LitOnSamus(samusColours)));
        // The HUD and the escape timer fade between their own colours and their dark shades, which keep their lit colours,
        // as far as the display's fade has come (#2289).
        running.Shade(frame, GameplayPictures.Hud,
            running.BetweenCommonRooms ? DoorFadeShades.HudDarkBetweenCommonRooms : DoorFadeShades.HudDark);
        running.Shade(frame, GameplayPictures.EscapeTimer, DoorFadeShades.EscapeTimerDark);
    }

    /// <summary>Where a door's fade is: no fade, or a running stage.</summary>
    /// <param name="Stage">The stage.</param>
    private abstract record Fade(DoorFadeStage Stage);

    /// <summary>No door is fading: the room shows its own colours.</summary>
    private sealed record NoFade() : Fade(DoorFadeStage.None)
    {
        /// <summary>The one such state.</summary>
        internal static NoFade Instance { get; } = new();
    }

    /// <summary>A running stage of a door's fade.</summary>
    /// <param name="Stage">The stage.</param>
    /// <param name="BetweenCommonRooms">Whether the door leads from one common room into another, which keeps more of the HUD lit.</param>
    private abstract record Running(DoorFadeStage Stage, bool BetweenCommonRooms) : Fade(Stage)
    {
        /// <summary>The effect the stage shows over the whole display.</summary>
        internal abstract ColourEffect DisplayEffect();

        /// <summary>
        /// Shades <paramref name="pictures"/> at the stage toward or from <paramref name="dark"/>: a fade out from their own
        /// colours toward it, which shows their own colours until its first moving step, the dark shade while the room is
        /// dark, and a fade in from it to their own colours.
        /// </summary>
        internal abstract void Shade(GameplayShadingBuilder frame, GameplayPictures pictures, PictureShade dark);

        /// <summary>
        /// Samus's shades the stage keeps lit: none while the room being left fades out or stays dark; her beams' through the
        /// fade in, which the room's load starts, and her visor's door-completion green after them once the door has opened.
        /// </summary>
        internal virtual ImmutableArray<PictureShade> LitOnSamus(SamusColours colours) => [];
    }

    /// <summary>The room being left fades out to black over <paramref name="Steps"/>.</summary>
    /// <param name="Steps">The fade's steps, which the door takes one per update.</param>
    /// <param name="BetweenCommonRooms">Whether the door leads from one common room into another.</param>
    private sealed record FadingOut(GradualFadeSteps Steps, bool BetweenCommonRooms)
        : Running(DoorFadeStage.FadingOut, BetweenCommonRooms)
    {
        /// <inheritdoc/>
        internal override ColourEffect DisplayEffect() => Steps.Toward(RgbColor.Black);

        /// <inheritdoc/>
        internal override void Shade(GameplayShadingBuilder frame, GameplayPictures pictures, PictureShade dark) =>
            frame.Shade(pictures, Steps.Between(PictureShading.Standard, PictureShading.Shaded(dark)));
    }

    /// <summary>The room has faded out, and stays dark while the door loads and scrolls.</summary>
    /// <param name="BetweenCommonRooms">Whether the door leads from one common room into another.</param>
    private sealed record Dark(bool BetweenCommonRooms) : Running(DoorFadeStage.Dark, BetweenCommonRooms)
    {
        /// <inheritdoc/>
        internal override ColourEffect DisplayEffect() => new ColourFlash(RgbColor.Black);

        /// <inheritdoc/>
        internal override void Shade(GameplayShadingBuilder frame, GameplayPictures pictures, PictureShade dark) =>
            frame.Shade(pictures, dark);
    }

    /// <summary>The room being entered fades in from black over <paramref name="Steps"/>.</summary>
    /// <param name="Steps">The fade's steps, which the door takes one per update.</param>
    /// <param name="BetweenCommonRooms">Whether the door leads from one common room into another.</param>
    /// <param name="DoorOpened">Whether the door has opened in the room being entered, which keeps Samus's visor lit.</param>
    private sealed record FadingIn(GradualFadeSteps Steps, bool BetweenCommonRooms, bool DoorOpened)
        : Running(DoorFadeStage.FadingIn, BetweenCommonRooms)
    {
        /// <inheritdoc/>
        internal override ColourEffect DisplayEffect() => Steps.From(RgbColor.Black);

        /// <inheritdoc/>
        internal override void Shade(GameplayShadingBuilder frame, GameplayPictures pictures, PictureShade dark) =>
            frame.Shade(pictures, Steps.Between(PictureShading.Shaded(dark), PictureShading.Standard));

        /// <inheritdoc/>
        internal override ImmutableArray<PictureShade> LitOnSamus(SamusColours colours) => DoorOpened
            ? [colours.Beams, SamusShades.DoorCompletionVisor]
            : [colours.Beams];
    }
}
