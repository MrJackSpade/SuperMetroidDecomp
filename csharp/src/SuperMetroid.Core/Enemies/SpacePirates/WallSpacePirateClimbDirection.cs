namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>Which way a wall Space Pirate climbs.</summary>
public enum WallSpacePirateClimbDirection
{
    /// <summary>Selects the downward climb animation, whose movement cues add three room pixels to Y before collision handling.</summary>
    Down,
    /// <summary>Selects the upward climb animation, whose movement cues subtract three room pixels from Y before collision handling.</summary>
    Up,
}
