namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>How a colour effect starts (#1554).</summary>
public enum ColourEffectStart
{
    /// <summary>The effect starts its program as authored.</summary>
    Plain = 1,
    /// <summary>The heat shimmer starts the program of the suit Samus wears, the Gravity Suit's before the Varia Suit's.</summary>
    ChoosesHeatAnimationForSuit,
    /// <summary>The effect does not start once the area's mini-boss is recorded dead: Spore Spawn's spores.</summary>
    SkippedOnceAreaMiniBossIsDead,
}
