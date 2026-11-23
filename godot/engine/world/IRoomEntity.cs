using Engine.Collision;

namespace Engine.World;

/// <summary>Something that lives in a room: it has an exact place there, and the room steps it and shows it.</summary>
public interface IRoomEntity : ICollisionOwner
{
    /// <summary>Called once as the thing joins <paramref name="room"/>, before its first update there.</summary>
    void EnterRoom(Room room);

    /// <summary>Puts the thing's Godot node where its exact place says, for drawing.</summary>
    void ShowPosition();
}
