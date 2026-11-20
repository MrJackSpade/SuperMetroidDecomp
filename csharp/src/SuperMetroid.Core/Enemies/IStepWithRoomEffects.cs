namespace SuperMetroid.Core.Enemies;

/// <summary>
/// A display effect an encounter steps with the room's own effects (#2571), once each ordinary gameplay update no message
/// box waits on, before the room effects step: the runtime steps every such effect the room's species compose, in their
/// order.
/// </summary>
internal interface IStepWithRoomEffects
{
    /// <summary>Steps the effect for this update, before the room's effects step.</summary>
    void StepWithRoomEffects();
}
