using SuperMetroid.TestSupport.RepositoryInstallation;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Verification access to <see cref="InstalledRoomObjectNoobTubeVisuals"/> members production does not use.</summary>
internal static class InstalledRoomObjectNoobTubeVisualsAccess
{
    /// <summary>Leaves Noob Tube stock cells calculated while allowing custom breakage frames.</summary>
    extension(InstalledRoomObjectNoobTubeVisuals)
    {
        /// <summary>The stock catalog the repository installation loads, with no override selected.</summary>
        internal static InstalledRoomObjectNoobTubeVisuals Stock() => RepositoryInstallation.Installation.LoadRoomObjectNoobTubeVisuals();
    }
}
