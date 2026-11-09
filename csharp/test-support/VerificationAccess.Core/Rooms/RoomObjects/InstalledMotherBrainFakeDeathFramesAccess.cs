using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Verification access to the repository installation's <see cref="InstalledMotherBrainFakeDeathFrames"/>.</summary>
internal static class InstalledMotherBrainFakeDeathFramesAccess
{
    /// <summary>The installed pictures of Mother Brain's fake-death room frames.</summary>
    extension(InstalledMotherBrainFakeDeathFrames)
    {
        /// <summary>The pictures the repository installation holds, as installed.</summary>
        internal static InstalledMotherBrainFakeDeathFrames Stock() => RepositoryInstallation.Installation.LoadRoomObjectMotherBrainFakeDeathVisuals();
    }
}
