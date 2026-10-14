namespace SuperMetroid.Core.Game;

/// <summary>Room scroll access shared by every enemy service set that changes the camera's zones.</summary>
public interface IRoomScrollServices
{
    /// <summary>The room's scroll zones.</summary>
    IRoomScrollZoneAccess RoomScroll { get; }
}
