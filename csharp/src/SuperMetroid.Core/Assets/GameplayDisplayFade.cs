namespace SuperMetroid.Core.Assets;

/// <summary>
/// A fade of the whole gameplay display (#1512): a door's fade or the death fade, one colour effect the backend applies
/// after every picture's and every shaded sprite draw's own shading, except where the game keeps colours lit.
/// </summary>
/// <param name="Effect">The fade's colour effect.</param>
/// <param name="Spares">What the fade leaves in its own colours.</param>
/// <param name="OverSamus">
/// A shade of Samus's whose colours were written over the faded display and which the fade leaves as they are, or null.
/// </param>
public sealed record GameplayDisplayFade(ColourEffect Effect, GameplayFadeSpares Spares, PictureShade? OverSamus = null)
{
    /// <summary>The fade's colour effect.</summary>
    public ColourEffect Effect { get; } = Ensure.NotNull(Effect);

    /// <summary>
    /// Whether the HUD's pictures keep their own colours, because the import shades the steps of a fade that keeps some of
    /// them lit, as a door's does.
    /// </summary>
    public bool SparesHud => Spares == GameplayFadeSpares.Hud;

    /// <summary>Whether Samus's draws keep their own colours, as the death fade keeps her lit.</summary>
    public bool SparesSamus => Spares == GameplayFadeSpares.Samus;
}
