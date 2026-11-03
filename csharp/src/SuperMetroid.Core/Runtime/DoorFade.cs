using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.World;

namespace SuperMetroid.Core.Runtime;

/// <summary>
/// The colours a door's fades show (#1512): the room being left fades to black, stays dark while the door loads and
/// scrolls, and the room being entered fades in from black. The room's pictures, its layer-3 effect, the boss body and
/// the shaded sprite draws take the fade as an effect of the colours they show; the HUD takes the import's shade of each
/// step, which keeps its minimap and highlighted item lit, and between two common rooms its item and minimap backgrounds.
/// </summary>
internal sealed class DoorFade : IGameplayShadingSource
{
    /// <summary>The pictures that take the fade as an effect of the colours they show.</summary>
    private static readonly GameplayPictures[] Faded =
        [GameplayPictures.Room, GameplayPictures.DoorSourceRoom, GameplayPictures.Layer3, GameplayPictures.BossBody];

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
        int taken = fade.Taken.Length;
        (ColourEffect effect, PictureShade hud) = Stage switch
        {
            DoorFadeStage.FadingOut => ((ColourEffect)fade.Toward(RgbColor.Black), HudFadingOut(taken)),
            DoorFadeStage.Dark => (new ColourFlash(RgbColor.Black), HudFadingOut(taken)),
            DoorFadeStage.FadingIn => (fade.From(RgbColor.Black), HudFadingIn(taken)),
            _ => throw new InvalidOperationException($"Undefined door fade stage {Stage}."),
        };
        foreach (GameplayPictures pictures in Faded)
            frame.Affect(pictures, effect);
        frame.AffectSprites(effect);
        frame.Shade(GameplayPictures.Hud, hud);
    }

    /// <summary>The HUD's shade after <paramref name="taken"/> steps of a fade out of the room.</summary>
    private PictureShade HudFadingOut(int taken) => betweenCommonRooms
        ? DoorFadeShades.HudFadingOutBetweenCommonRooms(taken) : DoorFadeShades.HudFadingOut(taken);

    /// <summary>The HUD's shade after <paramref name="taken"/> steps of a fade into the room.</summary>
    private PictureShade HudFadingIn(int taken) => betweenCommonRooms
        ? DoorFadeShades.HudFadingInBetweenCommonRooms(taken) : DoorFadeShades.HudFadingIn(taken);
}
