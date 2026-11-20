namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// Resuming gameplay from the pause disarms the elevator's pseudo-door (#2283): whatever armed it, a collision with it or
/// an elevator door load, is forgotten, so an elevator ridden down when the game unpaused starts its door the moment Samus
/// reaches the pseudo-door, without the down-elevator wait (<see cref="ElevatorDoorDefinitions.DownwardsElevatorDelayFrames"/>). The ride's status and travel are kept. The skipped
/// wait is gameplay-observable: the door, the room load and every later update come one wait earlier.
/// </summary>
/// <remarks>
/// <para>
/// Consulted once by the pause's teardown, on the update gameplay resumes, after Samus's equipment and the room's stations
/// are restored and before the update's controller input is accepted. A stateless rule over the elevator journey's
/// arming, whose identities the journey supplies.
/// </para>
/// <para>
/// Evidence: the resume ($80:A149, game state $11) reloads the room's tiles through $82:E78C, whose first write clears the
/// elevator's properties ($0E16), the word the pseudo-door's collision and the elevator door load set. In the 13% SMV Samus
/// pauses while riding an elevator down and, with the word cleared, reaches the bottom without the down-elevator wait.
/// </para>
/// </remarks>
[CompatibilityService("resuming gameplay from the pause disarms the elevator's pseudo-door, so a ride unpaused on starts its door without the down-elevator wait")]
public static class UnpauseDisarmsElevator
{
    /// <summary>The pseudo-door's arming once gameplay resumes from the pause: <paramref name="unarmed"/>, whatever armed it.</summary>
    /// <typeparam name="TArming">What arms the pseudo-door.</typeparam>
    /// <param name="unarmed">The arming of a pseudo-door nothing has armed.</param>
    internal static TArming ArmingAfterResume<TArming>(TArming unarmed) where TArming : struct, Enum => unarmed;
}
