using SuperMetroid.Core.Enemies.Projectiles;
namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>One style of Mother Brain's death explosions (<see cref="MotherBrainDeathExplosionStyles"/>).</summary>
/// <param name="IntervalUpdates">The updates the style waits between bursts.</param>
/// <param name="Anchors">The anchors of a group the style's burst explodes at.</param>
/// <param name="Animation">The animation each of the style's explosions shows, from the random number drawn for it.</param>
internal sealed record MotherBrainDeathExplosionStyleDefinition(
    int IntervalUpdates,
    Func<MotherBrainDeathExplosionGroup, IReadOnlyList<Offset16<RoomPixels>>> Anchors,
    Func<int, DustExplosionAnimation> Animation);
