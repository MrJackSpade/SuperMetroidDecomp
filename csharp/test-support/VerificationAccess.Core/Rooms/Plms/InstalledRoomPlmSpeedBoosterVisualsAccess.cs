namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Verification access to <see cref="InstalledRoomPlmSpeedBoosterVisuals"/> members production does not use.</summary>
internal static class InstalledRoomPlmSpeedBoosterVisualsAccess
{
    /// <summary>Builds bomb-reveal speed-block art from the reveal draw's own appearance.</summary>
    extension(InstalledRoomPlmSpeedBoosterVisuals)
    {
        /// <summary>Builds the bomb-reveal visual from the speed-block reveal draw's first block.</summary>
        internal static InstalledRoomPlmSpeedBoosterVisuals Stock() => new(
            [new RoomPlmVisualEntry(
                SpeedBoosterBlockPlmDrawDefinitions.BombRevealVisualId,
                [SpeedBoosterBlockPlmDrawDefinitions.BombReveal.Runs.Span[0].Blocks.Span[0].Appearance])]);
    }
}
