namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>The two stages of Phantoon's dying fade-out (#2560).</summary>
public enum PhantoonDeathStage
{
    /// <summary>His body's pixelation grows, one size at the death's cadence, until it reaches its last.</summary>
    Pixelating,
    /// <summary>His pixelation is done, and his body's fade runs on to the end of his death.</summary>
    FadingOut,
}
