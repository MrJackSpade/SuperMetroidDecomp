namespace SuperMetroid.Core.Enemies.Evir;

/// <summary>The state of an Evir's arms, which follow their body's position and facing.</summary>
public sealed class EvirArmsState : EvirPartState
{
    /// <summary>Starts the state of arms whose body, listed right before them, is <paramref name="body"/>.</summary>
    /// <param name="body">The body's place.</param>
    internal EvirArmsState(RoomEnemyPlace body) => Body = Ensure.NotNull(body);

    /// <summary>
    /// The place of the arms' body, which their room lists right before them. The arms read the body there after it has
    /// left its place (<see cref="RoomEnemyPlace.FoundEnemy"/>): the update a hit kills the body they run once more on
    /// its cleared place, reading it facing left, and the arms of a body a power bomb's follow-up deleted
    /// (<see cref="FollowUpAfterStruckPart"/>) live on following that body's last facing.
    /// </summary>
    internal RoomEnemyPlace Body { get; }
}
