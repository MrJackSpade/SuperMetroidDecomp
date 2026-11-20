namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>Which way a wall Space Pirate climbs.</summary>
public enum WallSpacePirateClimbDirection
{
    /// <summary>Selects the downward climb animation, whose movement cues move it down before collision handling.</summary>
    Down,
    /// <summary>Selects the upward climb animation, whose movement cues move it up before collision handling.</summary>
    Up,
}
