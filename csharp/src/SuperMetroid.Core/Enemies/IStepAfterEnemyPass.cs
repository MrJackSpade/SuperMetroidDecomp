namespace SuperMetroid.Core.Enemies;

/// <summary>
/// Work an encounter steps after the enemy pass has published its events and the runtime has carried out the pass's room
/// effects, on updates whose time is not frozen (#2571): its order against those effects is observable, so the enemy phase
/// runs it there, for every encounter the room's species compose, in their order.
/// </summary>
internal interface IStepAfterEnemyPass
{
    /// <summary>Steps the encounter's work after the enemy pass.</summary>
    void StepAfterEnemyPass();
}
