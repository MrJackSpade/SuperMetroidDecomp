namespace SuperMetroid.Core.Enemies.Dragon;

/// <summary>
/// Dragon's rising and falling fireball animations (#641): one segment per animation a spawn, branch or family step enters, in authored order.
/// Their steps are <see cref="DragonFireballAnimations"/>.
/// </summary>
internal enum DragonFireballSegment
{
    /// <summary>A fireball rising to the left, flaring and drawing in, repeating.</summary>
    RisingLeft,
    /// <summary>A fireball rising to the right, flaring and drawing in, repeating.</summary>
    RisingRight,
    /// <summary>A fireball falling to the left, flaring and drawing in, repeating.</summary>
    FallingLeft,
    /// <summary>A fireball falling to the right, flaring and drawing in, repeating.</summary>
    FallingRight,
}
