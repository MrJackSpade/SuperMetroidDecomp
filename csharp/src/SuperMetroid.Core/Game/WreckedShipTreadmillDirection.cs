namespace SuperMetroid.Core.Game;

/// <summary>
/// The direction encoded by Wrecked Ship's two bank-$87 treadmill animated-tile objects.
/// </summary>
public enum WreckedShipTreadmillDirection
{
    /// <summary>Frames $8E64,$8E84,$8EA4,$8EC4, selected by object $8275.</summary>
    Rightwards,

    /// <summary>Frames $8EC4,$8EA4,$8E84,$8E64, selected by object $827B.</summary>
    Leftwards,
}
