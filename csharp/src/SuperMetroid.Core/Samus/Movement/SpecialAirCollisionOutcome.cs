namespace SuperMetroid.Core.Samus.Movement;

/// <summary>What a special-air block's collision reaction does to the movement that reached it.</summary>
internal enum SpecialAirCollisionOutcome
{
    /// <summary>The movement passes through the block as through air.</summary>
    PassesThrough,

    /// <summary>The movement passes into the block and Samus lands on the quicksand's surface (#2560).</summary>
    LandsOnSurface,

    /// <summary>The block stops the movement.</summary>
    Blocks,
}
