namespace SuperMetroid.Core.Enemies;

/// <summary>
/// A room-wide glow an enemy keeps (#2571): a colour effect that steps once every enemy of the pass has updated, whether
/// or not time is frozen. The enemy pass steps every glow the room's species compose, in their order.
/// </summary>
internal interface IRoomGlow
{
    /// <summary>Steps the glow after every enemy of the pass has updated.</summary>
    void StepGlow();
}
