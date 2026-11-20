namespace SuperMetroid.Core.Enemies.Yard;

/// <summary>What a Yard doing one of its behaviours does (<see cref="YardBehaviorDefinitions.Behaviors"/>).</summary>
/// <param name="Airborne">Whether the Yard is in the air, so a quake cannot drop it again.</param>
/// <param name="WatchesSamus">Whether the Yard watches Samus, to hide from her or turn toward her.</param>
/// <param name="Solid">Whether the Yard is solid to Samus.</param>
/// <param name="TakesHits">Whether a touch or a shot reaches the Yard, to kick it or launch it into the air.</param>
internal sealed record YardBehaviorDefinition(bool Airborne, bool WatchesSamus, bool Solid, bool TakesHits);
