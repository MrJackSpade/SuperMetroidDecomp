using SuperMetroid.TestSupport.RepositoryInstallation;

namespace SuperMetroid.Core.Rooms.RoomObjects;
/// <summary>Verification access to <see cref="InstalledRoomObjectMaridiaElevatubeVisuals"/> members production does not use.</summary>
internal static class InstalledRoomObjectMaridiaElevatubeVisualsAccess
{
    /// <summary>The catalog the repository installation holds.</summary>
    extension(InstalledRoomObjectMaridiaElevatubeVisuals)
    {
        /// <summary>The elevatube's picture as the repository installation holds it.</summary>
        internal static InstalledRoomObjectMaridiaElevatubeVisuals Stock() => RepositoryInstallation.Installation.LoadRoomObjectMaridiaElevatubeVisuals();
    }
}
