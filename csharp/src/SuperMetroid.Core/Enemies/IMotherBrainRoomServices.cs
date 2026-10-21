using SuperMetroid.Core.Rooms.Scrolling;

namespace SuperMetroid.Core.Enemies;

/// <summary>What Mother Brain's fight needs: the room effects and the scroll zones.</summary>
public interface IMotherBrainRoomServices : IRoomScrollServices
{
    /// <summary>Mother Brain's room effects.</summary>
    IMotherBrainRoomEffects MotherBrain { get; }
}
