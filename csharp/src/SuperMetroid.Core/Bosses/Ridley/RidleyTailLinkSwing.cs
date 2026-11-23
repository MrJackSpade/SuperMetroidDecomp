namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>Where one of Ridley's tail links stands in its swing (#1413, #2410, #2560).</summary>
public enum RidleyTailLinkSwing
{
    /// <summary>Out of the current wave or whip.</summary>
    Resting,
    /// <summary>In the wave or whip, staggering until it reaches the tail's ideal link spacing and starts the next link.</summary>
    Staggering,
    /// <summary>In the wave or whip after starting the next link, swinging without staggering.</summary>
    Swinging,
}
