namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// Verification's native view of the station animations (#1414, #1994): the bank-$84 instruction lists each
/// <see cref="StationAnimation"/>'s images are stored in. Core's values are arbitrary; the retail oracles translate
/// through this table.
/// </summary>
internal static class StationNativeWords
{
    /// <summary>
    /// The bank-$84 lists of each station animation's images, in the cartridge's order: one list holding all of an
    /// animation's images, or, for the save's flash, one list for each image.
    /// </summary>
    private static readonly Dictionary<StationAnimation, ushort[]> ImageLists = new Dictionary<StationAnimation, ushort[]>
    {
        [StationAnimation.MapIdle] = [0xad66],
        [StationAnimation.MapAcquired] = [0xad76],
        [StationAnimation.Energy] = [0xadc6],
        [StationAnimation.Missile] = [0xae50],
        [StationAnimation.SaveIdle] = [RoomObjectAnimationScriptsConstants.SaveStationIdleDraw],
        [StationAnimation.SaveFlash] =
            [RoomObjectAnimationScriptsConstants.SaveStationAnimationFirstFrame, RoomObjectAnimationScriptsConstants.SaveStationAnimationSecondFrame],
    };

    /// <summary>Every list, in the cartridge's order.</summary>
    internal static IEnumerable<ushort> Lists =>
        Enum.GetValues<StationAnimation>().SelectMany(animation => ImageLists[animation]);

    /// <summary>
    /// The bank-$84 address of image <paramref name="beat"/> of <paramref name="animation"/>: its own list, or its place
    /// four bytes per earlier image along the animation's one list.
    /// </summary>
    internal static ushort Record(StationAnimation animation, int beat) =>
        ImageLists[animation] is { Length: > 1 } perImage ? perImage[beat] : checked((ushort)(ImageLists[animation][0] + 4 * beat));
}
