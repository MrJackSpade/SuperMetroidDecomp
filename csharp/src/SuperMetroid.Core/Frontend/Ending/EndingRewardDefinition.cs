using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Ending.Artwork;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// One ending reward's post-credits presentation (#1513): what Samus is dressed in and the sheet her actors draw from,
/// the colour groups its reveal fades in, and the actors its idle pose, gesture and jump spawn.
/// </summary>
/// <param name="Dress">What the reward dresses Samus in, whose characters her post-credits actors draw from.</param>
/// <param name="Sheet">The sheet of the post-credits Samus the reward shows, until she falls, and of the shooting stars.</param>
/// <param name="RevealFades">
/// The colour groups the reveal crossfades the waiting backdrop into, in fade order; none for a reward that shows its own
/// colours at once, over no background.
/// </param>
/// <param name="Idle">The idle actors the credits' end spawns, in spawn order.</param>
/// <param name="Gesture">The gesture actors, in the order they spawn, step and draw: a suited head first, then body and arm.</param>
/// <param name="JumpHead">The jump's head and where it is readied and launched; none when the body carries the head.</param>
/// <param name="JumpBody">The jump's body actor.</param>
internal sealed record EndingRewardDefinition(
    EndingRewardDress Dress,
    SpriteSheetId Sheet,
    IReadOnlyList<EndingFade> RevealFades,
    IReadOnlyList<EndingSpriteDefinition> Idle,
    IReadOnlyList<EndingRewardActor> Gesture,
    EndingRewardJumpHead? JumpHead,
    EndingRewardActor JumpBody)
{
    /// <summary>
    /// Whether the reveal crossfades the waiting backdrop into the reward's colours: the background stays shown and the
    /// sprites glow over it while it does.
    /// </summary>
    internal bool CrossfadesReveal => RevealFades.Count > 0;
}
