using SuperMetroid.AssetExtraction.Rooms;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Verification access to <see cref="InstalledRoomObjectSpeedBoosterVisuals"/> members production does not use.</summary>
internal static class InstalledRoomObjectSpeedBoosterVisualsAccess
{
    /// <summary>Builds bomb-reveal speed-block art from the reveal draw's own appearance.</summary>
    extension(InstalledRoomObjectSpeedBoosterVisuals)
    {
        /// <summary>Builds the bomb-reveal visual from the speed-block reveal draw's first block.</summary>
        internal static InstalledRoomObjectSpeedBoosterVisuals Stock() => RoomObjectSpeedBoosterVisualFiles.Create(
            [new RoomObjectVisualEntry(
                RoomObjectSpeedBoosterVisualFiles.BombRevealVisualId,
                [RoomObjectSpeedBoosterVisualFiles.BombReveal.Runs.Span[0].Blocks.Span[0].Appearance])]);
    }
}
