namespace SuperMetroid.Core.Enemies.Dragon;

/// <summary>
/// Dragon's rising and falling fireball animations (#641): one segment per animation a spawn, branch or family step enters, in authored order.
/// Their steps are <see cref="DragonFireballAnimations"/>.
/// </summary>
internal enum DragonFireballSegment
{
    /// <summary>The rising left animation.</summary>
    RisingLeft,
    /// <summary>The rising right animation.</summary>
    RisingRight,
    /// <summary>The falling left animation.</summary>
    FallingLeft,
    /// <summary>The falling right animation.</summary>
    FallingRight,
}
