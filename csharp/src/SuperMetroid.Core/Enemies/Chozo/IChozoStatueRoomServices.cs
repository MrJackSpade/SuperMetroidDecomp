using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>What the Chozo statues' room setup needs: Samus's control lock and the scroll zones.</summary>
public interface IChozoStatueRoomServices : IRoomScrollServices
{
    /// <summary>Samus's control lock.</summary>
    ISamusControlLock SamusControls { get; }
}
