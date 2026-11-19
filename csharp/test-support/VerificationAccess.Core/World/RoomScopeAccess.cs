using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Areas.Maridia;

namespace SuperMetroid.Core.World;

/// <summary>The room mains' states checks reach through the main the room composed (#2570).</summary>
internal static class RoomScopeAccess
{
    /// <summary>Reads the state the room's main runs.</summary>
    extension(RoomScope self)
    {
        /// <summary>The Maridia elevatube's ride, or null outside the elevatube.</summary>
        internal MaridiaElevatubeRoomMainState? MaridiaElevatube => self.Main is MaridiaElevatubeMain main ? main.tube : null;

        /// <summary>
        /// The Ceres elevator shaft's turn and elevator, or null outside the shaft and in the shaft entered other than through
        /// its elevator door.
        /// </summary>
        internal CeresElevatorShaftRoomMainState? CeresElevatorShaft => self.Main is CeresElevatorShaftMain main ? main.shaft : null;
    }
}
