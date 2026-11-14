using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Ending.Artwork;

/// <summary>
/// The shades the import pre-colours the ending's sheets in (#1512): each colour load, each level of each faded group,
/// each flash and each logo crossfade step, over every ending picture and floor, and each of those authored for an actor
/// (#1544) under the colouring it draws in, over its pictures. Which inks each recolours, and to what, is the import's.
/// </summary>
public static class EndingShades
{
    /// <summary>The levels every faded group moves through: a group at level L shows its colours scaled by L over this.</summary>
    public const int FadeLevels = EndingExplosionFadeDefinitions.Steps;

    /// <summary>The logo crossfade's steps.</summary>
    public const int LogoCrossfadeSteps = EndingLogoDefinitions.CrossfadeSteps;

    /// <summary>The colours <paramref name="load"/> shows over those it replaces.</summary>
    public static PictureShade Loaded(EndingColourLoad load) => new($"Ending{load}", 0);

    /// <summary>The colours of <paramref name="colours"/> at fade <paramref name="level"/> of <see cref="FadeLevels"/>.</summary>
    public static PictureShade Faded(EndingFade colours, int level) => new($"EndingFade{colours}", level);

    /// <summary>The colours <paramref name="flash"/> covers, in its solid colour.</summary>
    public static PictureShade Flashed(EndingFlash flash) => new($"EndingFlash{flash}", 0);

    /// <summary>The logo's background and sprite colours at crossfade <paramref name="step"/>.</summary>
    public static PictureShade LogoCrossfade(int step) => new("EndingLogoCrossfade", step);

    /// <summary>An actor's palette-inheriting parts drawn in <paramref name="colouring"/>, in the sheet's own colours.</summary>
    public static PictureShade Drawn(EndingSpriteColouring colouring) => new($"EndingDrawn{colouring}", 0);

    /// <summary>
    /// What a screen shade <paramref name="shown"/> authored for the actors drawn in <paramref name="drawn"/> gives their
    /// pictures (#1544): the colours it shows in that colouring's colours over their palette-inheriting parts' inks, and
    /// over their own-palette parts' inks in those parts' own colours.
    /// </summary>
    public static PictureShade Under(PictureShade drawn, PictureShade shown) => new($"{drawn.Effect}.{shown.Effect}", shown.Step);
}
