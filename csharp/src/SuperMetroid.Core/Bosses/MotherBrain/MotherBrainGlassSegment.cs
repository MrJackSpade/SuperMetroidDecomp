namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Mother Brain's glass projectile animations: the eight spins a shard plays by the arc of the turn it is launched into,
/// measured clockwise from straight up (<see cref="MotherBrainGlassAnimations.ShardAnimations"/>), and the sparkle.
/// </summary>
public enum MotherBrainGlassSegment
{
    /// <summary>A shard launched up, within the sixteenth of the turn clockwise from straight up.</summary>
    ShardFlyingUp,
    /// <summary>A shard launched up and to the right, within the eighth of the turn about the diagonal.</summary>
    ShardFlyingUpRight,
    /// <summary>A shard launched right, from a sixteenth of the turn above the horizontal to the diagonal down and to the right.</summary>
    ShardFlyingRight,
    /// <summary>A shard launched down and to the right, from the diagonal to straight down.</summary>
    ShardFlyingDownRight,
    /// <summary>A shard launched down, within the sixteenth of the turn clockwise from straight down.</summary>
    ShardFlyingDown,
    /// <summary>A shard launched down and to the left, within the eighth of the turn about the diagonal.</summary>
    ShardFlyingDownLeft,
    /// <summary>A shard launched left, from a sixteenth of the turn below the horizontal to the diagonal up and to the left.</summary>
    ShardFlyingLeft,
    /// <summary>A shard launched up and to the left, from the diagonal to straight up.</summary>
    ShardFlyingUpLeft,
    /// <summary>The finite sparkle a falling shard sheds.</summary>
    Sparkle,
}
