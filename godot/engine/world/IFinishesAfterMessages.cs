namespace Engine.World;

/// <summary>
/// Something of a room that begins a thing on one update and finishes it once the messages asked for on that update
/// have been shown: an item, whose pickup ends as its message closes. It asks the room
/// (<see cref="Room.FinishAfterMessages"/>), and the room calls it as that same update goes on after its objects
/// have played, whether a message was shown or none was asked for.
/// </summary>
public interface IFinishesAfterMessages
{
    /// <summary>Finishes what it began: every message asked for by then has been shown.</summary>
    void FinishAfterMessages();
}
