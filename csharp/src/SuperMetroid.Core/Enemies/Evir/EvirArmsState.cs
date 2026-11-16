namespace SuperMetroid.Core.Enemies.Evir;

/// <summary>The state of an Evir's arms, which follow their body's position and facing.</summary>
public sealed class EvirArmsState : EvirPartState
{
    /// <summary>Starts the state of arms whose body, listed right before them, is <paramref name="body"/>.</summary>
    /// <param name="body">The body's place.</param>
    /// <param name="bodyState">The body's state as the arms found it.</param>
    internal EvirArmsState(RoomEnemyPlace body, EvirBodyState bodyState)
    {
        Body = Ensure.NotNull(body);
        BodyState = Ensure.NotNull(bodyState);
    }

    /// <summary>The place of the arms' body, which their room lists right before them.</summary>
    internal RoomEnemyPlace Body { get; }

    /// <summary>
    /// The body's state as the arms found it when they were placed. The arms keep reaching it after the body has left its
    /// place (#1543): the update a hit kills the body they run once more on it, reading it facing left
    /// (<see cref="KilledEvirBodyFacing"/>), and the arms of a body a power bomb's follow-up deleted
    /// (<see cref="FollowUpAfterStruckPart"/>) live on following that body's last facing.
    /// </summary>
    internal EvirBodyState BodyState { get; }
}
