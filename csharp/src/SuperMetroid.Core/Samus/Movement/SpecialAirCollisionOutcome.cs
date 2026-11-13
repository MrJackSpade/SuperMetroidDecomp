namespace SuperMetroid.Core.Samus.Movement;

/// <summary>What a special-air block's collision reaction does to the movement that reached it.</summary>
internal enum SpecialAirCollisionOutcome
{
    /// <summary>The movement passes through the block as through air.</summary>
    PassesThrough,

    /// <summary>The block stops the movement.</summary>
    Blocks,
}
