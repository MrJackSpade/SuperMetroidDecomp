namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>The binary climb direction stored in variable C.</summary>
public enum WallSpacePirateClimbDirection
{
    /// <summary>Variable-C value zero selects the downward climb animation, whose movement cues add three room pixels to Y before collision handling.</summary>
    Down = 0,
    /// <summary>Variable-C value one selects the upward climb animation, whose movement cues subtract three room pixels from Y before collision handling.</summary>
    Up = 1,
}
