using SuperMetroid.Core.Fx.ColourEffects;

namespace SuperMetroid.Verification.Bosses.Torizo;

/// <summary>
/// A Torizo that keeps its health through a check: the belly glow lighting it plays its whole cycle, and a check that plays
/// a Torizo's cues without his fight names it as the Torizo playing them.
/// </summary>
internal sealed class LivingTorizo : ILitTorizo
{
    /// <summary>The one Torizo.</summary>
    internal static LivingTorizo Instance { get; } = new();

    /// <inheritdoc/>
    public bool IsDefeated => false;

    /// <summary>Shows nothing: a check reads each frame the glow shows from the colour effects themselves.</summary>
    /// <param name="frame">The glow's frame.</param>
    public void ShowGlow(ColourEffectFrame frame)
    {
    }
}
