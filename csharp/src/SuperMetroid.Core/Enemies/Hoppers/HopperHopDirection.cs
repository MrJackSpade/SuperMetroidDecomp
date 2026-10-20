namespace SuperMetroid.Core.Enemies.Hoppers;

/// <summary>Which way a hop goes relative to the hopper.</summary>
internal enum HopperHopDirection
{
    /// <summary>Rightward, chosen when Samus is not to the left.</summary>
    Forward,

    /// <summary>Leftward, chosen when Samus is to the left.</summary>
    Backward,
}
