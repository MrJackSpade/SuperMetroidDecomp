using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Rendering;

/// <summary>
/// A fade of the whole gameplay display (#1512): a door's fade or the death fade, one colour effect the backend applies
/// after every picture's and every shaded sprite draw's own shading, except where the game keeps colours lit.
/// </summary>
/// <param name="Effect">The fade's colour effect.</param>
/// <param name="SparesHud">
/// Whether the HUD's pictures keep their own colours, because the import shades the steps of a fade that keeps some of
/// them lit, as a door's does.
/// </param>
/// <param name="SparesSamus">Whether Samus's draws keep their own colours, as the death fade keeps her lit.</param>
public sealed record GameplayDisplayFade(ColourEffect Effect, bool SparesHud, bool SparesSamus)
{
    /// <summary>The fade's colour effect.</summary>
    public ColourEffect Effect { get; } = Ensure.NotNull(Effect);
}
