using SuperMetroid.AssetExtraction.Rooms;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Verification access to <see cref="SpeedBoosterBlockObjectDrawDefinitions"/> members production does not use.</summary>
internal static class SpeedBoosterBlockObjectDrawDefinitionsAccess
{
    /// <summary>Resolves the speed-block bomb-reveal draw from its native pointer.</summary>
    extension(SpeedBoosterBlockObjectDrawDefinitions)
    {
        /// <summary>Recognizes the speed-block bomb-reveal draw pointer.</summary>
        internal static bool TryGet(RoomObjectDraw request,
            out RoomObjectShotBlockDrawDefinitions.DrawList list)
        {
            if (request == SpeedBoosterBlockObjectDrawDefinitions.BombRevealDraw)
            {
                list = RoomObjectSpeedBoosterVisualFiles.BombReveal;
                return true;
            }
            list = default;
            return false;
        }
    }
}
