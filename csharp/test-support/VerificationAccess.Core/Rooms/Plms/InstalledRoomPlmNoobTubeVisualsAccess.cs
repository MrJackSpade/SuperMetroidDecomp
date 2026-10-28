using SuperMetroid.TestSupport.RepositoryInstallation;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Verification access to <see cref="InstalledRoomPlmNoobTubeVisuals"/> members production does not use.</summary>
internal static class InstalledRoomPlmNoobTubeVisualsAccess
{
    /// <summary>Leaves Noob Tube stock cells calculated while allowing custom breakage frames.</summary>
    extension(InstalledRoomPlmNoobTubeVisuals)
    {
        /// <summary>The stock catalog the repository installation loads, with no override selected.</summary>
        internal static InstalledRoomPlmNoobTubeVisuals Stock() => RepositoryInstallation.Installation.LoadRoomPlmNoobTubeVisuals();
    }
}
