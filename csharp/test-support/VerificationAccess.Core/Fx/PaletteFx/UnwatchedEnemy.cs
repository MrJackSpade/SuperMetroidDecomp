namespace SuperMetroid.Core.Fx.ColourEffects;

/// <summary>
/// An enemy a check hands a colour effect an enemy shows (#2288) when the check reads each frame the effect shows from the
/// colour effects themselves: it shows nothing.
/// </summary>
internal sealed class UnwatchedEnemy : ILitTorizo
{
    /// <summary>Never: the check's Torizo stays undefeated, so a belly glow lighting it runs until the check ends it.</summary>
    public bool IsDefeated => false;

    /// <summary>The one unwatched enemy.</summary>
    internal static UnwatchedEnemy Instance { get; } = new();

    /// <summary>Shows nothing: the check reads each frame the effect shows from the colour effects themselves.</summary>
    /// <param name="frame">The effect's frame.</param>
    public void ShowGlow(ColourEffectFrame frame)
    {
    }
}
