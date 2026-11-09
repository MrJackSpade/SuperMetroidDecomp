using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Verification access to the repository installation's <see cref="InstalledEscapeGateFrames"/>.</summary>
internal static class InstalledEscapeGateFramesAccess
{
    /// <summary>The installed pictures of Mother Brain's escape-gate frames.</summary>
    extension(InstalledEscapeGateFrames)
    {
        /// <summary>The pictures the repository installation holds, as installed.</summary>
        internal static InstalledEscapeGateFrames Stock() => RepositoryInstallation.Installation.LoadRoomObjectEscapeGateVisuals();
    }
}
