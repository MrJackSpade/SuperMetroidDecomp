namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Verification access to <see cref="SpeedBoosterBlockPlmDrawDefinitions"/> members production does not use.</summary>
internal static class SpeedBoosterBlockPlmDrawDefinitionsAccess
{
    /// <summary>Resolves the speed-block bomb-reveal draw from its native pointer.</summary>
    extension(SpeedBoosterBlockPlmDrawDefinitions)
    {
        /// <summary>Recognizes the speed-block bomb-reveal draw pointer.</summary>
        internal static bool TryGet(PlmDraw request,
            out RoomPlmShotBlockDrawDefinitions.DrawList list)
        {
            if (request == SpeedBoosterBlockPlmDrawDefinitions.BombRevealDraw)
            {
                list = SpeedBoosterBlockPlmDrawDefinitions.BombReveal;
                return true;
            }
            list = default;
            return false;
        }
    }
}
