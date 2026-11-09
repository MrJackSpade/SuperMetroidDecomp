using SuperMetroid.TestSupport.RepositoryInstallation;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Verification access to <see cref="InstalledRoomObjectSetupBlocks"/> members production does not use.</summary>
internal static class InstalledRoomObjectSetupBlocksAccess
{
    /// <summary>The pictures the repository installation holds.</summary>
    extension(InstalledRoomObjectSetupBlocks)
    {
        /// <summary>Every setup block's picture as the repository installation holds it.</summary>
        internal static InstalledRoomObjectSetupBlocks Stock() => RepositoryInstallation.Installation.LoadRoomObjectSetupBlocks();
    }
}
