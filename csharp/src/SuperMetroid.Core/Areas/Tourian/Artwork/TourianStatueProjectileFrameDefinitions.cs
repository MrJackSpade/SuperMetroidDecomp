namespace SuperMetroid.Core.Areas.Tourian.Artwork;

/// <summary>
/// The presentation boundary of the statue projectile programs (#641): the installed artwork key of each
/// <see cref="TourianStatueProjectileFrame"/> pose, which is the bank-$86 address of the native frame's spritemap operand.
/// </summary>
internal static class TourianStatueProjectileFrameDefinitions
{
    /// <summary>The poses, in installed artwork order.</summary>
    internal const int PoseCount = 28;

    /// <summary>The installed artwork key of each pose.</summary>
    internal static IReadOnlyDictionary<TourianStatueProjectileFrame, ushort> PresentationKeys { get; } = new Dictionary<TourianStatueProjectileFrame, ushort>
    {
        [new(0)] = 0xb7a3,
        [new(1)] = 0xb7a7,
        [new(2)] = 0xb7ab,
        [new(3)] = 0xb7af,
        [new(4)] = 0xb7b5,
        [new(5)] = 0xb7b9,
        [new(6)] = 0xb7bd,
        [new(7)] = 0xb7c1,
        [new(8)] = 0xb7c5,
        [new(9)] = 0xb7c9,
        [new(10)] = 0xb7cd,
        [new(11)] = 0xb7d1,
        [new(12)] = 0xb7d5,
        [new(13)] = 0xb7d9,
        [new(14)] = 0xb804,
        [new(15)] = 0xb808,
        [new(16)] = 0xb80e,
        [new(17)] = 0xb812,
        [new(18)] = 0xb825,
        [new(19)] = 0xb82d,
        [new(20)] = 0xb835,
        [new(21)] = 0xb83d,
        [new(22)] = 0xb850,
        [new(23)] = 0xb854,
        [new(24)] = 0xb85c,
        [new(25)] = 0xb864,
        [new(26)] = 0xb86c,
        [new(27)] = 0xb874,
    };
}
